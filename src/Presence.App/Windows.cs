using Presence.Core;

namespace Presence.App;

internal sealed class MainWindow : Form
{
    private readonly PresenceContext app;
    private readonly ScrollBody body = new();
    private readonly WrapLabel status;
    private readonly WrapLabel network;
    private readonly TableLayoutPanel root;
    private SpeedTestWindow? speedTest;
    private string signature = "";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool AllowClose { get; set; }
    public MainWindow(PresenceContext context)
    {
        app = context; var s = app.Engine.Data.Settings;
        Ui.Configure(this, "Presence", new Size(640, 760), new Size(500, 480), s);
        Text = "Presence"; MinimizeBox = MaximizeBox = true; StartPosition = FormStartPosition.CenterScreen;
        var header = Ui.Stack(); header.Padding = new Padding(Ui.Page, Ui.Section, Ui.Page, Ui.Space);
        var heading = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 2, Margin = new Padding(0, 0, 0, Ui.Space) };
        heading.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); heading.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var title = Ui.Label("Presence", s); title.Font = Ui.TitleFont;
        heading.Controls.Add(title, 0, 0); heading.Controls.Add(Ui.Button("&Settings", ShowSettings, s), 1, 0); Ui.Add(header, heading);
        status = Ui.Label("Finding your network…", s); status.Font = Ui.StrongFont; status.Margin = new Padding(0, 0, 0, Ui.Small); Ui.Add(header, status);
        network = Ui.Label("", s, true); Ui.Add(header, network);
        var footer = Ui.Stack();
        var hint = Ui.Label("Monitoring continues in the tray when you close this window.", s, true); hint.Margin = new Padding(Ui.Page, Ui.Gap, Ui.Page, 0); Ui.Add(footer, hint);
        Ui.Add(footer, Ui.Actions(Ui.Button("&Refresh", async () => await app.Scan(true), s, true), Ui.Button("Speed &test", OpenSpeedTest, s), Ui.Button("&Activity", ShowActivity, s)));
        root = Ui.Shell(this, header, body, footer);
        FormClosing += (_, e) => { if (!AllowClose && e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); } };
        app.Changed += Render; VisibleChanged += (_, _) => { if (Visible) Render(); }; Render();
    }
    private void Render()
    {
        if (IsDisposed) return;
        var s = app.Engine.Data.Settings;
        status.Text = app.Monitoring ? (app.IsScanning ? "Scanning your network…" : "Monitoring your network") : "Waiting for your network";
        network.Text = string.Join("\n", new[] { app.NetworkLabel, app.Status }.Where(x => x.Length > 0));
        var next = s.Theme + ":" + Ui.Dark(s) + ":" + app.NetworkScope + ":" + app.Monitoring + ":" + string.Join(";", app.Engine.Data.People.Select(p => $"{p.Id}:{p.Name}:{p.State}:{p.ChangedAt:O}")) + ":" + string.Join(";", app.Engine.Data.Devices.Select(d => $"{d.Mac}:{d.Network}:{d.Ip}:{d.DisplayName}:{d.Kind}:{d.State}:{d.PersonId}:{d.IsPrimary}:{d.ChangedAt:O}"));
        if (next == signature) return;
        signature = next; BackColor = Ui.Background(s); ForeColor = Ui.Text(s); Ui.Theme(root, s);
        var scroll = -body.AutoScrollPosition.Y;
        body.Content.SuspendLayout();
        foreach (var control in body.Content.Controls.Cast<Control>().ToArray()) control.Dispose();
        body.Content.Controls.Clear(); body.Content.RowStyles.Clear(); body.Content.RowCount = 0;
        var local = app.Engine.Data.Devices.Where(d => app.NetworkScope != "" && d.Network == app.NetworkScope).ToList();
        var people = app.Engine.Data.People.Where(p => p.State != PresenceState.Unknown && local.Any(d => d.PersonId == p.Id && d.Kind == DeviceKind.Person)).ToList();
        var devices = local.Where(d => d.Kind == DeviceKind.Known).ToList();
        var home = new List<(string State, string Name, string When, Action Open)>();
        var away = new List<(string State, string Name, string When, Action Open)>();
        foreach (var person in people)
        {
            var device = local.Where(d => d.PersonId == person.Id && d.Kind == DeviceKind.Person).OrderByDescending(d => d.IsPrimary).First();
            var list = person.State is PresenceState.Home or PresenceState.ProbablyHome ? home : away;
            list.Add((Ui.State(person.State), person.Name, Ui.Time(person.ChangedAt), () => ShowDevice(device)));
        }
        foreach (var d in devices)
        {
            if (d.State is PresenceState.Home or PresenceState.ProbablyHome) home.Add((Ui.State(d.State), d.DisplayName, Ui.Time(d.ChangedAt), () => ShowDevice(d)));
            else if (d.State == PresenceState.Away) away.Add(("Away", d.DisplayName, Ui.Time(d.ChangedAt), () => ShowDevice(d)));
        }
        AddSection("Home now", home, "No recognized devices are home right now.", s);
        AddSection("Away", away, "No recognized devices are away.", s);
        var unknown = local.Where(d => d.Kind == DeviceKind.Unknown && (d.State is PresenceState.Home or PresenceState.ProbablyHome || d.Consecutive > 0)).OrderByDescending(d => d.FirstSeen).Select(d => ("New", d.DisplayName, "Details", (Action)(() => ShowDevice(d)))).ToList();
        AddSection("Unknown devices", unknown, "No unidentified devices. Newly discovered devices will appear here.", s);
        var recent = Ui.Group("Recent activity", s); var history = app.Store.History(limit: 5);
        Ui.Add(recent, history.Count == 0 ? Ui.Label("Arrivals and departures will appear here. Your first scan establishes a quiet baseline.", s, true) : Ui.History(history, s, true)); Ui.Add(body.Content, recent);
        body.Content.ResumeLayout(true); body.AutoScrollPosition = new Point(0, scroll);
    }
    private void AddSection(string title, List<(string State, string Name, string When, Action Open)> rows, string empty, Settings s)
    {
        var section = Ui.Group(title + "  ·  " + rows.Count, s);
        if (rows.Count == 0) Ui.Add(section, Ui.Label(empty, s, true));
        else
        {
            var grid = Ui.Grid(s); grid.ColumnHeadersVisible = false; grid.AccessibleName = title + ". Select a row to open device details."; grid.Cursor = Cursors.Hand;
            var state = Ui.Column("State", "State", 76, DataGridViewAutoSizeColumnMode.None); state.Width = 76;
            var when = Ui.Column("When", "Changed", 118, DataGridViewAutoSizeColumnMode.None); when.Width = 118;
            grid.Columns.Add(state); grid.Columns.Add(Ui.Column("Name", "Device / person", 120)); grid.Columns.Add(when);
            foreach (var row in rows) { var i = grid.Rows.Add(row.State, row.Name, row.When); grid.Rows[i].Cells[1].Style.Font = Ui.StrongFont; grid.Rows[i].Cells[1].ToolTipText = row.Name + " — open details"; }
            grid.CellClick += (_, e) => { if (e.RowIndex >= 0) rows[e.RowIndex].Open(); };
            grid.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter && grid.CurrentRow is { } row) { e.Handled = e.SuppressKeyPress = true; rows[row.Index].Open(); } };
            Ui.FitRows(grid); Ui.Add(section, grid);
        }
        Ui.Add(body.Content, section);
    }
    public void ShowDevice(Device device) { Show(); using var dialog = new DeviceWindow(app, device); dialog.ShowDialog(this); }
    public void ShowActivity() { Show(); using var dialog = new ActivityWindow(app); dialog.ShowDialog(this); }
    public void ShowSettings() { Show(); using var dialog = new SettingsWindow(app); dialog.ShowDialog(this); }
    private void OpenSpeedTest()
    {
        if (speedTest is { IsDisposed: false }) { speedTest.Activate(); return; }
        speedTest = new SpeedTestWindow(app.Engine.Data.Settings); speedTest.FormClosed += (_, _) => speedTest = null; speedTest.Show(this);
    }
    protected override void Dispose(bool disposing) { if (disposing) { app.Changed -= Render; speedTest?.Close(); } base.Dispose(disposing); }
}

internal sealed class SettingsWindow : Form
{
    public SettingsWindow(PresenceContext app)
    {
        var s = app.Engine.Data.Settings;
        Ui.Configure(this, "Settings", new Size(640, 760), new Size(540, 460), s);
        var body = new ScrollBody();
        var monitoring = Ui.Group("Monitoring", s); var fields = Ui.Fields();
        Ui.Field(fields, "Scan interval", Ui.Number(s.ScanIntervalSeconds, 2, 600, "seconds", s, out var scan), s);
        Ui.Field(fields, "Departure grace", Ui.Number(s.DepartureGraceSeconds, 10, 3600, "seconds", s, out var left), s); Ui.Add(monitoring, fields);
        Ui.Add(monitoring, Ui.Label("A device is marked away after this grace period without a fresh response. Sleeping phones may briefly stop responding.", s, true)); Ui.Add(body.Content, monitoring);
        var notifications = Ui.Group("Notifications", s);
        var arrive = Ui.Check("Notify when devices arrive", s.Arrivals, s); var depart = Ui.Check("Notify when devices leave", s.Departures, s); var unknown = Ui.Check("Notify about new devices", s.UnknownDevices, s); var sound = Ui.Check("Play an alert sound", s.AlertSound, s);
        foreach (var checkbox in new[] { arrive, depart, unknown, sound }) Ui.Add(notifications, checkbox);
        var durationFields = Ui.Fields(); durationFields.Margin = new Padding(0, Ui.Space, 0, 0); Ui.Field(durationFields, "Floating alert duration", Ui.Number(s.PopupSeconds, 3, 60, "seconds", s, out var duration), s); Ui.Add(notifications, durationFields); Ui.Add(body.Content, notifications);
        var quietSection = Ui.Group("Quiet hours", s); var quiet = Ui.Check("Pause notifications during quiet hours", s.QuietHours, s); Ui.Add(quietSection, quiet);
        var hours = Ui.Fields();
        Ui.Field(hours, "From", Ui.Number(s.QuietStart, 0, 23, "hour", s, out var start), s); Ui.Field(hours, "Until", Ui.Number(s.QuietEnd, 0, 23, "hour", s, out var end), s);
        hours.Enabled = quiet.Checked; quiet.CheckedChanged += (_, _) => hours.Enabled = quiet.Checked;
        Ui.Add(quietSection, hours); Ui.Add(quietSection, Ui.Label("Uses 24-hour time. For example, 22 until 7 pauses alerts overnight.", s, true)); Ui.Add(body.Content, quietSection);
        var application = Ui.Group("Application", s); var startup = Ui.Check("Start quietly with Windows", s.StartWithWindows, s); Ui.Add(application, startup);
        var appearance = new ThemeComboBox(); appearance.Items.AddRange(["System", "Light", "Dark"]); appearance.SelectedItem = s.Theme;
        var appFields = Ui.Fields(); appFields.Margin = new Padding(0, Ui.Space, 0, 0); Ui.Field(appFields, "Appearance", appearance, s); Ui.Add(application, appFields);
        Ui.Add(application, Ui.Label("Home network adapter", s));
        var network = new ThemeComboBox { Margin = new Padding(0, Ui.Gap, 0, Ui.Gap) }; network.Items.Add("Automatic · physical Wi-Fi first"); var lans = Discovery.Interfaces(); foreach (var lan in lans) network.Items.Add(lan.Name); network.SelectedIndex = Math.Max(0, lans.FindIndex(n => n.Id == s.InterfaceId) + 1); network.AccessibleName = "Home network adapter"; Ui.Add(application, network); Ui.Add(body.Content, application);
        var storage = Ui.Group("Local history", s); var retentionFields = Ui.Fields(); Ui.Field(retentionFields, "Keep event history", Ui.Number(s.RetentionDays, 7, 365, "days", s, out var retention), s); Ui.Add(storage, retentionFields);
        Ui.Add(storage, Ui.Label("Device names, person associations and presence history stay on this computer. Device presence is an estimate of whether someone is home; sleeping phones and private MAC changes can affect it.", s, true)); Ui.Add(body.Content, storage);
        var footer = Ui.Stack(); var error = Ui.Label("", s); error.Tag = "error"; error.Visible = false; error.Margin = new Padding(Ui.Page, Ui.Gap, Ui.Page, 0); Ui.Add(footer, error);
        var save = Ui.Button("&Save", () =>
        {
            try
            {
                app.ApplyStartup(startup.Checked); s.ScanIntervalSeconds = (int)scan.Value; s.DepartureGraceSeconds = (int)left.Value; s.Arrivals = arrive.Checked; s.Departures = depart.Checked; s.UnknownDevices = unknown.Checked; s.AlertSound = sound.Checked; s.PopupSeconds = (int)duration.Value; s.QuietHours = quiet.Checked; s.QuietStart = (int)start.Value; s.QuietEnd = (int)end.Value; s.StartWithWindows = startup.Checked; s.Theme = appearance.SelectedItem?.ToString() ?? "System"; s.InterfaceId = network.SelectedIndex <= 0 ? "" : lans[network.SelectedIndex - 1].Id; s.RetentionDays = (int)retention.Value; app.Save(); Close();
            }
            catch (Exception ex) { error.Text = "Could not save settings. " + ex.Message; error.ForeColor = Ui.Negative(s); error.Visible = true; }
        }, s, true);
        var cancel = Ui.Button("&Cancel", Close, s); cancel.DialogResult = DialogResult.Cancel;
        Ui.Add(footer, Ui.Actions(save, cancel, Ui.Button("Test &alert", app.TestNotification, s), Ui.Button("&Devices", () => ShowDevices(app), s)));
        AcceptButton = save; CancelButton = cancel;
        Ui.Shell(this, Ui.Header("Settings", "Choose how Presence monitors your network and notifies you.", s), body, footer); Ui.Theme(this, s);
    }
    private void ShowDevices(PresenceContext app) { using var dialog = new DevicesWindow(app); dialog.ShowDialog(this); }
}

internal sealed class DeviceWindow : Form
{
    public DeviceWindow(PresenceContext app, Device device)
    {
        var s = app.Engine.Data.Settings;
        Ui.Configure(this, "Device details", new Size(660, 780), new Size(540, 480), s);
        var body = new ScrollBody(); var identity = Ui.Group("Identity and tracking", s); var fields = Ui.Fields();
        var name = new TextBox { Text = device.Name, PlaceholderText = device.DisplayName, MaxLength = 80 };
        var kind = new ThemeComboBox(); kind.Items.AddRange(["Unknown device", "Person / presence device", "Known device", "Ignore"]); kind.SelectedIndex = (int)device.Kind;
        Ui.Field(fields, "Device name", name, s); Ui.Field(fields, "Track as", kind, s); Ui.Add(identity, fields);
        Ui.Add(identity, Ui.Label("Ignored devices stay in your local list but do not generate presence alerts.", s, true)); Ui.Add(body.Content, identity);
        var association = Ui.Group("Person association", s);
        Ui.Add(association, Ui.Label("Choose an existing person to link another device or a new private MAC, or enter a new person's name.", s, true));
        var person = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown, Dock = DockStyle.Top, MaxLength = 80, Margin = new Padding(0, Ui.Space, 0, Ui.Gap), AccessibleName = "Assigned person" };
        person.Items.AddRange(app.Engine.Data.People.Select(p => (object)p.Name).ToArray()); person.Text = app.Engine.Data.People.FirstOrDefault(p => p.Id == device.PersonId)?.Name ?? ""; Ui.Add(association, person);
        var primary = Ui.Check("Use as this person's primary phone", device.IsPrimary || device.PersonId is null, s); Ui.Add(association, primary);
        void EnabledState() { person.Enabled = kind.SelectedIndex != (int)DeviceKind.Ignore; primary.Enabled = person.Enabled && !string.IsNullOrWhiteSpace(person.Text); }
        kind.SelectedIndexChanged += (_, _) => EnabledState(); person.TextChanged += (_, _) => EnabledState(); EnabledState(); Ui.Add(body.Content, association);
        var details = Ui.Group("Network information", s); var values = Ui.Fields();
        foreach (var field in new[] { ("MAC address", device.Mac + (Identity.IsPrivateMac(device.Mac) ? " (private)" : ""), true), ("IP address", device.Ip, true), ("Hostname", device.Hostname, true), ("Manufacturer", device.Vendor, false), ("First seen", device.FirstSeen.ToLocalTime().ToString("g"), false), ("Last seen", device.LastSeen.ToLocalTime().ToString("g"), false), ("Presence", Ui.State(device.State), false) }) Ui.Field(values, field.Item1, Ui.Value(field.Item2, field.Item1, s, field.Item3), s);
        Ui.Add(details, values); Ui.Add(details, Ui.Label("Right-click a value to copy it.", s, true)); Ui.Add(body.Content, details);
        var historySection = Ui.Group("Presence history", s); var history = app.Store.History(device.Mac, device.PersonId, 50);
        if (history.Count == 0) Ui.Add(historySection, Ui.Label("No events yet. The initial scan is a quiet baseline; future arrivals and departures appear here.", s, true));
        else { var grid = Ui.History(history, s); grid.Dock = DockStyle.Top; grid.Height = 220; Ui.Add(historySection, grid); }
        Ui.Add(body.Content, historySection);
        var footer = Ui.Stack(); var error = Ui.Label("", s); error.Tag = "error"; error.Visible = false; error.Margin = new Padding(Ui.Page, Ui.Gap, Ui.Page, 0); Ui.Add(footer, error);
        var save = Ui.Button("&Save", () =>
        {
            if (kind.SelectedIndex == (int)DeviceKind.Person && string.IsNullOrWhiteSpace(person.Text)) { error.Text = "Enter or choose a person's name to use this as a presence device."; error.ForeColor = Ui.Negative(s); error.Visible = true; person.Focus(); return; }
            try { device.Name = name.Text.Trim(); device.Kind = (DeviceKind)kind.SelectedIndex; if (device.Kind != DeviceKind.Ignore && !string.IsNullOrWhiteSpace(person.Text)) app.Engine.Assign(device, person.Text, primary.Checked); else { device.PersonId = null; device.IsPrimary = false; } app.Save(); Close(); }
            catch (Exception ex) { error.Text = "Could not save this device. " + ex.Message; error.ForeColor = Ui.Negative(s); error.Visible = true; }
        }, s, true);
        var cancel = Ui.Button("&Cancel", Close, s); cancel.DialogResult = DialogResult.Cancel; Ui.Add(footer, Ui.Actions(save, cancel)); AcceptButton = save; CancelButton = cancel;
        Ui.Shell(this, Ui.Header("Device details", device.DisplayName, s), body, footer); Ui.Theme(this, s);
    }
}

internal sealed class ActivityWindow : Form
{
    public ActivityWindow(PresenceContext app)
    {
        var s = app.Engine.Data.Settings; Ui.Configure(this, "Activity", new Size(720, 560), new Size(560, 400), s);
        var events = app.Store.History(); var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(Ui.Page, Ui.Space, Ui.Page, Ui.Space) };
        if (events.Count == 0) { var empty = Ui.Label("No activity yet. Your first network scan creates a quiet baseline. Future arrivals, departures and new devices will be recorded here.", s, true); empty.AutoSize = false; body.Controls.Add(empty); } else body.Controls.Add(Ui.History(events, s));
        var close = Ui.Button("&Close", Close, s); close.DialogResult = DialogResult.Cancel; CancelButton = close;
        Ui.Shell(this, Ui.Header("Activity", "Recent arrivals, departures and newly discovered devices.", s), body, Ui.Actions(close));
    }
}

internal sealed class DevicesWindow : Form
{
    public DevicesWindow(PresenceContext app)
    {
        var s = app.Engine.Data.Settings; Ui.Configure(this, "All devices", new Size(760, 560), new Size(600, 400), s);
        var devices = app.Engine.Data.Devices.OrderBy(d => d.DisplayName).ToList();
        var grid = Ui.Grid(s); grid.AccessibleName = "All devices";
        grid.Columns.Add(Ui.Column("Device", "Device", 160)); grid.Columns.Add(Ui.Column("State", "Presence", 85, DataGridViewAutoSizeColumnMode.AllCells)); grid.Columns.Add(Ui.Column("Kind", "Tracking", 100, DataGridViewAutoSizeColumnMode.AllCells)); grid.Columns.Add(Ui.Column("IP", "IP address", 120, DataGridViewAutoSizeColumnMode.AllCells));
        foreach (var device in devices) { var i = grid.Rows.Add(device.DisplayName, Ui.State(device.State), device.Kind == DeviceKind.Person ? "Person" : device.Kind.ToString(), device.Ip); grid.Rows[i].Cells[0].ToolTipText = device.DisplayName; }
        void Open() { if (grid.CurrentRow is not { } row) return; using var dialog = new DeviceWindow(app, devices[row.Index]); dialog.ShowDialog(this); var d = devices[row.Index]; grid.Rows[row.Index].SetValues(d.DisplayName, Ui.State(d.State), d.Kind == DeviceKind.Person ? "Person" : d.Kind.ToString(), d.Ip); }
        grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Open(); }; grid.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = e.SuppressKeyPress = true; Open(); } };
        var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(Ui.Page, Ui.Space, Ui.Page, Ui.Space) }; if (devices.Count == 0) { var empty = Ui.Label("No devices discovered yet. Connect to your local network and refresh Presence.", s, true); empty.AutoSize = false; body.Controls.Add(empty); } else body.Controls.Add(grid);
        var edit = Ui.Button("&Open device", Open, s, true); edit.Enabled = devices.Count > 0;
        var close = Ui.Button("&Close", Close, s); close.DialogResult = DialogResult.Cancel; CancelButton = close;
        Ui.Shell(this, Ui.Header("All devices", "Select a device to edit its name, person association or tracking behavior.", s), body, Ui.Actions(edit, close));
    }
}

