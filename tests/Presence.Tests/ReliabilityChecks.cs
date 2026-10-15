using Presence.Core;
using Microsoft.Data.Sqlite;
using System.Text.Json;

internal static class ReliabilityChecks
{
    public static void Run()
    {
        var failures = new List<string>();
        void Check(bool condition, string message) { if (!condition) failures.Add(message); }
        var start = DateTimeOffset.Parse("2026-10-14T12:00:00Z");
        const string mac = "02:11:22:33:44:55";
        const string scope = "fixture";
        var data = new Snapshot(); var engine = new PresenceEngine(data);
        Observation Seen(int seconds, string ip = "192.0.2.20") => new(mac, ip, "Fixture", "", "fresh-arp", start.AddSeconds(seconds));
        engine.Apply([Seen(0)], scope, start);
        engine.Apply([], scope, start.AddSeconds(31));
        var replay = engine.Apply([Seen(0)], scope, start.AddSeconds(32));
        Check(data.Devices.Single().State == PresenceState.Away && replay.Count == 0, "Replayed observations must not revive a departed device.");
        engine.Apply([Seen(33), Seen(34, "192.0.2.21")], scope, start.AddSeconds(35));
        Check(data.Devices.Single().Ip == "192.0.2.21" && data.Devices.Single().LastSeen == start.AddSeconds(34), "Use newest observation when a batch repeats a MAC.");
        engine.ResetNetwork();
        engine.Apply([Seen(-3600)], scope, start.AddHours(-1));
        Check(data.Devices.Single().State == PresenceState.Home && data.Devices.Single().LastSeen == start.AddHours(-1), "A corrected clock after reconnect must not suppress fresh observations until old LastSeen catches up.");

        var json = JsonSerializer.Serialize(data);
        Check(!json.Contains("DisplayName") && !json.Contains("DeviceType"), "Computed presentation properties must not be serialized into every snapshot.");

        var folder = Path.Combine(Path.GetTempPath(), "PresenceReliability", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(folder, "presence.db");
        using (var store = new Repository(path))
        using (var connection = new SqliteConnection($"Data Source={path}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            int Count() { command.CommandText = "SELECT COUNT(*) FROM Observation"; return Convert.ToInt32(command.ExecuteScalar()); }
            store.Save(data, observations: [Seen(0), Seen(1), Seen(2) with { Signal = "identity-only" }], at: start);
            Check(Count() == 1, "Throttle duplicate observations within one transaction and exclude identity metadata.");
            command.CommandText = "CREATE TRIGGER RejectSave BEFORE INSERT ON PresenceEvent BEGIN SELECT RAISE(ABORT,'fixture failure'); END"; command.ExecuteNonQuery();
            try { store.Save(data, [PresenceEvent.Create(start, "new", "Fixture", mac)], [Seen(31)], start.AddSeconds(31)); Check(false, "Injected save must fail."); }
            catch (SqliteException) { }
            command.CommandText = "DROP TRIGGER RejectSave"; command.ExecuteNonQuery();
            store.Save(data, observations: [Seen(31)], at: start.AddSeconds(31));
            Check(Count() == 2, "Failed transaction must not advance observation throttling.");
            store.Save(data, observations: [Seen(-10)], at: start.AddSeconds(-10));
            Check(Count() == 3, "A backwards clock jump must not indefinitely suppress observation persistence.");
            var person = "fixture-person";
            var events = new[] { PresenceEvent.Create(start, "arrived", "First", mac), PresenceEvent.Create(start.AddSeconds(1), "left", "Second", "02:11:22:33:44:66", person), PresenceEvent.Create(start.AddSeconds(2), "arrived", "Third", mac, person) };
            store.Save(data, events, at: start);
            Check(store.History(mac, person).Select(e => e.Name).SequenceEqual(new[] { "Third", "Second", "First" }), "Device/person history must stay ordered and avoid duplicate union matches.");
        }
        foreach (var failure in failures) Console.WriteLine("FAIL: " + failure);
        if (failures.Count > 0) throw new Exception($"{failures.Count} reliability regressions failed.");
        Console.WriteLine("Observation freshness, clock recovery, snapshot and persistence reliability checks passed.");
    }
}
