using System.Text.Json;
using Microsoft.Data.Sqlite;
using Presence.Core;

internal static class ReliabilityChecks
{
    internal static void Run()
    {
        var failures = new List<string>(); var passed = 0;
        void Check(bool condition, string name) { if (condition) passed++; else failures.Add(name); }
        var start = DateTimeOffset.Parse("2026-10-14T12:00:00Z");
        const string mac = "02:11:22:33:44:55", other = "02:11:22:33:44:66", scope = "fixture";
        var data = new Snapshot(); var engine = new PresenceEngine(data);
        var original = new Observation(mac, "192.0.2.20", "Fixture", "", "fresh-arp", start);
        engine.Apply([original], scope, start);
        engine.Apply([], scope, start.AddSeconds(31), evaluatedMacs: [mac]);
        var events = engine.Apply([original], scope, start.AddSeconds(32), partial: true);
        Check(events.Count == 0 && data.Devices[0].State == PresenceState.Away,
            "Replayed observation cannot revive an absent device or duplicate arrival");

        data = new Snapshot(); engine = new PresenceEngine(data);
        engine.Apply([original], scope, start);
        for (var seconds = 10; seconds <= 100; seconds += 10)
            engine.Apply([new Observation(other, "192.0.2.21", "", "", "fresh-arp", start.AddSeconds(seconds))], scope, start.AddSeconds(seconds), partial: true);
        events = engine.Apply([], scope, start.AddSeconds(101), evaluatedMacs: [mac]);
        Check(events.Count == 0 && data.Devices[0].MissingSeconds == 0,
            "Unrelated partial discoveries cannot hide a long gap in completed health checks");

        data = new Snapshot(); engine = new PresenceEngine(data);
        engine.Apply([original], scope, start);
        for (var seconds = 10; seconds <= 100; seconds += 10)
            engine.Apply([], scope, start.AddSeconds(seconds), evaluatedMacs: []);
        events = engine.Apply([], scope, start.AddSeconds(101), evaluatedMacs: [mac]);
        Check(events.Count == 0 && data.Devices[0].MissingSeconds == 0,
            "Long gaps between evaluations of one device cannot count as monitored absence");

        data = new Snapshot(); engine = new PresenceEngine(data);
        engine.Apply([original], scope, start);
        var latest = original with { Ip = "192.0.2.30", Hostname = "New-IP", At = start.AddSeconds(2) };
        engine.Apply([original, latest], scope, start.AddSeconds(3));
        Check(data.Devices[0].Ip == latest.Ip && data.Devices[0].LastSeen == latest.At,
            "Duplicate MAC observations choose the freshest evidence in a batch");

        var folder = Path.Combine(Path.GetTempPath(), "PresenceReliability", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(folder, "presence.db");
        using (var store = new Repository(path))
        using (var db = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path }.ToString()))
        {
            db.Open(); using var command = db.CreateCommand();
            store.Save(data, observations: [original, original, original with { Signal = "identity-only" }], at: start);
            command.CommandText = "SELECT COUNT(*) FROM Observation";
            Check(Convert.ToInt32(command.ExecuteScalar()) == 1, "Observation batch deduplicates physical evidence and excludes DNS metadata");
            command.CommandText = "SELECT Json FROM State WHERE Id=1";
            using (var saved = JsonDocument.Parse((string)command.ExecuteScalar()!))
                Check(!saved.RootElement.GetProperty("Devices")[0].TryGetProperty("DisplayName", out _) &&
                    !saved.RootElement.GetProperty("Devices")[0].TryGetProperty("DeviceType", out _),
                    "Snapshot stores source data without redundant computed display fields");
            command.CommandText = "CREATE TRIGGER RejectSave BEFORE UPDATE ON State BEGIN SELECT RAISE(ABORT,'fixture'); END"; command.ExecuteNonQuery();
            var next = original with { At = start.AddSeconds(40) };
            try { store.Save(data, observations: [next], at: start.AddSeconds(40)); failures.Add("Injected save failure must propagate"); }
            catch (SqliteException) { passed++; }
            command.CommandText = "DROP TRIGGER RejectSave"; command.ExecuteNonQuery();
            store.Save(data, observations: [next], at: start.AddSeconds(40));
            command.CommandText = "SELECT COUNT(*) FROM Observation";
            Check(Convert.ToInt32(command.ExecuteScalar()) == 2, "Failed saves do not advance observation throttle");
        }
        Console.WriteLine($"Reliability checks: {passed} passed, {failures.Count} failed.");
        foreach (var failure in failures) Console.WriteLine("FAIL: " + failure);
        if (failures.Count > 0) throw new Exception("Reliability regressions failed.");
    }
}
