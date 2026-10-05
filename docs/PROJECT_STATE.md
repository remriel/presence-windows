# Presence project state

## Architecture
- .NET 10 Windows Forms tray app. Self-contained win-x64 single-file release.
- Presence.Core contains identity, inference, settings and SQLite repository. The app layer has LAN discovery, floating alerts, internet speed measurement and the native UI.
- Local device mappings/events stay at `%LOCALAPPDATA%\Presence\presence.db`.
- Presence discovery is IPv4-only, with fresh ARP, ICMP and multicast DNS. Known devices are prioritized and results stream to the UI as they arrive. A dedicated bounded worker pool keeps blocking ARP calls off the UI/CLR thread pool. A silent first scan forms the baseline; default scan is 3 sec and departure grace 30 sec. /24 and /23 networks are covered each pass; larger ranges rotate in bounded chunks.
- Sleep, adapter changes or a missing gateway reset monitoring without accruing absence. Gateway/proxy-ARP answers for unrelated IPs are excluded.
- User-requested devices are automatically tracked at first fresh response. Person assignment is optional and a primary phone controls optional person aggregation. Ignored devices never send events.
- Floating icon notices use a custom non-activating window and the owner's bundled Codex Usage Counter audio/icon assets. They operate independently of Windows toast settings and are kept local.
- User-facing theme follows current AGENTS: warm paper `#F7F6EF`, charcoal ink `#151821`, lemon action `#F4E54D`, coral `#F25B3D`, teal positive `#0E7C66`; dark canvas/surface `#151821` / `#242832` with `#F7F6EF` text, mint positive `#7AE5C5`, coral error `#FF8A76`. Thick outlined controls, compact responsive rows.

## Speed test
- `InternetSpeedTest.cs` and `SpeedTestWindow` are a separate internet performance tool started only by the bottom-bar **Speed test** button. The dialog auto-starts after the click and can be closed to cancel.
- The test uses HTTPS `speed.cloudflare.com/__down` and `/__up` for download/upload, four zero-byte timing requests for median HTTPS latency, and mean consecutive latency jitter. Samples ramp in parallel 128 KiB blocks until ~0.6 seconds per direction or 8 MiB; the complete test is capped at 35 seconds.
- No `__meta`, TURN or `__results` request is made by this implementation; results display only and are not saved. Cloudflare sees the public IP and test traffic. Cloudflare documents that its speed-test measurements are collected for aggregate connection-quality insights. Presence device IDs, MACs, hostnames, person mappings and history are never sent.
- The Cloudflare speed-test endpoint, measurement types and aggregation note were verified against [cloudflare/speedtest](https://github.com/cloudflare/speedtest). Run the test only when the owner wants a result.

## Persistence, build and release
- SQLite stores device/person state, presence events and throttled observations transactionally; events default to 90-day retention, observations to at most seven days.
- The repo contains a deterministic executable core test program and a Windows GitHub Actions workflow from the 1.0.2 scanner update. Its previously recorded run 37237103022 passed for that earlier code. Current user asked build-once; no new local tests were run.
- Build intermediates and publish output use `C:\Users\Gev\AppData\Local\PresenceBuild` because apphost generation on Google Drive hit a mapped-file lock once. Portable outputs go in task `outputs`.
- Repository is the private `remriel/presence-windows`. Latest prior release v1.0.2; current package is v1.0.3. The current local Presence installation/database are not modified by this GitHub release request.
- `--preview-image` creates a native screenshot of fictional demo data without scanning. Native Windows screenshot capture previously timed out, so use this app export for the requested screenshot.

## Proof boundary
- Current v1.0.3 Windows x64 production publish succeeded after fixing the missing `Presence.Core` namespace import in the speed-test dialog.
- No new automated tests or real Wi-Fi, Cloudflare speed-transfer, sound-listening or popup-click tests were performed. Upstream CI applies to older scanner code only. Physical Wi-Fi transitions and live internet speed remain manual owner acceptance.
- Cloudflare speed measurement is edge-based internet throughput/HTTP latency. It is not the Wi-Fi PHY link rate, router throughput or saved ISP diagnostic history.

## RESUME HERE

Presence v1.0.3 has been built, packaged, pushed, and published on GitHub. The update combines three-second local scans, streaming discovery, responsive low-churn UI and an on-demand Cloudflare internet speed test in the bottom bar. The test measures download/upload, median HTTPS latency and jitter; it sends test requests/public IP to Cloudflare only when clicked, and saves no results or Presence device data.

Do not run another build or tests without a new request. The local v1.0.1 install and existing database were not replaced. Runtime internet speed, Wi-Fi connect/disconnect, sleep, popup click/audio and long-duration behavior remain manual owner acceptance. Preserve the current source/ref and use `docs/MANUAL_ACCEPTANCE.md` for those steps.

Private repository: https://github.com/remriel/presence-windows
Release: https://github.com/remriel/presence-windows/releases/tag/v1.0.3
