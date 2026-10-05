using Microsoft.Win32;
using Presence.Core;

namespace Presence.App;

internal static class Ui
{
    public const int Small = 4, Gap = 8, Space = 12, Section = 20, Page = 24;
    public static readonly Font StandardFont = new("Segoe UI", 10);
    public static readonly Font StrongFont = new("Segoe UI", 10, FontStyle.Bold);
    public static readonly Font HeadingFont = new("Segoe UI", 11, FontStyle.Bold);
    public static readonly Font TitleFont = new("Segoe UI", 16, FontStyle.Bold);
    public static readonly Font TechnicalFont = new("Consolas", 10);
    public static bool Dark(Settings s)
    {
        if (s.Theme != "System") return s.Theme == "Dark";
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
    }
    public static Color Background(Settings s) => Dark(s) ? Color.FromArgb(30, 33, 39) : Color.FromArgb(247, 248, 250);
    public static Color Surface(Settings s) => Dark(s) ? Color.FromArgb(41, 45, 53) : Color.White;
    public static Color Text(Settings s) => Dark(s) ? Color.FromArgb(242, 244, 247) : Color.FromArgb(31, 37, 45);
    public static Color Muted(Settings s) => Dark(s) ? Color.FromArgb(188, 197, 209) : Color.FromArgb(83, 96, 112);
    public static Color Border(Settings s) => Dark(s) ? Color.FromArgb(68, 77, 89) : Color.FromArgb(211, 218, 227);
    public static Color Positive(Settings s) => Dark(s) ? Color.FromArgb(123, 221, 188) : Color.FromArgb(15, 111, 86);
    public static Color Negative(Settings s) => Dark(s) ? Color.FromArgb(255, 170, 162) : Color.FromArgb(173, 38, 31);
    public static string Time(DateTimeOffset? at) => at is null ? "Not yet seen" : at.Value.LocalDateTime.Date == DateTime.Now.Date ? at.Value.ToLocalTime().ToString("t") : at.Value.ToLocalTime().ToString("MMM d") + ", " + at.Value.ToLocalTime().ToString("t");
    public static string State(PresenceState state) => state switch { PresenceState.Home => "Home", PresenceState.ProbablyHome => "Recent", PresenceState.Away => "Away", _ => "Unknown" };

    public static void Configure(Form form, string title, Size size, Size minimum, Settings s)
    {
        form.SuspendLayout();
        form.Text = "Presence · " + title;
        form.Icon = PresenceIcons.Window;
        form.Font = StandardFont;
        form.AutoScaleDimensions = new SizeF(96, 96);
        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.ClientSize = size; form.MinimumSize = minimum;
        form.StartPosition = FormStartPosition.CenterParent;
        form.BackColor = Background(s); form.ForeColor = Text(s);
        form.MinimizeBox = false; form.MaximizeBox = false;
        form.Shown += (_, _) => { var area = Screen.FromControl(form).WorkingArea; var margin = (int)(Ui.Space * form.DeviceDpi / 96f); form.Size = new Size(Math.Min(form.Width, area.Width - margin * 2), Math.Min(form.Height, area.Height - margin * 2)); form.Location = new Point(Math.Clamp(form.Left, area.Left, Math.Max(area.Left, area.Right - form.Width)), Math.Clamp(form.Top, area.Top, Math.Max(area.Top, area.Bottom - form.Height))); };
    }
    public static TableLayoutPanel Stack() => new ContentTable() { Size = Size.Empty, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Dock = DockStyle.Top, Margin = Padding.Empty, ColumnStyles = { new ColumnStyle(SizeType.Percent, 100) } };
    public static void Add(TableLayoutPanel table, Control control)
    {
        var row = table.RowCount++; table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(control, 0, row);
    }
    public static WrapLabel Label(string text, Settings s, bool muted = false) => new() { Text = text, ForeColor = muted ? Muted(s) : Text(s), Tag = muted ? "muted" : "text" };
    public static TableLayoutPanel Header(string title, string detail, Settings s)
    {
        var panel = Stack(); panel.Padding = new Padding(Page, Section, Page, Space);
        var heading = Label(title, s); heading.Font = TitleFont; heading.Margin = new Padding(0, 0, 0, Gap); Add(panel, heading);
        if (detail.Length > 0) Add(panel, Label(detail, s, true));
        return panel;
    }
    public static TableLayoutPanel Group(string title, Settings s)
    {
        var group = Stack(); group.Margin = new Padding(0, 0, 0, Section);
        var label = Label(title, s); label.Font = HeadingFont; label.Margin = new Padding(0, 0, 0, Space); Add(group, label);
        return group;
    }
    public static TableLayoutPanel Shell(Form form, Control header, Control body, Control footer)
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.Dock = DockStyle.Fill; body.Dock = DockStyle.Fill; footer.Dock = DockStyle.Fill;
        header.Margin = body.Margin = footer.Margin = Padding.Empty;
        root.Controls.Add(header, 0, 0); root.Controls.Add(body, 0, 1); root.Controls.Add(footer, 0, 2);
        form.Controls.Add(root); form.ResumeLayout(true); return root;
    }
    public static Button Button(string text, Action action, Settings s, bool primary = false)
    {
        var button = new Button { Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(96, 36), Padding = new Padding(Space, Small, Space, Small), Margin = new Padding(0, 0, Gap, 0), Font = StandardFont, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false, Tag = primary ? "primary" : "button", AccessibleName = text.Replace("&", "") };
        button.FlatAppearance.BorderSize = 1; StyleButton(button, s); button.Click += (_, _) => action(); return button;
    }
    private static void StyleButton(Button b, Settings s)
    {
        var primary = Equals(b.Tag, "primary");
        b.BackColor = primary ? (Dark(s) ? Color.FromArgb(123, 221, 188) : Color.FromArgb(15, 111, 86)) : Surface(s);
        b.ForeColor = primary ? (Dark(s) ? Color.FromArgb(20, 35, 30) : Color.White) : Text(s);
        b.FlatAppearance.BorderColor = primary ? b.BackColor : Border(s);
    }
    public static FlowLayoutPanel Actions(params Control[] buttons)
    {
        var flow = new FlowLayoutPanel { Size = Size.Empty, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, WrapContents = true, Margin = Padding.Empty, Padding = new Padding(Page, Space, Page, Section) };
        flow.Controls.AddRange(buttons); return flow;
    }
    public static TableLayoutPanel Fields() => new ContentTable() { Size = Size.Empty, ColumnCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, Margin = Padding.Empty, ColumnStyles = { new ColumnStyle(SizeType.Percent, 42), new ColumnStyle(SizeType.Percent, 58) } };
    public static void Field(TableLayoutPanel fields, string caption, Control control, Settings s)
    {
        var row = fields.RowCount++; fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var label = Label(caption, s); label.Margin = new Padding(0, Small, Space, Space);
        control.Margin = new Padding(0, 0, 0, Space); control.Dock = DockStyle.Top; control.AccessibleName = caption;
        fields.Controls.Add(label, 0, row); fields.Controls.Add(control, 1, row);
    }
    public static Control Number(int value, int min, int max, string unit, Settings s, out NumericUpDown number)
    {
        number = new NumericUpDown { Minimum = min, Maximum = max, Value = Math.Clamp(value, min, max), Width = 100, BackColor = Surface(s), ForeColor = Text(s), Margin = new Padding(0, 0, Gap, 0) };
        var panel = new FlowLayoutPanel { Size = Size.Empty, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, WrapContents = true, Margin = Padding.Empty };
        panel.Controls.Add(number); panel.Controls.Add(new Label { Text = unit, AutoSize = true, ForeColor = Muted(s), Margin = new Padding(0, Small, 0, 0) }); return panel;
    }
    public static CheckBox Check(string text, bool value, Settings s) => new() { Text = text, Checked = value, AutoSize = true, Dock = DockStyle.Top, MinimumSize = new Size(0, 30), Margin = new Padding(0, 0, 0, Small), ForeColor = Text(s), UseVisualStyleBackColor = true };
    public static Control Value(string value, string name, Settings s, bool technical = false)
    {
        var label = Label(string.IsNullOrEmpty(value) ? "Unavailable" : value, s);
        label.AccessibleName = name + ": " + label.Text;
        if (technical) label.Font = TechnicalFont;
        var menu = new ContextMenuStrip(); menu.Items.Add("Copy value", null, (_, _) => Clipboard.SetText(label.Text)); label.ContextMenuStrip = menu;
        label.Disposed += (_, _) => menu.Dispose(); return label;
    }
    public static DataGridView Grid(Settings s)
    {
        var grid = new PresenceGrid { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, RowHeadersVisible = false, AutoGenerateColumns = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells, BorderStyle = BorderStyle.None, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single, EnableHeadersVisualStyles = false, BackgroundColor = Background(s), GridColor = Border(s), Margin = Padding.Empty, RowTemplate = { MinimumHeight = 34 } };
        grid.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Surface(s), ForeColor = Text(s), SelectionBackColor = Dark(s) ? Color.FromArgb(53, 76, 86) : Color.FromArgb(221, 239, 232), SelectionForeColor = Text(s), Padding = new Padding(Gap, Small, Gap, Small), WrapMode = DataGridViewTriState.True, Font = StandardFont };
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Background(s), ForeColor = Muted(s), Padding = new Padding(Gap, Gap, Gap, Gap), Font = StrongFont, WrapMode = DataGridViewTriState.False, SelectionBackColor = Background(s), SelectionForeColor = Text(s) };
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        return grid;
    }
    public static DataGridViewTextBoxColumn Column(string name, string title, int minimum, DataGridViewAutoSizeColumnMode mode = DataGridViewAutoSizeColumnMode.Fill) => new() { Name = name, HeaderText = title, MinimumWidth = minimum, AutoSizeMode = mode, SortMode = DataGridViewColumnSortMode.NotSortable };
    public static DataGridView History(IEnumerable<PresenceEvent> events, Settings s, bool fitHeight = false)
    {
        var grid = Grid(s); grid.AccessibleName = "Presence event history";
        grid.Columns.Add(Column("When", "When", 135, DataGridViewAutoSizeColumnMode.AllCells));
        grid.Columns.Add(Column("Name", "Device / person", 150));
        grid.Columns.Add(Column("Event", "Event", 85, DataGridViewAutoSizeColumnMode.AllCells));
        foreach (var e in events) { var i = grid.Rows.Add(Time(e.At), e.Name, e.Type == "new" ? "New device" : e.Type == "arrived" ? "Arrived" : "Left"); grid.Rows[i].Cells[0].ToolTipText = e.At.ToLocalTime().ToString("f"); grid.Rows[i].Cells[1].ToolTipText = e.Name; }
        if (fitHeight) FitRows(grid);
        return grid;
    }
    public static void FitRows(DataGridView grid)
    {
        grid.Dock = DockStyle.Top; grid.ScrollBars = ScrollBars.None;
        void Fit() { if (grid.IsDisposed) return; grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells); grid.Height = (grid.ColumnHeadersVisible ? grid.ColumnHeadersHeight : 0) + grid.Rows.Cast<DataGridViewRow>().Sum(r => r.Height) + 2; }
        grid.SizeChanged += (_, _) => Fit(); grid.DataBindingComplete += (_, _) => Fit(); grid.HandleCreated += (_, _) => Fit(); Fit();
    }
    public static void Theme(Control control, Settings s)
    {
        if (control is DataGridView) return;
        control.BackColor = control is Label ? Color.Transparent : control is TextBox or ComboBox or NumericUpDown ? Surface(s) : Background(s);
        control.ForeColor = Equals(control.Tag, "muted") ? Muted(s) : Equals(control.Tag, "error") ? Negative(s) : Text(s);
        if (control is Button button) StyleButton(button, s);
        foreach (Control child in control.Controls) Theme(child, s);
    }
}

internal sealed class WrapLabel : Label
{
    public WrapLabel() { AutoSize = true; Dock = DockStyle.Fill; Margin = Padding.Empty; UseMnemonic = false; UseCompatibleTextRendering = true; TextAlign = ContentAlignment.TopLeft; }
    public override Size GetPreferredSize(Size proposedSize)
    {
        var width = proposedSize.Width > 1 && proposedSize.Width < 10000 ? proposedSize.Width : Math.Max(100, Parent?.ClientSize.Width ?? 480);
        using var graphics = CreateGraphics();
        var measured = graphics.MeasureString(Text.Length == 0 ? " " : Text, Font, Math.Max(1, width - Padding.Horizontal - 2));
        return new Size(width, (int)Math.Ceiling(measured.Height) + Padding.Vertical + 3);
    }
}

internal sealed class ScrollBody : Panel
{
    public TableLayoutPanel Content { get; } = Ui.Stack();
    public ScrollBody() { Dock = DockStyle.Fill; AutoScroll = true; Padding = new Padding(Ui.Page, Ui.Space, Ui.Page, Ui.Space); Margin = Padding.Empty; Controls.Add(Content); }
    protected override void OnLayout(LayoutEventArgs e)
    {
        var width = Math.Max(1, ClientSize.Width - Padding.Horizontal);
        if (Content.MaximumSize.Width != width) { Content.MaximumSize = new Size(width, 0); Content.MinimumSize = new Size(width, 0); }
        base.OnLayout(e);
    }
}

internal sealed class ThemeComboBox : ComboBox
{
    private readonly ToolTip hint = new();
    public ThemeComboBox() { DrawMode = DrawMode.OwnerDrawFixed; DropDownStyle = ComboBoxStyle.DropDownList; Dock = DockStyle.Top; Margin = Padding.Empty; }
    protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); ItemHeight = Font.Height + Ui.Gap; }
    protected override void OnSelectedIndexChanged(EventArgs e) { base.OnSelectedIndexChanged(e); hint.SetToolTip(this, Text); }
    protected override void OnDropDown(EventArgs e) { DropDownWidth = Math.Min(Screen.FromControl(this).WorkingArea.Width, Math.Max(Width, Items.Cast<object>().Select(i => TextRenderer.MeasureText(i.ToString(), Font).Width + 40).DefaultIfEmpty(Width).Max())); base.OnDropDown(e); }
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        var selected = (e.State & DrawItemState.Selected) != 0;
        using var brush = new SolidBrush(selected ? SystemColors.Highlight : BackColor); e.Graphics.FillRectangle(brush, e.Bounds);
        var bounds = Rectangle.Inflate(e.Bounds, -Ui.Small, 0);
        TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, bounds, selected ? SystemColors.HighlightText : ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        e.DrawFocusRectangle();
    }
    protected override void Dispose(bool disposing) { if (disposing) hint.Dispose(); base.Dispose(disposing); }
}



internal sealed class PresenceGrid : DataGridView
{
    private int appliedDpi = 96;
    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ScaleColumns(); }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); ScaleColumns(); }
    private void ScaleColumns()
    {
        if (DeviceDpi == appliedDpi) return;
        var ratio = DeviceDpi / (float)appliedDpi; appliedDpi = DeviceDpi;
        foreach (DataGridViewColumn c in Columns) { var width = c.Width; c.MinimumWidth = (int)Math.Ceiling(c.MinimumWidth * ratio); if (c.AutoSizeMode == DataGridViewAutoSizeColumnMode.None) c.Width = (int)Math.Ceiling(width * ratio); }
        var p = DefaultCellStyle.Padding; DefaultCellStyle.Padding = new Padding((int)(p.Left * ratio), (int)(p.Top * ratio), (int)(p.Right * ratio), (int)(p.Bottom * ratio));
        var h = ColumnHeadersDefaultCellStyle.Padding; ColumnHeadersDefaultCellStyle.Padding = new Padding((int)(h.Left * ratio), (int)(h.Top * ratio), (int)(h.Right * ratio), (int)(h.Bottom * ratio));
        RowTemplate.MinimumHeight = (int)(RowTemplate.MinimumHeight * ratio);
    }
}


// These content tables contain only auto-height rows. Measure each row at its
// allocated column width so nested wrapping flows cannot reserve phantom rows.
internal sealed class ContentTable : TableLayoutPanel
{
    public override Size GetPreferredSize(Size proposedSize)
    {
        var width = proposedSize.Width > 1 && proposedSize.Width < 10000 ? proposedSize.Width : Math.Max(1, Width);
        var available = Math.Max(1, width - Padding.Horizontal);
        var total = Padding.Vertical;
        for (var row = 0; row < RowCount; row++)
        {
            var height = 0;
            for (var column = 0; column < ColumnCount; column++)
            {
                var c = GetControlFromPosition(column, row);
                if (c is null || !c.Visible) continue;
                var fraction = ColumnCount == 1 ? 1f : ColumnStyles[column].Width / 100f;
                var cellWidth = Math.Max(1, (int)(available * fraction) - c.Margin.Horizontal);
                var desired = c is DataGridView ? c.Height : c.GetPreferredSize(new Size(cellWidth, 0)).Height;
                height = Math.Max(height, desired + c.Margin.Vertical);
            }
            total += height;
        }
        return new Size(width, total);
    }
}
