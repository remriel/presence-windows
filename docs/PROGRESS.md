# Presence progress

Current objective: publish Presence 1.2.2 from consolidated PR #5 and verify the public release package.
Progress: **100%** [####################]

- [x] Read instructions/handoffs and reconcile source, Git and GitHub.
- [x] Confirm PR #4 was superseded by consolidated PR #5, including save rollback and bounded discovery.
- [x] Set version 1.2.2, update README/release notes, and include README/third-party notices in ZIP packaging.
- [x] Pass final PR Windows tests and packaging: https://github.com/remriel/presence-windows/actions/runs/38763761891.
- [x] Merge PR #5 as bc644851863d47be3ac7eba10fe8feb8cf1bfaba with title Release Presence 1.2.2.
- [x] Pass merged-target Windows CI and publish v1.2.2: https://github.com/remriel/presence-windows/actions/runs/38764156565.
- [x] Download the public ZIP and checksum; match local SHA-256 to the manifest and GitHub asset digest.
- [x] Verify the tag target, extracted package files, binary version/source and clean packaged-preview exit (code 0).
- [x] Synchronize final repository handoffs and export both to outputs.

Release: https://github.com/remriel/presence-windows/releases/tag/v1.2.2
PR: https://github.com/remriel/presence-windows/pull/5 (merged)
Release/tag target: bc644851863d47be3ac7eba10fe8feb8cf1bfaba
ZIP: outputs/Presence-1.2.2-win-x64.zip (57,472,969 bytes)
SHA-256: aff8d0dea419910e00bd9141a378aaefbbf0ca78743b997477d7a8650cb10bd5
File version: 1.2.2.0
Product version: 1.2.2+bc644851863d47be3ac7eba10fe8feb8cf1bfaba
Screenshot: outputs/Presence-1.2.2-preview.png (fictional devices)
Checkout: C:/Drive/2026-10-14/fix-all-bugs-and-optimize/work/presence-audit; branch main.

Verification: deterministic core suites and 11 reliability checks; 16 native reliability, 16 visibility/DNS and 24 bulk/reset/pause checks; broader 26-capture smoke; self-contained publish/package; final release binary smoke. Both final PR and merged-target Windows CI pass. The public ZIP includes Presence.exe, oui.csv, README.md and THIRD_PARTY_NOTICES.md.
Blockers: none. Physical LAN, sleeping-device, suspend/resume and long-term resource behavior remain manual acceptance. Bounded batches can increase initial discovery time on large networks. No installation or real profile change was performed; existing data is preserved during updates.

Exact ordered next steps:
1. Requested release is complete; no publishing steps remain.
2. If installing, quit the old tray app, extract the complete 1.2.2 ZIP, and preserve %LOCALAPPDATA%/Presence/presence.db. Do not reset the profile.
3. Follow docs/MANUAL_ACCEPTANCE.md for physical network checks.
4. For future work, read AGENTS.md and both handoffs and reconcile actual Git/source/tests.
