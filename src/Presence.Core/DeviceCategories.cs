namespace Presence.Core;

// Numeric values are persisted in existing snapshot JSON. Keep them stable.
public enum DeviceCategory
{
    Automatic = 0, Phone = 1, Laptop = 2, Desktop = 3, Tablet = 4,
    TvStreaming = 5, GameConsole = 6, Printer = 7, Speaker = 8,
    SmartHome = 9, Camera = 10, Wearable = 11, Storage = 12,
    NetworkEquipment = 13, Other = 14
}

public static class DeviceCategories
{
    public static string Label(DeviceCategory category) => category switch
    {
        DeviceCategory.Automatic => "Unknown",
        DeviceCategory.TvStreaming => "TV / streaming",
        DeviceCategory.GameConsole => "Game console",
        DeviceCategory.SmartHome => "Smart home",
        DeviceCategory.Storage => "NAS / storage",
        DeviceCategory.NetworkEquipment => "Network equipment",
        _ when Enum.IsDefined(category) => category.ToString(),
        _ => "Unknown"
    };
}
