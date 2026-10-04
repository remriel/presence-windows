using System.Text.Json;
using Presence.Core;

namespace Presence.App;

internal static class Diagnostic
{
    public static void Run(string[] args)
    {
        var i = Array.IndexOf(args, "--output");
        var path = i >= 0 && args.Length > i + 1 ? args[i + 1] : System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Presence", "discovery-check.json");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path))!);
        try
        {
            var watch = System.Diagnostics.Stopwatch.StartNew(); var result = new Discovery().ScanAsync(new Settings(), [], CancellationToken.None).GetAwaiter().GetResult();
            // Sanitized evidence: never export discovered MACs, addresses, hostnames or network labels.
            var report = new { success = true, durationSeconds = watch.Elapsed.TotalSeconds, devices = result.Observations.Count, freshArp = result.Observations.Count(o => o.Signal.Contains("fresh-arp")), icmp = result.Observations.Count(o => o.Signal.Contains("icmp")), mdns = result.Observations.Count(o => o.Signal.Contains("mdns")), initialBaseline = result.InitialSweep, addressesInSubnet = result.Total, covered = result.Coverage, localInterfacePresent = result.Observations.Any(o => o.Signal == "local-interface"), windows = Environment.OSVersion.Version.ToString() };
            File.WriteAllText(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) { File.WriteAllText(path, JsonSerializer.Serialize(new { success = false, errorType = ex.GetType().Name, message = ex.Message })); Environment.ExitCode = 1; }
    }
}
