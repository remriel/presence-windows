using Windows.Networking.Connectivity;

namespace Presence.App;

internal static class NetworkIdentity
{
    // Match the selected physical adapter, rather than the system's internet route.
    // Windows may restrict Wi-Fi identity access; naming must still work without it.
    public static string Ssid(string adapterId)
    {
        try
        {
            if (!Guid.TryParse(adapterId, out var id)) return "";
            foreach (var profile in NetworkInformation.GetConnectionProfiles())
                if (profile.IsWlanConnectionProfile && profile.NetworkAdapter?.NetworkAdapterId == id)
                    return profile.WlanConnectionProfileDetails.GetConnectedSsid() ?? "";
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Runtime.InteropServices.COMException or NotSupportedException) { }
        return "";
    }
}
