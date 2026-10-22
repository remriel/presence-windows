using Presence.Core;

namespace Presence.App;

internal static class CategoryAppearance
{
    public static (Color Ink, Color Fill) Colors(DeviceCategory category, Settings settings)
    {
        var (lightInk, lightFill, darkInk, darkFill) = category switch
        {
            DeviceCategory.Phone => (0x874200, 0xFFF0CF, 0xFFD080, 0x49351B),
            DeviceCategory.Laptop => (0x164F9F, 0xE2EDFF, 0xA6CBFF, 0x233955),
            DeviceCategory.Desktop => (0x5634A0, 0xEEE5FF, 0xCDB7FF, 0x382D50),
            DeviceCategory.Tablet => (0x942A79, 0xFCE3F4, 0xF5B2DF, 0x4B2944),
            DeviceCategory.TvStreaming => (0x00685D, 0xD7F5EC, 0x8DE2CA, 0x193F39),
            DeviceCategory.GameConsole => (0xA32F29, 0xFFE5DE, 0xFFB2A1, 0x4E2D29),
            DeviceCategory.Printer => (0x5E5908, 0xF2F0C8, 0xDEDB87, 0x3E3C23),
            DeviceCategory.Speaker => (0x793D92, 0xF2E5F8, 0xDFB5EE, 0x422F4B),
            DeviceCategory.SmartHome => (0x226A31, 0xE0F4D9, 0xA9D99D, 0x2C4228),
            DeviceCategory.Camera => (0x00647C, 0xDFF2F8, 0x99DCEB, 0x213E48),
            DeviceCategory.Wearable => (0x963D60, 0xFBE5EC, 0xF5B3CC, 0x4A2E3A),
            DeviceCategory.Storage => (0x774B26, 0xF8EAD9, 0xE9C299, 0x433629),
            DeviceCategory.NetworkEquipment => (0x335B72, 0xE0EEF4, 0xAED4E6, 0x293C48),
            _ => (0x485464, 0xE9EDF2, 0xC7D0DC, 0x353E49)
        };
        Color Rgb(int value) => Color.FromArgb((value >> 16) & 255, (value >> 8) & 255, value & 255);
        return Ui.Dark(settings) ? (Rgb(darkInk), Rgb(darkFill)) : (Rgb(lightInk), Rgb(lightFill));
    }

    public static void StyleCell(DataGridViewCell cell, Device device, Settings settings)
    {
        var (ink, fill) = Colors(device.EffectiveCategory, settings);
        cell.Style.ForeColor = cell.Style.SelectionForeColor = ink;
        cell.Style.BackColor = cell.Style.SelectionBackColor = fill;
        cell.ToolTipText = device.DeviceType + (device.Category == DeviceCategory.Automatic ? " · automatically detected" : " · saved category");
    }
}

internal sealed class CategoryPicker : TableLayoutPanel
{
    private readonly List<CategoryChoice> choices = [];
    private bool updating;
    public DeviceCategory SelectedCategory { get; private set; }

    public CategoryPicker(Device device, Settings settings)
    {
        SelectedCategory = Enum.IsDefined(device.Category) ? device.Category : DeviceCategory.Automatic;
        AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink; Dock = DockStyle.Top;
        Margin = new Padding(0, Ui.Gap, 0, 0); ColumnCount = 3;
        for (var column = 0; column < 3; column++) ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
        AccessibleName = "Device category";
        AccessibleDescription = "Choose one category. Uncheck a category to return to automatic detection.";
        var categories = Enum.GetValues<DeviceCategory>();
        RowCount = 1 + (int)Math.Ceiling((categories.Length - 1) / 3d);
        for (var row = 0; row < RowCount; row++) RowStyles.Add(new RowStyle(SizeType.AutoSize));
        foreach (var category in categories)
        {
            var caption = category == DeviceCategory.Automatic ? "Automatic · " + DeviceIdentification.InferType(device.Hostname) : DeviceCategories.Label(category);
            var choice = new CategoryChoice(category, caption) { Checked = category == SelectedCategory };
            choices.Add(choice);
            var index = (int)category - 1;
            Controls.Add(choice, category == DeviceCategory.Automatic ? 0 : index % 3, category == DeviceCategory.Automatic ? 0 : 1 + index / 3);
            if (category == DeviceCategory.Automatic) SetColumnSpan(choice, 3);
            choice.CheckedChanged += (_, _) =>
            {
                if (updating) return;
                updating = true;
                SelectedCategory = choice.Checked ? category : DeviceCategory.Automatic;
                foreach (var other in choices) { other.Checked = other.Category == SelectedCategory; other.ApplyTheme(settings); }
                updating = false;
            };
            choice.ApplyTheme(settings);
        }
    }
    protected override void OnLayout(LayoutEventArgs e)
    {
        // Keep full labels readable when the details window is narrowed.
        var columns = ClientSize.Width >= 540 * DeviceDpi / 96f ? 3 : 2;
        if (choices.Count == Enum.GetValues<DeviceCategory>().Length && ColumnCount != columns)
        {
            SuspendLayout();
            ColumnCount = columns; ColumnStyles.Clear(); RowStyles.Clear();
            for (var column = 0; column < columns; column++) ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            RowCount = 1 + (int)Math.Ceiling((choices.Count - 1) / (double)columns);
            for (var row = 0; row < RowCount; row++) RowStyles.Add(new RowStyle(SizeType.AutoSize));
            SetColumnSpan(choices[0], columns);
            for (var index = 1; index < choices.Count; index++) SetCellPosition(choices[index], new TableLayoutPanelCellPosition((index - 1) % columns, 1 + (index - 1) / columns));
            ResumeLayout(false);
        }
        base.OnLayout(e);
    }
}

internal sealed class CategoryChoice : CheckBox
{
    public DeviceCategory Category { get; }
    public CategoryChoice(DeviceCategory category, string caption)
    {
        Category = category; Text = caption; AccessibleName = "Category: " + (category == DeviceCategory.Automatic ? "Automatic" : DeviceCategories.Label(category));
        AccessibleDescription = "Select one device category. Space toggles this checkbox.";
        Dock = DockStyle.Fill; AutoSize = false; Height = 32; MinimumSize = new Size(0, 32);
        Margin = new Padding(0, 0, Ui.Small, Ui.Small); Padding = new Padding(Ui.Gap, 0, Ui.Small, 0);
        UseVisualStyleBackColor = false; FlatStyle = FlatStyle.Flat; Cursor = Cursors.Hand;
    }
    public void ApplyTheme(Settings settings)
    {
        var (ink, fill) = CategoryAppearance.Colors(Category, settings);
        ForeColor = ink; BackColor = Checked ? fill : Ui.Surface(settings);
        Font = Checked ? Ui.StrongFont : Ui.StandardFont;
        FlatAppearance.CheckedBackColor = fill; FlatAppearance.BorderColor = ink;
        Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(ForeColor, Checked ? Math.Max(2, DeviceDpi / 48f) : 1);
        e.Graphics.DrawRectangle(pen, 1, 1, Math.Max(0, Width - 3), Math.Max(0, Height - 3));
    }
}
