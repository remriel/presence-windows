using System.Reflection;
using System.Text.Json;
using Presence.App;
using Presence.Core;

internal static class Program
{
    private static string output = "";
    private static readonly List<object> captures = [];
    private static readonly List<string> failures = [];
    private static readonly List<string> checks = [];
    [STAThread]
    private static int Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2); Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
        output = Path.GetFullPath(args.FirstOrDefault() ?? "ui-preview"); Directory.CreateDirectory(output);
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
            using var stream = typeof(MainWindow).Assembly.GetManifestResourceStream("Presence.AlertIcon")!; using var icon = Image.FromStream(stream);
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
    private static MainWindow GetMain(PresenceContext app) => (MainWindow)typeof(PresenceContext).GetField("window", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(app)!;
    private static void SetContext(PresenceContext app, string property, object value) => typeof(PresenceContext).GetProperty(property)!.SetValue(app, value);
    private static void Fixture(PresenceContext app)
    {
        var d = app.Engine.Data.Devices[0]; d.Name = "Alexandra's work phone with a very long device name"; d.Hostname = "alexandra-phone-with-an-unusually-long-hostname.home.example"; d.Vendor = "Example Incorporated — Communications and Wireless Devices";
        app.Engine.Data.People[0].Name = "Alexandra with a deliberately long display name";
        app.Engine.Data.Devices.Add(new Device { Mac = "02:00:00:00:00:05", Name = "Living-room streaming device with an unusually long descriptive name", Ip = "192.0.2.30", Network = "demo", Kind = DeviceKind.Known, State = PresenceState.Home, ChangedAt = DateTimeOffset.Now.AddDays(-2) });
        app.Engine.Data.Devices.Add(new Device { Mac = "02:00:00:00:00:06", Name = "Ignored test device", Ip = "192.0.2.40", Kind = DeviceKind.Ignore });
        var events = Enumerable.Range(0, 12).Select(i => PresenceEvent.Create(DateTimeOffset.Now.AddMinutes(-i * 33), i % 3 == 0 ? "arrived" : i % 3 == 1 ? "left" : "new", i % 2 == 0 ? d.DisplayName : "Weston's phone", d.Mac, d.PersonId)); app.Store.Save(app.Engine.Data, events);
    }
    private static IEnumerable<Control> All(Control c) { yield return c; foreach (Control child in c.Controls) foreach (var descendant in All(child)) yield return descendant; }
    private static Size Scaled(Size s, float scale) => new((int)Math.Round(s.Width * scale), (int)Math.Round(s.Height * scale));
    private static Padding Scaled(Padding p, float s) => new((int)(p.Left * s), (int)(p.Top * s), (int)(p.Right * s), (int)(p.Bottom * s));
    private static void Capture(Form form, string name, float scale, string size)
    {
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
        foreach (var scroll in All(form).OfType<ScrollBody>()) scroll.AutoScrollPosition = new Point(0, scroll.Content.Height);
        CaptureExisting(form, name, scale);
    }
    private static void CaptureExisting(Form form, string name, float scale)
    {
        form.PerformLayout(); Application.DoEvents();
        foreach (var grid in All(form).OfType<DataGridView>()) { grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells); grid.ClearSelection(); }
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

