# Presence project state

## Active audit implementation (2026-10-14)
- Branch: `fix/reliability-performance`; baseline `531ad4e`; current checkout is under this task's `work/presence-windows`.
- Core fixes: choose newest observation per MAC, reject timestamp replays, advance global health clock only for completed scans, and exclude long per-device evaluation gaps from absence. Repository excludes metadata-only observations, deduplicates physical evidence per save and omits computed display fields from snapshot JSON. Eight new core regressions pass after seven reproduced initial failures.
- Discovery: rotating 512-address budgets for ARP and ICMP, 32 concurrent ICMP operations (formerly 128), cadence follows configured scan interval, per-lane observation dictionaries isolate obsolete work, background work runs off UI context, and ARP threads start lazily. Priority known-device probes remain independent. Report actual foreground coverage rather than claiming the full subnet was probed.
- UI: settings use a copy and commit only after persistence, restoring previous settings/startup preference on failure. Device edits restore all affected primary flags, associations and person states on failure. Disconnected adapter choices survive saving. Hidden/minimized main windows skip inventory rendering until shown. The broad preview harness uses the correct embedded icon.
- Demo saves now persist only in their separate fictional profiles so injected SQLite failures exercise the same save path. No production profile is used for fixture checks.
- RESUME HERE: run `--reliability`, `--device-visibility`, `--reset-bulk` native suites; inspect fixture screenshots; publish and smoke-test portable package; push PR and verify Windows CI. Record final results and export both handoffs. Local build startup is slow; shared compiler bypass is in use. Earlier release completion records below are historical.

## Current work (2026-10-14)
- User request: fix bugs and optimize https://github.com/remriel/presence-windows. No subagents authorized.
- Active checkout: `C:\Drive\2026-10-14\fix-all-bugs-and-optimize\work\presence-windows`; branch `fix/reliability-performance`; clean baseline `531ad4e`.
- Existing core checks pass locally with .NET SDK 10.0.402. Build artifacts belong on local disk under `%LOCALAPPDATA%\PresenceBuild\Reliability` to avoid mapped-file issues on Drive.
- Audit focus: discovery scheduling/cancellation, partial observations and false departures, transactional settings/device updates, hidden-window rendering and persistence overhead, malformed local-network inputs.
- Discovered mismatch: README says bounded 512-address sweeps, but `Discovery.BackgroundDiscovery` repeatedly ARP-probes the entire subnet and sends up to 4096 ICMP probes per pass. Verify and repair with bounded rotating batches and independent priority probes.
- RESUME HERE: baseline checks passed; finish reading UI editing paths and harness, reproduce concrete issues, add focused regression checks and implement fixes. Current progress is in PROGRESS.md; older completion statements below describe previous work.

## Previous release
- Objective: repair missing known/unknown devices in the main UI and improve hostname/type identification.
- Repository: https://github.com/remriel/presence-windows. Checkout: C:\Drive\2026-10-13\away\work\presence-windows. Branch: main; implementation commit 124c702cb65f049fbf4cbaa3556f2cb17fb381dc.
- PR: https://github.com/remriel/presence-windows/pull/3. Source version and portable build: 1.2.1. Release 1.2.1 is published at https://github.com/remriel/presence-windows/releases/tag/v1.2.1; PR #3 is merged. The installed app has not been replaced.
- Verified package: C:\Drive\2026-10-13\away\outputs\Presence-1.2.1-win-x64.zip. SHA-256 aeb15d862132dba7a38307e042452ec0e6b29597fd946d5150fc43d0dd61c2c4. Packaged file version 1.2.1.0; informational version includes release target fb3d7573568945f3b8e5aca6375b8e71c71c0d85.
- Core checks passed; focused native suite 16 checks / zero findings / 6 captures; existing bulk/reset/pause suite 24 checks / zero findings / 12 captures. Self-contained publish and fictional packaged preview succeeded. Final PR Windows CI passed at https://github.com/remriel/presence-windows/actions/runs/38491343616. The main-branch run validates the merged target independently.
- Historical 1.2.0 release and older checkout records below describe earlier work. Current installed app and real profile have not been modified.
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
Requested work is complete. Presence 1.2.0 is published, PR #2 merged, CI passed, and ZIP digest verified. Release target: 02afe990131156b989422e93e87b9d88c3e03242. Package: C:\Drive\2026-10-11\ao\outputs\Presence-1.2.0-win-x64.zip; SHA-256 01af0efb2eee95d5ad87f559ef3638d73cc5acee302128f4842692d099a84c19. Binary SHA-256 e85635decc424ccf96a7665a23b0edae1c30a4dbfd8a8bcad3ebd0d9be512fb2; file version 1.2.0.0. Informational version includes starting HEAD b646e15 because publish preceded feature commit. A packaged-app demo smoke capture is in outputs/presence-verification/packaged-app-smoke.png. No installation or real-profile reset was performed. If installation is requested, preserve the original user data and use the published ZIP. Both handoffs are exported to outputs. Future physical LAN testing follows MANUAL_ACCEPTANCE.md.

## 2026-10-13 device visibility and identification fix
- Active checkout: C:\Drive\2026-10-13\away\work\presence-windows; starting main ac304743. Branch fix/device-visibility-identification. Earlier release paths above are historical.
- User reports saved known/unknown devices appear in Select devices but disappear from normal main UI; also requests hostname/device-type resolution and related bug fixes.
- Root causes: Unknown-kind rows were filtered by live state/consecutive responses; uncertain recognized rows existed only in selection mode; ResetNetwork clears presence but evaluated absent saved devices never graduated from Unknown; orphan Person-kind records were silently omitted.
- Main UI retains unidentified records on current network with actual presence labels; pending known/person groups have Checking presence; orphan associations fall back to individual rows. Existing grouping, Ignore and network boundaries retained.
- PresenceEngine silently establishes Away for saved devices after evaluated healthy absence reaches configured grace, including after restart. Partial discoveries cannot infer absence; no false departure alert for Unknown to Away.
- Background hostname enrichment: current mDNS hostnames plus bounded reverse DNS (16 candidates, 4 concurrent, 1.2-second lookup timeout, 5-minute negative retry, scope-local cache). DNS forward results must include observed IP. Metadata-only results preserve timestamps and presence, cannot create devices, cannot revive absent devices, and cannot overwrite real pending observation logs. Ignore identity remains MAC-based.
- Service-only mDNS response senders now count as observed even without an A record; hostname remains empty until useful evidence is available.
- DeviceIdentification infers conservative likely categories from hostnames (phone, tablet, computer, TV/streaming, printer, speaker, console, NAS, network equipment). Manufacturer alone does not determine type. Inference does not change DeviceKind or user names/associations.
- Added native fixture regression mode --device-visibility and injected DNS success/mismatch/failure/timeout/cancellation checks. CI runs it and existing --reset-bulk suite before publishing.
- Real LAN reverse DNS/mDNS availability and physical absent-device timing remain manual validation. No installed binary or real profile changed.
- RESUME HERE: Finish final verification/publish, synchronize branch/PR with GitHub, export final handoffs and package. Do not claim all conceivable application bugs are fixed; report reproduced visibility bugs and tested identification behavior.

## Final verification status (2026-10-13)
- Core inference, SQLite reset/rollback/persistence, restart absence, identity-only stale-IP/new-identity/network/clock isolation and type-inference checks pass.
- Final focused native suite: 16 passed checks, zero layout findings, six light/dark fixture captures. DNS tests cover matching-address hostname, mismatch, numeric/control labels, socket failure, timeout and external cancellation.
- Existing bulk/reset/pause suite: 24 passed checks, zero layout findings, twelve fixture captures.
- Final Release build and self-contained Windows x64 publish passed without warnings/errors. Version 1.2.1. Screenshots remain in task outputs, not committed.
- Identity-only enrichment now returns before changing the monitoring clock/baseline/scope and is excluded from persisted physical observation logs.
- RESUME HERE: Source and local verification are ready for PR. Real LAN hostname availability remains environment-dependent and was not exercised; no real data or installed app modified. Final PR/CI/package references follow in the exported handoffs.

## Final deliverable / RESUME HERE
- Local implementation and verification are complete; PR #3 and the 1.2.1 portable package are available. Current work summary at the top supersedes historical checkout/release records.
- Packaged application was smoke-tested in fictional preview mode. No real profile or installed app was touched.
- Before merging/releasing, inspect the latest Windows CI check on PR #3. For installation/physical acceptance, follow docs/MANUAL_ACCEPTANCE.md. Future agents must not treat fixture tests as proof of real-LAN hostname availability or installed-app behavior.

## 2026-10-13 release completion / RESUME HERE
- User explicitly requested the new release. PR #3 merged (squash) as fb3d7573568945f3b8e5aca6375b8e71c71c0d85. Current branch main; previous PR/packaging-only statements above are historical.
- Presence 1.2.1 published: https://github.com/remriel/presence-windows/releases/tag/v1.2.1. Tag target verified as the merged commit. ZIP and checksum uploaded; GitHub ZIP digest matches the tested local archive.
- Final release rebuilt at merged target; file version 1.2.1.0, informational version 1.2.1+fb3d7573568945f3b8e5aca6375b8e71c71c0d85. Packaged fictional smoke launch passed; extracted binary equals tested publish.
- ZIP SHA-256: aeb15d862132dba7a38307e042452ec0e6b29597fd946d5150fc43d0dd61c2c4. Package size 57,455,617 bytes.
- Final PR Windows CI passed at https://github.com/remriel/presence-windows/actions/runs/38491343616. Main merged-target CI at https://github.com/remriel/presence-windows/actions/runs/38491556806 is independent and can be inspected.
- Requested fixes and release are complete. Original installation/profile unchanged. If installing, quit old tray app, replace extracted binaries, preserve presence.db; no reset needed. Physical LAN hostname availability and timing remain manual acceptance.
- Final merged release-target Windows CI also passed: https://github.com/remriel/presence-windows/actions/runs/38491556806. Source, release assets, tag, package digest and final repository documentation are synchronized; working tree is clean.
