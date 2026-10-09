# Presence project state

## Current result and repository
- Public repository: https://github.com/remriel/presence-windows. Current branch: main. PR https://github.com/remriel/presence-windows/pull/1 is merged as e7a550ac43262f24ee2e21a533efc121ae3731ae.
- Presence 1.1.2 is built, packaged and running from C:\Presence\Presence.exe with --tray. Windows startup points to the same path. Published release: https://github.com/remriel/presence-windows/releases/tag/v1.1.2, targeting merge commit e7a550ac43262f24ee2e21a533efc121ae3731ae. ZIP and checksum are uploaded; GitHub reports the same ZIP digest as the verified local package.
- This checkout: C:\Drive\2026-10-09\gi\work\presence-windows. Starting main was 3daa9f5644eb3adfc0d2ab527c1e02a2023c6364. Feature commit: 3e53b471eb31ef7d492133b4692fedd66ade4d38. Earlier working copies are historical; do not infer their current state from old handoffs.
- Never use subagents unless explicitly requested. Follow AGENTS.md and current user instructions. Never commit real-device data/screenshots, credentials or local diagnostic records.

## Architecture and dependencies
- .NET 10 Windows Forms tray app; target net10.0-windows10.0.19041.0, supported minimum Windows 10 x64 1809. Releases are self-contained single-file win-x64 executables; do not trim WinForms/WinRT.
- Presence.Core/Models.cs holds identities, DeviceKind, settings and events. PresenceEngine.cs is the deterministic inference state machine. Repository.cs stores a JSON Snapshot in SQLite plus transactional event/observation tables.
- Presence.App/Discovery.cs supplies local network observations. PresenceContext.cs owns tray/UI lifecycle, persistence, notification dispatch and reconnects. Windows.cs contains MainWindow, SettingsWindow, DeviceWindow and inventory/activity dialogs. Ui.cs centralizes measured layouts, colors, typography, scrolling and grids.
- Dependencies: Microsoft.Data.Sqlite 10.0.12, SQLitePCLRaw.bundle_e_sqlite3 3.0.5, CommunityToolkit.WinUI.Notifications 7.1.2. Local IEEE OUI snapshot provides offline manufacturer lookup; no runtime vendor download.
- Local user data stays at %LOCALAPPDATA%\Presence\presence.db. Transactions preserve state/events; detailed observations are throttled. Defaults: 90-day event retention, at most seven days of observations.

## Presence and reconnect decisions
- Only fresh ARP, ICMP and multicast DNS responses count as presence evidence. Cached ARP entries supply probe addresses, not proof of presence. Private MAC identities cannot be matched safely without user association.
- Automatic physical-adapter selection favors Wi-Fi, or Settings can choose an explicit adapter. Physical IPv4 LANs are supported; IPv6-only devices and router-assisted client tables are not implemented.
- Known devices receive priority probes, with background subnet/mDNS discovery. First fresh detection tracks a device automatically. One silent successful baseline prevents startup/reconnect notification bursts. Default interval is three seconds; departure grace is 30 seconds.
- A primary phone determines an associated person's presence; without one, assigned person devices aggregate. Normal device transitions generate one event per device; person aggregation does not add duplicate notifications.
- Missing gateways, sleep, address/availability changes or source-address changes reset monitoring without counting unmonitored time as absence. PresenceContext cancels per-scan CTS, increments an epoch and rejects obsolete results and exceptions, clears the label/list and debounces reconnect for 750 ms.
- PresenceEngine.ResetNetwork keeps identities, names, assignments, timestamps and history while resetting live state and the quiet baseline. Discovery uses a generation for gateway work and a lane key including interface/source/gateway IP, so DHCP rebinding restarts workers even on the same stable persisted network identity.
- MainWindow.Render scopes every live section to PresenceContext.NetworkScope. Blank scope does not mean show all. Full saved inventory/history stays accessible from Settings > Devices.

## Ignored devices decision in v1.1.2
- Owner clarified that the existing Ignore option is sufficient for excluding devices from notifications, but ignored devices must remain visible on the main window. No second mute preference or persistence migration was introduced.
- Windows.cs/MainWindow.Render adds a counted Ignored devices section after Unknown devices and before Recent activity. It includes all DeviceKind.Ignore devices in the active network, irrespective of presence state, sorted by DisplayName.
- Ignored rows open the existing DeviceWindow by click or Enter. Save another tracking type to restore normal tracking. Cancel preserves Ignore. The empty section and device-details copy explain the behavior.
- Discovery intentionally excludes ignored devices from priority probes. Rows therefore say Ignored rather than asserting an accurate current Home/Away signal. Existing PresenceEngine Ignore suppression prevents new/arrival/departure events and alerts; existing histories remain local.
- Kind is already in the MainWindow render signature, so app.Save/Refresh updates placement immediately. Devices from the old network clear during reconnect and never leak into a new network's ignored section.

## UI, alerts and internet speed
- Current Ui.cs uses the existing neutral light/dark native interface. This focused feature reuses its measured controls and spacing. Historical palette notes did not describe the current source colors.
- ScrollBody provides one width-constrained vertical scroller; pinned headers/actions remain visible. ContentTable measures wrapped labels at allocated widths. PresenceGrid refreshes widths/padding on DPI changes. Forms use a 96-DPI baseline and PerMonitorV2 awareness.
- FloatingAlerts.cs uses app-owned, non-activating icon popups and a bundled chime, independent of Windows toast/banner settings. Popups wrap text, stay above ordinary windows, pause dismissal while hovered and open the device when clicked. Quiet hours/global event switches suppress real notifications while retaining ordinary recorded events. Test alert bypasses quiet hours and follows saved sound.
- Alert queue is bounded to 20, with overflow summarized. Existing app/tray/popup icons are embedded; no new visual assets were required.
- InternetSpeedTest.cs runs only after the Speed test action. HTTPS Cloudflare __down/__up samples run for at least 2.5 seconds per direction, with median HTTPS latency/jitter and a 35-second deadline. No results or Presence identities/history are saved/sent. This is internet edge throughput, not Wi-Fi PHY rate or router throughput. Speed testing was not exercised for this change.

## Build, verification and local package
- One local Windows x64 self-contained/single-file production publish passed with no warnings/errors. Existing deterministic Presence.Core checks passed. Build intermediates/output: %LOCALAPPDATA%\PresenceBuild\IgnoredDevices-1.1.2.
- A scratch preview harness at this task's work/ignored-preview referenced the already-built assemblies rather than rebuilding the production app. Fifteen checks passed with zero findings: present/away ignored rows, no duplicates, other-network exclusion, row-click editing, Save/Cancel, SQLite reload, reconnect/new-network clearing, empty text, existing new/arrival/departure suppression and restoration of normal events.
- Light, dark, compact and ignored-device details captures were visually inspected at native Windows 150% DPI. Screenshots and verification.json are in task outputs/ignored-devices and contain only fictional devices. Layout checks found no clipped labels or horizontal form scrolling.
- GitHub Actions run https://github.com/remriel/presence-windows/actions/runs/37976713773 passed core checks, Windows publish, packaging and artifact upload for feature commit 3e53b47. Documentation follow-ups do not change the verified app source.
- Portable ZIP: C:\Drive\2026-10-09\gi\outputs\Presence-1.1.2-win-x64.zip; SHA-256 b2f60f709f9e90c6d99d7f2c91c3379460b286dcc9ebe873154112d2a05d789b, with a .sha256 companion.
- Installed executable is version 1.1.2.0; SHA-256 b69fad432165b4d0a2ddf3c17ff83fbc060ee0ae6b854e567326a18c143be182. The matching process, path, startup command and original profile existence were verified. Prior binary/OUI files are backed up at %LOCALAPPDATA%\PresenceBuild\IgnoredDevices-1.1.2\previous-install. Profile data was not copied, moved or deleted.
- Numeric binary version is 1.1.2; informational version includes the pre-commit starting HEAD 3daa9f5 because the single local publish preceded the feature commit.

## Constraints, failed approaches and remaining proof
- Build to local disk using --artifacts-path: apphost generation on the Google Drive workspace once hit a mapped-file lock. User-facing portable packages/captures go in task outputs; never commit binaries or profiles.
- An off-screen Settings DrawToBitmap previously produced a blank capture. Show the fictional form, process its layout/messages, then capture visible native controls. Do not use real-device screenshots.
- GitHub release creation previously rejected an abbreviated commit target; use a full commit SHA if a later release is authorized. Immediate binary replacement once hit a process lock; back up files, wait for exit and use bounded copy retries. Stop on copy errors and restore the backup before restarting.
- Real Wi-Fi join/leave/sleep, isolation/proxy ARP, rapid physical network switching, popup sound/click and long-duration discovery reliability still require owner acceptance. Fictional UI checks and compilation do not prove physical LAN behavior. See docs/MANUAL_ACCEPTANCE.md.
- No unresolved implementation bug was found in the ignored-section change. Broader priorities remain discovery reliability, adaptive probing, sleeping-device departure confidence, IPv6/router options and observable diagnostics, as described in README.

## RESUME HERE
Presence 1.1.2 is released, the feature is merged to GitHub main, and the matching local build is installed at C:\Presence. Release: https://github.com/remriel/presence-windows/releases/tag/v1.1.2. The portable ZIP/checksum upload and archive digest were confirmed. No implementation, build, installation or publication work remains. The owner explicitly requested immediate release; use the already-verified package without extra rebuilds/tests. Preserve the current-network filter and the Ignored display label. Future physical LAN acceptance remains in docs/MANUAL_ACCEPTANCE.md.
