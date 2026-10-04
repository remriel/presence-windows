using Microsoft.Win32;
using Presence.Core;

namespace Presence.App;

internal static class Ui
{
    public static bool Dark(Settings settings)
    {
        if (settings.Theme != "System") return settings.Theme == "Dark";
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
    }
    public static Color Background(Settings s) => Dark(s) ? Color.FromArgb(30, 32, 35) : Color.FromArgb(250, 250, 249);
    public static Color Text(Settings s) => Dark(s) ? Color.FromArgb(237, 238, 240) : Color.FromArgb(30, 32, 35);
    public static Color Muted(Settings s) => Dark(s) ? Color.FromArgb(168, 174, 180) : Color.FromArgb(104, 109, 114);
    public static void Theme(Control control, Settings s)
    {
        control.BackColor = Background(s); control.ForeColor = Text(s); control.Font = new Font("Segoe UI", 10);
        foreach (Control child in control.Controls) Theme(child, s);
    }
    public static Label Label(string text, int width, int height = 30, bool muted = false, Settings? settings = null) => new() { Text = text, Width = width, Height = height, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft, ForeColor = settings is null ? SystemColors.ControlText : muted ? Muted(settings) : Text(settings) };
    public static Button Button(string text, Action action, int width = 100)
    {
        var b = new Button { Text = text, Width = width, Height = 32, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Margin = new Padding(0, 5, 8, 5) }; b.FlatAppearance.BorderColor = Color.Gray; b.Click += (_, _) => action(); return b;
    }
    public static Form Dialog(string title, Size size, Settings s)
    {
        var f = new Form { Text = title, ClientSize = size, StartPosition = FormStartPosition.CenterParent, MinimizeBox = false, MaximizeBox = false, FormBorderStyle = FormBorderStyle.FixedDialog, AutoScaleMode = AutoScaleMode.Dpi, Font = new Font("Segoe UI", 10), BackColor = Background(s), ForeColor = Text(s) };
        return f;
    }
    public static string Time(DateTimeOffset? date) => date?.ToLocalTime().ToString(date.Value.LocalDateTime.Date == DateTime.Now.Date ? "t" : "MMM d, t") ?? "Unconfirmed";
    public static FlowLayoutPanel Flow(int width) => new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(24, 14, 16, 18) };
}

internal sealed class MainWindow : Form
{
    private readonly PresenceContext app;
    private readonly FlowLayoutPanel content;
    private readonly Label status;
    private readonly ToolTip hints = new();
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool AllowClose { get; set; }
    public MainWindow(PresenceContext context)
    {
        app = context; Text = "Presence"; ClientSize = new Size(430, 580); MinimumSize = new Size(400, 490); StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10); AutoScaleMode = AutoScaleMode.Dpi;
        var header = new Panel { Dock = DockStyle.Top, Height = 88, Padding = new Padding(24, 16, 24, 4) };
        var title = new Label { Text = "PRESENCE", Location = new Point(24, 20), AutoSize = true, Font = new Font("Segoe UI", 20, FontStyle.Regular) }; header.Controls.Add(title);
        var activity = Ui.Button("Activity", ShowActivity, 76); activity.Location = new Point(240, 27); activity.FlatAppearance.BorderSize = 0; header.Controls.Add(activity);
        var settings = Ui.Button("Settings", ShowSettings, 78); settings.Location = new Point(322, 27); settings.FlatAppearance.BorderSize = 0; header.Controls.Add(settings);
        var refresh = Ui.Button("Refresh", async () => await app.Scan(), 85); refresh.Location = new Point(21, 56); refresh.Height = 27; refresh.FlatAppearance.BorderSize = 0; header.Controls.Add(refresh);
        status = new Label { Dock = DockStyle.Bottom, Height = 68, Padding = new Padding(24, 9, 20, 10), AutoEllipsis = true };
        content = Ui.Flow(380); Controls.Add(content); Controls.Add(header); Controls.Add(status);
        FormClosing += (_, e) => { if (!AllowClose) { e.Cancel = true; Hide(); } };
        app.Changed += Render; VisibleChanged += (_, _) => { if (Visible) Render(); }; Render();
    }
    private void Render()
    {
        if (IsDisposed) return;
        var s = app.Engine.Data.Settings; BackColor = Ui.Background(s); ForeColor = Ui.Text(s);
        foreach (Control c in Controls) { c.BackColor = BackColor; c.ForeColor = ForeColor; foreach (Control child in c.Controls) { child.BackColor = BackColor; child.ForeColor = ForeColor; } }
        content.SuspendLayout();
        foreach (var c in content.Controls.Cast<Control>().ToArray()) { content.Controls.Remove(c); c.Dispose(); }
        var people = app.Engine.Data.People.Where(p => app.Engine.Data.Devices.Any(d => d.PersonId == p.Id && d.Kind == DeviceKind.Person)).ToList();
        Section("HOME NOW");
        var home = people.Where(p => p.State is PresenceState.Home or PresenceState.ProbablyHome).ToList();
        foreach (var person in home) PersonRow(person, true);
        var known = app.Engine.Data.Devices.Where(d => d.Kind == DeviceKind.Known && (app.Engine.Network == "" || d.Network == app.Engine.Network)).ToList();
        var presentDevices = known.Where(d => d.State is PresenceState.Home or PresenceState.ProbablyHome).ToList();
        foreach (var device in presentDevices) Row("●  " + device.DisplayName, Ui.Time(device.ChangedAt), () => ShowDevice(device), true);
        if (home.Count == 0 && presentDevices.Count == 0) Empty("No recognized devices home.");
        Section("AWAY");
        var away = people.Except(home).ToList();
        foreach (var person in away) PersonRow(person, false);
        var absentDevices = known.Where(d => d.State == PresenceState.Away).ToList();
        foreach (var device in absentDevices) Row("○  " + device.DisplayName, Ui.Time(device.ChangedAt), () => ShowDevice(device), false);
        if (away.Count == 0 && absentDevices.Count == 0) Empty(people.Count == 0 && known.Count == 0 ? "Identify a device to get started." : "No recognized devices away.");
        Section("UNKNOWN DEVICES");
        var unknown = app.Engine.Data.Devices.Where(d => d.Kind == DeviceKind.Unknown && (app.Engine.Network == "" || d.Network == app.Engine.Network) && (d.State is PresenceState.Home or PresenceState.ProbablyHome || d.Consecutive > 0)).OrderByDescending(d => d.FirstSeen).ToList();
        foreach (var device in unknown) Row("+  " + device.DisplayName, device.Consecutive == 1 ? "Confirming" : "Identify", () => ShowDevice(device), false);
        if (unknown.Count == 0) Empty("No unidentified devices right now.");
        if (people.Count == 0 && unknown.Count == 0) { var text = Ui.Label("Presence is quietly learning your network.\nOpen a device when it appears and assign its phone to a person.", 365, 75, true, s); text.AutoEllipsis = false; content.Controls.Add(text); }
        status.ForeColor = Ui.Muted(s); status.Text = app.Status + "\n" + (app.NetworkLabel == "" ? "Close this window to keep running in the tray." : app.NetworkLabel);
        hints.SetToolTip(status, "Device presence is a proxy for human presence. A sleeping phone or isolated Wi-Fi client may be invisible.");
        content.ResumeLayout();
    }
    private void Section(string text)
    {
        var label = Ui.Label(text, 360, 40, true, app.Engine.Data.Settings); label.Font = new Font("Segoe UI", 9, FontStyle.Bold); label.Margin = new Padding(0, text == "HOME NOW" ? 0 : 17, 0, 0); content.Controls.Add(label);
    }
    private void Empty(string text) { var label = Ui.Label(text, 362, 38, true, app.Engine.Data.Settings); label.Margin = Padding.Empty; content.Controls.Add(label); }
    private void PersonRow(Person person, bool home)
    {
        var device = app.Engine.Data.Devices.First(d => d.PersonId == person.Id && d.Kind == DeviceKind.Person && d.IsPrimary || d.PersonId == person.Id && d.Kind == DeviceKind.Person);
        Row((home ? "●  " : "○  ") + person.Name, person.State == PresenceState.Unknown ? "Unconfirmed" : Ui.Time(person.ChangedAt), () => ShowDevice(device), home, person.State == PresenceState.ProbablyHome ? "Last detected recently; short radio silence is tolerated." : "Open the person's phone");
    }
    private void Row(string name, string when, Action action, bool home, string? hint = null)
    {
        var row = new Panel { Width = 360, Height = 42, Margin = Padding.Empty, BackColor = BackColor };
        var b = Ui.Button(name, action, 236); b.FlatAppearance.BorderSize = 0; b.TextAlign = ContentAlignment.MiddleLeft; b.Font = new Font("Segoe UI", 12); b.Location = new Point(-3, 0); b.Height = 40; b.ForeColor = home ? (Ui.Dark(app.Engine.Data.Settings) ? Color.FromArgb(135, 209, 172) : Color.FromArgb(30, 100, 70)) : ForeColor;
        var time = Ui.Label(when, 121, 40, true, app.Engine.Data.Settings); time.TextAlign = ContentAlignment.MiddleRight; time.Location = new Point(237, 0); row.Controls.Add(b); row.Controls.Add(time); if (hint is not null) hints.SetToolTip(b, hint); content.Controls.Add(row);
    }
    public void ShowDevice(Device device)
    {
        Show(); using var dialog = new DeviceWindow(app, device); dialog.ShowDialog(this);
    }
    public void ShowActivity()
    {
        Show(); using var f = Ui.Dialog("Presence · Activity", new Size(450, 480), app.Engine.Data.Settings); var flow = Ui.Flow(400); f.Controls.Add(flow);
        var events = app.Store.History();
        if (events.Count == 0) flow.Controls.Add(Ui.Label("No arrivals or departures recorded yet.", 390, 50, true, app.Engine.Data.Settings));
        foreach (var group in events.GroupBy(e => e.At.LocalDateTime.Date))
        {
            var heading = Ui.Label(group.Key == DateTime.Today ? "TODAY" : group.Key.ToString("dddd, MMM d").ToUpperInvariant(), 390, 40); heading.Font = new Font("Segoe UI", 9, FontStyle.Bold); flow.Controls.Add(heading);
            foreach (var e in group) flow.Controls.Add(Ui.Label(e.At.ToLocalTime().ToString("t").PadRight(12) + "  " + e.Message, 390, 36));
        }
        f.ShowDialog(this);
    }
    public void ShowSettings() { Show(); using var f = new SettingsWindow(app); f.ShowDialog(this); }
    protected override void Dispose(bool disposing) { if (disposing) { app.Changed -= Render; hints.Dispose(); } base.Dispose(disposing); }
}

internal sealed class DeviceWindow : Form
{
    public DeviceWindow(PresenceContext app, Device d)
    {
        var s = app.Engine.Data.Settings; Text = "Presence · " + d.DisplayName; ClientSize = new Size(470, 660); StartPosition = FormStartPosition.CenterParent; MinimizeBox = false; MaximizeBox = false; FormBorderStyle = FormBorderStyle.FixedDialog; Font = new Font("Segoe UI", 10); AutoScaleMode = AutoScaleMode.Dpi; BackColor = Ui.Background(s); ForeColor = Ui.Text(s);
        var flow = Ui.Flow(422); Controls.Add(flow);
        flow.Controls.Add(Ui.Label("Name", 406)); var name = new TextBox { Width = 404, Text = d.Name, PlaceholderText = d.DisplayName, MaxLength = 80 }; flow.Controls.Add(name);
        flow.Controls.Add(Ui.Label("Use this device as", 406)); var kind = new ComboBox { Width = 404, DropDownStyle = ComboBoxStyle.DropDownList }; kind.Items.AddRange(new object[] { "Unknown device", "Person / presence device", "Known device", "Ignore" }); kind.SelectedIndex = (int)d.Kind; flow.Controls.Add(kind);
        flow.Controls.Add(Ui.Label("Assigned person · choose existing to link a private MAC", 406)); var person = new ComboBox { Width = 404, DropDownStyle = ComboBoxStyle.DropDown, MaxLength = 80 }; person.Items.AddRange(app.Engine.Data.People.Select(p => (object)p.Name).ToArray()); person.Text = app.Engine.Data.People.FirstOrDefault(p => p.Id == d.PersonId)?.Name ?? ""; flow.Controls.Add(person);
        var primary = new CheckBox { Text = "Use as this person's primary phone", Width = 405, Height = 35, Checked = d.IsPrimary || d.PersonId is null }; flow.Controls.Add(primary);
        void EnabledState() { person.Enabled = kind.SelectedIndex == (int)DeviceKind.Person; primary.Enabled = person.Enabled; }
        kind.SelectedIndexChanged += (_, _) => EnabledState(); EnabledState();
        var buttons = new FlowLayoutPanel { Width = 405, Height = 47, Margin = Padding.Empty };
        var save = Ui.Button("Save", () =>
        {
            try
            {
                if (kind.SelectedIndex == (int)DeviceKind.Person && string.IsNullOrWhiteSpace(person.Text)) { MessageBox.Show(this, "Enter or choose a person's name.", "Presence"); return; }
                d.Name = name.Text.Trim(); d.Kind = (DeviceKind)kind.SelectedIndex;
                if (d.Kind == DeviceKind.Person) app.Engine.Assign(d, person.Text, primary.Checked); else { d.PersonId = null; d.IsPrimary = false; }
                app.Save(); Close();
            }
            catch (Exception ex) { MessageBox.Show(this, "Could not save: " + ex.Message, "Presence"); }
        }); buttons.Controls.Add(save); buttons.Controls.Add(Ui.Button("Cancel", Close)); flow.Controls.Add(buttons); AcceptButton = save;
        flow.Controls.Add(Ui.Label("DEVICE DETAILS", 405, 34, true, s));
        var metadata = "MAC    " + d.Mac + (Identity.IsPrivateMac(d.Mac) ? "  (private)" : "") + "\nIP    " + d.Ip + "\nHostname    " + (d.Hostname == "" ? "Unavailable" : d.Hostname) + "\nManufacturer    " + (d.Vendor == "" ? "Unknown" : d.Vendor) + "\nFirst seen    " + d.FirstSeen.ToLocalTime().ToString("g") + "\nLast seen    " + d.LastSeen.ToLocalTime().ToString("g") + "\nState    " + d.State;
        var meta = Ui.Label(metadata, 403, 175, true, s); meta.AutoEllipsis = false; meta.TextAlign = ContentAlignment.TopLeft; flow.Controls.Add(meta);
        flow.Controls.Add(Ui.Label("PRESENCE HISTORY", 405, 34, true, s));
        var history = app.Store.History(d.Mac, d.PersonId, 50);
        if (history.Count == 0) flow.Controls.Add(Ui.Label("No events yet. Initial detection is a silent baseline.", 404, 42, true, s));
        foreach (var e in history) flow.Controls.Add(Ui.Label(Ui.Time(e.At) + "  " + e.Message, 404, 30));
        foreach (Control c in flow.Controls) { c.BackColor = BackColor; if (c is not Label) c.ForeColor = ForeColor; }
    }
}

internal sealed class SettingsWindow : Form
{
    public SettingsWindow(PresenceContext app)
    {
        var s = app.Engine.Data.Settings; Text = "Presence · Settings"; ClientSize = new Size(490, 700); StartPosition = FormStartPosition.CenterParent; MinimizeBox = false; MaximizeBox = false; FormBorderStyle = FormBorderStyle.FixedDialog; AutoScaleMode = AutoScaleMode.Dpi; Font = new Font("Segoe UI", 10); BackColor = Ui.Background(s); ForeColor = Ui.Text(s);
        var flow = Ui.Flow(440); Controls.Add(flow);
        var intervals = new FlowLayoutPanel { Width = 430, Height = 66, Margin = Padding.Empty }; var scan = Number(s.ScanSeconds, 30, 600); var left = Number(s.DepartureMinutes, 2, 60);
        var a = new FlowLayoutPanel { Width = 206, Height = 65, Margin = Padding.Empty }; a.Controls.Add(Ui.Label("Scan every (seconds)", 200)); a.Controls.Add(scan); intervals.Controls.Add(a);
        var b = new FlowLayoutPanel { Width = 218, Height = 65, Margin = Padding.Empty }; b.Controls.Add(Ui.Label("Declare left after (minutes)", 210)); b.Controls.Add(left); intervals.Controls.Add(b); flow.Controls.Add(intervals);
        flow.Controls.Add(Ui.Label("NOTIFICATIONS", 430, 35, true, s));
        var arrive = Check("Arrivals", s.Arrivals); var depart = Check("Departures", s.Departures); var unknown = Check("Unknown devices", s.UnknownDevices); flow.Controls.Add(arrive); flow.Controls.Add(depart); flow.Controls.Add(unknown);
        var quiet = Check("Quiet hours", s.QuietHours); flow.Controls.Add(quiet);
        var hours = new FlowLayoutPanel { Width = 430, Height = 36, Margin = Padding.Empty }; hours.Controls.Add(Ui.Label("From", 45)); var start = Number(s.QuietStart, 0, 23); start.Width = 65; hours.Controls.Add(start); hours.Controls.Add(Ui.Label("until", 42)); var end = Number(s.QuietEnd, 0, 23); end.Width = 65; hours.Controls.Add(end); hours.Controls.Add(Ui.Label("(24-hour time)", 145)); flow.Controls.Add(hours);
        var startup = Check("Start quietly with Windows", s.StartWithWindows); flow.Controls.Add(startup);
        flow.Controls.Add(Ui.Label("Appearance", 425)); var theme = new ComboBox { Width = 425, DropDownStyle = ComboBoxStyle.DropDownList }; theme.Items.AddRange(new object[] { "System", "Light", "Dark" }); theme.SelectedItem = s.Theme; flow.Controls.Add(theme);
        flow.Controls.Add(Ui.Label("Home network adapter", 425)); var network = new ComboBox { Width = 425, DropDownStyle = ComboBoxStyle.DropDownList }; network.Items.Add("Automatic · physical Wi-Fi first"); var lans = Discovery.Interfaces(); foreach (var lan in lans) network.Items.Add(lan.Name); network.SelectedIndex = Math.Max(0, lans.FindIndex(n => n.Id == s.InterfaceId) + 1); flow.Controls.Add(network);
        flow.Controls.Add(Ui.Label("Keep event history (days)", 425)); var retention = Number(s.RetentionDays, 7, 365); flow.Controls.Add(retention);
        var buttons = new FlowLayoutPanel { Width = 430, Height = 44, Margin = Padding.Empty }; var save = Ui.Button("Save", () =>
        {
            try
            {
                app.ApplyStartup(startup.Checked); s.ScanSeconds = (int)scan.Value; s.DepartureMinutes = (int)left.Value; s.Arrivals = arrive.Checked; s.Departures = depart.Checked; s.UnknownDevices = unknown.Checked; s.QuietHours = quiet.Checked; s.QuietStart = (int)start.Value; s.QuietEnd = (int)end.Value; s.StartWithWindows = startup.Checked; s.Theme = theme.SelectedItem?.ToString() ?? "System"; s.InterfaceId = network.SelectedIndex == 0 ? "" : lans[network.SelectedIndex - 1].Id; s.RetentionDays = (int)retention.Value; app.Save(); Close();
            }
            catch (Exception ex) { MessageBox.Show(this, "Could not save settings: " + ex.Message, "Presence"); }
        }); buttons.Controls.Add(save); buttons.Controls.Add(Ui.Button("Test alert", app.TestNotification)); buttons.Controls.Add(Ui.Button("Devices", () => ShowDevices(app))); flow.Controls.Add(buttons); AcceptButton = save;
        var privacy = Ui.Label("Presence stays on this computer. It never uploads device or presence data. Device presence is only a proxy for a person being home. Sleeping phones, private MAC changes and Wi-Fi isolation can hide devices.", 426, 83, true, s); privacy.AutoEllipsis = false; flow.Controls.Add(privacy);
        foreach (Control c in flow.Controls) { c.BackColor = BackColor; if (c is not Label) c.ForeColor = ForeColor; foreach (Control child in c.Controls) { child.BackColor = BackColor; child.ForeColor = ForeColor; } }
    }
    private static NumericUpDown Number(int value, int min, int max) => new() { Minimum = min, Maximum = max, Value = Math.Clamp(value, min, max), Width = 120 };
    private static CheckBox Check(string text, bool value) => new() { Text = text, Checked = value, Width = 425, Height = 28, Margin = Padding.Empty };
    private void ShowDevices(PresenceContext app)
    {
        using var f = Ui.Dialog("Presence · All devices", new Size(460, 500), app.Engine.Data.Settings); var flow = Ui.Flow(412); f.Controls.Add(flow);
        foreach (var d in app.Engine.Data.Devices.OrderBy(d => d.DisplayName)) { var button = Ui.Button(d.DisplayName + " · " + d.Kind, () => { using var details = new DeviceWindow(app, d); details.ShowDialog(f); }, 396); button.TextAlign = ContentAlignment.MiddleLeft; button.Height = 38; flow.Controls.Add(button); }
        if (app.Engine.Data.Devices.Count == 0) flow.Controls.Add(Ui.Label("No devices discovered yet.", 390, 50)); f.ShowDialog(this);
    }
}
