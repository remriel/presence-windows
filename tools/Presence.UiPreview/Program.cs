using System.Reflection;
using System.Text.Json;
using Presence.App;
using Presence.Core;

internal static partial class Program
{
    private static string output = "";
    private static readonly List<object> captures = [];
    private static readonly List<string> failures = [];
    private static readonly List<string> checks = [];
    [STAThread]
    private static int Main(string[] args)
    {
        // CI must report UI callback failures instead of waiting on an unseen error dialog.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.SetHighDpiMode(args.Contains("--dpi96") ? HighDpiMode.DpiUnaware : HighDpiMode.PerMonitorV2); Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
        output = Path.GetFullPath(args.FirstOrDefault() ?? "ui-preview"); Directory.CreateDirectory(output);
        if (args.Contains("--device-categories")) return DeviceCategoryChecks();
        if (args.Contains("--interface")) return InterfaceChecks();
        if (args.Contains("--timeline")) return TimelineChecks();
        if (args.Contains("--saved-networks")) return SavedNetworkChecks();
        if (args.Contains("--reset-bulk")) return ResetBulkChecks();
        if (args.Contains("--device-visibility")) return DeviceVisibilityChecks();
        if (args.Contains("--reliability")) return ReliabilityChecks();
        if (args.Contains("--network-change"))
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PresenceBuild", "NetworkPreview", Guid.NewGuid().ToString("N"));
            using var app = new PresenceContext(folder, true, true);
            app.Engine.Data.Settings.Theme = "Light";
            var main = GetMain(app);
            Capture(main, "network-before", 1.5f, "default");
            app.Engine.ResetNetwork(); SetContext(app, "Monitoring", false); SetContext(app, "NetworkLabel", "");
            SetContext(app, "Status", "Connecting to your network…"); app.Refresh();
            CaptureExisting(main, "network-reconnecting", 1.5f);
            app.Engine.Apply([new Observation("02:00:00:00:00:10", "198.51.100.10", "Example phone", "Private MAC", "preview")], "preview-network-b", DateTimeOffset.UtcNow, initialSweep: true);
            SetContext(app, "Monitoring", true); SetContext(app, "NetworkLabel", "Preview network B · fictional devices");
            SetContext(app, "Status", "Learning the network…"); app.Refresh();
            CaptureExisting(main, "network-after", 1.5f);
            main.AllowClose = true; main.Close();
            Console.WriteLine("Captured three fictional network-transition screens.");
            return 0;
        }
        foreach (var theme in (args.Contains("--quick") ? new[] { "Light" } : new[] { "Light", "Dark" }))
        foreach (var scale in (args.Contains("--quick") ? new[] { 1.5f } : new[] { 1f, 1.25f, 1.5f, 2f }))
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PresenceBuild", "UiPreview", Guid.NewGuid().ToString("N"));
            using var app = new PresenceContext(folder, true, true);
            var main = GetMain(app); var s = app.Engine.Data.Settings; s.Theme = theme; s.PopupSeconds = 60;
            Fixture(app); app.Refresh();
            var prefix = theme + "-" + (int)(scale * 100);
            foreach (var size in new[] { "default", "minimum", "large", "maximized" })
            {
                using var view = new MainWindow(app); Capture(view, prefix + "-main-" + size, scale, size); view.AllowClose = true;
            }
            foreach (var size in new[] { "default", "minimum" })
            {
                using (var form = new SettingsWindow(app))
                {
                    var adapter = All(form).OfType<ThemeComboBox>().First(c => c.AccessibleName == "Home network adapter");
                    adapter.Items.Add("Example Wireless Adapter with an unusually long network connection name"); adapter.SelectedIndex = adapter.Items.Count - 1;
                    Capture(form, prefix + "-settings-" + size, scale, size); CaptureBottom(form, prefix + "-settings-" + size + "-bottom", scale);
                }
                using (var form = new DeviceWindow(app, app.Engine.Data.Devices[0])) { Capture(form, prefix + "-device-" + size, scale, size); CaptureBottom(form, prefix + "-device-" + size + "-bottom", scale); }
                using (var form = new ActivityWindow(app)) Capture(form, prefix + "-activity-" + size, scale, size);
                using (var form = new DevicesWindow(app)) Capture(form, prefix + "-devices-" + size, scale, size);
            }
            using (var speed = new SpeedTestWindow(s, false))
            {
                speed.ShowProgress("Measuring download speed…"); Capture(speed, prefix + "-speed-loading", scale, "default");
                speed.ShowResult(new(1234.5, 987.6, 18, 2.3)); CaptureExisting(speed, prefix + "-speed-result", scale);
                speed.ShowError("The Cloudflare speed test took longer than 35 seconds. Try again when your connection is less busy."); CaptureExisting(speed, prefix + "-speed-error", scale);
                speed.Size = speed.MinimumSize; CaptureExisting(speed, prefix + "-speed-error-minimum", scale);
            }
            using var stream = typeof(MainWindow).Assembly.GetManifestResourceStream("Presence.ProductIcon")!; using var icon = Image.FromStream(stream);
            using (var popup = new FloatingAlertWindow("Alexandra's work phone with a very long device name arrived", "Local network · click to open this device and its presence history", icon, s, _ => { }, "")) Capture(popup, prefix + "-popup-long", scale, "default");
            app.Engine.Data.Devices.Clear(); app.Engine.Data.People.Clear(); app.Refresh();
            using (var empty = new MainWindow(app)) { Capture(empty, prefix + "-main-empty", scale, "minimum"); empty.AllowClose = true; }
            SetContext(app, "Status", "Finding your local network…"); SetContext(app, "Monitoring", false); app.Refresh();
            using (var loading = new MainWindow(app)) { Capture(loading, prefix + "-main-loading", scale, "minimum"); loading.AllowClose = true; }
            SetContext(app, "Status", "Monitoring paused · No connected physical Wi-Fi or Ethernet adapter is available. Connect to your home network and refresh."); app.Refresh();
            using (var error = new MainWindow(app)) { Capture(error, prefix + "-main-error", scale, "minimum"); error.AllowClose = true; }
            using (var devices = new DevicesWindow(app)) Capture(devices, prefix + "-devices-empty", scale, "minimum");
            using var emptyApp = new PresenceContext(folder + "-empty", true, true); emptyApp.Engine.Data.Settings.Theme = theme;
            using (var activity = new ActivityWindow(emptyApp)) Capture(activity, prefix + "-activity-empty", scale, "minimum");
            if (scale == 1) BehaviorChecks(emptyApp, s, icon);
            GetMain(emptyApp).AllowClose = true; GetMain(emptyApp).Close(); main.AllowClose = true; main.Close();
        }
        File.WriteAllText(Path.Combine(output, "verification.json"), JsonSerializer.Serialize(new { captures, failures, checks, scaling = "150% native Windows display scaling on this PC; 100/125/200% are normalized font-and-layout simulations on the same native WinForms controls. Physical desktop DPI is recorded per capture." }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Rendered {captures.Count} screens; {failures.Count} layout findings; {checks.Count} behavior checks.");
        foreach (var failure in failures.Take(30)) Console.WriteLine(failure);
        return failures.Count == 0 ? 0 : 2;
    }
    private static int TimelineChecks()
    {
        void Check(bool condition, string name) { if (condition) checks.Add(name); else failures.Add(name); }
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PresenceBuild", "TimelinePreview", Guid.NewGuid().ToString("N"));
        using var app = new PresenceContext(folder, true, true);
        app.Engine.Data.Devices.Clear();
        app.Engine.Data.People.Clear();
        var now = DateTimeOffset.Now;
        var home = new Device { Mac = "02:00:00:00:00:21", Name = "Anne's fictional phone", Network = "demo", FirstSeen = now.AddDays(-28), State = PresenceState.Home, ChangedAt = now.AddHours(-3) };
        var away = new Device { Mac = "02:00:00:00:00:22", Name = "John's fictional tablet", Network = "demo", FirstSeen = now.AddDays(-28), State = PresenceState.Away, ChangedAt = now.AddDays(-3) };
        var unknown = new Device { Mac = "02:00:00:00:00:23", Name = "Unrecorded fictional device", Network = "demo", FirstSeen = now.AddDays(-2), State = PresenceState.Unknown };
        var devices = new List<Device> { home, away, unknown };
        devices.AddRange(Enumerable.Range(0, 17).Select(i => new Device
        {
            Mac = $"02:00:00:00:00:{0x30 + i:X2}",
            Name = $"Fictional device {i + 4:00}",
            Network = "demo",
            FirstSeen = now.AddDays(-i - 1),
            State = PresenceState.Unknown
        }));
        app.Engine.Data.Devices.AddRange(devices);
        app.Store.Save(app.Engine.Data,
        [
            PresenceEvent.Create(now.AddDays(-12), "arrived", home.Name, home.Mac),
            PresenceEvent.Create(now.AddDays(-6), "left", home.Name, home.Mac),
            PresenceEvent.Create(now.AddHours(-3), "arrived", home.Name, home.Mac),
            PresenceEvent.Create(now.AddDays(-7), "arrived", away.Name, away.Mac),
            PresenceEvent.Create(now.AddDays(-3), "left", away.Name, away.Mac)
        ]);

        foreach (var theme in new[] { "Light", "Dark" })
        {
            app.Engine.Data.Settings.Theme = theme;
            using var form = new PresenceTimelineWindow(app);
            form.Show(); Application.DoEvents();
            var grid = All(form).OfType<DataGridView>().Single();
            Check(grid.Rows.Count == 20 && grid.Columns.Count == 15, theme + " 14-day device and date layout");
            Check(grid.DisplayedRowCount(false) == grid.Rows.Count, theme + " all 20 devices fit without a vertical scrollbar");
            Check(grid.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) <= grid.ClientSize.Width, theme + " default 14-day timeline fits without a horizontal scrollbar");
            Check(grid.Columns.Cast<DataGridViewColumn>().Skip(1).All(column => column.HeaderCell.ToolTipText.Length > 0), theme + " date tooltips");
            Check(grid.Rows.Cast<DataGridViewRow>().Single(row => ReferenceEquals(row.Tag, home)).Cells[1].ToolTipText.Contains("present"),
                theme + " presence details are available per day");
            CaptureExisting(form, "timeline-" + theme.ToLowerInvariant() + "-14d", 1);

            All(form).OfType<Button>().Single(button => button.AccessibleName == "24 hours").PerformClick();
            Application.DoEvents();
            grid = All(form).OfType<DataGridView>().Single();
            Check(grid.Rows.Count == 20 && grid.Columns.Count == 25, theme + " 24-hour device and hour layout");
            Check(grid.DisplayedRowCount(false) == grid.Rows.Count, theme + " all 20 hourly rows fit without a vertical scrollbar");
            Check(grid.Columns.Cast<DataGridViewColumn>().Skip(1).All(column => column.HeaderCell.ToolTipText.Contains("UTC")), theme + " hourly labels include exact local intervals and offsets");
            Check(grid.Rows.Cast<DataGridViewRow>().Single(row => ReferenceEquals(row.Tag, home)).Cells.Cast<DataGridViewCell>().Skip(1).Any(cell => cell.ToolTipText.Contains("present")),
                theme + " hourly presence details are available per device");
            Check(grid.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) <= grid.ClientSize.Width, theme + " 24-hour timeline fits without horizontal clipping");
            CaptureExisting(form, "timeline-" + theme.ToLowerInvariant() + "-24h", 1);

            All(form).OfType<Button>().Single(button => button.AccessibleName == "14 days").PerformClick();
            Application.DoEvents();
            grid = All(form).OfType<DataGridView>().Single();

            All(form).OfType<Button>().Single(button => button.AccessibleName == "12 hours").PerformClick();
            Application.DoEvents();
            grid = All(form).OfType<DataGridView>().Single();
            Check(grid.Rows.Count == 20 && grid.Columns.Count == 13, theme + " 12-hour device and hour layout");
            Check(grid.AccessibleName!.Contains("12 hours"), theme + " 12-hour accessible description");
            Check(grid.DisplayedRowCount(false) == grid.Rows.Count, theme + " all 20 rows fit in 12-hour view");
            Check(grid.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) <= grid.ClientSize.Width, theme + " 12-hour view fits horizontally");
            Check(grid.Columns.Cast<DataGridViewColumn>().Skip(1).All(column => column.HeaderCell.ToolTipText.Contains("UTC")), theme + " 12-hour exact local intervals");
            Check(All(form).OfType<WrapLabel>().Any(label => label.Text.Contains("last 12 hours")), theme + " 12-hour legend");
            CaptureExisting(form, "timeline-" + theme.ToLowerInvariant() + "-12h", 1);

            foreach (var (days, columns) in new[] { (7, 8), (21, 22) })
            {
                All(form).OfType<Button>().Single(button => button.AccessibleName == days + " days").PerformClick();
                Application.DoEvents();
                grid = All(form).OfType<DataGridView>().Single();
                Check(grid.Columns.Count == columns, theme + $" {days}-day range selector");
                Check(grid.DisplayedRowCount(false) == grid.Rows.Count, theme + $" all 20 devices fit in the {days}-day view");
                Check(grid.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) <= grid.ClientSize.Width, theme + $" {days}-day view fits without a horizontal scrollbar");
            }
            All(form).OfType<Button>().Single(button => button.AccessibleName == "14 days").PerformClick();
            Application.DoEvents();
            All(form).OfType<Button>().Single(button => button.AccessibleName == "28 days").PerformClick();
            Application.DoEvents();
            grid = All(form).OfType<DataGridView>().Single();
            Check(grid.Columns.Count == 29, theme + " 28-day range selector");
            Check(grid.DisplayedRowCount(false) == grid.Rows.Count, theme + " all 20 devices fit in the 28-day view");
            Check(grid.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) <= grid.ClientSize.Width, theme + " timeline fits without horizontal clipping");
            CaptureExisting(form, "timeline-" + theme.ToLowerInvariant() + "-28d", 1);
        }

        app.Engine.Data.Devices.Clear();
        using (var empty = new PresenceTimelineWindow(app))
        {
            Check(All(empty).OfType<WrapLabel>().Any(label => label.Text.Contains("No devices")), "Empty network has an explanatory timeline state");
        }
        File.WriteAllText(Path.Combine(output, "verification.json"), JsonSerializer.Serialize(new { captures, failures, checks }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Presence timeline: {checks.Count} checks, {failures.Count} findings, {captures.Count} captures.");
        foreach (var failure in failures) Console.WriteLine(failure);
        return failures.Count == 0 ? 0 : 1;
    }
    private static int SavedNetworkChecks()
    {
        void Check(bool condition, string name) { if (condition) checks.Add(name); else failures.Add(name); }
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PresenceBuild", "Networks", Guid.NewGuid().ToString("N"));
        using var app = new PresenceContext(folder, true, true);
        var apply = typeof(PresenceContext).GetMethod("ApplyResult", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = new ScanResult("demo", "Fictional adapter · 192.0.2.0/24", [], true, 0, 254, Ssid: "Fictional Wi-Fi");
        apply.Invoke(app, [result]);
        foreach (var theme in new[] { "Light", "Dark" })
        {
            app.Engine.Data.Settings.Theme = theme;
            using var dialog = new NetworksWindow(app);
            Capture(dialog, "networks-" + theme.ToLowerInvariant(), 1, "minimum");
            Check(All(dialog).OfType<TextBox>().Single().Text == "Fictional Wi-Fi", theme + " SSID default");
        }
        using (var dialog = new NetworksWindow(app))
        {
            dialog.Show(); Application.DoEvents();
            All(dialog).OfType<Button>().Single(b => b.AccessibleName == "Save network").PerformClick();
            Check(dialog.IsDisposed, "Save network button persists and closes");
        }
        Check(app.Store.Load().Networks.Single().Name == "Fictional Wi-Fi", "Network persists to SQLite");
        app.SaveNetwork("demo", "Home");
        Check(app.Engine.Data.Networks.Count == 1 && app.NetworkLabel.StartsWith("Home · saved network"), "Rename replaces and updates header");
        app.Engine.ResetNetwork(); apply.Invoke(app, [result]);
        Check(app.NetworkLabel.StartsWith("Home · saved network"), "Reconnect result restores saved label");
        Capture(GetMain(app), "networks-main", 1, "default");
        using (var saved = new NetworksWindow(app)) Capture(saved, "networks-saved", 1, "minimum");
        try { app.SaveNetwork("other", "Wrong"); failures.Add("Stale network save accepted"); } catch (InvalidOperationException) { checks.Add("Stale network rejected"); }
        using (var db = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={Path.Combine(folder, "presence.db")}"))
        {
            db.Open(); using var command = db.CreateCommand();
            command.CommandText = "CREATE TRIGGER RejectNetworkSave BEFORE UPDATE ON State BEGIN SELECT RAISE(ABORT,'fixture failure'); END"; command.ExecuteNonQuery();
            try { app.SaveNetwork("demo", "Failed"); failures.Add("Failed save accepted"); } catch (Microsoft.Data.Sqlite.SqliteException) { }
            Check(app.Engine.Data.Networks.Single().Name == "Home" && app.Store.Load().Networks.Single().Name == "Home", "Save failure rolls back memory and database");
            try { app.ForgetNetwork("demo"); failures.Add("Failed forget accepted"); } catch (Microsoft.Data.Sqlite.SqliteException) { }
            Check(app.Engine.Data.Networks.Single().Name == "Home", "Forget failure rolls back");
            command.CommandText = "DROP TRIGGER RejectNetworkSave"; command.ExecuteNonQuery();
        }
        var devices = app.Engine.Data.Devices.Count;
        app.ForgetNetwork("demo");
        Check(app.Store.Load().Networks.Count == 0 && app.Engine.Data.Devices.Count == devices, "Forget preserves devices");
        SetContext(app, "Monitoring", false);
        using (var disconnected = new NetworksWindow(app))
        {
            Capture(disconnected, "networks-disconnected", 1, "minimum");
            Check(!All(disconnected).OfType<Button>().Single(b => b.AccessibleName == "Save network").Enabled, "Disconnected save disabled");
        }
        GetMain(app).AllowClose = true;
        File.WriteAllText(Path.Combine(output, "verification.json"), JsonSerializer.Serialize(new { captures, failures, checks }));
        Console.WriteLine($"Saved networks: {checks.Count} checks, {failures.Count} findings.");
        foreach (var failure in failures) Console.WriteLine(failure);
        return failures.Count == 0 ? 0 : 1;
    }

    private static int ReliabilityChecks()
    {
        void Check(bool condition, string name) { if (condition) checks.Add(name); else failures.Add(name); }
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PresenceBuild", "ReliabilityPreview", Guid.NewGuid().ToString("N"));
        using var app = new PresenceContext(folder, true, true);
        var main = GetMain(app); main.Show(); Application.DoEvents();
        var settings = app.Engine.Data.Settings; settings.InterfaceId = "disconnected-fixture-adapter";
        app.Store.Save(app.Engine.Data);
        using var db = new Microsoft.Data.Sqlite.SqliteConnection(new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = app.Store.Path }.ToString()); db.Open();
        using var command = db.CreateCommand();
        command.CommandText = "CREATE TRIGGER RejectSave BEFORE UPDATE ON State BEGIN SELECT RAISE(ABORT,'fixture save failure'); END"; command.ExecuteNonQuery();
        using (var form = new SettingsWindow(app, availableNetworks: []))
        {
            form.Show(); Application.DoEvents();
            var adapter = All(form).OfType<ThemeComboBox>().Single(c => c.AccessibleName == "Home network adapter");
            Check(adapter.SelectedIndex == 1 && adapter.Text.Contains("disconnected"), "Unavailable selected adapter is retained and visible");
            All(form).OfType<NumericUpDown>().First().Value = 17;
            All(form).OfType<Button>().Single(b => b.AccessibleName == "Save").PerformClick();
            Check(!form.IsDisposed && ReferenceEquals(app.Engine.Data.Settings, settings) && settings.ScanIntervalSeconds == 3,
                "Failed settings save rolls back live settings and leaves editor open");
            Capture(form, "settings-save-failure", 1, "default"); CaptureBottom(form, "settings-save-failure-bottom", 1);
            command.CommandText = "DROP TRIGGER RejectSave"; command.ExecuteNonQuery();
            All(form).OfType<Button>().Single(b => b.AccessibleName == "Save").PerformClick();
            Check(form.IsDisposed && app.Store.Load().Settings.ScanIntervalSeconds == 17 && app.Store.Load().Settings.InterfaceId == settings.InterfaceId,
                "Retry saves settings while preserving disconnected adapter choice");
        }
        command.CommandText = "CREATE TRIGGER RejectSave BEFORE UPDATE ON State BEGIN SELECT RAISE(ABORT,'fixture save failure'); END"; command.ExecuteNonQuery();
        var first = app.Engine.Data.Devices[0]; var target = app.Engine.Data.Devices[3]; var oldName = target.Name;
        try { app.SaveDevice(target, "Unsaved", DeviceKind.Person, app.Engine.Data.People[0].Name, true); Check(false, "Device save failure propagates"); }
        catch (Microsoft.Data.Sqlite.SqliteException) { Check(true, "Device save failure propagates"); }
        Check(first.IsPrimary && target.Name == oldName && target.Kind == DeviceKind.Unknown && target.PersonId is null,
            "Failed device edit restores names, associations and previous primary phone");
        var peopleCount = app.Engine.Data.People.Count;
        try { app.SaveDevice(target, "Unsaved", DeviceKind.Person, "New fictional person", true); }
        catch (Microsoft.Data.Sqlite.SqliteException) { }
        Check(app.Engine.Data.People.Count == peopleCount && target.PersonId is null, "Failed assignment leaves no orphan person");
        try { app.SetDeviceKinds([first.Mac], DeviceKind.Ignore); }
        catch (Microsoft.Data.Sqlite.SqliteException) { }
        Check(first.Kind == DeviceKind.Person && first.IsPrimary && first.PersonId is not null, "Failed bulk save preserves previous primary association");
        command.CommandText = "DROP TRIGGER RejectSave"; command.ExecuteNonQuery();
        app.SaveDevice(target, "Fixture renamed", DeviceKind.Person, app.Engine.Data.People[0].Name, true);
        Check(!first.IsPrimary && target.IsPrimary && app.Store.Load().Devices.Single(d => d.Mac == target.Mac).Name == "Fixture renamed",
            "Successful edit commits name and primary replacement");
        main.Hide(); var signatureField = typeof(MainWindow).GetField("signature", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var signature = (string)signatureField.GetValue(main)!; target.Name = "Changed while hidden"; app.Refresh();
        Check((string)signatureField.GetValue(main)! == signature, "Hidden window skips inventory rendering");
        main.Show(); Application.DoEvents();
        Check((string)signatureField.GetValue(main)! != signature, "Showing window renders current inventory");
        CaptureExisting(main, "reliability-main", 1);
        var lan = new Lan("fixture", "Fictional LAN", 1, System.Net.IPAddress.Parse("192.0.2.2"), 24, System.Net.IPAddress.Parse("192.0.2.1"), first.Mac);
        var small = new SubnetSweep(lan).Next();
        Check(small.Count == 252 && small.All(ip => lan.Contains(ip) && !ip.Equals(lan.Address) && !ip.Equals(lan.Gateway)), "Small subnet covers all eligible hosts and excludes infrastructure");
        lan = lan with { Address = System.Net.IPAddress.Parse("198.18.0.2"), Gateway = System.Net.IPAddress.Parse("198.18.0.1"), Prefix = 16 };
        var sweep = new SubnetSweep(lan); var visited = new HashSet<string>(); var bounded = true;
        for (var i = 0; i < 128; i++) { var batch = sweep.Next(); bounded &= batch.Count <= 512 && batch.Distinct().Count() == batch.Count; foreach (var ip in batch) visited.Add(ip.ToString()); }
        Check(bounded && visited.Count == 65532, "Large subnet rotates through every eligible host in batches of at most 512");
        using (var workers = new ArpWorkers(12))
        {
            Check(!(bool)typeof(ArpWorkers).GetField("started", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(workers)!, "Idle discovery creates no ARP worker threads");
            using var canceled = new CancellationTokenSource(); canceled.Cancel();
            Check(workers.ResolveAsync(lan, lan.Gateway, canceled.Token).IsCanceled, "Canceled ARP request does not enqueue work");
        }
        app.ResetEverything();
        try { app.SaveDevice(target, "Stale", DeviceKind.Known, "", false); Check(false, "Stale editor cannot restore a reset device"); }
        catch (InvalidOperationException) { Check(app.Engine.Data.Devices.Count == 0, "Stale editor cannot restore a reset device"); }
        main.AllowClose = true; main.Close();
        app.Dispose(); app.Dispose(); app.Exit();
        Check(app.Stopping, "Repeated context disposal and exit are safe");
        File.WriteAllText(Path.Combine(output, "verification.json"), JsonSerializer.Serialize(new { checks, failures, captures }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Reliability: {checks.Count} checks; {failures.Count} findings; {captures.Count} captures.");
        foreach (var failure in failures) Console.WriteLine(failure);
        return failures.Count == 0 ? 0 : 2;
    }
    private static MainWindow GetMain(PresenceContext app) => (MainWindow)typeof(PresenceContext).GetField("window", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(app)!;
    private static int DeviceVisibilityChecks()
    {
        void Check(bool condition, string name) { if (condition) checks.Add(name); else failures.Add(name); }
        const string address = "192.0.2.20";
        string Resolve(string hostname, string answer) => HostnameResolver.ResolveAsync(address, CancellationToken.None, (_, _) => Task.FromResult(new System.Net.IPHostEntry { HostName = hostname, AddressList = [System.Net.IPAddress.Parse(answer)] })).GetAwaiter().GetResult();
        Check(Resolve("Fixture-iPhone.local.", address) == "Fixture-iPhone.local", "DNS hostname with matching address resolves");
        Check(Resolve("Wrong-host", "192.0.2.21") == "", "DNS forward mismatch rejected");
        Check(Resolve(address, address) == "" && Resolve("Bad\nname", address) == "", "Numeric and control-character DNS labels rejected");
        Check(HostnameResolver.ResolveAsync(address, CancellationToken.None, (_, _) => Task.FromException<System.Net.IPHostEntry>(new System.Net.Sockets.SocketException())).GetAwaiter().GetResult() == "", "DNS failure is nonfatal");
        Check(HostnameResolver.ResolveAsync(address, CancellationToken.None, (_, _) => new TaskCompletionSource<System.Net.IPHostEntry>().Task).GetAwaiter().GetResult() == "", "DNS timeout is bounded and nonfatal");
        using (var canceled = new CancellationTokenSource())
        {
            canceled.Cancel();
            try { HostnameResolver.ResolveAsync(address, canceled.Token, (_, token) => Task.FromCanceled<System.Net.IPHostEntry>(token)).GetAwaiter().GetResult(); Check(false, "DNS external cancellation propagated"); }
            catch (OperationCanceledException) { Check(true, "DNS external cancellation propagated"); }
        }
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PresenceBuild", "VisibilityPreview", Guid.NewGuid().ToString("N"));
        using var app = new PresenceContext(folder, true, true); var main = GetMain(app);
        app.Engine.Data.Devices.Clear(); app.Engine.Data.People.Clear();
        var now = DateTimeOffset.UtcNow;
        var fixtures = new[] {
            new Device { Mac = "02:00:00:00:00:10", Name = "Known away laptop", Kind = DeviceKind.Known, State = PresenceState.Away },
            new Device { Mac = "02:00:00:00:00:11", Name = "Known pending phone", Kind = DeviceKind.Known, State = PresenceState.Unknown },
            new Device { Mac = "02:00:00:00:00:12", Hostname = "Fictional-iPhone", Kind = DeviceKind.Unknown, State = PresenceState.Away },
            new Device { Mac = "02:00:00:00:00:13", Name = "Unidentified pending", Kind = DeviceKind.Unknown, State = PresenceState.Unknown },
            new Device { Mac = "02:00:00:00:00:14", Name = "Orphan associated device", Kind = DeviceKind.Person, PersonId = "missing", State = PresenceState.Away },
            new Device { Mac = "02:00:00:00:00:15", Name = "Ignored fixture", Kind = DeviceKind.Ignore },
            new Device { Mac = "02:00:00:00:00:16", Name = "Assigned fixture phone", Kind = DeviceKind.Person, PersonId = "fixture-person", State = PresenceState.Unknown }
        };
        foreach (var d in fixtures) { d.Network = "demo"; d.FirstSeen = now.AddDays(-1); d.LastSeen = now.AddHours(-1); d.ChangedAt = now; }
        app.Engine.Data.Devices.AddRange(fixtures);
        app.Engine.Data.People.Add(new Person { Id = "fixture-person", Name = "Fictional person", State = PresenceState.Unknown });
        app.Engine.Data.Devices.Add(new Device { Mac = "02:00:00:00:00:99", Network = "other", State = PresenceState.Home });
        foreach (var theme in new[] { "Light", "Dark" })
        {
            app.Engine.Data.Settings.Theme = theme; app.Refresh(); main.Show(); main.WindowState = FormWindowState.Normal; main.ClientSize = new Size(850, 1100); Application.DoEvents();
            var grids = All(main).OfType<DataGridView>().Where(g => g.AccessibleName != null).ToList();
            bool In(string section, Device d) => grids.Any(g => g.AccessibleName!.StartsWith(section + ".") && g.Rows.Cast<DataGridViewRow>().Any(r => Equals(r.Tag, d.Mac)));
            Check(In("Away", fixtures[0]) && In("Away", fixtures[4]), theme + " known absent and orphan person devices visible normally");
            Check(In("Checking presence", fixtures[1]) && In("Checking presence", fixtures[6]), theme + " uncertain known and grouped person visible normally");
            Check(In("Unknown devices", fixtures[2]) && In("Unknown devices", fixtures[3]), theme + " unidentified absent and uncertain visible normally");
            Check(!grids.SelectMany(g => g.Rows.Cast<DataGridViewRow>()).Any(r => Equals(r.Tag, "02:00:00:00:00:99")), theme + " other networks excluded");
            CaptureExisting(main, theme + "-device-visibility", 1);
            All(main).OfType<Button>().Single(b => b.AccessibleName == "Select devices").PerformClick(); Application.DoEvents();
            Check(All(main).OfType<DataGridView>().Where(g => g.Columns.Contains("Select")).Sum(g => g.Rows.Count) == fixtures.Length, theme + " selection and normal view retain all identities");
            All(main).OfType<Button>().Single(b => b.AccessibleName == "Select devices").PerformClick(); Application.DoEvents();
            using var details = new DeviceWindow(app, fixtures[2]); Capture(details, theme + "-device-identification", 1, "default"); CaptureBottom(details, theme + "-device-identification-details", 1);
        }
        main.AllowClose = true; main.Close();
        File.WriteAllText(Path.Combine(output, "verification.json"), JsonSerializer.Serialize(new { captures, failures, checks }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Device visibility: {checks.Count} checks; {failures.Count} findings; {captures.Count} captures.");
        foreach (var failure in failures) Console.WriteLine(failure);
        return failures.Count == 0 ? 0 : 2;
    }
    private static int ResetBulkChecks()
    {
        void Check(bool condition, string name) { if (condition) checks.Add(name); else failures.Add(name); }
        void Click(Form form, string name) { All(form).OfType<Button>().Single(b => b.AccessibleName == name).PerformClick(); Application.DoEvents(); }
        void Select(MainWindow main, string mac, bool keyboard = false)
        {
            var grid = All(main).OfType<DataGridView>().Single(g => g.Columns.Contains("Select") && g.Rows.Cast<DataGridViewRow>().Any(r => Equals(r.Tag, mac)));
            var row = grid.Rows.Cast<DataGridViewRow>().Single(r => Equals(r.Tag, mac));
            if (keyboard) { grid.CurrentCell = row.Cells[0]; typeof(DataGridView).GetMethod("OnKeyDown", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(grid, [new KeyEventArgs(Keys.Space)]); }
            else typeof(DataGridView).GetMethod("OnCellClick", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(grid, [new DataGridViewCellEventArgs(0, row.Index)]);
            Application.DoEvents();
        }
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PresenceBuild", "ResetBulkPreview", Guid.NewGuid().ToString("N"));
        using var app = new PresenceContext(folder, true, true); var main = GetMain(app); main.Show(); Application.DoEvents();
        var first = app.Engine.Data.Devices[0]; var second = app.Engine.Data.Devices[3];
        app.Engine.Data.Devices.Add(new Device { Mac = "02:00:00:00:00:99", Network = "other-network", Kind = DeviceKind.Unknown, State = PresenceState.Home });
        foreach (var theme in new[] { "Light", "Dark" })
        {
            main.WindowState = FormWindowState.Normal; main.Show(); Application.DoEvents();
            app.Engine.Data.Settings.Theme = theme; app.Refresh();
            Click(main, "Select devices"); Select(main, first.Mac); Select(main, second.Mac, true);
            Check(All(main).OfType<DataGridView>().Where(g => g.Columns.Contains("Select")).SelectMany(g => g.Rows.Cast<DataGridViewRow>()).Count(r => Equals(r.Cells[0].Value, true)) == 2, theme + " cross-section mouse and Space selection");
            Check(!All(main).OfType<DataGridView>().SelectMany(g => g.Rows.Cast<DataGridViewRow>()).Any(r => Equals(r.Tag, "02:00:00:00:00:99")), theme + " excludes other networks");
            CaptureExisting(main, theme + "-bulk-selected", 1.5f);
            main.Size = main.MinimumSize; CaptureExisting(main, theme + "-bulk-minimum", 1.5f); main.ClientSize = new Size(960, 1000);
            Click(main, "Ignore selected");
            Check(first.Kind == DeviceKind.Ignore && second.Kind == DeviceKind.Ignore && first.PersonId is null && !first.IsPrimary, theme + " Ignore updates both and clears association");
            Check(!All(main).OfType<Button>().Single(b => b.AccessibleName == "Ignore selected").Enabled, theme + " clears selection after action");
            Select(main, first.Mac); Select(main, second.Mac); Click(main, "Set known");
            Check(first.Kind == DeviceKind.Known && second.Kind == DeviceKind.Known, theme + " Set known restores both devices");
            app.Store.Save(app.Engine.Data); var saved = app.Store.Load();
            Check(saved.Devices.Single(d => d.Mac == first.Mac).Kind == DeviceKind.Known && saved.Devices.Single(d => d.Mac == second.Mac).Kind == DeviceKind.Known, theme + " bulk kinds serialize and reload");
            Select(main, first.Mac); first.Name = "Renamed fictional phone"; app.Refresh();
            Check(All(main).OfType<DataGridView>().Where(g => g.Columns.Contains("Select")).SelectMany(g => g.Rows.Cast<DataGridViewRow>()).Single(r => Equals(r.Tag, first.Mac)).Cells[0].Value is true, theme + " selection survives render");
            Click(main, "Select devices");
            using (var settings = new SettingsWindow(app, _ => false)) { Capture(settings, theme + "-settings", 1.5f, "default"); CaptureBottom(settings, theme + "-settings-reset", 1.5f); Click(settings, "Reset everything"); Check(app.Engine.Data.Devices.Count == 5 && !settings.IsDisposed, theme + " canceled reset preserves data"); }
            using (var device = new DeviceWindow(app, second))
            {
                var primary = All(device).OfType<CheckBox>().Single(c => c.Text == "Use as this person's primary phone");
                Check(!primary.Checked && Math.Abs(primary.Font.Size - 12) < .01 && Equals(primary.Tag, "primary-device"), theme + " primary opt-in unchecked and 20 percent larger");
                Capture(device, theme + "-primary-checkbox", 1.5f, "default");
            }
        }
        Click(main, "Pause scanning"); Check(app.ScanningPaused, "pause icon sets manual pause"); CaptureExisting(main, "paused", 1.5f);
        Click(main, "Resume scanning"); Check(!app.ScanningPaused && app.Engine.Baselining, "resume resets quiet baseline");
        app.Store.Save(app.Engine.Data, [PresenceEvent.Create(DateTimeOffset.UtcNow, "new", "Fictional", first.Mac)], [new Observation(first.Mac, "192.0.2.10", "Fictional", "", "test")]);
        Click(main, "Select devices"); Select(main, first.Mac); SetContext(app, "Monitoring", false); app.Refresh();
        Check(!All(main).OfType<Button>().Single(b => b.AccessibleName == "Ignore selected").Enabled, "network disconnect clears selection");
        Click(main, "Pause scanning");
        using (var settings = new SettingsWindow(app, _ => true)) { settings.Show(); Click(settings, "Reset everything"); Check(settings.IsDisposed, "confirmed reset closes settings"); }
        Check(app.Engine.Data.Devices.Count == 0 && app.Engine.Data.People.Count == 0 && !app.ScanningPaused && app.Engine.Data.Settings.ScanIntervalSeconds == 3 && app.Engine.Data.Settings.Theme == "System", "full reset clears runtime and restores defaults");
        Check(app.Store.Load().Devices.Count == 0 && app.Store.History().Count == 0, "full reset persists empty profile and history");
        CaptureExisting(main, "reset-complete", 1.5f); main.AllowClose = true; main.Close();
        File.WriteAllText(Path.Combine(output, "verification.json"), JsonSerializer.Serialize(new { checks, failures, captures }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{checks.Count} behavior checks passed; {failures.Count} findings; {captures.Count} captures."); foreach (var failure in failures) Console.WriteLine(failure);
        return failures.Count == 0 ? 0 : 2;
    }
    private static void SetContext(PresenceContext app, string property, object value) => typeof(PresenceContext).GetProperty(property)!.SetValue(app, value);
    private static void Fixture(PresenceContext app)
    {
        var d = app.Engine.Data.Devices[0]; d.Name = "Alexandra's work phone with a very long device name"; d.Hostname = "alexandra-phone-with-an-unusually-long-hostname.home.example"; d.Vendor = "Example Incorporated — Communications and Wireless Devices";
        app.Engine.Data.People[0].Name = "Alexandra with a deliberately long display name";
        app.Engine.Data.People[1].Name = "Jordan"; app.Engine.Data.Devices[1].Name = "Jordan's fictional phone";
        app.Engine.Data.People[2].Name = "Casey"; app.Engine.Data.Devices[2].Name = "Casey's fictional phone";
        app.Engine.Data.Devices.Add(new Device { Mac = "02:00:00:00:00:05", Name = "Living-room streaming device with an unusually long descriptive name", Ip = "192.0.2.30", Network = "demo", Kind = DeviceKind.Known, State = PresenceState.Home, ChangedAt = DateTimeOffset.Now.AddDays(-2) });
        app.Engine.Data.Devices.Add(new Device { Mac = "02:00:00:00:00:06", Name = "Ignored test device", Ip = "192.0.2.40", Kind = DeviceKind.Ignore });
        // Always cover both clock-only and previous-date timestamps, regardless of runner timezone.
        var midnight = new DateTimeOffset(DateTime.Today.AddMinutes(15));
        var events = Enumerable.Range(0, 12).Select(i => PresenceEvent.Create(midnight.AddMinutes(-i * 33), i % 3 == 0 ? "arrived" : i % 3 == 1 ? "left" : "new", i % 2 == 0 ? d.DisplayName : "Jordan's fictional phone", d.Mac, d.PersonId)); app.Store.Save(app.Engine.Data, events);
    }
    private static IEnumerable<Control> All(Control c) { yield return c; foreach (Control child in c.Controls) foreach (var descendant in All(child)) yield return descendant; }
    private static Size Scaled(Size s, float scale) => new((int)Math.Round(s.Width * scale), (int)Math.Round(s.Height * scale));
    private static Padding Scaled(Padding p, float s) => new((int)(p.Left * s), (int)(p.Top * s), (int)(p.Right * s), (int)(p.Bottom * s));
    private static void Capture(Form form, string name, float scale, string size)
    {
        Console.WriteLine("Rendering " + name);
        form.Show(); Application.DoEvents();
        if (size == "minimum") form.Size = form.MinimumSize;
        if (size == "large") form.Size = new Size(1000, 900);
        if (size == "maximized") form.WindowState = FormWindowState.Maximized;
        var ratio = scale / (form.DeviceDpi / 96f);
        if (Math.Abs(ratio - 1) > 0.01f)
        {
            var originalSize = form.Size; var originalMinimum = form.MinimumSize; var originalMaximum = form.MaximumSize;
            var fonts = All(form).Select(c => (Control: c, Font: c.Font)).ToList();
            var grids = All(form).OfType<DataGridView>().Select(g => (Grid: g, Body: g.DefaultCellStyle.Font ?? g.Font, Head: g.ColumnHeadersDefaultCellStyle.Font ?? g.Font, Padding: g.DefaultCellStyle.Padding, HeadPadding: g.ColumnHeadersDefaultCellStyle.Padding, Columns: g.Columns.Cast<DataGridViewColumn>().Select(c => (c, c.MinimumWidth, c.Width)).ToList())).ToList();
            form.AutoScaleMode = AutoScaleMode.None; form.SuspendLayout(); form.MinimumSize = Size.Empty; form.MaximumSize = Size.Empty;
            form.Scale(new SizeF(ratio, ratio));
            foreach (var item in fonts) item.Control.Font = new Font(item.Font.FontFamily, item.Font.Size * ratio, item.Font.Style);
            foreach (var item in grids)
            {
                item.Grid.DefaultCellStyle.Font = new Font(item.Body.FontFamily, item.Body.Size * ratio, item.Body.Style); item.Grid.ColumnHeadersDefaultCellStyle.Font = new Font(item.Head.FontFamily, item.Head.Size * ratio, item.Head.Style);
                item.Grid.DefaultCellStyle.Padding = Scaled(item.Padding, ratio); item.Grid.ColumnHeadersDefaultCellStyle.Padding = Scaled(item.HeadPadding, ratio);
                foreach (var (column, min, width) in item.Columns) { column.MinimumWidth = (int)(min * ratio); if (column.AutoSizeMode == DataGridViewAutoSizeColumnMode.None) column.Width = (int)(width * ratio); }
            }
            form.MinimumSize = Scaled(originalMinimum, ratio); form.MaximumSize = Scaled(originalMaximum, ratio);
            if (size != "maximized") form.Size = Scaled(originalSize, ratio);
            form.ResumeLayout(true);
        }
        CaptureExisting(form, name, scale);
    }
    private static void CaptureBottom(Form form, string name, float scale)
    {
        form.PerformLayout(); Application.DoEvents();
        foreach (var scroll in All(form).OfType<ScrollBody>()) scroll.AutoScrollPosition = new Point(0, scroll.Content.Height);
        CaptureExisting(form, name, scale, false);
    }
    private static void CaptureExisting(Form form, string name, float scale, bool layout = true)
    {
        if (form.WindowState == FormWindowState.Minimized) { form.WindowState = FormWindowState.Normal; form.Show(); }
        if (layout) { form.PerformLayout(); Application.DoEvents(); }
        foreach (var grid in All(form).OfType<DataGridView>()) { if (grid.AutoSizeRowsMode != DataGridViewAutoSizeRowsMode.None) grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells); grid.ClearSelection(); }
        using var bitmap = new Bitmap(form.Width, form.Height); form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size)); bitmap.Save(Path.Combine(output, name + ".png"));
        captures.Add(new { name, width = form.Width, height = form.Height, desktopDpi = form.DeviceDpi, scale });
        Audit(form, name, scale);
        if (name.Contains("150") && (name.Contains("settings-default") || name.Contains("popup") || name.Contains("speed-result"))) File.WriteAllText(Path.Combine(output, name + ".layout.json"), JsonSerializer.Serialize(All(form).Select(c => new { type = c.GetType().Name, text = c.Text, bounds = c.Bounds.ToString(), preferred = c.GetPreferredSize(c.Size).ToString(), padding = c.Padding.ToString(), visible = c.Visible }), new JsonSerializerOptions { WriteIndented = true }));
    }
    private static void Audit(Form form, string name, float scale)
    {
        foreach (var c in All(form).Where(c => c.Visible))
        {
            if (c is WrapLabel label && label.Height + 2 < label.GetPreferredSize(new Size(label.Width, 0)).Height) failures.Add($"{name}: clipped label height '{label.Text}' ({label.Height})");
            if (c is Button button)
            {
                var measure = TextRenderer.MeasureText(button.Text.Replace("&", ""), button.Font);
                if (button.Width + 2 < measure.Width + button.Padding.Horizontal || button.Height + 2 < measure.Height + button.Padding.Vertical) failures.Add($"{name}: clipped button '{button.Text}' {button.Size}");
                if (button.Enabled && !button.TabStop && form is not FloatingAlertWindow) failures.Add($"{name}: action is not keyboard reachable '{button.Text}'");
            }
            if (c is WrapLabel or Button or CheckBox or ComboBox or NumericUpDown or TextBox)
            {
                var parent = c.Parent;
                if (parent is null || parent is DataGridView || parent is NumericUpDown || c is TextBox && parent is ComboBox) continue;
                if (c.Right > parent.ClientSize.Width + 3 && parent is not ScrollableControl { HorizontalScroll.Visible: true }) failures.Add($"{name}: control exceeds parent width '{c.Text}' ({c.Right}>{parent.ClientSize.Width})");
            }
            if (c is ScrollBody body && body.HorizontalScroll.Visible) failures.Add($"{name}: unwanted horizontal form scrolling");
        }
    }
    private static void BehaviorChecks(PresenceContext app, Settings s, Image icon)
    {
        using (var settings = new SettingsWindow(app))
        {
            settings.Show(); var scan = All(settings).OfType<NumericUpDown>().First(); scan.Value = 8;
            All(settings).OfType<Button>().First(b => b.Text == "&Save").PerformClick();
            if (app.Engine.Data.Settings.ScanIntervalSeconds != 8) failures.Add("Settings save did not retain scan interval"); else checks.Add("Settings Save updates existing settings model");
        }
        var device = app.Engine.Data.Devices[0];
        using (var details = new DeviceWindow(app, device))
        {
            details.Show(); var name = All(details).OfType<TextBox>().First(t => t.AccessibleName == "Device name"); name.Text = "Renamed preview phone";
            All(details).OfType<Button>().First(b => b.Text == "&Save").PerformClick();
            if (device.Name != "Renamed preview phone") failures.Add("Device Save did not rename the device"); else checks.Add("Device Save retains rename and person association");
        }
        using (var popup = new FloatingAlertWindow("Preview arrived", "Click to open", icon, s, _ => { }, ""))
        {
            popup.Show(); All(popup).OfType<Button>().First(b => b.AccessibleName == "Dismiss notification").PerformClick();
            if (!popup.IsDisposed) failures.Add("Popup dismiss failed"); else checks.Add("Floating alert dismiss closes the notification");
        }
    }
}
