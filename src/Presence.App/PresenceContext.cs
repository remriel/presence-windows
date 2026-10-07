using Microsoft.Win32;
using Presence.Core;

namespace Presence.App;

internal sealed class PresenceContext : ApplicationContext
{
    public PresenceEngine Engine { get; }
    public Repository Store { get; }
    public string Status { get; private set; } = "Finding your local network…";
    public string NetworkLabel { get; private set; } = "";
    public bool Monitoring { get; private set; }
    public bool IsScanning => scanning;
    public bool Stopping { get; private set; }
    public string NetworkScope => Monitoring ? (demo && Engine.Network == "" ? "demo" : Engine.Network) : "";
    public CancellationToken Token => cancel.Token;
    public event Action? Changed;
    private readonly CancellationTokenSource cancel = new();
    private readonly Discovery discovery = new();
    private readonly NotifyIcon tray;
    private readonly MainWindow window;
    private readonly System.Windows.Forms.Timer timer;
    private readonly System.Windows.Forms.Timer repaint;
    private readonly System.Windows.Forms.Timer reconnect;
    private readonly FloatingAlerts alerts;
    private bool scanning;
    private bool suspended;
    private CancellationTokenSource? scanCancellation;
    private int epoch;
    private readonly bool demo;
    private bool refreshQueued;
    private bool dirty;
    private string adapterId = "";
    private DateTimeOffset lastSaved;
    private readonly List<PresenceEvent> pendingEvents = [];
    private readonly Dictionary<string, Observation> pendingObservations = [];
    public PresenceContext(string dataDir, bool demoMode, bool hidden)
    {
        demo = demoMode;
        Store = new Repository(System.IO.Path.Combine(dataDir, "presence.db"));
        StartupTrace.Mark("database-open");
        var data = Store.Load();
        if (demo) SeedDemo(data);
        Engine = new(data);
        if (!demo) Engine.ResetNetwork();
        ApplyStartup(data.Settings.StartWithWindows);
        if (!demo) Store.Save(data);
        window = new MainWindow(this); _ = window.Handle;
        alerts = new FloatingAlerts(() => Engine.Data.Settings, () => Screen.FromControl(window).WorkingArea, Activate, action => { if (!Stopping && !window.IsDisposed) window.BeginInvoke(action); });
        StartupTrace.Mark("window-created");
        tray = new NotifyIcon { Text = "Presence · starting", Icon = PresenceIcons.Tray, Visible = true };
        var menu = new ContextMenuStrip(); menu.Items.Add("Open Presence", null, (_, _) => Activate("")); menu.Items.Add("Activity", null, (_, _) => window.ShowActivity());
        menu.Items.Add("Refresh now", null, async (_, _) => await Scan(true)); menu.Items.Add("Settings", null, (_, _) => window.ShowSettings()); menu.Items.Add(new ToolStripSeparator()); menu.Items.Add("Quit Presence", null, (_, _) => Exit()); tray.ContextMenuStrip = menu;
        tray.DoubleClick += (_, _) => Activate("");
        timer = new System.Windows.Forms.Timer { Interval = 250 }; timer.Tick += async (_, _) => { timer.Interval = Engine.Data.Settings.ScanIntervalSeconds * 1000; await Scan(); }; timer.Start();
        repaint = new System.Windows.Forms.Timer { Interval = 250 }; repaint.Tick += (_, _) => { if (dirty && !Stopping) { dirty = false; Refresh(); } }; repaint.Start();
        reconnect = new System.Windows.Forms.Timer { Interval = 750 };
        reconnect.Tick += async (_, _) => { reconnect.Stop(); await Scan(true); };
        adapterId = data.Settings.InterfaceId;
        SystemEvents.PowerModeChanged += PowerChanged;
        System.Net.NetworkInformation.NetworkChange.NetworkAddressChanged += NetworkChanged;
        System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChanged;
        StartupTrace.Mark("tray-created; hidden=" + hidden);
        if (!hidden) window.Show();
        StartupTrace.Mark("window-visible=" + window.Visible);
        if (demo) { NetworkLabel = "Preview · fictional devices"; Status = "Preview mode · no network scanning or notifications"; Monitoring = true; Refresh(); }
    }
    public void Activate(string mac)
    {
        if (Stopping || window.IsDisposed) return;
        if (window.InvokeRequired) { window.BeginInvoke(() => Activate(mac)); return; }
        window.Show(); window.WindowState = FormWindowState.Normal; window.Activate();
        if (mac != "") { var device = Engine.Data.Devices.FirstOrDefault(d => d.Mac == mac); if (device is not null) window.ShowDevice(device); }
    }
    public void ExportPreview(string path)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path))!);
        using var bitmap = new Bitmap(window.ClientSize.Width, window.ClientSize.Height);
        window.DrawToBitmap(bitmap, new Rectangle(Point.Empty, window.ClientSize));
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
    }
    public void ExportAlertPreview(string path) => alerts.ExportPreview(path);
    public void ExportSettingsPreview(string path)
    {
        Engine.Data.Settings.Theme = "Dark";
        using var preview = new SettingsWindow(this);
        _ = preview.Handle;
        var bitmap = new Bitmap(preview.ClientSize.Width, preview.ClientSize.Height);
        preview.DrawToBitmap(bitmap, new Rectangle(Point.Empty, preview.ClientSize));
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path))!);
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        bitmap.Dispose();
    }
    public async Task Scan(bool requested = false)
    {
        if (suspended || demo || Stopping || reconnect.Enabled) return;
        if (scanning) { if (requested) refreshQueued = true; return; }
        scanning = true; dirty = true; var generation = epoch;
        using var scanCancel = CancellationTokenSource.CreateLinkedTokenSource(Token);
        scanCancellation = scanCancel;
        try
        {
            var devices = Engine.Data.Devices.Select(d => new Device { Mac = d.Mac, Ip = d.Ip, Network = d.Network, Kind = d.Kind }).ToList();
            var progress = new Progress<ScanResult>(result =>
            {
                if (Stopping || suspended || generation != epoch || result.Scope != discovery.CurrentScope) return;
                ApplyResult(result);
            });
            var result = await discovery.ScanAsync(Engine.Data.Settings, devices, scanCancel.Token, progress);
            if (Stopping || suspended || generation != epoch) return;
            ApplyResult(result);
        }
        catch (OperationCanceledException)
        {
            if (!Stopping && generation == epoch) { discovery.Reset(); Engine.ResetNetwork(); NetworkLabel = ""; Monitoring = false; Status = "Reconnecting to your network…"; dirty = true; }
        }
        catch (Exception ex)
        {
            if (Stopping || generation != epoch) return;
            Monitoring = false; Engine.ResetNetwork(); discovery.Reset(); NetworkLabel = "";
            Status = ex is IOException ? ex.Message : "Monitoring paused · " + ex.Message;
            Refresh();
        }
        finally
        {
            scanCancellation = null; scanning = false; dirty = true;
            if (refreshQueued && !Stopping && !suspended && !reconnect.Enabled) { refreshQueued = false; window.BeginInvoke(async () => await Scan()); }
        }
    }
    private void ApplyResult(ScanResult result)
    {
        var now = DateTimeOffset.UtcNow;
        var events = Engine.Apply(result.Observations, result.Scope, now, result.InitialSweep, result.EvaluatedMacs, result.IsPartial);
        pendingEvents.AddRange(events);
        foreach (var observation in result.Observations) pendingObservations[observation.Mac] = observation;
        Monitoring = true; NetworkLabel = result.Description;
        Status = result.InitialSweep ? "Learning the network…" : "Monitoring · updated " + now.ToLocalTime().ToString("T");
        foreach (var e in events) Notify(e);
        try { Flush(events.Count > 0); }
        catch (Exception ex) when (ex is IOException or Microsoft.Data.Sqlite.SqliteException) { Status = "History save pending · " + ex.Message; }
        dirty = true;
    }
    private void Flush(bool force)
    {
        if (demo || (!force && DateTimeOffset.UtcNow - lastSaved < TimeSpan.FromSeconds(10))) return;
        Store.Save(Engine.Data, pendingEvents, pendingObservations.Values);
        pendingEvents.Clear(); pendingObservations.Clear(); lastSaved = DateTimeOffset.UtcNow;
    }
    private void Notify(PresenceEvent e)
    {
        if (!Engine.Data.Settings.Allows(e, DateTimeOffset.Now) || demo) return;
        try
        {
            alerts.Show(e.Message, "Local network · click to open device", e.Mac ?? "");
        }
        catch (Exception ex) { Status = "Monitoring · notifications unavailable: " + ex.Message; }
    }
    public void TestNotification()
    {
        if (demo) return;
        try { alerts.Show("Presence is ready", "Test alert · click to open Presence", "", immediate: true); }
        catch (Exception ex) { MessageBox.Show("Could not display the floating alert.\n" + ex.Message, "Presence"); }
    }
    public void Save()
    {
        Engine.ReconcilePeople(DateTimeOffset.UtcNow); Flush(true); timer.Interval = Engine.Data.Settings.ScanIntervalSeconds * 1000;
        if (adapterId != Engine.Data.Settings.InterfaceId) { adapterId = Engine.Data.Settings.InterfaceId; RestartDiscovery(); }
        Refresh();
    }
    public void ApplyStartup(bool enabled)
    {
        if (demo) return;
        using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
        if (enabled) key.SetValue("Presence", "\"" + Environment.ProcessPath + "\" --tray"); else key.DeleteValue("Presence", false);
    }
    public void Refresh()
    {
        var home = Engine.Data.Devices.Count(d => d.Kind != DeviceKind.Ignore && d.Network == NetworkScope && d.State is PresenceState.Home or PresenceState.ProbablyHome);
        tray.Text = Monitoring ? "Presence · " + home + " devices present" : "Presence · monitoring paused";
        Changed?.Invoke();
    }
    private void PowerChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (window.IsDisposed || Stopping || e.Mode == PowerModes.StatusChange) return;
        window.BeginInvoke(() => { suspended = e.Mode == PowerModes.Suspend; RestartDiscovery(); });
    }
    private void NetworkChanged(object? sender, EventArgs e)
    {
        if (!Stopping && !window.IsDisposed) window.BeginInvoke(RestartDiscovery);
    }
    private void NetworkAvailabilityChanged(object? sender, System.Net.NetworkInformation.NetworkAvailabilityEventArgs e) => NetworkChanged(sender, e);
    private void RestartDiscovery()
    {
        if (Stopping || demo) return;
        epoch++; scanCancellation?.Cancel(); discovery.Reset(); Engine.ResetNetwork(); Monitoring = false; NetworkLabel = "";
        Status = suspended ? "Paused while the computer sleeps" : "Connecting to your network…"; Refresh();
        // Coalesce the address/availability burst while Windows acquires a gateway.
        refreshQueued = false; reconnect.Stop();
        if (!suspended) reconnect.Start();
    }
    public void Exit()
    {
        try { Flush(true); } catch (Exception ex) { MessageBox.Show("Recent history could not be saved. Presence is still running.\n" + ex.Message, "Presence"); return; }
        Stopping = true; cancel.Cancel(); discovery.Reset(); timer.Stop(); repaint.Stop(); reconnect.Stop(); tray.Visible = false; window.AllowClose = true; window.Close(); ExitThread();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { Stopping = true; cancel.Cancel(); SystemEvents.PowerModeChanged -= PowerChanged; System.Net.NetworkInformation.NetworkChange.NetworkAddressChanged -= NetworkChanged; System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged -= NetworkAvailabilityChanged; discovery.Dispose(); alerts.Dispose(); timer.Dispose(); repaint.Dispose(); reconnect.Dispose(); tray.Dispose(); window.Dispose(); Store.Dispose(); cancel.Dispose(); }
        base.Dispose(disposing);
    }
    private static void SeedDemo(Snapshot data)
    {
        data.Devices.Clear(); data.People.Clear(); var now = DateTimeOffset.Now;
        var gev = new Person { Name = "Gev", State = PresenceState.Home, ChangedAt = now.AddMinutes(-18) };
        var weston = new Person { Name = "Weston", State = PresenceState.ProbablyHome, ChangedAt = now.AddMinutes(-29) };
        var mom = new Person { Name = "Mom", State = PresenceState.Away, ChangedAt = now.AddHours(-3) }; data.People.AddRange([gev, weston, mom]);
        data.Devices.AddRange(new[] { new Device { Mac = "02:00:00:00:00:01", Name = "Gev’s phone", Ip = "192.0.2.10", Kind = DeviceKind.Person, PersonId = gev.Id, IsPrimary = true, State = gev.State }, new Device { Mac = "02:00:00:00:00:02", Name = "Weston’s phone", Ip = "192.0.2.11", Kind = DeviceKind.Person, PersonId = weston.Id, IsPrimary = true, State = weston.State }, new Device { Mac = "02:00:00:00:00:03", Name = "Mom’s phone", Ip = "192.0.2.12", Kind = DeviceKind.Person, PersonId = mom.Id, IsPrimary = true, State = mom.State }, new Device { Mac = "02:00:00:00:00:04", Hostname = "iPhone", Vendor = "Private MAC", Ip = "192.0.2.14", State = PresenceState.Home, Kind = DeviceKind.Unknown } });
        foreach (var d in data.Devices) { d.FirstSeen = now.AddDays(-3); d.LastSeen = d.State == PresenceState.Away ? now.AddHours(-3) : now; d.Network = "demo"; d.Announced = true; }
    }
}
