using System.Net;
using System.Net.Sockets;

namespace Presence.App;

internal static class HostnameResolver
{
    internal static async Task<string> ResolveAsync(string ip, CancellationToken ct, Func<string, CancellationToken, Task<IPHostEntry>>? lookup = null)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(1200);
        try
        {
            var entry = await (lookup is null ? Dns.GetHostEntryAsync(ip, AddressFamily.InterNetwork, timeout.Token) : lookup(ip, timeout.Token)).WaitAsync(timeout.Token).ConfigureAwait(false);
            var hostname = entry.HostName.Trim().TrimEnd('.');
            return !IPAddress.TryParse(hostname, out _) && hostname.Length is > 0 and <= 253 && !hostname.Any(char.IsControl)
                && entry.AddressList.Any(address => address.ToString() == ip) ? hostname : "";
        }
        catch (SocketException) { return ""; }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { return ""; }
    }
}
