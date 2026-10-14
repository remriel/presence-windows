# Presence progress

Current objective: fix missing known/unknown records in the main UI and improve hostname/device-type resolution, including related discovery and state bugs.

Progress: **100% local verification** [####################]

- [x] Read instructions and handoffs; reconcile main ac304743 with source and Git.
- [x] Reproduce selection-only visibility and absent unknown filtering.
- [x] Keep uncertain recognized records visible; retain absent unidentified records; display orphan associations individually.
- [x] Establish Away silently for evaluated absent saved devices after restart/reconnect and the configured grace period.
- [x] Add bounded reverse DNS, matching-IP validation, service-only mDNS senders, conservative likely hostname-derived types.
- [x] Keep metadata separate from presence, observation logs, monitoring clock, baseline and network scope.
- [x] Pass core inference, transactional reset/persistence, restart absence and metadata-isolation checks.
- [x] Pass 16 focused native/DNS checks with zero findings and 6 fictional captures.
- [x] Pass 24 existing bulk/reset/pause checks with zero findings and 12 fictional captures.
- [x] Inspect light/dark main/detail screenshots; publish self-contained Windows x64 without warnings/errors.
- [x] Launch packaged binary in fictional preview mode; verify version and package checksum.
- [x] Push implementation, create and attach PR #3, add both native regression suites to Windows CI.
- [x] Export package and handoffs to current task outputs.

Implementation: version 1.2.1; commit 124c702cb65f049fbf4cbaa3556f2cb17fb381dc. Branch fix/device-visibility-identification. Application source fixes are complete. Installation and real profile untouched. No public release created or main-branch merge performed.

Repository: https://github.com/remriel/presence-windows
PR: https://github.com/remriel/presence-windows/pull/3
CI: inspect the current Windows check on PR #3; source validation runs independently on GitHub.
Package: C:\Drive\2026-10-13\away\outputs\Presence-1.2.1-win-x64.zip
SHA-256: 8702ed41a8e30f7ecbc65e095d93c009b1654586310928cf07fc62c3f90942c4
File version: 1.2.1.0. Informational version: 1.2.1+124c702cb65f049fbf4cbaa3556f2cb17fb381dc.

Blockers: none for the verified local build. Physical LAN hostname availability, restart absence timing, Wi-Fi sleep/reconnect and actual installed-app behavior remain manual acceptance; fixtures cannot prove those environmental behaviors. No claim that every possible app bug is eliminated.

Exact ordered next steps:
1. Check the current Windows CI result on PR #3 before merging or releasing.
2. If installing, quit the old tray process, preserve %LOCALAPPDATA%\Presence\presence.db, extract the entire portable package and launch Presence.exe. No reset is needed.
3. Follow the 1.2.1 section in docs/MANUAL_ACCEPTANCE.md on the user's real LAN.
4. Continue future work from docs/PROJECT_STATE.md and actual Git/source/tests.
