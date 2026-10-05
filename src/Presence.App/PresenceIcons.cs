using System.Reflection;

namespace Presence.App;

internal static class PresenceIcons
{
    public static readonly Icon Tray = Load(16);
    public static readonly Icon Window = Load(32);

    private static Icon Load(int size)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Presence.AppIcon")
            ?? throw new InvalidDataException("Missing Presence icon.");
        using var icon = new Icon(stream, new Size(size, size));
        return (Icon)icon.Clone();
    }
}
