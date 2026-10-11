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
