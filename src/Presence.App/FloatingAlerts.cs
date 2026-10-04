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
    private readonly Bitmap icon;
    private readonly GCHandle sound;
    private FloatingAlertWindow? current;
    private int overflow;
    private bool disposed;
    public FloatingAlerts(Func<Settings> getSettings, Func<Rectangle> getWorkArea, Action<string> onActivate)
    {
        settings = getSettings; workArea = getWorkArea; activate = onActivate;
        using var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Presence.AlertIcon") ?? throw new InvalidDataException("Missing alert icon.");
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
            if (!disposed) { var dispatcher = Application.OpenForms.Cast<Form>().FirstOrDefault(f => !f.IsDisposed && f.IsHandleCreated); dispatcher?.BeginInvoke(Pump); }
        };
        var area = workArea(); popup.Location = new Point(Math.Max(area.Left + 12, area.Right - popup.Width - 20), Math.Max(area.Top + 12, area.Bottom - popup.Height - 20));
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
    private int remaining;
    protected override bool ShowWithoutActivation => true;
    protected override CreateParams CreateParams { get { var p = base.CreateParams; p.ExStyle |= 0x08000080; return p; } }
    public FloatingAlertWindow(string message, string detail, Image icon, Settings settings, Action<string> onActivate, string deviceMac)
    {
        activate = onActivate; mac = deviceMac; remaining = Math.Clamp(settings.PopupSeconds, 3, 60);
        Text = "Presence notification"; FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false; TopMost = true; StartPosition = FormStartPosition.Manual; AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(360, 104); BackColor = Ui.Background(settings); ForeColor = Ui.Text(settings); Font = new Font("Segoe UI", 10); Cursor = Cursors.Hand;
        var picture = new PictureBox { Image = icon, SizeMode = PictureBoxSizeMode.Zoom, Bounds = new Rectangle(17, 30, 42, 42), BackColor = BackColor };
        var header = new Label { Text = "PRESENCE", Bounds = new Rectangle(73, 14, 245, 21), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Ui.Dark(settings) ? Color.FromArgb(135, 209, 172) : Color.FromArgb(30, 100, 70), BackColor = BackColor };
        var title = new Label { Text = message, Bounds = new Rectangle(73, 37, 259, 31), Font = new Font("Segoe UI", 11), AutoEllipsis = true, ForeColor = ForeColor, BackColor = BackColor, TextAlign = ContentAlignment.MiddleLeft };
        var subtitle = new Label { Text = detail, Bounds = new Rectangle(73, 71, 259, 20), Font = new Font("Segoe UI", 8.5f), AutoEllipsis = true, ForeColor = Ui.Muted(settings), BackColor = BackColor };
        var close = new Label { Text = "×", Bounds = new Rectangle(326, 5, 28, 28), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 15), ForeColor = Ui.Muted(settings), BackColor = BackColor, AccessibleName = "Dismiss notification" };
        close.Click += (_, _) => Close(); Controls.AddRange([picture, header, title, subtitle, close]);
        foreach (var c in new Control[] { picture, header, title, subtitle }) c.Click += (_, _) => OpenDevice();
        Click += (_, _) => OpenDevice();
        dismiss = new System.Windows.Forms.Timer { Interval = 1000 };
        dismiss.Tick += (_, _) => { if (ClientRectangle.Contains(PointToClient(Cursor.Position))) return; if (--remaining <= 0) Close(); };
        Shown += (_, _) => dismiss.Start();
    }
    private void OpenDevice() { Close(); activate(mac); }
    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using var pen = new Pen(Color.FromArgb(110, 117, 124)); e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1); }
    protected override void WndProc(ref Message m) { if (m.Msg == 0x21) { m.Result = (IntPtr)3; return; } base.WndProc(ref m); } // MA_NOACTIVATE
    protected override void Dispose(bool disposing) { if (disposing) dismiss.Dispose(); base.Dispose(disposing); }
}
