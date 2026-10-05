# Presence project state

## Architecture and behavior
- C# / .NET 10 Windows Forms application, native tray and custom floating alerts; offline local discovery, SQLite persistence and a separate explicitly user-triggered internet speed-test service.
- Release target is Windows 10 x64, single-file and self-contained. Build intermediates under `%LOCALAPPDATA%\PresenceBuild` because Google Drive locks apphost files.
- Local-network discovery uses fresh ARP, ICMP and mDNS. The 3-second scan loop delivers known/recent devices first and streams observations to the UI. A dedicated bounded worker pool keeps blocking native ARP waits off the CLR/UI thread pool. /24 and /23 sweeps are covered each pass; larger networks rotate 256-target batches.
- `PresenceEngine.Apply` accepts one fresh response for arrivals; device absence accumulates using time actually spent monitoring and only for IPs that were evaluated. First successful scan is silent baseline. Default departure grace is 30 seconds.
- Network/power state changes cancel the current discovery scope, pause missed-time accumulation and force a fresh baseline. Local gateway/fresh-ARP checks continue; proxy-ARP gateway responses do not create devices.
- Local history/device/person/settings are SQLite-backed. Observation writes are batched/throttled; event changes save immediately; history pruning runs hourly, not on every fast refresh.
- Floating popups use native non-activating windows and embedded reference assets. They are independent of Windows notification banners.
- Main window lists stay cached until meaningful UI state changes; dynamic row widths track the form width; the bottom bar offers Speed test without obscuring the device list. Settings/device actions remain accessible in scrollable detail dialogs.

## Internet speed test
- `InternetSpeedTest.cs` connects to Cloudflare's public edge over HTTPS only after the user opens the bottom-bar Speed test. It times four `__down?bytes=0` responses, then runs concurrent download probes and upload probes against Cloudflare's documented `__down` and `__up` endpoints.
- Download/upload probes ramp from 128 KiB chunks until the sample runs about 600 ms or reaches an 8 MiB cap per direction. Whole test has a 35-second timeout and honors window-close cancellation.
- Displays estimated Mbps, median HTTPS round-trip latency and mean consecutive-sample jitter. This is an internet-edge estimate, not a Wi-Fi PHY or router throughput reading.
- Cloudflare's upstream `cloudflare/speedtest` README says its speed test collects measurements for aggregated internet-quality insights. Speed requests expose the normal public client IP and test traffic. Presence never includes MAC addresses, hostnames, person mappings, or stored local history, and stores no speed-test result. Privacy notice is shown in the speed-test window and README. No `/meta`, `/__results`, TURN, or third-party metadata request is sent by this implementation.
- Cloudflare documents `https://speed.cloudflare.com/__down` and `https://speed.cloudflare.com/__up`; requests use fixed endpoints, no auth token and no runtime package download.

## Visual system
- Per current AGENTS instructions, the palette is bold graphic neo-brutalist: warm-paper canvas `#F7F6EF`, charcoal ink `#151821`, lemon action `#F4E54D`, coral band/accent `#F25B3D`, teal positive `#0E7C66`; dark surfaces `#151821` / `#242832`, warm-white text `#F7F6EF`, mint positive `#7AE5C5`, coral error `#FF8A76`.
- Primary buttons have thick charcoal outlines; native lists remain compact and resize with the window.

## Release status and constraints
- User authorized a new v1.0.3 GitHub release. GitHub currently shows v1.0.2 as the latest published release.
- The single final production publish for v1.0.3 has succeeded after fixing one missing `Presence.Core` using directive. Do not rerun the build. Build-once instructions persist: no local tests/review/cleanup. Upstream CI run 37237103022 passed earlier scanner code/build; the new speed test and final scanner runtime are not behavior-tested.
- The present local stable install/database should remain unchanged; user asked for GitHub package/release.
- Project repo is private `remriel/presence-windows`; release 1.0.1 is historical.

## `RESUME HERE`
The v1.0.3 build and fictional native main-window preview are complete. Finish its portable/source/doc package, commit and push the pending merge, then create GitHub release/tag v1.0.3 and upload the package, source, docs and screenshot. Do not rerun the successful build or run tests. Tell the owner that internet speed-test and Wi-Fi acceptance remain unverified.

