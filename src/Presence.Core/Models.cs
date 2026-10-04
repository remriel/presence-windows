namespace Presence.Core;

public enum DeviceKind { Unknown, Person, Known, Ignore }
public enum PresenceState { Unknown, Home, ProbablyHome, Away }
public sealed class Person
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public PresenceState State { get; set; }
    public DateTimeOffset? ChangedAt { get; set; }
}
public sealed class Device
{
    public string Mac { get; set; } = "";
    public string Name { get; set; } = "";
    public string Ip { get; set; } = "";
    public string Hostname { get; set; } = "";
    public string Vendor { get; set; } = "";
    public string Network { get; set; } = "";
    public DeviceKind Kind { get; set; }
    public string? PersonId { get; set; }
    public bool IsPrimary { get; set; }
    public DateTimeOffset FirstSeen { get; set; }
    public DateTimeOffset LastSeen { get; set; }
    public DateTimeOffset? ChangedAt { get; set; }
    public PresenceState State { get; set; }
    public int Consecutive { get; set; }
    public double MissingSeconds { get; set; }
    public bool Announced { get; set; }
    public string DisplayName => !string.IsNullOrWhiteSpace(Name) ? Name : !string.IsNullOrWhiteSpace(Hostname) ? Hostname : !string.IsNullOrWhiteSpace(Vendor) ? Vendor + " device" : "Device " + Mac[^5..];
}
public sealed record Observation(string Mac, string Ip, string Hostname, string Vendor, string Signal);
public sealed record PresenceEvent(string Id, DateTimeOffset At, string Type, string Name, string? Mac, string? PersonId)
{
    public static PresenceEvent Create(DateTimeOffset at, string type, string name, string? mac, string? person = null) => new(Guid.NewGuid().ToString("N"), at, type, name, mac, person);
    public string Message => Type == "new" ? "New device joined: " + Name : Name + " " + Type;
}
public sealed class Settings
{
    public int ScanIntervalSeconds { get; set; } = 10;
    public int DepartureGraceSeconds { get; set; } = 45;
    public bool Arrivals { get; set; } = true;
    public bool Departures { get; set; } = true;
    public bool UnknownDevices { get; set; } = true;
    public bool AlertSound { get; set; } = true;
    public int PopupSeconds { get; set; } = 10;
    public bool QuietHours { get; set; }
    public int QuietStart { get; set; } = 22;
    public int QuietEnd { get; set; } = 7;
    public string Theme { get; set; } = "System";
    public bool StartWithWindows { get; set; } = true;
    public string InterfaceId { get; set; } = "";
    public int RetentionDays { get; set; } = 90;
    public bool IsQuiet(DateTimeOffset now) => QuietHours && (QuietStart == QuietEnd || (QuietStart < QuietEnd ? now.Hour >= QuietStart && now.Hour < QuietEnd : now.Hour >= QuietStart || now.Hour < QuietEnd));
    public bool Allows(PresenceEvent e, DateTimeOffset now) => !IsQuiet(now) && e.Type switch { "arrived" => Arrivals, "left" => Departures, "new" => UnknownDevices, _ => false };
}
public sealed class Snapshot
{
    public List<Device> Devices { get; set; } = [];
    public List<Person> People { get; set; } = [];
    public Settings Settings { get; set; } = new();
}
public static class Identity
{
    public static string NormalizeMac(string value)
    {
        var hex = new string(value.Where(Uri.IsHexDigit).ToArray()).ToUpperInvariant();
        if (hex.Length != 12 || hex.All(c => c == '0') || hex.All(c => c == 'F') || (Convert.ToByte(hex[..2], 16) & 1) != 0) return "";
        return string.Join(":", Enumerable.Range(0, 6).Select(i => hex.Substring(i * 2, 2)));
    }
    public static bool IsPrivateMac(string mac) => mac.Length >= 2 && (Convert.ToByte(mac[..2], 16) & 2) != 0;
}
