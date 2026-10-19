# Presence progress

Current objective: add a rolling 24-hour hourly view to the Presence timeline, publish Presence 1.5.0, and replace the PC copies without resetting the profile.

Progress: **70%** — implementation, local regression/build/package checks, and the fictional screenshot are complete; GitHub CI/publication and PC replacement remain.

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
- [ ] Push the feature branch, open the release PR, wait for Windows CI, and merge through the release workflow.
- [ ] Verify the public v1.5.0 ZIP, asset digest, checksum file, version, and packaged launch.
- [ ] Quit the running 1.4.0 instance and update both portable copies while preserving the local Presence database and current startup command.
- [ ] Relaunch the app, verify its version and profile integrity, then finalize project/shared-memory handoffs.

Current implementation is on `feature/timeline-24-hour`, based on `v1.4.0` / `cf8cdfa`. PC inspection found a running 1.4.0 instance and a second identical portable copy; startup targets the running copy. Preserve the standard local Presence database. Keep machine-specific paths, process IDs, hashes, profile sizes, real-device details, and diagnostics out of the repository.
