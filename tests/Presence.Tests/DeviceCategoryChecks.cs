using System.Text.Json;
using Presence.Core;

internal static class DeviceCategoryChecks
{
    public static void Run()
    {
        static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
        var legacy = JsonSerializer.Deserialize<Device>("{\"Mac\":\"02:00:00:00:00:21\",\"Hostname\":\"Alex-iPhone\",\"Kind\":2}")!;
        Check(legacy.Category == DeviceCategory.Automatic && legacy.DeviceType == "Phone", "Legacy devices retain automatic category hints.");
        Check(DeviceIdentification.InferType("Work-MacBook") == "Laptop" && DeviceIdentification.InferType("DESKTOP-FIXTURE") == "Desktop", "Automatic detection distinguishes laptops and desktops.");
        Check(DeviceIdentification.InferType("Galaxy-tablet") == "Tablet", "Specific tablet hints take precedence over phone-family names.");

        legacy.Category = DeviceCategory.Laptop;
        legacy.Network = "fixture";
        legacy.Kind = DeviceKind.Person;
        legacy.PersonId = "fixture-person";
        legacy.IsPrimary = true;
        var data = new Snapshot { Devices = [legacy], People = [new Person { Id = "fixture-person", Name = "Fictional person" }] };
        var engine = new PresenceEngine(data);
        var now = DateTimeOffset.UtcNow;
        engine.Apply([new Observation(legacy.Mac, "192.0.2.21", "New-iPhone", "Private MAC", "fixture")], "fixture", now);
        Check(legacy.Category == DeviceCategory.Laptop && legacy.DeviceType == "Laptop", "Discovery cannot override a saved category.");
        engine.Apply([new Observation(legacy.Mac, "192.0.2.21", "New-iPad", "Private MAC", "identity-only")], "fixture", now.AddSeconds(1), partial: true);
        Check(legacy.DeviceType == "Laptop", "Hostname enrichment cannot override a saved category.");
        var folder = Path.Combine(Path.GetTempPath(), "PresenceCategoryTests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(folder, "presence.db");
        var savedEvent = PresenceEvent.Create(now, "arrived", "Fictional laptop", legacy.Mac);
        using (var repository = new Repository(path)) repository.Save(data, [savedEvent]);
        using (var repository = new Repository(path))
        {
            var loaded = repository.Load().Devices.Single();
            Check(loaded.Category == DeviceCategory.Laptop && loaded.DeviceType == "Laptop", "The manual category survives closing and reopening SQLite.");
            Check(loaded.Kind == DeviceKind.Person && loaded.PersonId == "fixture-person" && loaded.IsPrimary && repository.History().Single().Id == savedEvent.Id,
                "Category persistence preserves tracking, person, primary selection and history.");
        }
        legacy.Category = DeviceCategory.Automatic;
        Check(legacy.DeviceType == "Tablet", "Returning to Automatic uses the newest hostname hint.");
        legacy.Category = DeviceCategory.Other;
        Check(legacy.DeviceType == "Other", "Other is an explicit override of hostname detection.");
        legacy.Category = (DeviceCategory)999;
        Check(legacy.EffectiveCategory == DeviceCategory.Tablet, "Unrecognized future categories safely fall back to detection.");
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(legacy));
        Check(document.RootElement.TryGetProperty("Category", out _) && !document.RootElement.TryGetProperty("EffectiveCategory", out _) && !document.RootElement.TryGetProperty("DeviceType", out _),
            "Snapshots store the category source without redundant computed fields.");
        Console.WriteLine("Device category compatibility, inference, override and SQLite persistence checks passed.");
    }
}
