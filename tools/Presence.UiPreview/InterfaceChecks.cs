using System.Text.Json;
using Presence.App;
using Presence.Core;

internal static partial class Program
{
    private static int InterfaceChecks()
    {
        void Check(bool condition, string name) { if (condition) checks.Add(name); else failures.Add(name); }
        void Settle() { var clock = System.Diagnostics.Stopwatch.StartNew(); while (clock.ElapsedMilliseconds < 250) { Application.DoEvents(); Thread.Sleep(5); } }
        double Contrast(Color a, Color b)
        {
            double L(Color c) { double C(byte n) { var v = n / 255d; return v <= .04045 ? v / 12.92 : Math.Pow((v + .055) / 1.055, 2.4); } return .2126 * C(c.R) + .7152 * C(c.G) + .0722 * C(c.B); }
            var x = L(a); var y = L(b); return (Math.Max(x, y) + .05) / (Math.Min(x, y) + .05);
        }
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PresenceBuild", "InterfacePreview", Guid.NewGuid().ToString("N"));
        using var app = new PresenceContext(folder, true, true);
        Fixture(app);
        foreach (var theme in new[] { "Light", "Dark" })
        {
            var s = app.Engine.Data.Settings; s.Theme = theme;
            foreach (var surface in new[] { Ui.Background(s), Ui.Surface(s) })
            {
                Check(Contrast(Ui.Text(s), surface) >= 4.5, theme + " text contrast");
                Check(Contrast(Ui.Muted(s), surface) >= 4.5, theme + " secondary text contrast");
            }
            using (var action = Ui.Button("Action", () => { }, s, true))
                Check(Contrast(action.ForeColor, action.BackColor) >= 4.5, theme + " action contrast");
            using (var main = new MainWindow(app))
            {
                Capture(main, theme + "-home", 1.5f, "default");
                Check(All(main).OfType<Button>().Any(b => b.AccessibleName == "Devices"), theme + " direct inventory navigation");
                main.AllowClose = true;
            }
            using (var devices = new DevicesWindow(app))
            {
                devices.Show(); Application.DoEvents();
                var search = All(devices).OfType<TextBox>().Single(t => t.AccessibleName == "Search devices");
                var grid = All(devices).OfType<DataGridView>().Single();
                var total = grid.Rows.Count;
                var device = (Device)grid.Rows[0].Tag!;
                search.Text = device.Mac; Settle();
                Check(grid.Rows.Cast<DataGridViewRow>().Count(r => r.Visible) == 1 && grid.CurrentRow?.Tag == device, theme + " MAC search selects the matching device");
                search.Text = "no such fictional device xyz"; Settle();
                Check(grid.Rows.Cast<DataGridViewRow>().All(r => !r.Visible) && !All(devices).OfType<Button>().Single(b => b.AccessibleName == "Open device").Enabled, theme + " no matches disables edit");
                CaptureExisting(devices, theme + "-inventory-empty-search", 1.5f);
                search.Clear(); Settle();
                Check(grid.Rows.Cast<DataGridViewRow>().Count(r => r.Visible) == total && app.Engine.Data.Devices.Count == total, theme + " clearing search restores inventory without changing data");
                CaptureExisting(devices, theme + "-inventory", 1.5f);
            }
            using (var settings = new SettingsWindow(app))
            {
                Capture(settings, theme + "-settings", 1.5f, "minimum");
                var autoKnown = All(settings).OfType<CheckBox>().Single(c => c.Text == "Automatically mark joining devices as known");
                Check(!autoKnown.Checked, theme + " auto-known starts off");
                autoKnown.Checked = true;
                Click(settings, "Save");
                Check(app.Engine.Data.Settings.AutoMarkJoiningDevicesKnown && app.Store.Load().Settings.AutoMarkJoiningDevicesKnown,
                    theme + " auto-known persists on save");
            }
            using (var settings = new SettingsWindow(app))
            {
                settings.Show(); Application.DoEvents();
                var autoKnown = All(settings).OfType<CheckBox>().Single(c => c.Text == "Automatically mark joining devices as known");
                Check(autoKnown.Checked, theme + " auto-known appears checked after reopening settings");
                autoKnown.Checked = false; Click(settings, "Cancel");
                Check(app.Engine.Data.Settings.AutoMarkJoiningDevicesKnown && app.Store.Load().Settings.AutoMarkJoiningDevicesKnown,
                    theme + " cancel keeps the saved option");
            }
            using (var settings = new SettingsWindow(app))
            {
                settings.Show(); Application.DoEvents();
                All(settings).OfType<CheckBox>().Single(c => c.Text == "Automatically mark joining devices as known").Checked = false;
                Click(settings, "Save");
                Check(!app.Engine.Data.Settings.AutoMarkJoiningDevicesKnown && !app.Store.Load().Settings.AutoMarkJoiningDevicesKnown,
                    theme + " disabling auto-known persists");
            }
            using (var device = new DeviceWindow(app, app.Engine.Data.Devices[0])) Capture(device, theme + "-device", 1.5f, "minimum");
            using (var activity = new ActivityWindow(app)) Capture(activity, theme + "-activity", 1.5f, "minimum");
        }
        File.WriteAllText(Path.Combine(output, "verification.json"), JsonSerializer.Serialize(new { checks, failures, captures }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Presence interface: {checks.Count} checks, {failures.Count} findings, {captures.Count} captures.");
        foreach (var failure in failures) Console.WriteLine(failure);
        return failures.Count == 0 ? 0 : 2;
    }
}
