# Presence project state

## Current release / RESUME HERE (2026-10-15)

Presence 1.2.2 is published: https://github.com/remriel/presence-windows/releases/tag/v1.2.2. Consolidated PR #5 is merged as bc644851863d47be3ac7eba10fe8feb8cf1bfaba, the verified release/tag target. It superseded closed PR #4 and includes its fixes plus save rollback, disconnected-adapter retention, bounded discovery and hidden-window work reductions. User explicitly authorized publication. No subagents, installation or production-profile changes.

Final PR Windows CI: https://github.com/remriel/presence-windows/actions/runs/38763761891 (passed). Merged-target release CI: https://github.com/remriel/presence-windows/actions/runs/38764156565 (passed; published assets). Version 1.2.2; README/download links and release notes updated. Release ZIP now includes README.md and THIRD_PARTY_NOTICES.md with Presence.exe and oui.csv.

Verified public archive: outputs/Presence-1.2.2-win-x64.zip, 57,472,969 bytes. SHA-256 aff8d0dea419910e00bd9141a378aaefbbf0ca78743b997477d7a8650cb10bd5 matches both the uploaded checksum and GitHub's asset digest. Tag points to the merged target. Extracted file version 1.2.2.0; product version 1.2.2+bc644851863d47be3ac7eba10fe8feb8cf1bfaba. Packaged fictional preview exits with code 0; screenshot at outputs/Presence-1.2.2-preview.png. The official public package replaces the earlier audit-preview build as the deliverable.

Active checkout: C:/Drive/2026-10-14/fix-all-bugs-and-optimize/work/presence-audit; branch main. Final documentation commits after bc64485 only record delivery; do not move the release tag. Both handoffs are exported under task outputs. Requested publication is complete; installation and physical acceptance remain optional future work. Preserve the production database; updates do not require reset.
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
