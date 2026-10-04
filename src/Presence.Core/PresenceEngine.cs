namespace Presence.Core;

// Pure state machine: only fresh observations count. Unmonitored time never causes departures.
public sealed class PresenceEngine(Snapshot snapshot)
{
    public Snapshot Data { get; } = snapshot;
    private DateTimeOffset? previous;
    private string network = "";
    private int baselineScans;
    private readonly HashSet<string> baselineDevices = [];
    public string Network => network;
    public bool Baselining => baselineScans < 1;

    public void Pause()
    {
        previous = null;
        foreach (var d in Data.Devices) d.Consecutive = 0;
    }
    public List<PresenceEvent> Apply(IEnumerable<Observation> observations, string scope, DateTimeOffset now, bool initialSweep = false)
    {
        if (network != scope)
        {
            network = scope; baselineScans = 0; baselineDevices.Clear(); previous = null;
            foreach (var d in Data.Devices) { d.Consecutive = 0; d.MissingSeconds = 0; }
        }
        var delta = previous.HasValue ? (now - previous.Value).TotalSeconds : 0;
        // A suspend, long scan failure or clock jump forces a silent re-baseline.
        if (delta < 0 || delta > Math.Max(90, Data.Settings.ScanIntervalSeconds * 3))
        {
            delta = 0; baselineScans = 0; baselineDevices.Clear();
            foreach (var d in Data.Devices) { d.Consecutive = 0; d.MissingSeconds = 0; }
        }
        previous = now;
        var silent = Baselining || initialSweep;
        var events = new List<PresenceEvent>();
        var seen = new HashSet<string>();
        foreach (var o in observations)
        {
            var mac = Identity.NormalizeMac(o.Mac);
            if (mac == "" || !seen.Add(mac)) continue;
            var d = Data.Devices.FirstOrDefault(d => d.Mac == mac);
            if (d is null) { d = new Device { Mac = mac, FirstSeen = now }; Data.Devices.Add(d); }
            if (silent) baselineDevices.Add(mac);
            d.Ip = o.Ip; d.Network = scope; d.LastSeen = now;
            if (o.Hostname != "") d.Hostname = o.Hostname;
            if (o.Vendor != "") d.Vendor = o.Vendor;
            d.MissingSeconds = 0; d.Consecutive++;
            // A fresh ARP/mDNS response is sufficient. No user approval or second scan is required.
            if (d.Consecutive >= 1)
            {
                var arrived = d.State is not PresenceState.Home and not PresenceState.ProbablyHome;
                var firstConfirmation = !d.Announced;
                if (arrived) { d.State = PresenceState.Home; d.ChangedAt = now; }
                else d.State = PresenceState.Home;
                if (!d.Announced)
                {
                    d.Announced = true;
                    if (!silent && !baselineDevices.Contains(mac) && d.Kind == DeviceKind.Unknown)
                        events.Add(PresenceEvent.Create(now, "new", d.DisplayName, mac));
                }
                if (arrived && !silent && d.Kind != DeviceKind.Ignore && !(firstConfirmation && baselineDevices.Contains(mac)) && !(firstConfirmation && d.Kind == DeviceKind.Unknown))
                    events.Add(PresenceEvent.Create(now, "arrived", EventName(d), mac, d.PersonId));
            }
        }
        foreach (var d in Data.Devices.Where(d => d.Network == scope && !seen.Contains(d.Mac)))
        {
            d.Consecutive = 0; d.MissingSeconds += Math.Max(0, delta);
            if (d.State is PresenceState.Home or PresenceState.ProbablyHome)
            {
                if (d.MissingSeconds >= Data.Settings.DepartureGraceSeconds)
                {
                    d.State = PresenceState.Away; d.ChangedAt = now;
                    if (!silent && d.Kind != DeviceKind.Ignore) events.Add(PresenceEvent.Create(now, "left", EventName(d), d.Mac, d.PersonId));
                }
                else d.State = PresenceState.ProbablyHome;
            }
        }
        // Every device transition is recorded once. Person aggregation drives the list, not duplicate alerts.
        ReconcilePeople(now);
        baselineScans++;
        return events;
    }
    private string EventName(Device device) => !string.IsNullOrWhiteSpace(device.Name) ? device.Name : device.IsPrimary && device.PersonId is not null ? (Data.People.FirstOrDefault(p => p.Id == device.PersonId)?.Name ?? device.DisplayName) + "’s device" : device.DisplayName;
    public void ReconcilePeople(DateTimeOffset now, bool silent = true, List<PresenceEvent>? events = null)
    {
        foreach (var p in Data.People)
        {
            var assigned = Data.Devices.Where(d => d.PersonId == p.Id && d.Kind == DeviceKind.Person).ToList();
            var sources = assigned.Any(d => d.IsPrimary) ? assigned.Where(d => d.IsPrimary).ToList() : assigned;
            var local = sources.Where(d => d.Network == network).ToList();
            var next = local.Any(d => d.State == PresenceState.Home) ? PresenceState.Home : local.Any(d => d.State == PresenceState.ProbablyHome) ? PresenceState.ProbablyHome : local.Count > 0 && local.All(d => d.State == PresenceState.Away) ? PresenceState.Away : PresenceState.Unknown;
            var wasHome = p.State is PresenceState.Home or PresenceState.ProbablyHome;
            var isHome = next is PresenceState.Home or PresenceState.ProbablyHome;
            if (next != p.State)
            {
                // Only confirmed home/away transitions notify. Unknown is never treated as a departure.
                if (!silent && events is not null && ((!wasHome && next == PresenceState.Home) || (wasHome && next == PresenceState.Away)))
                    events.Add(PresenceEvent.Create(now, isHome ? "arrived" : "left", p.Name, local.FirstOrDefault(d => d.IsPrimary)?.Mac ?? local.FirstOrDefault()?.Mac, p.Id));
                if (wasHome != isHome || next == PresenceState.Away) p.ChangedAt = now;
                p.State = next;
            }
        }
    }
    public Person Assign(Device device, string personName, bool primary)
    {
        var name = personName.Trim();
        if (name.Length == 0) throw new ArgumentException("Enter a person's name.");
        var p = Data.People.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (p is null) { p = new Person { Name = name }; Data.People.Add(p); }
        if (primary) foreach (var other in Data.Devices.Where(d => d.PersonId == p.Id)) other.IsPrimary = false;
        device.PersonId = p.Id; device.Kind = DeviceKind.Person; device.IsPrimary = primary;
        ReconcilePeople(DateTimeOffset.UtcNow);
        return p;
    }
}
