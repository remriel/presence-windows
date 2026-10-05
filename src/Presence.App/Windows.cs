using Microsoft.Win32;
using Presence.Core;

namespace Presence.App;

internal static class Ui
{
    public static readonly Font StandardFont = new("Segoe UI", 10);
    public static readonly Font ButtonFont = new("Segoe UI", 10, FontStyle.Bold);
    public static readonly Color LightCanvas = ColorTranslator.FromHtml("#F7F6EF");
    public static readonly Color LightSurface = Color.White;
    public static readonly Color DarkCanvas = ColorTranslator.FromHtml("#151821");
    public static readonly Color DarkSurface = ColorTranslator.FromHtml("#242832");
    public static readonly Color Ink = ColorTranslator.FromHtml("#151821");
    public static readonly Color Paper = ColorTranslator.FromHtml("#F7F6EF");
    public static readonly Color Lemon = ColorTranslator.FromHtml("#F4E54D");
    public static readonly Color Coral = ColorTranslator.FromHtml("#F25B3D");
    public static readonly Color Teal = ColorTranslator.FromHtml("#0E7C66");
    public static readonly Color DarkTeal = ColorTranslator.FromHtml("#7AE5C5");
    public static readonly Color ErrorRed = ColorTranslator.FromHtml("#A72A24");
    public static readonly Color DarkError = ColorTranslator.FromHtml("#FF8A76");
    public static bool Dark(Settings settings)
    {
        if (settings.Theme != "System") return settings.Theme == "Dark";
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
    }
    public static Color Background(Settings s) => Dark(s) ? DarkCanvas : LightCanvas;
    public static Color Surface(Settings s) => Dark(s) ? DarkSurface : LightSurface;
    public static Color Text(Settings s) => Dark(s) ? Paper : Ink;
    public static Color Muted(Settings s) => Dark(s) ? Color.FromArgb(194, 196, 202) : Color.FromArgb(80, 83, 92);
    public static Color Positive(Settings s) => Dark(s) ? DarkTeal : Teal;
    public static Color Negative(Settings s) => Dark(s) ? DarkError : ErrorRed;
    public static void Theme(Control control, Settings s)
    {
        control.BackColor = control is Label ? Color.Transparent : Background(s);
        control.ForeColor = control is Label label && label.ForeColor != SystemColors.ControlText ? label.ForeColor : Text(s);
        control.Font = StandardFont;
        if (control is Button button) { button.BackColor = Lemon; button.ForeColor = Ink; button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderSize = 3; button.FlatAppearance.BorderColor = Ink; button.Font = ButtonFont; }
        else if (control is TextBox or ComboBox or NumericUpDown) control.BackColor = Surface(s);
        foreach (Control child in control.Controls) Theme(child, s);
    }
    public static Label Label(string text, int width, int height = 30, bool muted = false, Settings? settings = null) => new() { Text = text, Width = width, Height = height, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft, ForeColor = settings is null ? SystemColors.ControlText : muted ? Muted(settings) : Text(settings) };
    public static Button Button(string text, Action action, int width = 100)
    {
        var b = new Button { Text = text, Width = width, Height = 32, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Margin = new Padding(0, 5, 8, 5), BackColor = Lemon, ForeColor = Ink, Font = ButtonFont }; b.FlatAppearance.BorderSize = 3; b.FlatAppearance.BorderColor = Ink; b.Click += (_, _) => action(); return b;
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
    private SpeedTestWindow? speedTest;
    private readonly Panel header;
    private readonly Panel speedPanel;
    private readonly Button speedButton;
    private readonly Label speedHint;
    private readonly Font headingFont = new("Segoe UI", 11, FontStyle.Bold);
    private readonly Font rowFont = new("Segoe UI", 12, FontStyle.Bold);
    private string renderSignature = "";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool AllowClose { get; set; }
    public MainWindow(PresenceContext context)
    {
        app = context; Text = "Presence"; ClientSize = new Size(560, 720); MinimumSize = new Size(460, 540); MaximumSize = new Size(760, 900); MaximizeBox = false; StartPosition = FormStartPosition.CenterScreen;
        Font = Ui.StandardFont; AutoScaleMode = AutoScaleMode.Dpi;
        header = new Panel { Dock = DockStyle.Top, Height = 88, Padding = new Padding(24, 16, 24, 4), BackColor = Ui.Lemon };
        var title = new Label { Text = "PRESENCE", Location = new Point(24, 20), AutoSize = true, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Ui.Ink }; header.Controls.Add(title);
        var activity = Ui.Button("Activity", ShowActivity, 76); activity.FlatAppearance.BorderSize = 0; activity.Anchor = AnchorStyles.Top | AnchorStyles.Right; header.Controls.Add(activity);
        var settings = Ui.Button("Settings", ShowSettings, 78); settings.FlatAppearance.BorderSize = 0; settings.Anchor = AnchorStyles.Top | AnchorStyles.Right; header.Controls.Add(settings);
        void PlaceHeaderButtons() { settings.Location = new Point(Math.Max(250, header.ClientSize.Width - 108), 27); activity.Location = new Point(settings.Left - 82, 27); }
        header.Resize += (_, _) => PlaceHeaderButtons(); PlaceHeaderButtons();
        var refresh = Ui.Button("Refresh", async () => await app.Scan(), 85); refresh.Location = new Point(21, 56); refresh.Height = 27; refresh.FlatAppearance.BorderSize = 0; header.Controls.Add(refresh);
        status = new Label { Dock = DockStyle.Bottom, Height = 68, Padding = new Padding(24, 9, 20, 10), AutoEllipsis = true };
        speedPanel = new Panel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(18, 5, 16, 4), BackColor = Ui.Coral };
        speedButton = Ui.Button("Speed test", OpenSpeedTest, 105); speedButton.Location = new Point(18, 5); speedButton.Height = 34; speedPanel.Controls.Add(speedButton);
        speedHint = Ui.Label("Download · upload · latency · jitter", 260, 34); speedHint.ForeColor = Color.White; speedPanel.Controls.Add(speedHint);
        void PlaceSpeedHint() { speedHint.Location = new Point(132, 5); speedHint.Width = Math.Max(100, speedPanel.ClientSize.Width - speedHint.Left - 10); }
        speedPanel.Resize += (_, _) => PlaceSpeedHint(); PlaceSpeedHint();
        content = Ui.Flow(380); Controls.Add(content); Controls.Add(header); Controls.Add(status);
        Controls.Add(speedPanel);
        FormClosing += (_, e) => { if (!AllowClose) { e.Cancel = true; Hide(); } };
        app.Changed += Render; VisibleChanged += (_, _) => { if (Visible) Render(); }; Render();
    }
    private void Render()
    {
        if (IsDisposed) return;
        var s = app.Engine.Data.Settings; BackColor = Ui.Background(s); ForeColor = Ui.Text(s);
        var peopleSignature = string.Join(";", app.Engine.Data.People.OrderBy(p => p.Id).Select(p => $"{p.Id}:{p.Name}:{p.State}:{p.ChangedAt:O}"));
        var deviceSignature = string.Join(";", app.Engine.Data.Devices.OrderBy(d => d.Mac).Select(d => $"{d.Mac}:{d.Ip}:{d.Name}:{d.Hostname}:{d.Kind}:{d.State}:{d.PersonId}:{d.IsPrimary}:{d.ChangedAt:O}"));
        var signature = $"{s.Theme}:{content.ClientSize.Width}:{peopleSignature}:{deviceSignature}";
        if (signature == renderSignature) { UpdateStatus(s); return; }
        renderSignature = signature;
        content.BackColor = BackColor; status.BackColor = BackColor; status.ForeColor = Ui.Muted(s);
        header.BackColor = Ui.Lemon; speedPanel.BackColor = Ui.Coral; speedHint.ForeColor = Color.White; speedHint.BackColor = Ui.Coral;
        foreach (var label in header.Controls.OfType<Label>()) { label.ForeColor = Ui.Ink; label.BackColor = Ui.Lemon; }
        foreach (var button in header.Controls.OfType<Button>()) { button.BackColor = Ui.Surface(s); button.ForeColor = Ui.Text(s); button.FlatAppearance.BorderSize = 2; button.FlatAppearance.BorderColor = Ui.Text(s); button.Font = new Font("Segoe UI", 9, FontStyle.Bold); }
        speedButton.BackColor = Ui.Lemon; speedButton.ForeColor = Ui.Ink; speedButton.FlatAppearance.BorderSize = 3; speedButton.FlatAppearance.BorderColor = Ui.Ink; speedButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
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
        foreach (var device in unknown) Row("+  " + device.DisplayName, "Details", () => ShowDevice(device), false);
        if (unknown.Count == 0) Empty("No unidentified devices right now.");
        if (people.Count == 0 && unknown.Count == 0) { var text = Ui.Label("Presence is quietly learning your network.\nOpen a device when it appears and assign its phone to a person.", 365, 75, true, s); text.AutoEllipsis = false; content.Controls.Add(text); }
        UpdateStatus(s);
        content.ResumeLayout();
    }
    private void UpdateStatus(Settings s)
    {
        status.ForeColor = Ui.Muted(s); status.Text = app.Status + "\n" + (app.NetworkLabel == "" ? "Close this window to keep running in the tray." : app.NetworkLabel);
        hints.SetToolTip(status, "Device presence is an estimate based on local-network responses.");
    }
    private int RowWidth => Math.Max(260, content.ClientSize.Width - content.Padding.Horizontal);
    private void Section(string text)
    {
        var label = Ui.Label(text, RowWidth, 40, false, app.Engine.Data.Settings); label.Font = headingFont; label.ForeColor = Ui.Coral; label.Margin = new Padding(0, text == "HOME NOW" ? 0 : 17, 0, 0); content.Controls.Add(label);
    }
    private void Empty(string text) { var label = Ui.Label(text, RowWidth, 38, true, app.Engine.Data.Settings); label.Margin = Padding.Empty; content.Controls.Add(label); }
    private void PersonRow(Person person, bool home)
    {
        var device = app.Engine.Data.Devices.First(d => d.PersonId == person.Id && d.Kind == DeviceKind.Person && d.IsPrimary || d.PersonId == person.Id && d.Kind == DeviceKind.Person);
        Row((home ? "●  " : "○  ") + person.Name, person.State == PresenceState.Unknown ? "Unconfirmed" : Ui.Time(person.ChangedAt), () => ShowDevice(device), home, person.State == PresenceState.ProbablyHome ? "Last detected recently; short radio silence is tolerated." : "Open the person's phone");
    }
    private void Row(string name, string when, Action action, bool home, string? hint = null)
    {
        var width = RowWidth; var timeWidth = Math.Min(121, Math.Max(86, width / 3));
        var row = new Panel { Width = width, Height = 42, Margin = Padding.Empty, BackColor = Ui.Surface(app.Engine.Data.Settings) };
        var b = Ui.Button(name, action, width - timeWidth - 4); b.FlatAppearance.BorderSize = 0; b.TextAlign = ContentAlignment.MiddleLeft; b.Font = rowFont; b.Location = new Point(-3, 0); b.Height = 40; b.BackColor = Ui.Surface(app.Engine.Data.Settings); b.ForeColor = home ? Ui.Positive(app.Engine.Data.Settings) : ForeColor;
        var time = Ui.Label(when, timeWidth, 40, true, app.Engine.Data.Settings); time.TextAlign = ContentAlignment.MiddleRight; time.Location = new Point(width - timeWidth, 0); row.Controls.Add(b); row.Controls.Add(time); if (hint is not null) hints.SetToolTip(b, hint); content.Controls.Add(row);
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
    private void OpenSpeedTest()
    {
        if (speedTest is { IsDisposed: false }) { speedTest.Activate(); return; }
        speedTest = new SpeedTestWindow(app.Engine.Data.Settings); speedTest.FormClosed += (_, _) => speedTest = null;
        speedTest.Show();
    }
    protected override void Dispose(bool disposing) { if (disposing) { app.Changed -= Render; hints.Dispose(); headingFont.Dispose(); rowFont.Dispose(); speedTest?.Close(); } base.Dispose(disposing); }
}

internal sealed class DeviceWindow : Form
{
    public DeviceWindow(PresenceContext app, Device d)
    {
        var s = app.Engine.Data.Settings; Text = "Presence · " + d.DisplayName; ClientSize = new Size(470, 660); StartPosition = FormStartPosition.CenterParent; MinimizeBox = false; MaximizeBox = false; FormBorderStyle = FormBorderStyle.FixedDialog; Font = new Font("Segoe UI", 10); AutoScaleMode = AutoScaleMode.Dpi; BackColor = Ui.Background(s); ForeColor = Ui.Text(s);
        var flow = Ui.Flow(422); Controls.Add(flow);
        flow.Controls.Add(Ui.Label("Name", 406)); var name = new TextBox { Width = 404, Text = d.Name, PlaceholderText = d.DisplayName, MaxLength = 80 }; flow.Controls.Add(name);
        flow.Controls.Add(Ui.Label("Use this device as", 406)); var kind = new ComboBox { Width = 404, DropDownStyle = ComboBoxStyle.DropDownList }; kind.Items.AddRange(new object[] { "Unknown device", "Person / presence device", "Known device", "Ignore" }); kind.SelectedIndex = (int)d.Kind; flow.Controls.Add(kind);
        flow.Controls.Add(Ui.Label("Person (optional) · choose existing to link a private MAC", 406)); var person = new ComboBox { Width = 404, DropDownStyle = ComboBoxStyle.DropDown, MaxLength = 80 }; person.Items.AddRange(app.Engine.Data.People.Select(p => (object)p.Name).ToArray()); person.Text = app.Engine.Data.People.FirstOrDefault(p => p.Id == d.PersonId)?.Name ?? ""; flow.Controls.Add(person);
        var primary = new CheckBox { Text = "Use as this person's primary phone", Width = 405, Height = 35, Checked = d.IsPrimary || d.PersonId is null }; flow.Controls.Add(primary);
        void EnabledState() { person.Enabled = kind.SelectedIndex != (int)DeviceKind.Ignore; primary.Enabled = person.Enabled && !string.IsNullOrWhiteSpace(person.Text); }
        kind.SelectedIndexChanged += (_, _) => EnabledState(); person.TextChanged += (_, _) => EnabledState(); EnabledState();
        var buttons = new FlowLayoutPanel { Width = 405, Height = 47, Margin = Padding.Empty };
        var save = Ui.Button("Save", () =>
        {
            try
            {
                if (kind.SelectedIndex == (int)DeviceKind.Person && string.IsNullOrWhiteSpace(person.Text)) { MessageBox.Show(this, "Enter or choose a person's name.", "Presence"); return; }
                d.Name = name.Text.Trim(); d.Kind = (DeviceKind)kind.SelectedIndex;
                if (d.Kind != DeviceKind.Ignore && !string.IsNullOrWhiteSpace(person.Text)) app.Engine.Assign(d, person.Text, primary.Checked); else { d.PersonId = null; d.IsPrimary = false; }
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
        Ui.Theme(this, s);
    }
}

internal sealed class SettingsWindow : Form
{
    public SettingsWindow(PresenceContext app)
    {
        var s = app.Engine.Data.Settings; Text = "Presence · Settings"; ClientSize = new Size(490, 700); StartPosition = FormStartPosition.CenterParent; MinimizeBox = false; MaximizeBox = false; FormBorderStyle = FormBorderStyle.FixedDialog; AutoScaleMode = AutoScaleMode.Dpi; Font = new Font("Segoe UI", 10); BackColor = Ui.Background(s); ForeColor = Ui.Text(s);
        var flow = Ui.Flow(440); Controls.Add(flow);
        var intervals = new FlowLayoutPanel { Width = 430, Height = 66, Margin = Padding.Empty }; var scan = Number(s.ScanIntervalSeconds, 2, 600); var left = Number(s.DepartureGraceSeconds, 10, 3600);
        var a = new FlowLayoutPanel { Width = 206, Height = 65, Margin = Padding.Empty }; a.Controls.Add(Ui.Label("Scan interval (seconds)", 200, settings: s)); a.Controls.Add(scan); intervals.Controls.Add(a);
        var b = new FlowLayoutPanel { Width = 218, Height = 65, Margin = Padding.Empty }; b.Controls.Add(Ui.Label("Departure grace (sec)", 210, settings: s)); b.Controls.Add(left); intervals.Controls.Add(b); flow.Controls.Add(intervals);
        flow.Controls.Add(Ui.Label("NOTIFICATIONS", 430, 35, true, s));
        var arrive = Check("Arrivals", s.Arrivals); var depart = Check("Departures", s.Departures); var unknown = Check("Unknown devices", s.UnknownDevices); flow.Controls.Add(arrive); flow.Controls.Add(depart); flow.Controls.Add(unknown);
        var sound = Check("Play alert sound", s.AlertSound); flow.Controls.Add(sound);
        var popupRow = new FlowLayoutPanel { Width = 430, Height = 35, Margin = Padding.Empty }; popupRow.Controls.Add(Ui.Label("Alert duration (seconds)", 290, settings: s)); var duration = Number(s.PopupSeconds, 3, 60); duration.Width = 80; popupRow.Controls.Add(duration); flow.Controls.Add(popupRow);
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
                app.ApplyStartup(startup.Checked); s.ScanIntervalSeconds = (int)scan.Value; s.DepartureGraceSeconds = (int)left.Value; s.Arrivals = arrive.Checked; s.Departures = depart.Checked; s.UnknownDevices = unknown.Checked; s.AlertSound = sound.Checked; s.PopupSeconds = (int)duration.Value; s.QuietHours = quiet.Checked; s.QuietStart = (int)start.Value; s.QuietEnd = (int)end.Value; s.StartWithWindows = startup.Checked; s.Theme = theme.SelectedItem?.ToString() ?? "System"; s.InterfaceId = network.SelectedIndex == 0 ? "" : lans[network.SelectedIndex - 1].Id; s.RetentionDays = (int)retention.Value; app.Save(); Close();
            }
            catch (Exception ex) { MessageBox.Show(this, "Could not save settings: " + ex.Message, "Presence"); }
        }); buttons.Controls.Add(save); buttons.Controls.Add(Ui.Button("Test alert", app.TestNotification)); buttons.Controls.Add(Ui.Button("Devices", () => ShowDevices(app))); flow.Controls.Add(buttons); AcceptButton = save;
        var privacy = Ui.Label("Presence stays on this computer. It never uploads device or presence data. Device presence is only a proxy for a person being home. Sleeping phones, private MAC changes and Wi-Fi isolation can hide devices.", 426, 83, true, s); privacy.AutoEllipsis = false; flow.Controls.Add(privacy);
        Ui.Theme(this, s);
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
