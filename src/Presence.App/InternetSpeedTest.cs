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
        const long byteLimit = 8 * 1024 * 1024;
        long transferred = 0; var elapsed = 0d; var blockSize = 128 * 1024;
        while (transferred < byteLimit && elapsed < 0.6)
        {
            var size = (int)Math.Min(blockSize, (byteLimit - transferred) / 2);
            var pair = await Task.WhenAll(TransferAsync(size, upload, ct), TransferAsync(size, upload, ct));
            transferred += pair.Sum(x => x.Bytes);
            elapsed += pair.Max(x => x.Elapsed.TotalSeconds);
            blockSize = Math.Min(blockSize * 2, 2 * 1024 * 1024);
        }
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
    private readonly Label status;
    private readonly Button start;
    private readonly InternetSpeedTest speedTest = new();

    public SpeedTestWindow(Settings settings)
    {
        Text = "Presence · Internet speed"; ClientSize = new Size(365, 270); MinimumSize = new Size(365, 270);
        MaximumSize = new Size(365, 270); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog;
        AutoScaleMode = AutoScaleMode.Dpi; Font = new Font("Segoe UI", 10); BackColor = Ui.Background(settings); ForeColor = Ui.Text(settings);
        var title = new Label { Text = "INTERNET SPEED", Location = new Point(18, 16), Width = 325, Height = 40, Font = new Font("Segoe UI", 16, FontStyle.Bold), BackColor = Ui.Lemon, ForeColor = Ui.Ink, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0) };
        status = new Label { Text = "Measures download, upload, latency and jitter.", Location = new Point(23, 66), Width = 315, Height = 92, AutoEllipsis = true, TextAlign = ContentAlignment.TopLeft, BackColor = Ui.Surface(settings), ForeColor = Ui.Text(settings), Font = new Font("Segoe UI", 11, FontStyle.Bold) };
        var disclosure = new Label { Text = "ON DEMAND · Cloudflare receives your public IP, test traffic and speed results for aggregate insights.", Location = new Point(23, 171), Width = 315, Height = 45, Font = new Font("Segoe UI", 8), ForeColor = Ui.Muted(settings) };
        start = new Button { Text = "Run speed test", Location = new Point(204, 220), Size = new Size(134, 34), FlatStyle = FlatStyle.Flat, BackColor = Ui.Lemon, ForeColor = Ui.Ink, Font = new Font("Segoe UI", 9, FontStyle.Bold) }; start.FlatAppearance.BorderSize = 3; start.FlatAppearance.BorderColor = Ui.Ink;
        start.Click += async (_, _) => await RunTest();
        Controls.AddRange([title, status, disclosure, start]); AcceptButton = start;
        FormClosed += (_, _) => { cancel.Cancel(); speedTest.Dispose(); cancel.Dispose(); };
        Shown += async (_, _) => await RunTest();
    }

    private async Task RunTest()
    {
        if (cancel.IsCancellationRequested || !start.Enabled) return;
        start.Enabled = false; start.Text = "Testing…";
        var progress = new Progress<string>(message => { if (!IsDisposed) status.Text = message; });
        try
        {
            var result = await speedTest.RunAsync(progress, cancel.Token);
            if (!IsDisposed) status.Text = $"Download     {result.DownloadMbps:0.0} Mbps\nUpload          {result.UploadMbps:0.0} Mbps\nLatency          {result.LatencyMs:0} ms\nJitter               {result.JitterMs:0.0} ms";
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested) { }
        catch (Exception ex) { if (!IsDisposed) status.Text = "Speed test failed.\n" + ex.Message; }
        finally { if (!IsDisposed && !cancel.IsCancellationRequested) { start.Enabled = true; start.Text = "Run again"; } }
    }
}
