namespace Presence.Core;

// Hints from advertised hostnames, never a substitute for user tracking choices.
public static class DeviceIdentification
{
    public static string InferType(string hostname)
    {
        var name = hostname.ToLowerInvariant();
        bool Has(params string[] hints) => hints.Any(name.Contains);
        if (Has("iphone", "android", "pixel-", "galaxy", "oneplus")) return "Phone";
        if (Has("ipad", "tablet")) return "Tablet";
        if (Has("appletv", "apple-tv", "chromecast", "roku", "firetv", "fire-tv", "smarttv", "smart-tv", "bravia")) return "TV / streaming";
        if (Has("printer", "epson", "brother", "laserjet", "officejet")) return "Printer";
        if (Has("homepod", "sonos", "speaker", "echo-")) return "Speaker";
        if (Has("playstation", "xbox", "nintendo")) return "Game console";
        if (Has("macbook", "imac", "desktop-", "laptop-", "thinkpad")) return "Computer";
        if (Has("synology", "qnap", "diskstation")) return "NAS";
        if (Has("router", "gateway", "accesspoint", "access-point")) return "Network equipment";
        return "Unknown";
    }
}
