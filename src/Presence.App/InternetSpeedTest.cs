using System.Diagnostics;
using System.Net.Http.Headers;
using Presence.Core;

namespace Presence.App;

internal sealed record InternetSpeedResult(double DownloadMbps, double UploadMbps, double LatencyMs, double JitterMs);

/// <summary>
/// User-triggered internet speed measurement. Sends only throwaway test bytes to Cloudflare;
/// never includes Presence devices, names, MAC addresses, telemetry, or history.
/// </summary>
internal sealed class InternetSpeedTest : IDisposable
{
    private const string BaseUrl = "https://speed.cloudflare.com/";
    private readonly HttpClient client = new(new SocketsHttpHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.None,
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        ConnectTimeout = TimeSpan.FromSeconds(8)
    }) { Timeout = Timeout.InfiniteTimeSpan, BaseAddress = new Uri(BaseUrl) };

    public async Task<InternetSpeedResult> RunAsync(IProgress<string>? progress, CancellationToken cancellationToken)
    {
        var token = cancellationToken;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
        deadline.CancelAfter(TimeSpan.FromSeconds(35));
        var ct = deadline.Token;
        try
        {
            progress?.Report("Measuring latency…");
            var samples = new double[4];
            for (var i = 0; i < samples.Length; i++) samples[i] = await MeasureLatencyAsync(ct);
            var sorted = samples.Order().ToArray();
            var median = (sorted[1] + sorted[2]) / 2d;
            var jitter = Enumerable.Range(1, samples.Length - 1).Select(i => Math.Abs(samples[i] - samples[i - 1])).Average();

            progress?.Report("Measuring download speed…");
            var downloadMbps = await MeasureBandwidthAsync(upload: false, ct);

            progress?.Report("Measuring upload speed…");
            var uploadMbps = await MeasureBandwidthAsync(upload: true, ct);
            return new(Math.Round(downloadMbps, 1), Math.Round(uploadMbps, 1), Math.Round(median, 1), Math.Round(jitter, 1));
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            throw new TimeoutException("The Cloudflare speed test took longer than 35 seconds. Try again when your connection is less busy.");
        }
    }

    private async Task<double> MeasureBandwidthAsync(bool upload, CancellationToken ct)
    {
        var minimumDuration = TimeSpan.FromSeconds(2.5);
        long transferred = 0; var blockSize = 128 * 1024;
        var watch = Stopwatch.StartNew();
        while (watch.Elapsed < minimumDuration)
        {
            ct.ThrowIfCancellationRequested();
            var pair = await Task.WhenAll(TransferAsync(blockSize, upload, ct), TransferAsync(blockSize, upload, ct));
            transferred += pair.Sum(x => x.Bytes);
            blockSize = Math.Min(blockSize * 2, 2 * 1024 * 1024);
        }
        watch.Stop();
        var elapsed = watch.Elapsed.TotalSeconds;
        if (elapsed <= 0) throw new IOException("The speed-test server returned no measurements.");
        return transferred * 8d / elapsed / 1_000_000d;
    }

    private async Task<double> MeasureLatencyAsync(CancellationToken ct)
    {
        var watch = Stopwatch.StartNew();
        using var response = await client.GetAsync("__down?bytes=0", HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var one = new byte[1];
        _ = await stream.ReadAsync(one.AsMemory(), ct);
        watch.Stop();
        return watch.Elapsed.TotalMilliseconds;
    }

    private async Task<(long Bytes, TimeSpan Elapsed)> TransferAsync(int size, bool upload, CancellationToken ct)
    {
        var watch = Stopwatch.StartNew();
        using var request = new HttpRequestMessage(upload ? HttpMethod.Post : HttpMethod.Get,
            upload ? "__up" : "__down?bytes=" + size.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (upload)
        {
            var payload = new byte[size];
            System.Security.Cryptography.RandomNumberGenerator.Fill(payload);
            request.Content = new ByteArrayContent(payload);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        }
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        var actual = 0L;
        await using (var stream = await response.Content.ReadAsStreamAsync(ct))
        {
            var buffer = new byte[64 * 1024]; int read;
            while ((read = await stream.ReadAsync(buffer.AsMemory(), ct)) != 0) actual += read;
        }
        watch.Stop();
        var transferred = upload ? size : actual;
        if (transferred <= 0) throw new IOException("The speed-test server returned no data.");
        return (transferred, watch.Elapsed);
    }

    public void Dispose() => client.Dispose();
}

internal sealed class SpeedTestWindow : Form
{
    private readonly CancellationTokenSource cancel = new();
    private readonly WrapLabel status;
    private readonly Button start;
    private readonly InternetSpeedTest speedTest = new();
    private readonly Dictionary<string, WrapLabel> values = [];
    private readonly ProgressBar progressBar;
    private readonly Settings settings;
    private bool resourcesDisposed;
    public SpeedTestWindow(Settings settings, bool autoStart = true)
    {
        this.settings = settings;
        Ui.Configure(this, "Internet speed", new Size(500, 510), new Size(420, 400), settings);
        var body = new ScrollBody();
        status = Ui.Label("Ready to measure your internet connection.", settings); status.Margin = new Padding(0, 0, 0, Ui.Space); Ui.Add(body.Content, status);
        progressBar = new ProgressBar { Dock = DockStyle.Top, Height = 12, Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 30, Visible = false, Margin = new Padding(0, 0, 0, Ui.Section) }; Ui.Add(body.Content, progressBar);
        var metrics = Ui.Fields();
        foreach (var name in new[] { "Download", "Upload", "Latency", "Jitter" })
        {
            var value = Ui.Label("—", settings); value.Font = Ui.HeadingFont; values[name] = value; Ui.Field(metrics, name, value, settings);
        }
        Ui.Add(body.Content, metrics);
        var note = Ui.Label("Download and upload each measure for at least 2.5 seconds. Latency and jitter describe the responsiveness of this connection.", settings, true); note.Margin = new Padding(0, Ui.Space, 0, Ui.Section); Ui.Add(body.Content, note);
        Ui.Add(body.Content, Ui.Label("Cloudflare receives your public IP and test traffic. Presence sends no device data and does not save these results.", settings, true));
        start = Ui.Button("&Run speed test", async () => await RunTest(), settings, true);
        var close = Ui.Button("&Close", Close, settings); close.DialogResult = DialogResult.Cancel; AcceptButton = start; CancelButton = close;
        Ui.Shell(this, Ui.Header("Internet speed", "Measure download, upload, latency and jitter on demand.", settings), body, Ui.Actions(start, close));
        FormClosed += (_, _) => { cancel.Cancel(); speedTest.Dispose(); };
        if (autoStart) Shown += async (_, _) => await RunTest();
    }
    internal void ShowProgress(string message)
    {
        status.Text = message; status.ForeColor = Ui.Text(settings); progressBar.Visible = true;
        start.Enabled = false; start.Text = "Testing…";
    }
    internal void ShowResult(InternetSpeedResult result)
    {
        values["Download"].Text = $"{result.DownloadMbps:0.0} Mbps"; values["Upload"].Text = $"{result.UploadMbps:0.0} Mbps";
        values["Latency"].Text = $"{result.LatencyMs:0} ms"; values["Jitter"].Text = $"{result.JitterMs:0.0} ms";
        status.Text = "Test complete"; status.ForeColor = Ui.Positive(settings); Finish();
    }
    internal void ShowError(string message)
    {
        status.Text = "Speed test could not finish.\n" + message; status.ForeColor = Ui.Negative(settings); Finish();
    }
    private void Finish() { progressBar.Visible = false; start.Enabled = true; start.Text = "&Run again"; }
    private async Task RunTest()
    {
        if (cancel.IsCancellationRequested || !start.Enabled) return;
        foreach (var value in values.Values) value.Text = "—";
        ShowProgress("Starting speed test…");
        var progress = new Progress<string>(message => { if (!IsDisposed && !cancel.IsCancellationRequested) ShowProgress(message); });
        try { var result = await speedTest.RunAsync(progress, cancel.Token); if (!IsDisposed) ShowResult(result); }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested) { }
        catch (Exception ex) { if (!IsDisposed) ShowError(ex.Message); }
        finally { if (!IsDisposed && !cancel.IsCancellationRequested) Finish(); }
    }
    protected override void Dispose(bool disposing) { if (disposing && !resourcesDisposed) { resourcesDisposed = true; cancel.Cancel(); speedTest.Dispose(); cancel.Dispose(); } base.Dispose(disposing); }
}
