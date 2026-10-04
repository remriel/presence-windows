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
