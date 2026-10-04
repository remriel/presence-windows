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
    public uint Mask => Prefix == 0 ? 0 : uint.MaxValue << (32 - Prefix);
    public uint First => AddressNumber & Mask;
    public uint Last => First | ~Mask;
    public bool Contains(IPAddress ip) => ip.AddressFamily == AddressFamily.InterNetwork && (Number(ip) & Mask) == First;
    public static uint Number(IPAddress ip) { var b = ip.GetAddressBytes(); return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3]; }
    public static IPAddress Ip(uint value) => new(new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value });
}
public sealed record ScanResult(string Scope, string Description, IReadOnlyList<Observation> Observations, bool InitialSweep, int Coverage, int Total);

public sealed class Discovery
{
    private string scope = "";
    private uint cursor;
    private int sweeps;
    private readonly Dictionary<string, string> names = [];
    private readonly VendorLookup vendors = new();
    public static List<Lan> Interfaces()
    {
        var result = new List<Lan>();
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType is not NetworkInterfaceType.Wireless80211 and not NetworkInterfaceType.Ethernet) continue;
            var description = nic.Description.ToLowerInvariant();
            if (new[] { "virtual", "vpn", "vmware", "hyper-v", "wireguard", "tailscale", "tap-", "tunnel" }.Any(description.Contains)) continue;
            var props = nic.GetIPProperties();
            var gateway = props.GatewayAddresses.FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork && !g.Address.Equals(IPAddress.Any))?.Address;
            if (gateway is null) continue;
            foreach (var addr in props.UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork && a.PrefixLength is >= 16 and <= 30))
                result.Add(new(nic.Id, nic.Name, props.GetIPv4Properties().Index, addr.Address, addr.PrefixLength, gateway, Identity.NormalizeMac(nic.GetPhysicalAddress().ToString())));
        }
        return result.OrderByDescending(n => NetworkInterface.GetAllNetworkInterfaces().Any(i => i.Id == n.Id && i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)).ToList();
    }
    public async Task<ScanResult> ScanAsync(Settings settings, IReadOnlyList<Device> devices, CancellationToken ct)
    {
        var interfaces = Interfaces();
        var lan = settings.InterfaceId == "" ? interfaces.FirstOrDefault() : interfaces.FirstOrDefault(n => n.Id == settings.InterfaceId);
        if (lan is null) throw new IOException("No connected home LAN. Monitoring is paused.");
        var gatewayMac = await Task.Run(() => NativeNeighbors.Resolve(lan, lan.Gateway), ct);
        if (gatewayMac == "") throw new IOException("The gateway did not respond. Monitoring is paused.");
        var key = lan.Id + "|" + Lan.Ip(lan.First) + "/" + lan.Prefix + "|" + gatewayMac;
        if (scope != key) { scope = key; cursor = lan.First + 1; sweeps = 0; names.Clear(); }
        var targets = new HashSet<string> { lan.Gateway.ToString() };
        foreach (var d in devices.Where(d => d.Network == key && d.Kind != DeviceKind.Ignore))
            if (IPAddress.TryParse(d.Ip, out var ip) && lan.Contains(ip) && !ip.Equals(lan.Address)) targets.Add(ip.ToString());
        // Cached neighbor entries are candidates, never evidence. Resolve flushes and sends a new ARP request.
        foreach (var row in NativeNeighbors.Read(lan.Index)) if (lan.Contains(row.Ip) && !row.Ip.Equals(lan.Address)) targets.Add(row.Ip.ToString());
        bool initial = sweeps < 1;
        var count = (int)(lan.Last - lan.First - 1);
        var batch = Math.Min(count, 128);
        for (var i = 0; i < batch; i++)
        {
            if (cursor >= lan.Last) { cursor = lan.First + 1; sweeps++; }
            var ip = Lan.Ip(cursor++); if (!ip.Equals(lan.Address)) targets.Add(ip.ToString());
        }
        if (cursor >= lan.Last) { cursor = lan.First + 1; sweeps++; }
        var mdns = MdnsAsync(lan, ct);
        var found = new ConcurrentDictionary<string, Observation>();
        found[lan.Mac] = new(lan.Mac, lan.Address.ToString(), Environment.MachineName, vendors.Find(lan.Mac), "local-interface");
        found[gatewayMac] = new(gatewayMac, lan.Gateway.ToString(), "", vendors.Find(gatewayMac), "fresh-arp");
        await Parallel.ForEachAsync(targets.Take(512), new ParallelOptions { MaxDegreeOfParallelism = 48, CancellationToken = ct }, async (target, token) =>
        {
            var ip = IPAddress.Parse(target);
            var mac = await Task.Run(() => NativeNeighbors.Resolve(lan, ip), token);
            if (mac == "") return;
            string signal = "fresh-arp";
            using var ping = new Ping();
            try { if ((await ping.SendPingAsync(ip, 250)).Status == IPStatus.Success) signal += "+icmp"; } catch (PingException) { }
            found[mac] = new(mac, target, "", vendors.Find(mac), signal);
        });
        var multicast = await mdns;
        foreach (var pair in multicast)
        {
            names[pair.Key] = pair.Value;
            // Only a new multicast response received in this scan supplies evidence.
            var ip = IPAddress.Parse(pair.Key);
            var mac = await Task.Run(() => NativeNeighbors.Resolve(lan, ip), ct);
            if (mac == "") mac = NativeNeighbors.Read(lan.Index).FirstOrDefault(r => r.Ip.Equals(ip))?.Mac ?? "";
            if (mac != "") found[mac] = new(mac, pair.Key, pair.Value, vendors.Find(mac), "mdns-response");
        }
        foreach (var pair in found.ToArray())
        {
            if (names.TryGetValue(pair.Value.Ip, out var hostname)) found[pair.Key] = pair.Value with { Hostname = hostname };
        }
        return new(key, lan.Name + " · " + Lan.Ip(lan.First) + "/" + lan.Prefix, found.Values.ToList(), initial, initial ? Math.Min(count, (int)(cursor - lan.First - 1)) : count, count);
    }
    private static async Task<Dictionary<string, string>> MdnsAsync(Lan lan, CancellationToken ct)
    {
        var names = new Dictionary<string, string>();
        using var udp = new UdpClient(new IPEndPoint(lan.Address, 0));
        udp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, lan.Address.GetAddressBytes());
        // QU bit requests unicast replies; no multicast listener/firewall exception is needed.
        byte[] query = [0,0,0,0,0,1,0,0,0,0,0,0,9,(byte)'_', (byte)'s',(byte)'e',(byte)'r',(byte)'v',(byte)'i',(byte)'c',(byte)'e',(byte)'s',7,(byte)'_', (byte)'d',(byte)'n',(byte)'s',(byte)'-',(byte)'s',(byte)'d',4,(byte)'_', (byte)'u',(byte)'d',(byte)'p',5,(byte)'l',(byte)'o',(byte)'c',(byte)'a',(byte)'l',0,0,12,128,1];
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(1200);
        try
        {
            await udp.SendAsync(query, new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353), ct);
            while (!timeout.IsCancellationRequested)
            {
                var r = await udp.ReceiveAsync(timeout.Token);
                if (!lan.Contains(r.RemoteEndPoint.Address) || r.Buffer.Length > 9000) continue;
                foreach (var record in DnsPacket.ReadNames(r.Buffer)) if (IPAddress.TryParse(record.Key, out var ip) && lan.Contains(ip)) names[record.Key] = record.Value;
            }
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
        catch (SocketException) { }
        return names;
    }
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

internal sealed record Neighbor(IPAddress Ip, string Mac);
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
                if (mac != "") rows.Add(new(new IPAddress(BitConverter.GetBytes(row.Address.Address)), mac));
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
