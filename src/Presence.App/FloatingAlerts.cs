using System.Reflection;
using System.Runtime.InteropServices;
using Presence.Core;

namespace Presence.App;

// App-owned windows and direct WAV playback; no Windows notification service is involved.
internal sealed class FloatingAlerts : IDisposable
{
    private sealed record Alert(string Message, string Detail, string Mac);
    private readonly Queue<Alert> queue = new();
    private readonly Func<Settings> settings;
    private readonly Func<Rectangle> workArea;
    private readonly Action<string> activate;
    private readonly Action<Action> dispatch;
    private readonly Bitmap icon;
    private readonly GCHandle sound;
    private FloatingAlertWindow? current;
    private int overflow;
    private bool disposed;
    public FloatingAlerts(Func<Settings> getSettings, Func<Rectangle> getWorkArea, Action<string> onActivate, Action<Action> post)
    {
        settings = getSettings; workArea = getWorkArea; activate = onActivate; dispatch = post;
        using var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Presence.ProductIcon") ?? throw new InvalidDataException("Missing Presence icon.");
        using var source = Image.FromStream(imageStream); icon = new Bitmap(source);
        using var audio = Assembly.GetExecutingAssembly().GetManifestResourceStream("Presence.AlertSound") ?? throw new InvalidDataException("Missing alert sound.");
        using var bytes = new MemoryStream(); audio.CopyTo(bytes); sound = GCHandle.Alloc(bytes.ToArray(), GCHandleType.Pinned);
    }
    public void Show(string message, string detail, string mac, bool immediate = false)
    {
        if (disposed) return;
        var alert = new Alert(message, detail, mac);
        if (immediate)
        {
            var old = current; current = null; old?.Close(); Present(alert); return;
        }
        if (queue.Count >= 20) overflow++; else queue.Enqueue(alert);
        Pump();
    }
    private void Pump()
    {
        if (disposed || current is not null) return;
        if (queue.TryDequeue(out var next)) Present(next);
        else if (overflow > 0) { var count = overflow; overflow = 0; Present(new(count + " more network changes", "Open Presence to view activity", "")); }
    }
    private void Present(Alert alert)
    {
        var popup = new FloatingAlertWindow(alert.Message, alert.Detail, icon, settings(), activate, alert.Mac);
        current = popup;
        popup.FormClosed += (_, _) =>
        {
            if (current != popup) return;
            current = null;
            // Let the dismiss/click finish before advancing the bounded queue.
            if (!disposed) dispatch(Pump);
        };
        void Place() { var area = workArea(); var gap = (int)Math.Round(20 * popup.DeviceDpi / 96d); popup.Location = new Point(Math.Max(area.Left + gap, area.Right - popup.Width - gap), Math.Max(area.Top + gap, area.Bottom - popup.Height - gap)); }
        popup.SizeChanged += (_, _) => Place(); Place();
        popup.Show();
        if (settings().AlertSound) PlaySound(sound.AddrOfPinnedObject(), IntPtr.Zero, 0x0007); // MEMORY | ASYNC | NODEFAULT
    }
    public void ExportPreview(string path)
    {
        using var popup = new FloatingAlertWindow("Weston’s phone arrived", "Presence · click to open device", icon, settings(), _ => { }, "");
        _ = popup.Handle;
        using var bitmap = new Bitmap(popup.ClientSize.Width, popup.ClientSize.Height);
        popup.DrawToBitmap(bitmap, new Rectangle(Point.Empty, popup.ClientSize));
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path))!);
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true; current?.Close(); current = null; queue.Clear();
        PlaySound(IntPtr.Zero, IntPtr.Zero, 0); sound.Free(); icon.Dispose();
    }
    [DllImport("winmm.dll", EntryPoint = "PlaySoundW")] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PlaySound(IntPtr sound, IntPtr module, uint flags);
}

internal sealed class FloatingAlertWindow : Form
{
    private readonly System.Windows.Forms.Timer dismiss;
    private readonly Action<string> activate;
    private readonly string mac;
    private readonly Color border;
    private TableLayoutPanel? layout;
    private int remaining;
    protected override bool ShowWithoutActivation => true;
    protected override CreateParams CreateParams { get { var p = base.CreateParams; p.ExStyle |= 0x08000080; return p; } }
    public FloatingAlertWindow(string message, string detail, Image icon, Settings settings, Action<string> onActivate, string deviceMac)
    {
        SuspendLayout(); activate = onActivate; mac = deviceMac; border = Ui.Border(settings); remaining = Math.Clamp(settings.PopupSeconds, 3, 60);
        Text = "Presence notification"; FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false; TopMost = true; StartPosition = FormStartPosition.Manual;
        Font = Ui.StandardFont; AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Ui.Surface(settings); ForeColor = Ui.Text(settings); Cursor = Cursors.Hand;
        ClientSize = new Size(420, 160);
        var root = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 3, RowCount = 1, Padding = new Padding(Ui.Space + Ui.Small), Margin = Padding.Empty, Width = 420 };
        layout = root; root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
        var picture = new PictureBox { Image = icon, SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(40, 40), Margin = new Padding(0, Ui.Small, Ui.Space, 0), AccessibleName = "Presence" };
        var text = Ui.Stack(); text.Dock = DockStyle.Top;
        var heading = Ui.Label("Presence", settings); heading.Font = Ui.StrongFont; heading.ForeColor = Ui.Positive(settings); heading.Margin = new Padding(0, 0, 0, Ui.Small);
        var title = Ui.Label(message, settings); title.Font = Ui.StrongFont; title.Margin = new Padding(0, 0, 0, Ui.Gap);
        var subtitle = Ui.Label(detail, settings, true);
        Ui.Add(text, heading); Ui.Add(text, title); Ui.Add(text, subtitle);
        var close = new Button { Text = "×", Size = new Size(32, 32), Margin = new Padding(Ui.Gap, 0, 0, 0), FlatStyle = FlatStyle.Flat, ForeColor = Ui.Text(settings), BackColor = Ui.Surface(settings), AccessibleName = "Dismiss notification", TabStop = false };
        close.FlatAppearance.BorderSize = 0; close.Click += (_, _) => Close();
        root.Controls.Add(picture, 0, 0); root.Controls.Add(text, 1, 0); root.Controls.Add(close, 2, 0); Controls.Add(root);
        foreach (var c in new Control[] { root, text, picture, heading, title, subtitle }) c.Click += (_, _) => OpenDevice();
        Click += (_, _) => OpenDevice();
        dismiss = new System.Windows.Forms.Timer { Interval = 1000 };
        dismiss.Tick += (_, _) => { if (ClientRectangle.Contains(PointToClient(Cursor.Position))) return; if (--remaining <= 0) Close(); };
        Shown += (_, _) => dismiss.Start(); ResumeLayout(true);
    }
    private void OpenDevice() { Close(); activate(mac); }
    protected override void OnLayout(LayoutEventArgs e) { base.OnLayout(e); if (layout is not null) { var height = layout.GetPreferredSize(new Size(ClientSize.Width, 0)).Height; if (height > 0 && ClientSize.Height != height) ClientSize = new Size(ClientSize.Width, height); } }
    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using var pen = new Pen(border); e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1); }
    protected override void WndProc(ref Message m) { if (m.Msg == 0x21) { m.Result = (IntPtr)3; return; } base.WndProc(ref m); }
    protected override void Dispose(bool disposing) { if (disposing) dismiss.Dispose(); base.Dispose(disposing); }
}
