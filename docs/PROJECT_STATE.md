# Presence project state

## Architecture
- .NET 10 Windows Forms tray app. Self-contained win-x64 single-file release.
- Presence.Core contains identity, inference, settings and SQLite repository. The app layer has LAN discovery, floating alerts, internet speed measurement and the native UI.
- Local device mappings/events stay at `%LOCALAPPDATA%\Presence\presence.db`.
- Presence discovery is IPv4-only, with fresh ARP, ICMP and multicast DNS. Known devices are prioritized and results stream to the UI as they arrive. A dedicated bounded worker pool keeps blocking ARP calls off the UI/CLR thread pool. A silent first scan forms the baseline; default scan is 3 sec and departure grace 30 sec. /24 and /23 networks are covered each pass; larger ranges rotate in bounded chunks.
- Sleep, adapter changes or a missing gateway reset monitoring without accruing absence. Gateway/proxy-ARP answers for unrelated IPs are excluded.
- User-requested devices are automatically tracked at first fresh response. Person assignment is optional and a primary phone controls optional person aggregation. Ignored devices never send events.
- Floating icon notices use a custom non-activating window and the owner's bundled Codex Usage Counter chime. They operate independently of Windows toast settings and are kept local. The v1.1.0 Presence icon is shared by the executable, title bars, tray and floating notices.
- User-facing theme follows current AGENTS: warm paper `#F7F6EF`, charcoal ink `#151821`, lemon action `#F4E54D`, coral `#F25B3D`, teal positive `#0E7C66`; dark canvas/surface `#151821` / `#242832` with `#F7F6EF` text, mint positive `#7AE5C5`, coral error `#FF8A76`. Thick outlined controls, compact responsive rows.

## Speed test
- `InternetSpeedTest.cs` and `SpeedTestWindow` are a separate internet performance tool started only by the bottom-bar **Speed test** button. The dialog auto-starts after the click and can be closed to cancel.
- The test uses HTTPS `speed.cloudflare.com/__down` and `/__up` for download/upload, four zero-byte timing requests for median HTTPS latency, and mean consecutive latency jitter. In v1.0.5, samples ramp from parallel 128 KiB blocks to 2 MiB blocks for at least 2.5 wall-clock seconds per direction (at least five seconds total); there is no total byte cutoff; the complete test is capped at 35 seconds.
- No `__meta`, TURN or `__results` request is made by this implementation; results display only and are not saved. Cloudflare sees the public IP and test traffic. Cloudflare documents that its speed-test measurements are collected for aggregate connection-quality insights. Presence device IDs, MACs, hostnames, person mappings and history are never sent.
- The Cloudflare speed-test endpoint, measurement types and aggregation note were verified against [cloudflare/speedtest](https://github.com/cloudflare/speedtest). Run the test only when the owner wants a result.

## Persistence, build and release
- SQLite stores device/person state, presence events and throttled observations transactionally; events default to 90-day retention, observations to at most seven days.
- The repo contains a deterministic executable core test program and a Windows GitHub Actions workflow from the 1.0.2 scanner update. Its previously recorded run 37237103022 passed for that earlier code. Current user asked build-once; no new local tests were run.
- Build intermediates and publish output use `%LOCALAPPDATA%\PresenceBuild` because apphost generation on Google Drive hit a mapped-file lock once. Portable outputs go in task `outputs`.
- Repository is the public `remriel/presence-windows`. The current published release is v1.1.1. Presence v1.1.1 is installed at C:\Presence and runs in the tray; data stays in `%LOCALAPPDATA%\Presence\presence.db`.
- `--preview-image` creates the main-window screenshot using fictional demo data without scanning. An off-screen Settings `DrawToBitmap` attempt produced a blank form; capture visible dialog controls if a Settings screenshot is needed.

## Proof boundary
- v1.0.3 Windows x64 production publish succeeded after fixing the missing `Presence.Core` namespace import in the speed-test dialog. The v1.0.4 Windows x64 self-contained production publish completed once. It adds recursive dialog theme application so default black label text uses the chosen readable theme; muted labels keep their muted color. The project version conflict markers were removed before publishing. The portable archive SHA-256 is `95291ce7e44ed4ae70b6bb4b71c07404c376a89c0019bf43e759a44819e7e270`.
- No automated tests or real Wi-Fi, Cloudflare speed-transfer, sound-listening or popup-click tests were performed. The GitHub push carried `[skip ci]` to honor the no-tests instruction. Physical Wi-Fi transitions and live internet speed remain manual owner acceptance. The v1.0.4 tray process was confirmed running from the new Downloads folder. The prior v1.0.3 executable process was stopped, but its extracted folder remains because the environment's automatic filesystem policy rejected deletion. The dark Settings dialog was not captured live; an off-screen `DrawToBitmap` attempt returned a blank image and should not be reused for dialog screenshots.
- Cloudflare speed measurement is edge-based internet throughput/HTTP latency. It is not the Wi-Fi PHY link rate, router throughput or saved ISP diagnostic history.

## Previous v1.0.4 handoff

Presence v1.0.4 has been built, packaged, pushed, published and installed to correct unreadable default-colored labels in the dark Settings and device dialogs. It carries forward three-second local scans, streaming discovery, responsive UI and the on-demand Cloudflare internet speed test in the bottom bar. The test measures download/upload, median HTTPS latency and jitter; it sends test requests/public IP to Cloudflare only when clicked, and saves no results or Presence device data.

The v1.0.4 release is running in the tray and the existing database is preserved. The old v1.0.3 process is stopped, but its downloaded folder remains because automatic deletion was blocked. Do not rebuild or run tests without a new request. Runtime internet speed, Wi-Fi connect/disconnect, sleep, popup click/audio and long-duration behavior remain manual owner acceptance. Use `docs/MANUAL_ACCEPTANCE.md` for those steps.

Repository: https://github.com/remriel/presence-windows
Release: https://github.com/remriel/presence-windows/releases/tag/v1.0.4


## v1.0.5 update
Speed measurement now runs at least 2.5 seconds per direction using a Stopwatch, with no 8 MiB early cutoff. Successful tests take at least five seconds plus latency/overhead. One production build passed; no tests or live speed transfer were run. The local v1.0.5 app is running from Downloads and Windows startup points to that version. Existing presence data remains in its original profile. Older extracted folders remain as previously documented.


## v1.0.5 handoff
Presence v1.0.5 was published at https://github.com/remriel/presence-windows/releases/tag/v1.0.5 and previously ran locally from Downloads\Presence-1.0.5-win-x64. Speed tests measure at least 2.5 seconds per direction, five seconds total, before completing. No automated or live network tests were run. Older folder deletion remains previously blocked; no deletion retries were made in this update.


## UI architecture in v1.1.0
- `Ui.cs` centralizes spacing, typography, semantic light/dark colors, wrap-aware text, two-column settings fields, action bars, tables and the auto-height content layout.
- `ScrollBody` contains one width-constrained vertical scroller; headers and actions remain visible. `ContentTable` measures auto rows at their allocated columns to avoid phantom whitespace and wrapped-label clipping.
- Main home/away/new-device rows and activity use selectable DataGridViews with state words, aligned timestamps and keyboard opening. `PresenceGrid` reapplies column widths and padding when DPI changes.
- Settings and device forms use grouped sections, shared fields and pinned Save/Cancel actions. Technical values wrap and provide a Copy value context action.
- Floating alerts wrap message text and adjust their height before positioning. Speed results, loading and errors use one measured layout.
- Forms have a 96-DPI `AutoScaleDimensions` baseline. The app manifest declares PerMonitorV2 awareness.
- `tools/Presence.UiPreview` renders fictional screen states; `tools/ui_contact_sheet.py` composes screenshot sheets. Capture artifacts stay under task `outputs`, outside the source tree.

## v1.1.0 handoff
Presence v1.1.0 now runs from C:\Presence, replacing the active 1.0.5 executable. The shared local database remains at %LOCALAPPDATA%\Presence\presence.db. The Windows Run entry points to the installed executable with --tray. Source and the built package are ready for the 1.1.0 GitHub release.

## v1.1.0 publication state
The icon update's single Windows x64 publish succeeded. The running app at `C:\Presence` reports file version 1.1.0.0; the Run key points to it with `--tray`. The existing presence database is preserved. The updated portable archive SHA-256 is `6069ee29aa682aea7eac0bb19c833a7af9c1498d3eb8a12b3d69e022fe5f716b`. GitHub reports that digest for the published ZIP at https://github.com/remriel/presence-windows/releases/tag/v1.1.0.

## RESUME HERE
v1.1.0 is published, the matching build runs locally, and the repository is public. No release work remains. The earlier handoff sections document history. If the owner later requests runtime acceptance, use `docs/MANUAL_ACCEPTANCE.md`; the build-once release did not run live Wi-Fi, sound or speed-transfer checks.



## Automatic network refresh in v1.1.1
- `PresenceContext` now listens to both address and availability events. Reset cancels a per-scan CTS (including gateway resolution), increments the UI epoch, clears live presence/network label and restarts after a 750 ms debounce. Exception handlers also check the epoch; an obsolete scan must not reset a newer connection.
- `PresenceEngine.ResetNetwork()` keeps identities, names, assignments, timestamps and history, but sets presence to Unknown and resets baseline/evaluation clocks. Reconnecting to the same network now establishes a silent baseline.
- `Discovery.Reset()` invalidates gateway discovery with a generation and releases the background-task reference. A separate lane identity includes interface index/local IP/gateway IP, so DHCP rebinding restarts multicast/ARP workers without changing the persisted stable network key.
- `MainWindow.Render()` filters every live section to `PresenceContext.NetworkScope`; its signature includes network scope, monitoring state and each device network. Offline/reconnecting state never displays stored devices from the previous network. All saved inventory/history remains local.
- `tools/Presence.UiPreview --network-change` captures three fictional transition screenshots. Do not use real device screenshots or commit local discovery data.
- The referenced chat confirms the existing installed location C:\Presence, public GitHub repository and preference for a focused single-build release without automated/live network tests. The original checkout at G:\My Drive\Codex\2026-10-04\build-a-minimalist-windows-10-desktop\work\Presence is clean at 802a246. This task uses a fresh clone under its work directory; reconcile/sync the original checkout after publication.

## RESUME HERE — v1.1.1 work
Source changes are implemented but not yet built. Follow docs/PROGRESS.md for build/package/publication/install. The earlier v1.1.0 handoffs are historical. Do not claim physical network switching is verified by fictional screenshots or a successful compile.

## v1.1.1 build evidence
The single production `dotnet publish` succeeded for win-x64 with self-contained/single-file settings. Build intermediates/output are under %LOCALAPPDATA%\PresenceBuild\NetworkRefresh-1.1.1. The screenshot helper rendered three fictional network-transition states into this task's outputs/network-refresh. The reconnect screen has zero live rows and no old network label; the next-network screen has only Example phone. No automated checks or physical network transitions were run. Packaging/publication/install remain in docs/PROGRESS.md.


## v1.1.1 publication and installation
- Public release: https://github.com/remriel/presence-windows/releases/tag/v1.1.1, targeting source commit ec5564a2a93f27ec3a7dc79a28ff22c2b55e4b65. GitHub reports both ZIP/checksum uploaded. ZIP SHA-256 is a9e67e044e4857c0dc417f3ef7be0abd801f0fba8b018b070959f55d27fea4a5 and matches the local archive.
- C:\Presence\Presence.exe now reports file version 1.1.1.0 and runs in the tray. The Windows Run entry still points to that executable with --tray. The existing %LOCALAPPDATA%\Presence\presence.db was not copied, moved or deleted. Previous executable/OUI files are backed up under %LOCALAPPDATA%\PresenceBuild\NetworkRefresh-1.1.1\previous-install.
- The binary was built before committing the release; its informational version includes the prior HEAD 802a246, while file/product numeric version is 1.1.1 and the release commit contains the built source changes.
- Failed approaches to avoid: GitHub release creation rejected the abbreviated commit SHA (target_commitish invalid); passing the full SHA worked. Immediate executable replacement briefly encountered a process file lock; explicitly waiting for exit and bounded copy retries resolved it. Do not launch after a failed copy; installation scripts must stop on errors.
- No automated tests, physical network switching, sleep/resume, popup sound or internet speed transfer were performed. [skip ci] commits preserve the focused single-build release workflow. Fictional captures are presentation evidence only.

## RESUME HERE — v1.1.1 complete
The automatic network refresh source is synchronized to GitHub main, v1.1.1 is public, and the matching numeric-version build runs from C:\Presence with the local data path preserved. This task's working clone is C:\Drive\2026-10-07\github-plugin-github-openai-curated-remote\work\presence-windows. The original clean G:\My Drive\Codex\2026-10-04\build-a-minimalist-windows-10-desktop\work\Presence checkout is being fast-forwarded to the final documentation commit. No release work remains. If the owner requests live acceptance, follow docs/MANUAL_ACCEPTANCE.md; particularly exercise a network change during gateway resolution and a same-LAN reconnect. Never reintroduce Pause-only reconnect resets, blank-scope show-all filtering or exception handlers without epoch guards.

## Ignored devices section — v1.1.2 in progress
- Owner clarified that the existing Ignore option is sufficient for excluding devices from notifications, but those devices must stay visible in a dedicated main-window section. Do not add a second mute/notification preference for this request.
- MainWindow.Render in src/Presence.App/Windows.cs reuses AddSection for counted, clickable ignored rows after Unknown devices, before Recent activity. Include all ignored devices in the current network irrespective of presence state; label them Ignored rather than implying a currently accurate Home/Away signal because Discovery deliberately skips priority probes of ignored devices.
- Respect the v1.1.1 live-network boundary: use the existing local collection and never show other-network or saved devices during reconnect. Saved inventory remains accessible through Settings > Devices.
- DeviceWindow explains visibility and how to restore normal tracking. DeviceKind.Ignore continues to suppress presence events/alerts; no persistence schema change is needed.
- Reconciled source currently uses the neutral Ui.cs light/dark design; the earlier palette paragraph is historical and does not describe the current colors. This feature uses the existing controls/layout without a redesign.
- This task's checkout is C:\Drive\2026-10-09\gi\work\presence-windows, on codex/show-ignored-devices. Clean starting commit is 3daa9f5. The installed app is running from C:\Presence\Presence.exe with --tray.

## RESUME HERE — ignored devices in progress
The section and copy changes are implemented, target numeric app version 1.1.2. Follow docs/PROGRESS.md for fictional verification, screenshots, production package, GitHub synchronization and local update. Earlier no-test/single-build handoffs describe prior releases; the current feature request permits focused verification. No real-device screenshot or discovery data belongs in source control.

## v1.1.2 verified package and installation
- Main-window visibility was verified with 15 focused fictional checks: present/away ignored rows, no duplicates, active-network filtering, row click opening the correct DeviceWindow, canceled/restored Ignore choices, SQLite reload, reconnect/new-network clearing, empty text, existing new/arrival/departure suppression and normal-event restoration. Existing Presence.Core checks also passed.
- One production Windows x64 self-contained/single-file publish succeeded with no warnings/errors. All intermediates are under %LOCALAPPDATA%\PresenceBuild\IgnoredDevices-1.1.2; a scratch verification harness uses the already-built assemblies and does not rebuild the app. No new mirrored unit test was added for the small UI change.
- Native fictional screenshots at this PC's 150% DPI show light, dark, compact main views and an ignored device's details. The compact layout scrolls vertically without clipping the ignored rows. Captures are under this task's outputs/ignored-devices; no real-device screenshots or diagnostic records were committed.
- Portable ZIP SHA-256: b2f60f709f9e90c6d99d7f2c91c3379460b286dcc9ebe873154112d2a05d789b. Package is C:\Drive\2026-10-09\gi\outputs\Presence-1.1.2-win-x64.zip with a .sha256 companion.
- C:\Presence\Presence.exe was backed up, replaced and restarted with --tray. File version is 1.1.2.0, executable SHA-256 b69fad432165b4d0a2ddf3c17ff83fbc060ee0ae6b854e567326a18c143be182. The startup command remains "C:\Presence\Presence.exe" --tray and the original profile remains %LOCALAPPDATA%\Presence\presence.db. Old binary/OUI backup is %LOCALAPPDATA%\PresenceBuild\IgnoredDevices-1.1.2\previous-install.
- The verified binary was built before committing this feature; numeric version is 1.1.2, while informational version contains the clean starting HEAD 3daa9f5. The feature source is the final reviewed branch diff.
- The existing state machine and Discovery behavior are unchanged. Ignored devices are not prioritized for probes, so their section uses Ignored rather than asserting a current Home/Away state. No person association, history or persistence migration was introduced.
- Remaining work is GitHub synchronization/PR and final handoff exports. v1.1.1 remains the published GitHub release; the local 1.1.2 package has not been published as a release. Real Wi-Fi, sound and long-run acceptance were not exercised.
