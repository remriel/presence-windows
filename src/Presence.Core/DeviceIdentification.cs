namespace Presence.Core;

// Hints from advertised hostnames, never a substitute for user tracking choices.
public static class DeviceIdentification
{
    public static string InferType(string hostname) => DeviceCategories.Label(InferCategory(hostname));

    public static DeviceCategory InferCategory(string hostname)
    {
        var name = hostname.ToLowerInvariant();
        bool Has(params string[] hints) => hints.Any(name.Contains);
        if (Has("ipad", "tablet")) return DeviceCategory.Tablet;
        if (Has("iphone", "android", "pixel-", "galaxy", "oneplus")) return DeviceCategory.Phone;
        if (Has("appletv", "apple-tv", "chromecast", "roku", "firetv", "fire-tv", "smarttv", "smart-tv", "bravia")) return DeviceCategory.TvStreaming;
        if (Has("printer", "epson", "brother", "laserjet", "officejet")) return DeviceCategory.Printer;
        if (Has("homepod", "sonos", "speaker", "echo-")) return DeviceCategory.Speaker;
        if (Has("playstation", "xbox", "nintendo")) return DeviceCategory.GameConsole;
        if (Has("macbook", "laptop-", "thinkpad")) return DeviceCategory.Laptop;
        if (Has("imac", "desktop-")) return DeviceCategory.Desktop;
        if (Has("synology", "qnap", "diskstation")) return DeviceCategory.Storage;
        if (Has("router", "gateway", "accesspoint", "access-point")) return DeviceCategory.NetworkEquipment;
        return DeviceCategory.Automatic;
    }
}
