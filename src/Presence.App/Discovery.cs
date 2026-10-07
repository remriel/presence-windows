using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Presence.Core;

namespace Presence.App;

public sealed record Lan(string Id, string Name, int Index, IPAddress Address, int Prefix, IPAddress Gateway, string Mac)
{
    public uint AddressNumber => Number(Address);
    public uint Mask => uint.MaxValue << (32 - Prefix);
    public uint First => AddressNumber & Mask;
    public uint Last => First | ~Mask;
    public bool Contains(IPAddress ip) => ip.AddressFamily == AddressFamily.InterNetwork && Number(ip) > First && Number(ip) < Last;
    public static uint Number(IPAddress ip) { var b = ip.GetAddressBytes(); return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3]; }
    public static IPAddress Ip(uint value) => new(new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value });
}
public sealed record ScanResult(string Scope, string Description, IReadOnlyList<Observation> Observations, bool InitialSweep, int Coverage, int Total, IReadOnlyCollection<string>? EvaluatedMacs = null, bool IsPartial = false);

public sealed class Discovery : IDisposable
{
    private readonly ArpWorkers priority = new(12);
    private readonly ArpWorkers sweep = new(32);
    private readonly ConcurrentDictionary<string, Observation> recent = new(StringComparer.OrdinalIgnoreCase);
    private readonly VendorLookup vendors = new();
    private CancellationTokenSource? laneCancellation;
    private Task? background;
    private string scope = "";
    private string laneId = "";
    private int generation;
    private long gatewaySeen;
    private bool baselinePending = true;
    private Lan? cachedLan;
    private DateTimeOffset interfacesRead;
    public string CurrentScope => scope;
    public void Reset()
    {
        generation++; laneCancellation?.Cancel(); cachedLan = null; scope = ""; laneId = ""; baselinePending = true; recent.Clear(); background = null;
    }
    public static List<Lan> Interfaces()
    {
        var found = new List<(Lan Lan, bool Wifi)>();
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType is not NetworkInterfaceType.Wireless80211 and not NetworkInterfaceType.Ethernet) continue;
            var description = nic.Description.ToLowerInvariant();
            if (new[] { "virtual", "vpn", "vmware", "hyper-v", "wireguard", "tailscale", "tap-", "tunnel" }.Any(description.Contains)) continue;
            try
            {
                var props = nic.GetIPProperties(); var index = props.GetIPv4Properties()?.Index;
                if (index is null) continue;
                var gateways = props.GatewayAddresses.Where(g => g.Address.AddressFamily == AddressFamily.InterNetwork && !g.Address.Equals(IPAddress.Any)).Select(g => g.Address).ToList();
                foreach (var address in props.UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork && a.PrefixLength is >= 16 and <= 30))
                {
                    var mask = uint.MaxValue << (32 - address.PrefixLength);
                    var gateway = gateways.FirstOrDefault(g => (Lan.Number(g) & mask) == (Lan.Number(address.Address) & mask));
                    if (gateway is null) continue;
                    found.Add((new(nic.Id, nic.Name, index.Value, address.Address, address.PrefixLength, gateway, Identity.NormalizeMac(nic.GetPhysicalAddress().ToString())), nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211));
                }
            }
            catch (NetworkInformationException) { }
        }
        return found.OrderByDescending(x => x.Wifi).Select(x => x.Lan).ToList();
    }
    public async Task<ScanResult> ScanAsync(Settings settings, IReadOnlyList<Device> devices, CancellationToken ct, IProgress<ScanResult>? progress = null)
    {
        var scanGeneration = generation;
        if (cachedLan is null || DateTimeOffset.UtcNow - interfacesRead > TimeSpan.FromSeconds(10) || (settings.InterfaceId != "" && settings.InterfaceId != cachedLan.Id))
        {
            var interfaces = Interfaces();
            cachedLan = settings.InterfaceId == "" ? interfaces.FirstOrDefault() : interfaces.FirstOrDefault(n => n.Id == settings.InterfaceId);
            interfacesRead = DateTimeOffset.UtcNow;
        }
        var lan = cachedLan ?? throw new IOException("No connected local network. Monitoring is paused.");
        var gatewayMac = await priority.ResolveAsync(lan, lan.Gateway, ct);
        ct.ThrowIfCancellationRequested();
        if (scanGeneration != generation) throw new OperationCanceledException("Network changed during gateway discovery.");
        if (gatewayMac == "") { Reset(); throw new IOException("The gateway is unavailable. Monitoring is paused."); }
        Interlocked.Exchange(ref gatewaySeen, DateTimeOffset.UtcNow.UtcTicks);
        var key = lan.Id + "|" + Lan.Ip(lan.First) + "/" + lan.Prefix + "|" + gatewayMac;
        // DHCP can change the local address without changing the stable network identity.
        var nextLane = key + "|" + lan.Index + "|" + lan.Address + "|" + lan.Gateway;
        if (laneId != nextLane || laneCancellation?.IsCancellationRequested == true)
        {
            laneCancellation?.Cancel(); laneCancellation?.Dispose(); laneCancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);
            scope = key; laneId = nextLane; baselinePending = true; recent.Clear(); background = null;
        }
        var token = laneCancellation!.Token;
        var started = DateTimeOffset.UtcNow;
        var initial = baselinePending;
        var count = (int)(lan.Last - lan.First - 1);
        var description = lan.Name + " · " + Lan.Ip(lan.First) + "/" + lan.Prefix;
        void Observe(string mac, IPAddress ip, string hostname, string signal, bool quiet)
        {
            if (token.IsCancellationRequested || key != scope || mac == "") return;
            if (!ip.Equals(lan.Gateway) && mac == gatewayMac || !ip.Equals(lan.Address) && mac == lan.Mac) return;
            var at = DateTimeOffset.UtcNow;
            var previous = recent.GetValueOrDefault(mac);
            if (hostname == "" && previous?.Ip == ip.ToString()) hostname = previous.Hostname;
            var observation = new Observation(mac, ip.ToString(), hostname, vendors.Find(mac), signal, at);
            recent[mac] = observation;
            if (previous is null || at - previous.At.GetValueOrDefault() >= TimeSpan.FromMilliseconds(500))
                progress?.Report(new(key, description, [observation], quiet, 0, count, [], true));
        }
        Observe(gatewayMac, lan.Gateway, "", "fresh-arp", initial);
        if (lan.Mac != "") Observe(lan.Mac, lan.Address, Environment.MachineName, "local-interface", initial);
        if (background is null || background.IsCompleted)
            background = BackgroundDiscovery(lan, key, gatewayMac, Observe, token);
        var targets = new HashSet<string>();
        foreach (var d in devices.Where(d => d.Network == key && d.Kind != DeviceKind.Ignore))
            if (IPAddress.TryParse(d.Ip, out var ip) && lan.Contains(ip) && !ip.Equals(lan.Address) && !ip.Equals(lan.Gateway)) targets.Add(ip.ToString());
        foreach (var neighbor in NativeNeighbors.Read(lan.Index).Where(n => n.State == 5).Take(128))
            if (lan.Contains(neighbor.Ip) && !neighbor.Ip.Equals(lan.Address) && !neighbor.Ip.Equals(lan.Gateway)) targets.Add(neighbor.Ip.ToString());
        await Parallel.ForEachAsync(targets, new ParallelOptions { MaxDegreeOfParallelism = 24, CancellationToken = token }, async (target, cancellation) =>
        {
            var ip = IPAddress.Parse(target); var mac = await priority.ResolveAsync(lan, ip, cancellation);
            Observe(mac, ip, "", "fresh-arp", initial);
        });
        token.ThrowIfCancellationRequested();
        baselinePending = false;
        var evaluated = devices.Where(d => d.Network == key && (targets.Contains(d.Ip) || d.Mac == lan.Mac || d.Mac == gatewayMac)).Select(d => d.Mac).ToArray();
        return new(key, description, recent.Values.Where(o => o.At >= started).ToList(), initial, count, count, evaluated);
    }
    private async Task BackgroundDiscovery(Lan lan, string key, string gatewayMac, Action<string, IPAddress, string, string, bool> observe, CancellationToken ct)
    {
        var count = (int)(lan.Last - lan.First - 1);
        var quietUntil = DateTimeOffset.UtcNow.AddSeconds(3);
        async Task ArpSweep()
        {
            var firstPass = true;
            while (!ct.IsCancellationRequested)
            {
                await Parallel.ForEachAsync(Enumerable.Range(1, count), new ParallelOptions { MaxDegreeOfParallelism = 32, CancellationToken = ct }, async (offset, token) =>
                {
                    if ((DateTimeOffset.UtcNow.UtcTicks - Interlocked.Read(ref gatewaySeen)) > TimeSpan.FromSeconds(30).Ticks) return;
                    var ip = Lan.Ip(lan.First + (uint)offset);
                    if (ip.Equals(lan.Address) || ip.Equals(lan.Gateway)) return;
                    var mac = await sweep.ResolveAsync(lan, ip, token);
                    observe(mac, ip, "", "fresh-arp", firstPass && DateTimeOffset.UtcNow < quietUntil);
                });
                firstPass = false;
                await Task.Delay(1000, ct);
            }
        }
        async Task LocalResponses()
        {
            var firstPass = true; uint cursor = lan.First + 1;
            while (!ct.IsCancellationRequested)
            {
                var multicast = MdnsAsync(lan, ct);
                // Async ICMP finds responsive new hosts without blocking the known-device path.
                var targets = new List<IPAddress>();
                for (var i = 0; i < Math.Min(count, 4096); i++)
                {
                    if (cursor >= lan.Last) cursor = lan.First + 1;
                    var ip = Lan.Ip(cursor++); if (!ip.Equals(lan.Address) && !ip.Equals(lan.Gateway)) targets.Add(ip);
                }
                await Parallel.ForEachAsync(targets, new ParallelOptions { MaxDegreeOfParallelism = 128, CancellationToken = ct }, async (ip, token) =>
                {
                    using var ping = new Ping();
                    try
                    {
                        if ((await ping.SendPingAsync(ip, 350).WaitAsync(token)).Status != IPStatus.Success) return;
                        var mac = await sweep.ResolveAsync(lan, ip, token);
                        observe(mac, ip, "", "icmp+fresh-arp", firstPass && DateTimeOffset.UtcNow < quietUntil);
                    }
                    catch (PingException) { }
                });
                foreach (var pair in await multicast)
                {
                    var ip = IPAddress.Parse(pair.Key); var mac = await sweep.ResolveAsync(lan, ip, ct);
                    // The response's sender is fresh; cache supplies identity only if resolution is unavailable.
                    if (mac == "") mac = NativeNeighbors.Read(lan.Index).FirstOrDefault(n => n.Ip.Equals(ip))?.Mac ?? "";
                    observe(mac, ip, pair.Value, "mdns-response", firstPass && DateTimeOffset.UtcNow < quietUntil);
                }
                firstPass = false;
                await Task.Delay(3000, ct);
            }
        }
        try
        {
            var arp = ArpSweep(); var responses = LocalResponses();
            var completed = await Task.WhenAny(arp, responses);
            if (completed.IsFaulted && key == scope) laneCancellation?.Cancel();
            await Task.WhenAll(arp, responses);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex) when (ex is SocketException or NetworkInformationException or InvalidOperationException or IOException) { }
    }
    private static async Task<Dictionary<string, string>> MdnsAsync(Lan lan, CancellationToken ct)
    {
        var found = new Dictionary<string, string>();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(1200);
        try
        {
            using var udp = new UdpClient(new IPEndPoint(lan.Address, 0));
            udp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, lan.Address.GetAddressBytes());
            byte[] query = [0,0,0,0,0,1,0,0,0,0,0,0,9,95,115,101,114,118,105,99,101,115,7,95,100,110,115,45,115,100,4,95,117,100,112,5,108,111,99,97,108,0,0,12,128,1];
            await udp.SendAsync(query, new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353), ct);
            while (!timeout.IsCancellationRequested)
            {
                var reply = await udp.ReceiveAsync(timeout.Token);
                if (!lan.Contains(reply.RemoteEndPoint.Address) || reply.Buffer.Length > 9000) continue;
                foreach (var record in DnsPacket.ReadNames(reply.Buffer))
                    if (record.Key == reply.RemoteEndPoint.Address.ToString()) found[record.Key] = record.Value;
            }
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
        catch (SocketException) { }
        return found;
    }
    public void Dispose() { Reset(); priority.Dispose(); sweep.Dispose(); laneCancellation?.Dispose(); }
}

// Fixed background workers isolate blocking Windows ARP from the CLR/UI thread pool.
internal sealed class ArpWorkers : IDisposable
{
    private sealed record Work(Lan Lan, IPAddress Ip, CancellationToken Token, TaskCompletionSource<string> Completion);
    private readonly BlockingCollection<Work> queue = new(1024);
    private bool disposed;
    public ArpWorkers(int count)
    {
        for (var i = 0; i < count; i++) new Thread(Run, 256 * 1024) { IsBackground = true, Name = "Presence ARP" }.Start();
    }
    public Task<string> ResolveAsync(Lan lan, IPAddress ip, CancellationToken token)
    {
        if (disposed || token.IsCancellationRequested) return Task.FromCanceled<string>(new CancellationToken(true));
        var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        try { if (!queue.TryAdd(new(lan, ip, token, completion))) completion.TrySetException(new IOException("Discovery queue is busy.")); }
        catch (InvalidOperationException) { completion.TrySetCanceled(); }
        return completion.Task.WaitAsync(token);
    }
    private void Run()
    {
        foreach (var work in queue.GetConsumingEnumerable())
        {
            if (work.Token.IsCancellationRequested) { work.Completion.TrySetCanceled(work.Token); continue; }
            try { work.Completion.TrySetResult(NativeNeighbors.Resolve(work.Lan, work.Ip)); }
            catch (Exception ex) { work.Completion.TrySetException(ex); }
        }
    }
    public void Dispose() { if (disposed) return; disposed = true; queue.CompleteAdding(); }
}
internal static class DnsPacket
{
    public static Dictionary<string, string> ReadNames(byte[] packet)
    {
        var result = new Dictionary<string, string>();
        try
        {
            if (packet.Length < 12 || (packet[2] & 128) == 0) return result;
            int U16(int p) => (packet[p] << 8) | packet[p + 1];
            var pos = 12; var questions = U16(4); var records = U16(6) + U16(8) + U16(10);
            for (var i = 0; i < Math.Min(questions, 100); i++) { ReadName(packet, ref pos); pos += 4; }
            for (var i = 0; i < Math.Min(records, 300); i++)
            {
                var name = ReadName(packet, ref pos); var type = U16(pos); var length = U16(pos + 8); pos += 10;
                if (pos + length > packet.Length) break;
                if (type == 1 && length == 4 && name.EndsWith(".local", StringComparison.OrdinalIgnoreCase)) result[new IPAddress(packet.AsSpan(pos, 4)).ToString()] = name[..^6];
                pos += length;
            }
        }
        catch (Exception ex) when (ex is IndexOutOfRangeException or ArgumentException or InvalidDataException) { }
        return result;
    }
    private static string ReadName(byte[] p, ref int pos)
    {
        var labels = new List<string>(); var cursor = pos; bool jumped = false;
        for (var hops = 0; hops < 128; hops++)
        {
            if (cursor >= p.Length) throw new InvalidDataException();
            var length = p[cursor++];
            if (length == 0) { if (!jumped) pos = cursor; return string.Join('.', labels); }
            if ((length & 192) == 192) { if (cursor >= p.Length) throw new InvalidDataException(); if (!jumped) pos = cursor + 1; cursor = ((length & 63) << 8) | p[cursor]; jumped = true; continue; }
            if (length > 63 || cursor + length > p.Length) throw new InvalidDataException();
            labels.Add(Encoding.UTF8.GetString(p, cursor, length)); cursor += length;
        }
        throw new InvalidDataException();
    }
}

internal sealed record Neighbor(IPAddress Ip, string Mac, uint State = 0);
internal static class NativeNeighbors
{
    [StructLayout(LayoutKind.Explicit, Size = 28)] private struct Sockaddr { [FieldOffset(0)] public ushort Family; [FieldOffset(4)] public uint Address; }
    [StructLayout(LayoutKind.Explicit, Size = 88)] private struct Row
    {
        [FieldOffset(0)] public Sockaddr Address;
        [FieldOffset(28)] public uint InterfaceIndex;
        [FieldOffset(32)] public ulong Luid;
        [FieldOffset(40)] public ulong Mac0;
        [FieldOffset(48)] public ulong Mac1;
        [FieldOffset(56)] public ulong Mac2;
        [FieldOffset(64)] public ulong Mac3;
        [FieldOffset(72)] public uint Length;
        [FieldOffset(76)] public uint State;
        [FieldOffset(80)] public byte Flags;
        [FieldOffset(84)] public uint ReachabilityTime;
    }
    [DllImport("iphlpapi.dll")] private static extern uint ResolveIpNetEntry2(ref Row row, ref Sockaddr source);
    [DllImport("iphlpapi.dll")] private static extern uint GetIpNetTable2(ushort family, out IntPtr table);
    [DllImport("iphlpapi.dll")] private static extern void FreeMibTable(IntPtr table);

    public static string Resolve(Lan lan, IPAddress ip)
    {
        var row = new Row { InterfaceIndex = (uint)lan.Index, Address = new Sockaddr { Family = 2, Address = BitConverter.ToUInt32(ip.GetAddressBytes()) } };
        var source = new Sockaddr { Family = 2, Address = BitConverter.ToUInt32(lan.Address.GetAddressBytes()) };
        return ResolveIpNetEntry2(ref row, ref source) == 0 && row.Length == 6 ? Identity.NormalizeMac(Convert.ToHexString(BitConverter.GetBytes(row.Mac0)[..6])) : "";
    }

    public static List<Neighbor> Read(int index)
    {
        var rows = new List<Neighbor>();
        if (GetIpNetTable2(2, out var table) != 0) return rows;
        try
        {
            int count = Marshal.ReadInt32(table);
            for (var i = 0; i < Math.Min(count, 65536); i++)
            {
                var row = Marshal.PtrToStructure<Row>(table + 8 + i * 88);
                if (row.InterfaceIndex != index || row.Length != 6 || row.Address.Family != 2) continue;
                var mac = Identity.NormalizeMac(Convert.ToHexString(BitConverter.GetBytes(row.Mac0)[..6]));
                if (mac != "") rows.Add(new(new IPAddress(BitConverter.GetBytes(row.Address.Address)), mac, row.State));
            }
        }
        finally { FreeMibTable(table); }
        return rows;
    }
}

internal sealed class VendorLookup
{
    private readonly Dictionary<string, string> entries = [];
    public VendorLookup()
    {
        var path = System.IO.Path.Combine(AppContext.BaseDirectory, "oui.csv");
        if (!File.Exists(path)) return;
        foreach (var line in File.ReadLines(path).Skip(1))
        {
            var fields = line.Split(',', 4); if (fields.Length < 3) continue;
            var prefix = fields[1].Trim('"'); var vendor = fields[2].Trim('"');
            if (prefix.Length == 6 && prefix.All(Uri.IsHexDigit)) entries[prefix.ToUpperInvariant()] = vendor;
        }
    }
    public string Find(string mac)
    {
        if (mac.Length < 8) return "";
        if (Identity.IsPrivateMac(mac)) return "Private MAC";
        return entries.GetValueOrDefault(mac.Replace(":", "")[..6], "");
    }
}

