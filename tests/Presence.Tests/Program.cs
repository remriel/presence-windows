using Presence.Core;

static void Expect(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var start = DateTimeOffset.Parse("2026-10-04T12:00:00Z");
var mac = "02:11:22:33:44:55";
var scope = "test-network";

var snapshot = new Snapshot();
snapshot.Settings.ScanIntervalSeconds = 10;
snapshot.Settings.DepartureGraceSeconds = 30;
var engine = new PresenceEngine(snapshot);

var baseline = engine.Apply([new Observation(mac, "192.168.1.20", "phone", "Private MAC", "fresh-arp")], scope, start, initialSweep: true);
Expect(baseline.Count == 0, "Initial baseline must stay silent.");
Expect(snapshot.Devices.Single().State == PresenceState.Home, "Baseline observation should mark the device home.");

var shortMiss = engine.Apply([], scope, start.AddSeconds(10));
Expect(shortMiss.Count == 0, "One missed scan must not declare a departure.");
Expect(snapshot.Devices.Single().State == PresenceState.ProbablyHome, "Short radio silence should be tolerated.");

var departure = engine.Apply([], scope, start.AddSeconds(31));
Expect(departure.Count(e => e.Type == "left") == 1, "Departure should fire once after the grace period.");
Expect(snapshot.Devices.Single().State == PresenceState.Away, "Device should be away after the grace period.");

var returnEvents = engine.Apply([new Observation(mac, "192.168.1.20", "phone", "Private MAC", "fresh-arp")], scope, start.AddSeconds(40));
Expect(returnEvents.Count(e => e.Type == "arrived") == 1, "A returning known device should emit one arrival.");
Expect(snapshot.Devices.Single().State == PresenceState.Home, "Returning device should be home.");

var newcomer = "02:AA:BB:CC:DD:EE";
var newEvents = engine.Apply([new Observation(mac, "192.168.1.20", "phone", "Private MAC", "fresh-arp"), new Observation(newcomer, "192.168.1.21", "", "", "fresh-arp")], scope, start.AddSeconds(50));
Expect(newEvents.Count(e => e.Type == "new" && e.Mac == newcomer) == 1, "A newly discovered unknown device should emit one new-device event.");
Expect(newEvents.All(e => !(e.Type == "arrived" && e.Mac == newcomer)), "First unknown detection must not duplicate the new-device alert.");

// Automatic known classification applies on joining, never overrides saved choices,
// and preserves the existing first-discovery notification preference.
var autoSnapshot = new Snapshot();
Expect(!autoSnapshot.Settings.AutoMarkJoiningDevicesKnown, "Auto-known is off for new and legacy profiles.");
autoSnapshot.Settings.AutoMarkJoiningDevicesKnown = true;
var autoEngine = new PresenceEngine(autoSnapshot);
var baselineMac = "02:00:00:00:10:01";
var joinMac = "02:00:00:00:10:02";
var oldMac = "02:00:00:00:10:03";
var ignoredMac = "02:00:00:00:10:04";
var personMac = "02:00:00:00:10:05";
var baselineObservation = new Observation(baselineMac, "192.0.2.1", "Baseline", "", "test");
Expect(autoEngine.Apply([baselineObservation], scope, start, initialSweep: true).Count == 0 && autoSnapshot.Devices.Single().Kind == DeviceKind.Known,
    "Auto-known applies during the quiet baseline without emitting an alert.");
autoSnapshot.Devices.Add(new Device { Mac = oldMac, Kind = DeviceKind.Unknown, State = PresenceState.Away, Network = scope, Announced = true });
autoSnapshot.Devices.Add(new Device { Mac = ignoredMac, Kind = DeviceKind.Ignore, State = PresenceState.Away, Network = scope, Announced = true });
autoSnapshot.Devices.Add(new Device { Mac = personMac, Kind = DeviceKind.Person, PersonId = "person", State = PresenceState.Away, Network = scope, Announced = true });
var joining = autoEngine.Apply([
    new Observation(joinMac, "192.0.2.2", "New", "", "test"),
    new Observation(oldMac, "192.0.2.3", "Returning", "", "test"),
    new Observation(ignoredMac, "192.0.2.4", "Ignored", "", "test"),
    new Observation(personMac, "192.0.2.5", "Assigned", "", "test")], scope, start.AddSeconds(5));
Expect(autoSnapshot.Devices.Single(d => d.Mac == joinMac).Kind == DeviceKind.Known && autoSnapshot.Devices.Single(d => d.Mac == oldMac).Kind == DeviceKind.Known,
    "New and returning unidentified devices become known on joining.");
Expect(joining.Count(e => e.Mac == joinMac && e.Type == "new") == 1 && joining.All(e => e.Mac != joinMac || e.Type != "arrived"),
    "Automatic classification preserves exactly one new-device event on first discovery.");
Expect(autoSnapshot.Devices.Single(d => d.Mac == ignoredMac).Kind == DeviceKind.Ignore && joining.All(e => e.Mac != ignoredMac) &&
       autoSnapshot.Devices.Single(d => d.Mac == personMac).Kind == DeviceKind.Person && autoSnapshot.Devices.Single(d => d.Mac == personMac).PersonId == "person",
    "Ignored and person-assigned devices keep their classifications and notification behavior.");
autoSnapshot.Settings.AutoMarkJoiningDevicesKnown = false;
var laterMac = "02:00:00:00:10:06";
autoEngine.Apply([new Observation(laterMac, "192.0.2.6", "Later", "", "test")], scope, start.AddSeconds(10));
Expect(autoSnapshot.Devices.Single(d => d.Mac == laterMac).Kind == DeviceKind.Unknown, "Disabling auto-known restores unknown classification for new arrivals.");
var legacyAuto = System.Text.Json.JsonSerializer.Deserialize<Snapshot>("{\"Settings\":{}}")!;
Expect(!legacyAuto.Settings.AutoMarkJoiningDevicesKnown, "Existing snapshots default auto-known to off.");
var autoPath = Path.Combine(Path.GetTempPath(), "PresenceAutoKnownTests", Guid.NewGuid().ToString("N"), "presence.db");
autoSnapshot.Settings.AutoMarkJoiningDevicesKnown = true;
using (var repository = new Repository(autoPath)) repository.Save(autoSnapshot);
using (var repository = new Repository(autoPath)) Expect(repository.Load().Settings.AutoMarkJoiningDevicesKnown && repository.Load().Devices.Single(d => d.Mac == joinMac).Kind == DeviceKind.Known,
    "The auto-known setting and classified device survive database reopen.");

Console.WriteLine("Presence.Core checks passed.");

var resetFolder = Path.Combine(Path.GetTempPath(), "PresenceResetTests", Guid.NewGuid().ToString("N"));
var resetPath = Path.Combine(resetFolder, "presence.db");
using (var store = new Repository(resetPath))
{
    snapshot.Settings.Theme = "Dark"; snapshot.Settings.ScanIntervalSeconds = 12;
    store.Save(snapshot, [PresenceEvent.Create(start, "new", "Fictional phone", mac)], [new Observation(mac, "192.0.2.1", "Fictional", "", "test")], start);
    using (var db = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={resetPath}"))
    {
        db.Open(); using var command = db.CreateCommand();
        command.CommandText = "CREATE TRIGGER RejectReset BEFORE DELETE ON State BEGIN SELECT RAISE(ABORT,'test failure'); END"; command.ExecuteNonQuery();
        try { store.Reset(); throw new Exception("Reset should have failed."); } catch (Microsoft.Data.Sqlite.SqliteException) { }
        Expect(store.Load().Devices.Count == 2 && store.History().Count == 1, "Failed reset must roll back profile and history deletion.");
        command.CommandText = "SELECT COUNT(*) FROM Observation"; Expect(Convert.ToInt32(command.ExecuteScalar()) == 1, "Failed reset must roll back observation deletion.");
        command.CommandText = "DROP TRIGGER RejectReset"; command.ExecuteNonQuery();
        var defaults = store.Reset();
        Expect(defaults.Devices.Count == 0 && defaults.People.Count == 0 && defaults.Settings.Theme == "System" && defaults.Settings.ScanIntervalSeconds == 3, "Reset must return empty default state.");
        Expect(store.History().Count == 0, "Reset must clear events.");
        command.CommandText = "SELECT COUNT(*) FROM Observation"; Expect(Convert.ToInt32(command.ExecuteScalar()) == 0, "Reset must clear observations.");
        store.Save(defaults, observations: [new Observation(mac, "192.0.2.1", "Fictional", "", "test")], at: start.AddSeconds(1));
        Expect(Convert.ToInt32(command.ExecuteScalar()) == 1, "Reset must clear observation throttle cache.");
    }
}
using (var store = new Repository(resetPath)) Expect(store.Load().Devices.Count == 0 && store.Load().Settings.Theme == "System", "Reset must survive database reopen.");
Console.WriteLine("Transactional reset and persistence checks passed.");

var absentData = new Snapshot();
var absent = new Device { Mac = mac, Network = scope, Ip = "192.0.2.20", Kind = DeviceKind.Known, LastSeen = start.AddDays(-1) };
absentData.Devices.Add(absent); var absentEngine = new PresenceEngine(absentData);
Expect(absentEngine.Apply([], scope, start, evaluatedMacs: [mac]).Count == 0 && absent.State == PresenceState.Unknown, "Restart does not immediately infer absence.");
absentEngine.Apply([], scope, start.AddSeconds(15), evaluatedMacs: []);
Expect(absent.State == PresenceState.Unknown, "Unprobed devices stay uncertain.");
Expect(absentEngine.Apply([], scope, start.AddSeconds(31), evaluatedMacs: [mac]).Count == 0 && absent.State == PresenceState.Away, "Evaluated absent saved device becomes Away silently after grace.");
var beforeSeen = absent.LastSeen; var beforeChanged = absent.ChangedAt;
Expect(absentEngine.Apply([new Observation(mac, absent.Ip, "Alex-iPhone", "", "identity-only")], scope, start.AddSeconds(35), partial: true).Count == 0, "Hostname enrichment emits no events.");
Expect(absent.Hostname == "Alex-iPhone" && absent.State == PresenceState.Away && absent.LastSeen == beforeSeen && absent.ChangedAt == beforeChanged, "DNS metadata cannot revive an absent device or change presence timestamps.");
absentEngine.Apply([new Observation(mac, "192.0.2.99", "Wrong-host", "", "identity-only"), new Observation(newcomer, absent.Ip, "New-host", "", "identity-only")], scope, start.AddSeconds(36), partial: true);
Expect(absent.Hostname == "Alex-iPhone" && absentData.Devices.Count == 1, "Stale IP enrichment and DNS-only new identities are ignored.");
absentEngine.ResetNetwork(); absentEngine.Apply([], scope, start.AddSeconds(40), evaluatedMacs: [mac]);
absentEngine.Apply([], scope, start.AddSeconds(80), evaluatedMacs: [mac], partial: true);
Expect(absent.State == PresenceState.Unknown, "Partial discoveries never count as failed probes.");
Expect(DeviceIdentification.InferType("Alex-iPhone") == "Phone" && DeviceIdentification.InferType("Office-LaserJet") == "Printer" && DeviceIdentification.InferType("living-room-roku") == "TV / streaming", "Recognize common advertised hostname types.");
Expect(DeviceIdentification.InferType("Apple") == "Unknown" && DeviceIdentification.InferType("") == "Unknown", "Ambiguous manufacturer or absent hostname stays Unknown.");
var clockData = new Snapshot(); var clockEngine = new PresenceEngine(clockData);
clockEngine.Apply([new Observation(mac, "192.0.2.20", "", "", "test")], scope, start);
clockEngine.Apply([new Observation(mac, "192.0.2.20", "Fixture", "", "identity-only")], "other-network", start.AddSeconds(100));
Expect(clockEngine.Network == scope && clockData.Devices.Single().State == PresenceState.Home, "DNS metadata cannot change network scope.");
clockEngine.Apply([new Observation(mac, "192.0.2.20", "Fixture", "", "identity-only")], scope, start.AddSeconds(200));
clockEngine.Apply([], scope, start.AddSeconds(201));
Expect(clockData.Devices.Single().MissingSeconds == 0, "DNS metadata cannot mask a long monitoring gap or advance absence clock.");
Console.WriteLine("Restart absence and identity enrichment checks passed.");

ReliabilityChecks.Run();
DeviceCategoryChecks.Run();

var networks = new List<SavedNetwork>();
Expect(NetworkNames.Default("Café Wi-Fi", "Wi-Fi · subnet") == "Café Wi-Fi", "Use the actual SSID, including Unicode, by default.");
Expect(NetworkNames.Default("", "Ethernet · subnet") == "Ethernet · subnet", "Unavailable SSID has a usable adapter/subnet fallback.");
NetworkNames.Remember(networks, "network-a", " Home ");
NetworkNames.Remember(networks, "network-b", "Home");
NetworkNames.Remember(networks, "network-a", "Renamed");
Expect(networks.Count == 2 && networks.Single(n => n.Scope == "network-a").Name == "Renamed", "Separate identities may share a name; renaming does not duplicate a network.");
foreach (var invalidName in new[] { "", "   ", new string('a', 129) })
{
    try { NetworkNames.Remember(networks, "network-a", invalidName); throw new Exception("Invalid name accepted."); } catch (ArgumentException) { }
}
var legacy = System.Text.Json.JsonSerializer.Deserialize<Snapshot>("{\"Devices\":[],\"People\":[],\"Settings\":{}}")!;
Expect(legacy.Networks.Count == 0, "Existing profiles load with an empty saved-network list.");
var networkFolder = Path.Combine(Path.GetTempPath(), "PresenceNetworkTests", Guid.NewGuid().ToString("N"));
var networkPath = Path.Combine(networkFolder, "presence.db");
using (var repository = new Repository(networkPath)) repository.Save(new Snapshot { Networks = networks });
using (var repository = new Repository(networkPath))
{
    Expect(repository.Load().Networks.SequenceEqual(networks), "Saved names survive closing and reopening SQLite.");
    repository.Reset();
    Expect(repository.Load().Networks.Count == 0, "Reset clears saved network names.");
}
Console.WriteLine("Saved network identity, validation, migration and persistence checks passed.");

var timelineStart = DateTimeOffset.Parse("2026-10-03T00:00:00Z");
var timelineEnd = timelineStart.AddDays(1).AddHours(12);
var timelineDevice = new Device { Mac = mac, Name = "Fictional phone", FirstSeen = timelineStart, State = PresenceState.Home };
var timelineEvents = new[]
{
    PresenceEvent.Create(timelineStart.AddHours(12), "left", timelineDevice.Name, mac),
    PresenceEvent.Create(timelineStart.AddDays(1).AddHours(6), "arrived", timelineDevice.Name, mac)
};
var timeline = PresenceTimeline.Build([timelineDevice], timelineEvents, timelineStart, timelineEnd, TimeZoneInfo.Utc).Single();
Expect(timeline.Days.Count == 2 && Math.Abs(timeline.Days[0].Fraction!.Value - .5) < .001 && Math.Abs(timeline.Days[1].Fraction!.Value - .25) < .001,
    "Timeline reconstructs per-day presence from arrival and departure intervals.");
var unknownTimeline = PresenceTimeline.Build(
    [new Device { Mac = newcomer, FirstSeen = timelineStart, State = PresenceState.Unknown }],
    [], timelineStart, timelineEnd, TimeZoneInfo.Utc).Single();
Expect(unknownTimeline.Days.All(day => day.Fraction is null), "Timeline leaves days without recorded presence state unfilled.");
var baselineAway = new Device
{
    Mac = newcomer, FirstSeen = timelineStart.AddDays(-1), LastSeen = timelineStart.AddHours(6),
    ChangedAt = timelineStart.AddDays(1), State = PresenceState.Away
};
var inferredTimeline = PresenceTimeline.Build([baselineAway], [], timelineStart, timelineEnd, TimeZoneInfo.Utc).Single();
Expect(Math.Abs(inferredTimeline.Days[0].Fraction!.Value - .75) < .001 && inferredTimeline.Days[1].Fraction == 0,
    "Timeline uses saved last-seen and state-change timestamps when a quiet baseline emitted no events.");
var hourlyStart = new DateTimeOffset(2026, 10, 3, 0, 0, 0, TimeSpan.Zero);
var hourlyEnd = hourlyStart.AddHours(24);
var hourlyDevice = new Device
{
    Mac = "02:00:00:00:00:31", Name = "Hourly timeline device", FirstSeen = hourlyStart.AddDays(-2),
    State = PresenceState.Home, ChangedAt = hourlyStart.AddHours(20)
};
var hourlyEvents = new[]
{
    PresenceEvent.Create(hourlyStart.AddHours(8), "left", hourlyDevice.Name, hourlyDevice.Mac),
    PresenceEvent.Create(hourlyStart.AddHours(20), "arrived", hourlyDevice.Name, hourlyDevice.Mac)
};
var hourlyTimeline = PresenceTimeline.BuildHourly([hourlyDevice], hourlyEvents, hourlyStart, hourlyEnd).Single();
Expect(hourlyTimeline.Hours.Count == 24 && hourlyTimeline.Hours.All(hour => hour.Duration == TimeSpan.FromHours(1)),
    "The rolling 24-hour timeline contains 24 one-hour intervals.");
Expect(hourlyTimeline.Hours.Take(8).All(hour => hour.Fraction == 1) &&
       hourlyTimeline.Hours.Skip(8).Take(12).All(hour => hour.Fraction == 0) &&
       hourlyTimeline.Hours.Skip(20).All(hour => hour.Fraction == 1),
    "The hourly timeline reconstructs arrival and departure transitions into the correct hours.");
var twelveHourTimeline = PresenceTimeline.BuildHourly([hourlyDevice], hourlyEvents, hourlyEnd.AddHours(-12), hourlyEnd).Single();
Expect(twelveHourTimeline.Hours.Count == 12 && twelveHourTimeline.Hours[0].Start == hourlyEnd.AddHours(-12) &&
       twelveHourTimeline.Hours[^1].End == hourlyEnd && twelveHourTimeline.Hours.All(hour => hour.Duration == TimeSpan.FromHours(1)),
    "The rolling 12-hour timeline covers exactly twelve contiguous elapsed hours.");
Expect(twelveHourTimeline.Hours.Take(8).All(hour => hour.Fraction == 0) && twelveHourTimeline.Hours.Skip(8).All(hour => hour.Fraction == 1),
    "The 12-hour timeline clips older transitions and retains the most recent presence.");
var offsetEnd = DateTimeOffset.Parse("2026-11-01T04:00:00-08:00");
var offsetTimeline = PresenceTimeline.BuildHourly([hourlyDevice], [], offsetEnd.AddHours(-12).ToOffset(TimeSpan.FromHours(-7)), offsetEnd).Single();
Expect(offsetTimeline.Hours.Count == 12 && offsetTimeline.Hours.All(hour => hour.Duration == TimeSpan.FromHours(1)),
    "The 12-hour timeline uses elapsed UTC hours across a daylight-saving offset change.");
foreach (var invalidHours in new[] { 0.0, -12, 11.5, 13, 25 })
{
    var rejected = false;
    try { PresenceTimeline.BuildHourly([hourlyDevice], [], hourlyEnd.AddHours(-invalidHours), hourlyEnd); }
    catch (ArgumentOutOfRangeException) { rejected = true; }
    Expect(rejected, $"Unsupported hourly duration {invalidHours} is rejected.");
}
var unknownHourlyTimeline = PresenceTimeline.BuildHourly(
    [new Device { Mac = newcomer, FirstSeen = hourlyStart.AddDays(-1), State = PresenceState.Unknown }],
    [], hourlyStart, hourlyEnd).Single();
Expect(unknownHourlyTimeline.Hours.All(hour => hour.Fraction is null),
    "The hourly timeline leaves hours without a recorded presence state unfilled.");
using (var repository = new Repository(networkPath))
{
    repository.Save(new Snapshot(), [.. timelineEvents]);
    Expect(repository.TimelineHistory(timelineStart, [mac]).Count == 2 &&
           repository.TimelineHistory(timelineStart, [newcomer]).Count == 0,
        "Timeline history query is date-bounded and scoped to selected devices.");
}
Console.WriteLine("Presence timeline reconstruction and history checks passed.");
