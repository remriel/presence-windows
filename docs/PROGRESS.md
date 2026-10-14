# Presence progress

Current objective: repair missing known/unknown devices, improve hostname/device-type identification, and publish Presence 1.2.1.
Progress: **100%** [####################]

- [x] Reconcile instructions, handoffs, Git state and source.
- [x] Fix normal-view visibility for uncertain known/person records and absent unidentified devices, including orphan person associations.
- [x] Establish Away silently for saved devices absent after restart/reconnect and healthy evaluated departure grace.
- [x] Add bounded reverse DNS, address validation, service-only mDNS handling and conservative likely hostname-derived types.
- [x] Isolate metadata from presence, last-seen time, alerts, logs, baseline, network scope and monitoring clock.
- [x] Pass core inference, transactional reset/persistence, restart absence and metadata-isolation checks.
- [x] Pass 16 native visibility/DNS checks with zero findings and 6 fixture screenshots.
- [x] Pass 24 existing bulk/reset/pause checks with zero findings and 12 fixture screenshots.
- [x] Inspect native light/dark screens; publish self-contained Windows x64 without warnings/errors.
- [x] Push and attach PR #3, verify final passing Windows CI, merge into main.
- [x] Rebuild at merged target; verify version, packaged smoke launch, extracted binary and ZIP checksum.
- [x] Publish 1.2.1 ZIP/checksum to GitHub and verify uploaded digest/tag target.
- [x] Synchronize final documentation and export both handoffs.

Implementation: released version 1.2.1. Original installed app/profile untouched. No requested source or release work remains.
PR: https://github.com/remriel/presence-windows/pull/3 (merged)
Release: https://github.com/remriel/presence-windows/releases/tag/v1.2.1
Release target: fb3d7573568945f3b8e5aca6375b8e71c71c0d85
Passing final PR CI: https://github.com/remriel/presence-windows/actions/runs/38491343616
Main-target CI: https://github.com/remriel/presence-windows/actions/runs/38491556806
Package: C:\Drive\2026-10-13\away\outputs\Presence-1.2.1-win-x64.zip
ZIP SHA-256: aeb15d862132dba7a38307e042452ec0e6b29597fd946d5150fc43d0dd61c2c4
File version: 1.2.1.0. Informational version includes release target fb3d757.

Blockers: none. Actual LAN hostname availability, physical restart/absence timing and sleeping phones remain manual acceptance. Fixture tests establish reproduced code behavior, not every possible environment or application bug.

Exact ordered next steps:
1. No requested implementation or release steps remain.
2. To install, quit the old tray process, extract the entire new package and launch Presence.exe while preserving %LOCALAPPDATA%\Presence\presence.db. Do not reset the profile.
3. Follow docs/MANUAL_ACCEPTANCE.md for real LAN checks.
4. Before future changes, read AGENTS.md and both handoffs and reconcile actual Git/source/tests. Final release records supersede historical checkout paths.
