# Presence progress

Current objective: add a rolling 24-hour hourly view to the Presence timeline, publish Presence 1.5.0, and replace the PC copies without resetting the profile.

Progress: **100%** — the hourly view is implemented, tested, published as 1.5.0, and installed on this PC without resetting the saved profile.

Checklist:

- [x] Reconcile the repository, release status, existing project handoff, and actual PC install paths.
- [x] Add 24 one-hour bins covering the last 24 elapsed hours while retaining the 7/14/21/28-day views and 14-day default.
- [x] Reconstruct hourly estimates from existing events and saved device state without a schema or retention change.
- [x] Add focused core coverage and native light/dark timeline previews; pass 23 preview checks with zero findings.
- [x] Review the fictional 24-hour screenshot and fix clipped hour labels with three-hour ticks.
- [x] Update the app version to 1.5.0, README, and release notes.
- [x] Run all local release regression suites: core; timeline (23 checks); saved networks (11); visibility (16); reliability (16); broad UI (26 captures); bulk/reset/pause (24 checks), all with zero findings.
- [x] Save the reviewed fictional screenshot in `docs/images` and task outputs.
- [x] Complete the warning-free self-contained x64 publish; local ZIP is 57,489,505 bytes with SHA-256 `fde8813adc00943d5f5e3bfd16059c9bf612c80475cc71fff5ec8075c17a0539`.
- [x] Verify the local EXE version is 1.5.0.0 and launch the packaged fictional preview successfully.
- [x] Push the feature branch, merge PR #7 after Windows CI passed, and publish tag `v1.5.0` through the release workflow.
- [x] Verify the public v1.5.0 ZIP and checksum file against GitHub's asset digest; EXE version is 1.5.0.0 and the packaged fictional preview succeeded.
- [x] Replace both portable copies from the public package; both installed EXE hashes match it, and the startup command is unchanged.
- [x] Relaunch Presence from the startup target; confirm version 1.5.0.0 and SQLite integrity `ok` before and after, with existing saved state/history retained.
- [x] Finalize project state/progress, project `WISDOM.MD`, and shared AI Brain handoff.

Release commit: `25e999d538cd5cfae9e7f289b4e2bd3e04bd65a9` on `main`, tag `v1.5.0`. Windows CI run [39294576888](https://github.com/remriel/presence-windows/actions/runs/39294576888) passed. Official ZIP: 57,489,466 bytes; SHA-256 `6035dcd934c33754b7f46a2603732edad66ddd8c0c9c949fe501b79e0a6c50e1`.

No blockers or required next steps remain. Optional future work is real-LAN acceptance for reconnects and sleeping devices. Keep machine-specific install paths, process IDs, hashes, profile counts, real-device details, and diagnostics out of the public repository.
