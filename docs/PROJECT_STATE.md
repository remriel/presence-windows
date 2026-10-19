# Presence project state

## RESUME HERE — Presence 1.5.1 larger timeline window (complete, 2026-10-19)

Objective: make the timeline open large enough to show more devices without scrollbars, publish Presence 1.5.1, and replace both portable copies on this PC while preserving the local profile. All requested work is complete.

Implementation: `PresenceTimelineWindow` opens maximized, has a larger restored size, and allows maximize/restore. The timeline-specific header and footer use less vertical space. Daily/hourly `DataGridView`s recalculate columns and row heights after first display and on resize; rows retain a readable minimum and full device names remain available in tooltips. No database or timeline-calculation changes were needed.

Rationale and failed approaches: a larger fixed window and reduced toolbar spacing still left a scrollbar in the 20-device fixture at 144 DPI. The initial daily grid was also measured before the maximized layout settled. Refit on first display and resize, then size rows to the available viewport. Lists larger than the screen's readable capacity can still require scrolling.

Verification: the native fictional timeline preview passed 39 checks with zero findings and six light/dark captures at 1942×1056, 144 DPI. Twenty device rows fit without vertical or horizontal scrollbars across the tested daily/hourly ranges. PR #8 merged as `f6813837d00e77ca9fca09a846a6b057abfd30c0`; main-branch Windows CI [39299052655](https://github.com/remriel/presence-windows/actions/runs/39299052655) passed all behavior, UI and packaging steps. Release [v1.5.1](https://github.com/remriel/presence-windows/releases/tag/v1.5.1) is published. The official ZIP is 57,489,917 bytes; SHA-256 `9091e23de7b350bf79d8c8e33d4f1fce952168fdb382a574988b8ce9e2c0d55c` matches both the checksum asset and GitHub's asset digest. Its EXE reports 1.5.1.0, and the packaged fictional preview exported successfully.

PC update: both portable copies were replaced from the verified package and their executable hashes match it. The Windows startup entry remains unchanged; the active copy restarted as 1.5.1.0. A consistent local SQLite backup was made before update. Integrity checks passed before and after; saved device configurations/associations, people, saved networks, settings and retained event history were present afterward. No reset or schema migration was needed. Machine-specific paths, profile identifiers, counts and diagnostics remain private and are not committed.

### RESUME HERE

No required steps remain. The 20-device screenshot and fit check apply to this PC's 144-DPI, 1942×1056 display. Larger device lists or a smaller display may still need scrolling. Keep the demo screenshots fictional and preserve the local profile when updating future releases.

## Completed release — Presence 1.5.0 hourly timeline (2026-10-19)

Objective: add a rolling last-24-hours view to the device timeline, publish Presence 1.5.0, and update the copies on this PC without resetting the profile.

Baseline: public `main` and tag `v1.4.0` were at `cf8cdfa` when work began. Current work is on `feature/timeline-24-hour`. The existing daily views and 14-day default remain intact.

Implementation: `PresenceTimeline.BuildHourly` reconstructs exactly 24 one-hour elapsed-time bins from saved arrival/departure events and the current device state. Bins are calculated in UTC and rendered in local time. Each column tooltip shows the complete local interval and UTC offsets, so repeated or skipped clock labels around daylight-saving transitions remain distinguishable. No database/schema changes or retention changes are required. Unknown state stays neutral.

UI: the timeline range bar has a `24h` choice alongside 7/14/21/28 days. The hourly view displays one-hour cells, readable three-hour local-time ticks, and per-cell duration/coverage tooltips. Screenshot fixtures contain fictional devices only.

Verification: PR #7 merged as `25e999d538cd5cfae9e7f289b4e2bd3e04bd65a9`; main-branch Windows CI run [39294576888](https://github.com/remriel/presence-windows/actions/runs/39294576888) passed all behavior, UI, and packaging steps. Tag `v1.5.0` is published at that commit. The official 57,489,466-byte ZIP matches both its checksum file and GitHub asset digest: `6035dcd934c33754b7f46a2603732edad66ddd8c0c9c949fe501b79e0a6c50e1`. Its EXE reports 1.5.0.0, and a packaged fictional preview exported successfully. Local regression suites also passed: core; timeline (23); saved networks (11); visibility (16); reliability (16); broad UI (26 captures); bulk/reset/pause (24). The self-contained local Release build completed with zero warnings/errors.

PC update: both portable copies were replaced from the verified public package, and their executable hashes match. The startup command still targets the active copy. The app restarted as version 1.5.0.0. The existing local profile passed SQLite integrity checks before and after; saved state and event history remained, with one additional event recorded during normal monitoring after restart. No schema change or reset was needed. Machine-specific paths, hashes, process identifiers, profile counts and diagnostics remain private workspace context and must not be committed to this public repository.

### 1.5.0 delivery completion

The requested 24-hour timeline, 1.5.0 publication, and PC replacement are complete. No required steps remain. For optional future real-LAN acceptance, compare the hourly estimate against a known device reconnect and a sleeping-device interval; presence remains a network-response estimate, not proof that a person is home. Keep all device identifiers and real-device screenshots out of Git.

## Prior release context — Presence 1.4.0 (2026-10-18)

Completed prior feature: per-device daily presence graph, published as Presence 1.4.0. Its 7/14/21/28-day ranges, 14-day default, existing event-history estimate, focused UI previews and screenshot remain the baseline for the 1.5.0 hourly extension below.

Implementation: main-window Timeline action opens a local, current-network device heatmap with 7/14/21/28-day ranges. Daily color estimates present time from saved arrival/departure events plus last-seen/state-change timestamps; unknown/away time is neutral. Tooltips show duration and state coverage. Existing event retention is unchanged. No schema migration or reset is needed. Person reconciliation is silent at every production call site, so aggregate person transitions are not persisted as device-MAC events. Fictional preview screenshot at `docs/images/presence-timeline.png`.

Verification so far: core timeline reconstruction/history checks pass; native fixture preview passes 15 timeline checks in light/dark themes, including all ranges, date tooltips, no horizontal clipping and empty state. Final local suites pass: timeline (15), saved networks (11), visibility (16), reliability (16), broad UI (26 captures, zero layout findings), bulk/reset/pause (24), and core. Self-contained x64 build and fictional packaged preview pass; final Release build has zero warnings/errors. GitHub release and PC update remain pending. Preserve `%LOCALAPPDATA%/Presence/presence.db`; never include it or real-device screenshots in Git.

PC update context at the time of the 1.4.0 handoff (2026-10-18): inspection reported a portable installation under `%LOCALAPPDATA%/Programs/Presence`, live profile under `%LOCALAPPDATA%/Presence`, and a process/startup target under a build output. This is historical and was superseded by the direct 1.5.0 session inspection above, which found the active startup target and a second identical portable copy; the exact machine path remains private.

## Previous completed release — Presence 1.3.0

## RESUME HERE — Presence 1.3.0 published (2026-10-15)

Release: https://github.com/remriel/presence-windows/releases/tag/v1.3.0
Tag/implementation: 717586a1b8683e2a79d602e179b82fe1f8be4b7c. Windows CI succeeded: https://github.com/remriel/presence-windows/actions/runs/38808885693. Public portable ZIP downloaded and verified against checksum file AND GitHub asset digest: 3e72a3b026ec8665377c9e1e5de777b29fce6c43fa7cb0daf2720ec1b0d9c484; 57,474,482 bytes. Packaged fictional preview exit 0. ZIP includes Presence.exe, oui.csv, README.md and THIRD_PARTY_NOTICES.md. All local and CI suites passed. Requested feature and publication are complete. Do not move the release tag for documentation-only changes. No installation was requested or performed this session; the existing 1.2.2 installation/profile remains separate. Physical SSID permissions/reconnect acceptance remains manual. Handoffs and the public package exported to C:/Drive/2026-10-15/x/outputs.

## Saved networks implementation context

Completed objective: implement remember/save network with SSID as default name and publish Presence 1.3.0. User explicitly requested the feature and release. No subagents. Active checkout: C:/Drive/2026-10-15/x/work/presence-windows. Previous 1.2.2 installation is not part of this request; do not modify the live profile or install without a request.

Implementation: Snapshot.Networks holds SavedNetwork(scope, name) in the existing SQLite snapshot JSON. Legacy snapshots initialize an empty list. NetworkNames validates names and upserts by exact scope. Existing identity is adapter ID + subnet/prefix + gateway MAC; unchanged to preserve existing device isolation and profiles. SSID is a display default only and must never merge identities. Changing adapter/router/subnet may require saving again. Duplicate display names on distinct scopes are allowed.

NetworkIdentity.Ssid matches WinRT connection profiles by selected adapter GUID and reads GetConnectedSsid. Unauthorized/COM/unsupported failures fall back to adapter/subnet; no location permission prompt or password collection. Discovery caches SSID with adapter enumeration and propagates it in full/partial ScanResult, capturing the name per lane. PresenceContext updates saved labels on results, saves/forgets with rollback, rejects stale editors on network change, and retains device/person/history records when forgetting. Reset clears names with the snapshot. Windows.cs adds Networks entry point and dialog with current name, save/rename, saved list and forget. Offline names can be forgotten; renaming requires that network connected.

Verification so far: native build zero warnings/errors; core tests including Unicode/default fallback, name validation, legacy load, duplicate-name isolation, persistence across database reopen and reset pass. New --saved-networks native fixture suite: 11 checks, zero layout findings across light/dark, saved and disconnected captures, including persistence, stale save rejection and SQLite-trigger-injected save/forget rollback. Final local layout and regression checks pass: 26 broad captures, 16 reliability, 16 visibility/DNS and 24 bulk/reset/pause checks, zero findings. Full release CI and public package verification passed. Physical Wi-Fi permission/reconnect checks remain manual; fictional previews do not prove live SSID availability.
## Current release / RESUME HERE (2026-10-15)

Presence 1.2.2 is published: https://github.com/remriel/presence-windows/releases/tag/v1.2.2. Consolidated PR #5 is merged as bc644851863d47be3ac7eba10fe8feb8cf1bfaba, the verified release/tag target. It superseded closed PR #4 and includes its fixes plus save rollback, disconnected-adapter retention, bounded discovery and hidden-window work reductions. User explicitly authorized publication and subsequently requested updating their PC. No subagents. Presence 1.2.2 is now installed and running; the existing saved profile is preserved.

Final PR Windows CI: https://github.com/remriel/presence-windows/actions/runs/38763761891 (passed). Merged-target release CI: https://github.com/remriel/presence-windows/actions/runs/38764156565 (passed; published assets). Version 1.2.2; README/download links and release notes updated. Release ZIP now includes README.md and THIRD_PARTY_NOTICES.md with Presence.exe and oui.csv.

Verified public archive: outputs/Presence-1.2.2-win-x64.zip, 57,472,969 bytes. SHA-256 aff8d0dea419910e00bd9141a378aaefbbf0ca78743b997477d7a8650cb10bd5 matches both the uploaded checksum and GitHub's asset digest. Tag points to the merged target. Extracted file version 1.2.2.0; product version 1.2.2+bc644851863d47be3ac7eba10fe8feb8cf1bfaba. Packaged fictional preview exits with code 0; screenshot at outputs/Presence-1.2.2-preview.png. The official public package replaces the earlier audit-preview build as the deliverable.

Active checkout: C:/Drive/2026-10-14/fix-all-bugs-and-optimize/work/presence-audit; branch main. Final documentation commits after bc64485 only record delivery; do not move the release tag. Both handoffs are exported under task outputs. Requested publication and PC installation are complete. Program-file and consistent SQLite backups were created locally before replacing the existing executable in place. Post-restart checks verified one running 1.2.2 instance, unchanged startup configuration, SQLite integrity, and preservation of saved devices/names/classifications/people/primary associations/settings/all existing event history. Physical acceptance remains optional future work. Private backup paths, raw diagnostics and all real device data stay outside Git; local exported handoffs record the backup location. Updates do not require reset.
## Architecture and constraints

- .NET 10 Windows Forms tray app, net10.0-windows10.0.19041.0; Windows 10 x64 1809 minimum. Self-contained single-file packaging; never trim WinForms/WinRT.
- Core Models.cs owns snapshots/settings/identities; PresenceEngine.cs is deterministic presence inference and quiet baseline; Repository.cs stores snapshot JSON, events and physical observations in SQLite.
- App PresenceContext.cs coordinates UI-thread state, discovery epochs/cancellation, pending writes, tray/alerts, startup, pause/reset and shutdown. Discovery.cs handles physical adapter selection, fresh ARP, ICMP, mDNS and background loops. HostnameResolver.cs performs bounded validated DNS enrichment.
- Windows.cs holds forms; Ui.cs measured layouts/theming; FloatingAlerts.cs focus-preserving popup queue and embedded WAV; InternetSpeedTest.cs explicitly triggered Cloudflare measurements.
- Dependencies: Microsoft.Data.Sqlite 10.0.12, SQLitePCLRaw.bundle_e_sqlite3 3.0.5, CommunityToolkit.WinUI.Notifications 7.1.2; bundled offline IEEE OUI table.
- Real profile: %LOCALAPPDATA%/Presence/presence.db. Demo/fixtures are separate. Defaults: scan 3 seconds, departure grace 30 seconds, retention 90 days, System theme, automatic adapter, startup enabled.
- Physical IPv4 /16 through /30 only. Cached ARP supplies candidates/identity, never presence proof. Private MAC changes require user association. IPv6/router integration remains future work.
- No subagents. Never commit real devices/screenshots, credentials or local diagnostics. Fictional captures belong in task outputs only.

## Audit decisions and rationale

- Choose each MAC's newest effective sample per batch. Session-local accepted timestamps reject replays without trusting persisted LastSeen after clock correction. Clear on pause/network reset/clock-gap baseline. Repeated progress samples do not revive Away or reset absence.
- Only completed checks advance the global health clock. Partial observations cannot hide long monitoring gaps. Long per-device evaluation gaps do not count as monitored absence while other devices remain healthy.
- Repository.Save records one newest physical observation per MAC per transaction, excludes identity-only metadata and updates throttle memory only after commit. Backwards clocks cannot indefinitely suppress logging. Computed DisplayName/DeviceType are omitted from JSON; existing source fields remain compatible.
- Settings use a copy and roll back settings/startup preference after failed persistence. Device edits restore names, kinds, associations, displaced primary flags and person states, removing uncommitted people. Bulk rollback is retained. A stale editor cannot restore reset records.
- Disconnected selected adapters stay visible/selected; unrelated saves cannot silently choose Automatic. SettingsWindow accepts a fixture adapter list for deterministic tests.
- Known-device/neighbor priority probes remain independent. Background ARP and ICMP each rotate through at most 512 addresses per batch, waiting the configured interval. ICMP concurrency is 32 instead of 128. This bounds resource demand but can increase first discovery latency on large LANs.
- Background loops run off UI context. Per-lane observation dictionaries isolate obsolete work. ARP workers start on first use, so idle/demo contexts create no ARP threads. Diagnostic discovery is disposed.
- Hidden/minimized main windows defer inventory rendering until shown. Broad previews use Presence.ProductIcon instead of deleted Presence.AlertIcon. Repeated context disposal/Exit are safe.
- Demo saves persist only to fictional profiles so injected SQLite failures exercise real save paths. CI includes reliability and broad smoke alongside core, visibility, bulk/reset/pause and packaging.

## Earlier decisions retained

- Reset is atomic across State/PresenceEvent/Observation/defaults; failure rolls back. Context reset rolls back startup on failure, invalidates discovery, clears pending writes/alerts, resumes and quietly baselines. Installing never resets data.
- Bulk selection retains checked MACs across renders, clears on scope change and stays on current network. Ignore/Set known clear person/primary association but preserve names/history. Ignored devices remain listed and never notify.
- Manual pause is runtime-only, cancels scans and freezes absence; network/power changes cannot resume it. Resume quietly baselines. Primary phone is opt-in, highlighted and 20% larger; it determines person presence, otherwise any assigned presence device can keep a person home. Aggregation does not duplicate alerts.
- Saved uncertain devices stay under Checking presence and become Away silently after healthy evaluated absence. Unknown devices remain listed while absent. Orphan person associations fall back to individual rows; other networks stay excluded.
- DNS/mDNS enrichment cannot create devices, establish presence, change timestamps/monitoring clocks or notify. DNS is bounded to 16 candidates/four concurrent, 1.2-second timeout, five-minute retry and forward validation. Delayed results must match MAC/IP/scope. Type hints never change saved choices.
- Published v1.2.1 remains at fb3d7573568945f3b8e5aca6375b8e71c71c0d85: https://github.com/remriel/presence-windows/releases/tag/v1.2.1. Original ZIP SHA-256 aeb15d862132dba7a38307e042452ec0e6b29597fd946d5150fc43d0dd61c2c4. Historical handoffs remain in Git history.

## Verification, failed approaches and limits

- Original core suites and 11 added reliability checks pass. Tests reproduced stale revival, old ordering, monitoring-gap, duplicate-write and clock failures before fixes.
- Native: 16 reliability, 16 visibility/DNS and 24 bulk/reset/pause; zero findings. Broad: 26 captures, zero layout findings. Total 47 native fixture captures plus packaged preview. Representative light/dark/error screens inspected.
- Release/native build and self-contained publish: zero warnings/errors. ZIP digest, extracted EXE equality, informational version and packaged launch verified. Implementation CI passes all suites/packaging.
- Use local --artifacts-path to avoid Drive apphost/mapped-file locks. Current roots: %LOCALAPPDATA%/PresenceBuild/Reliability/artifacts and ReliabilityPackage. Slow shared-compiler startup was bypassed using UseSharedCompilation=false and nodeReuse:false. Running is not passing.
- Do not restore continuous full-subnet sweeps, use persisted LastSeen alone for replay detection, count partial callbacks as completed monitoring or retain failed edits.
- Capture shown/restored forms; do not force layout after scrolling, which resets scroll position. Physical DPI is recorded; other scaling values are simulations.
- Physical Wi-Fi reconnect/sleep latency, proxy ARP/client isolation, large networks, suspend/resume and long-term resource use require MANUAL_ACCEPTANCE.md. Fixtures cannot establish that every environmental bug is absent.

## Next steps

Presence 1.2.2 publication is complete. For installation, quit the old tray process and replace the full portable package while preserving the production database. No implementation or publication step remains. Future agents must read AGENTS.md and both handoffs, inspect Git status/diff and relevant tests, and reconcile notes against the repository.

## Release merge history (2026-10-15)
- Final release PR Windows CI passed: https://github.com/remriel/presence-windows/actions/runs/38763761891. PR #5 merged as bc644851863d47be3ac7eba10fe8feb8cf1bfaba with commit title Release Presence 1.2.2, triggering the existing main release workflow.
- Checkout now on main at that merged target. Release metadata sets 1.2.2; ZIP packaging includes README.md and THIRD_PARTY_NOTICES.md. No installation/profile changes.
- Completed: main CI passed, v1.2.2 was published, public assets were downloaded and verified, and final handoffs were synchronized/exported. Current release details at the top supersede this intermediate record.
