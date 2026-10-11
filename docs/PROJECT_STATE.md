# Presence project state

## Current work
- Repository: https://github.com/remriel/presence-windows. Starting main e7a550ac43262f24ee2e21a533efc121ae3731ae; current feature branch feat/reset-and-bulk-devices.
- User authorized reset to defaults, main-UI multi-device Ignore/Set known, pause/resume icon, opt-in primary-phone checkbox 20% larger with warm orange/red/yellow coloring, and a new release.
- Version 1.2.0 source and self-contained Windows x64 publish are ready. Publication is pending final focused UI verification and GitHub synchronization.
- This checkout is C:\Drive\2026-10-11\ao\work\presence-windows. Earlier checkout/install/release statements in historical handoffs are not evidence of current local app state. We have not modified the installed app or the real user profile in this session.

## Architecture
- .NET 10 Windows Forms tray app, net10.0-windows10.0.19041.0; Windows 10 x64 1809 minimum. Self-contained single-file releases; do not trim WinForms/WinRT.
- Core Models.cs owns Snapshot, devices, people and Settings defaults. PresenceEngine.cs is deterministic inference/quiet-baseline state machine. Repository.cs stores snapshot JSON plus event and observation tables in SQLite.
- App PresenceContext.cs owns discovery cancellation epochs, persistence queues, tray, notifications and UI. Windows.cs holds forms, Ui.cs measured layouts/native theming, Discovery.cs fresh ARP/ICMP/mDNS observations.
- Dependencies: Microsoft.Data.Sqlite 10.0.12, SQLitePCLRaw.bundle_e_sqlite3 3.0.5, CommunityToolkit.WinUI.Notifications 7.1.2; bundled offline OUI table.
- Real data: %LOCALAPPDATA%\Presence\presence.db. Defaults: scan 3 seconds, departure grace 30 seconds, retention 90 days, automatic adapter, System theme, startup enabled.

## 1.2.0 decisions
- Repository.Reset atomically deletes State, PresenceEvent and Observation and writes an empty Snapshot with defaults. Clear observation throttling only after commit. Tests inject a SQLite trigger failure to prove all deletions roll back.
- PresenceContext.ResetEverything applies default startup preference with rollback on database failure, then increments epoch/cancels active scans, clears pending events/observations and alerts, clears runtime identities and resets quiet baseline. Reset resumes scanning even if manually paused. Confirmation defaults to No, describes irreversible deletion and startup default, closes Settings on success. Installing an update does not call reset.
- MainWindow uses an explicit Select devices mode and checkbox cells. MAC selections survive renders, clear after actions and on network-scope changes, and never apply outside current network. Person groups expand to individual devices, including Not yet seen devices. Row click/Space/Enter toggles selection; ordinary mode retains opening details by click/Enter.
- SetDeviceKinds permits Ignore/Known only. Both remove person/primary associations; explain this on-screen. Preserve names/history and reconcile people silently. Persistence failures roll back kinds, associations and person state in memory.
- Manual ScanningPaused is runtime-only and separate from Windows sleep. Toggle cancels scan epoch and discovery, stops reconnect and alerts, freezes absence timing. Scan refuses explicit and automatic scans while paused. Resume establishes quiet baseline; network/sleep changes cannot implicitly resume manual pause. System pause/play glyphs have accessible names/tooltips.
- Primary checkbox retains saved IsPrimary but no longer defaults true for unassigned devices. Font 12pt versus normal 10pt (20%), bold, padded, amber/orange background with dark readable text in light/dark themes. Existing labels and device editor association logic remain.
- No new art assets are needed for standard controls/system icon glyphs. Existing current-network/ignored semantics are preserved.

## Existing constraints
- Cached ARP is a probe candidate, never presence proof. Private MAC identity matching requires user association. Physical IPv4 networks only; IPv6/router-client integration remains future work.
- Ignore suppresses arrival/departure/new events and priority probes. Ignored rows remain visible with an Ignored label rather than asserting live presence.
- Network/sleep changes invalidate scan epochs, clear live scope and restart quiet baseline. Unknown is never a departure.
- Build on local disk via --artifacts-path to avoid Google Drive apphost mapped-file locks. Current local build root: %LOCALAPPDATA%\PresenceBuild\ResetBulk.
- Never commit real-device data, screenshots, credentials or diagnostics. No subagents. Fixture screenshots are in task outputs only.
- Native captures need shown forms, restored WindowState, and no forced layout after scrolling (it resets the docked content scroll position). A minimized main window produced invalid tiny captures in the expanded QA rerun; corrected in preview harness.

## Verification and limits
- Core inference and transactional reset/persistence tests passed. Self-contained production publish passed with no warnings/errors.
- Focused UI harness --reset-bulk tests cross-section mouse/Space selection, network exclusion, classifications, render selection retention, canceled/confirmed reset, default checkbox/font, pause/resume and stored defaults. Final corrected rerun passed 24 behavior checks with zero layout findings and 12 screenshots. Reset, primary control, pause icon and bulk light/dark/compact captures were visually inspected.
- Real Wi-Fi transitions, pause/resume during physical discovery, sleep/network switches, popup sound and long-term monitoring remain manual acceptance (docs/MANUAL_ACCEPTANCE.md). Fictional UI checks do not prove those behaviors.

## RESUME HERE
Finish the corrected --reset-bulk UI preview and inspect captures. Then package the existing publish, push the feature branch, create/merge PR after CI, and publish v1.2.0 with ZIP/checksum. User explicitly requested release; no further permission is required. Never reset the real profile during testing. Update docs/PROGRESS.md and this file with final release/CI links and hashes; export both to outputs.
