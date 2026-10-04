using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using CommunityToolkit.WinUI.Notifications;

namespace Presence.App;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        StartupTrace.Enabled = args.Contains("--startup-trace"); StartupTrace.Mark("entry");
        ApplicationConfiguration.Initialize();
        StartupTrace.Mark("windows-initialized");
        if (args.Contains("--unregister")) { ToastNotificationManagerCompat.Uninstall(); return; }
        if (args.Contains("--diagnose")) { Diagnostic.Run(args); return; }
        var previewIndex = Array.IndexOf(args, "--preview-image");
        var demo = args.Contains("--demo") || previewIndex >= 0;
        var dataDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Presence" + (demo ? "-Demo" : ""));
        Directory.CreateDirectory(dataDir);
        var name = "Presence-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dataDir)))[..16];
        using var mutex = new Mutex(true, "Local\\" + name, out var first);
        StartupTrace.Mark("primary-instance=" + first);
        PresenceContext? context = null;
        string? pending = null;
        ToastNotificationManagerCompat.OnActivated += e =>
        {
            var mac = ToastArguments.Parse(e.Argument).TryGetValue("device", out var value) ? value : "";
            if (!first) { Forward(name, mac); Application.Exit(); return; }
            if (context is null) pending = mac;
            else context.Activate(mac);
        };
        StartupTrace.Mark("toast-handler-registered");
        if (!first)
        {
            if (!ToastNotificationManagerCompat.WasCurrentProcessToastActivated()) { Forward(name, ""); return; }
            using var wait = new System.Windows.Forms.Timer { Interval = 5000 }; wait.Tick += (_, _) => Application.Exit(); wait.Start(); Application.Run(); return;
        }
        try
        {
            context = new PresenceContext(dataDir, demo, args.Contains("--tray") || ToastNotificationManagerCompat.WasCurrentProcessToastActivated());
            StartupTrace.Mark("context-created");
            _ = Task.Run(async () =>
            {
                while (!context.Stopping)
                {
                    try
                    {
                        using var pipe = new NamedPipeServerStream(name, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                        await pipe.WaitForConnectionAsync(context.Token);
                        using var reader = new StreamReader(pipe);
                        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.Token); timeout.CancelAfter(2000);
                        var message = await reader.ReadLineAsync(timeout.Token);
                        if (message is not null && message.Length < 100) context.Activate(message);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (IOException) { }
                }
            });
            if (pending is not null) context.Activate(pending);
            if (previewIndex >= 0 && args.Length > previewIndex + 1)
            {
                var exportContext = context;
                Application.Idle += (_, _) => { exportContext.ExportPreview(args[previewIndex + 1]); exportContext.Exit(); };
            }
            Application.Run(context);
        }
        catch (Exception ex)
        {
            // Never silently overwrite corrupted state. Give the user a recoverable path.
            MessageBox.Show("Presence could not start. Your existing data has been preserved.\n\n" + ex.Message + "\n\nData folder: " + dataDir, "Presence", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { context?.Dispose(); mutex.ReleaseMutex(); }
    }
    private static void Forward(string name, string mac)
    {
        try { using var pipe = new NamedPipeClientStream(".", name, PipeDirection.Out, PipeOptions.CurrentUserOnly); pipe.Connect(2000); using var writer = new StreamWriter(pipe) { AutoFlush = true }; writer.WriteLine(mac); }
        catch (Exception ex) when (ex is IOException or TimeoutException) { MessageBox.Show("Presence is starting. Please try opening it again in a moment.", "Presence"); }
    }
}

internal static class StartupTrace
{
    internal static bool Enabled;
    internal static void Mark(string phase)
    {
        if (!Enabled) return;
        var folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PresenceBuild"); Directory.CreateDirectory(folder);
        File.AppendAllText(System.IO.Path.Combine(folder, "startup-trace.txt"), DateTimeOffset.Now.ToString("O") + " " + phase + Environment.NewLine);
    }
}
