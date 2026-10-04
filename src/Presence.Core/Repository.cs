using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace Presence.Core;

public sealed class Repository : IDisposable
{
    private readonly SqliteConnection db;
    public string Path { get; }
    public Repository(string path)
    {
        Path = path;
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        db = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path }.ToString()); db.Open();
        Execute("PRAGMA journal_mode=WAL; PRAGMA synchronous=FULL; CREATE TABLE IF NOT EXISTS State (Id INTEGER PRIMARY KEY CHECK(Id=1), Json TEXT NOT NULL); CREATE TABLE IF NOT EXISTS PresenceEvent (Id TEXT PRIMARY KEY, At TEXT NOT NULL, Type TEXT NOT NULL, Name TEXT NOT NULL, Mac TEXT, PersonId TEXT); CREATE INDEX IF NOT EXISTS EventAt ON PresenceEvent(At); CREATE TABLE IF NOT EXISTS Observation (Id INTEGER PRIMARY KEY, At TEXT NOT NULL, Mac TEXT NOT NULL, Ip TEXT NOT NULL, Signal TEXT NOT NULL); CREATE INDEX IF NOT EXISTS ObservationAt ON Observation(At);");
    }
    public Snapshot Load()
    {
        using var c = db.CreateCommand(); c.CommandText = "SELECT Json FROM State WHERE Id=1";
        if (c.ExecuteScalar() is not string json) return new();

        var data = JsonSerializer.Deserialize<Snapshot>(json) ?? throw new InvalidDataException("Presence state is empty.");

        // 1.0.1 stored very conservative timing as ScanSeconds/DepartureMinutes.
        // Preserve intentional custom values, but migrate the old 120s/5m defaults
        // to responsive monitoring. Unknown legacy JSON properties are otherwise safe.
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.TryGetProperty(nameof(Snapshot.Settings), out var settings))
        {
            if (!settings.TryGetProperty(nameof(Settings.ScanIntervalSeconds), out _) &&
                settings.TryGetProperty("ScanSeconds", out var oldScan) &&
                oldScan.TryGetInt32(out var scanSeconds))
                data.Settings.ScanIntervalSeconds = scanSeconds == 120 ? 10 : Math.Clamp(scanSeconds, 5, 600);

            if (!settings.TryGetProperty(nameof(Settings.DepartureGraceSeconds), out _) &&
                settings.TryGetProperty("DepartureMinutes", out var oldDeparture) &&
                oldDeparture.TryGetInt32(out var departureMinutes))
                data.Settings.DepartureGraceSeconds = departureMinutes == 5 ? 45 : Math.Clamp(departureMinutes * 60, 15, 3600);
        }

        data.Settings.ScanIntervalSeconds = Math.Clamp(data.Settings.ScanIntervalSeconds, 5, 600);
        data.Settings.DepartureGraceSeconds = Math.Clamp(data.Settings.DepartureGraceSeconds, 15, 3600);
        return data;
    }
    public void Save(Snapshot data, IEnumerable<PresenceEvent>? events = null, IEnumerable<Observation>? observations = null, DateTimeOffset? at = null)
    {
        using var tx = db.BeginTransaction();
        using (var c = db.CreateCommand()) { c.Transaction = tx; c.CommandText = "INSERT INTO State VALUES(1,$json) ON CONFLICT(Id) DO UPDATE SET Json=$json"; c.Parameters.AddWithValue("$json", JsonSerializer.Serialize(data)); c.ExecuteNonQuery(); }
        foreach (var e in events ?? [])
        {
            using var c = db.CreateCommand(); c.Transaction = tx; c.CommandText = "INSERT OR IGNORE INTO PresenceEvent VALUES($id,$at,$type,$name,$mac,$person)";
            c.Parameters.AddWithValue("$id", e.Id); c.Parameters.AddWithValue("$at", Stamp(e.At)); c.Parameters.AddWithValue("$type", e.Type); c.Parameters.AddWithValue("$name", e.Name); c.Parameters.AddWithValue("$mac", (object?)e.Mac ?? DBNull.Value); c.Parameters.AddWithValue("$person", (object?)e.PersonId ?? DBNull.Value); c.ExecuteNonQuery();
        }
        foreach (var o in observations ?? [])
        {
            using var c = db.CreateCommand(); c.Transaction = tx; c.CommandText = "INSERT INTO Observation(At,Mac,Ip,Signal) VALUES($at,$mac,$ip,$signal)";
            c.Parameters.AddWithValue("$at", Stamp(at ?? DateTimeOffset.UtcNow)); c.Parameters.AddWithValue("$mac", o.Mac); c.Parameters.AddWithValue("$ip", o.Ip); c.Parameters.AddWithValue("$signal", o.Signal); c.ExecuteNonQuery();
        }
        using (var c = db.CreateCommand()) { c.Transaction = tx; c.CommandText = "DELETE FROM PresenceEvent WHERE At < $cut; DELETE FROM Observation WHERE At < $obsCut"; c.Parameters.AddWithValue("$cut", Stamp((at ?? DateTimeOffset.UtcNow).AddDays(-data.Settings.RetentionDays))); c.Parameters.AddWithValue("$obsCut", Stamp((at ?? DateTimeOffset.UtcNow).AddDays(-Math.Min(7, data.Settings.RetentionDays)))); c.ExecuteNonQuery(); }
        tx.Commit();
    }
    public List<PresenceEvent> History(string? mac = null, string? personId = null, int limit = 300)
    {
        using var c = db.CreateCommand(); c.CommandText = "SELECT Id,At,Type,Name,Mac,PersonId FROM PresenceEvent WHERE ($mac IS NULL AND $person IS NULL) OR Mac=$mac OR PersonId=$person ORDER BY At DESC LIMIT $limit";
        c.Parameters.AddWithValue("$mac", (object?)mac ?? DBNull.Value); c.Parameters.AddWithValue("$person", (object?)personId ?? DBNull.Value); c.Parameters.AddWithValue("$limit", limit);
        using var r = c.ExecuteReader(); var events = new List<PresenceEvent>();
        while (r.Read()) events.Add(new(r.GetString(0), DateTimeOffset.Parse(r.GetString(1)), r.GetString(2), r.GetString(3), r.IsDBNull(4) ? null : r.GetString(4), r.IsDBNull(5) ? null : r.GetString(5)));
        return events;
    }
    private static string Stamp(DateTimeOffset value) => value.UtcDateTime.ToString("O");
    private void Execute(string sql) { using var c = db.CreateCommand(); c.CommandText = sql; c.ExecuteNonQuery(); }
    public void Dispose() => db.Dispose();
}
