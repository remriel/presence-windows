# Current objective — color-only timeline, 2026-11-06

Progress: **100%** — removed the markers, passed focused checks, published 1.11.1 and installed it on this PC. The additional full Windows CI run was still in progress at handoff.

- [x] Reconcile instructions, shared memory, repository and installed state.
- [x] Remove J/S letters, outlines, legend text, accessibility references and marker model while retaining the lighter-is-more color scale.
- [x] Verify native light/dark and rolling/daily views and refresh behavior; inspect fictional screenshots.
- [x] Publish 1.11.1, verify the official package, back up and update this PC, and verify its profile and startup.
- [x] Update project/shared handoffs and wisdom.

Current state: [1.11.1](https://github.com/remriel/presence-windows/releases/tag/v1.11.1) is published from `fbfb6a28574fca13c4e65d74037bf5d679cce124` and running from `C:\Presence`. Its ZIP matches the checksum and GitHub digest. Core checks and 181 native timeline checks passed. The saved profile and startup entry were verified after the update. Private program/profile backups were made.

Blockers: none. The old app was stopped directly after a recent SQLite write because Windows input was previously denied; a few seconds of buffered observations could have been lost. The full [Windows CI run](https://github.com/remriel/presence-windows/actions/runs/42332381558) was still in progress. Next: optionally check that run and inspect the installed timeline with a real device.

# Previous objective — timeline colors and highlights, 2026-11-06

Progress: **100%** — implemented, verified, published and installed on the active PC; profile and startup checks passed.

- [x] Read instructions, shared memory, source, tests, release metadata and clean Git state.
- [x] Make more presence progressively lighter in both themes and all timeline ranges.
- [x] Highlight the current device's last-seen time when away or last-joined time when present, with meaningful tooltips and accessible labels; update open timelines on changes.
- [x] Run core and native visual regressions and inspect fictional screenshots.
- [x] Pass Windows PR CI and publish release 1.11.0 from the existing release workflow.
- [x] Back up this PC's program and profile, install the verified official release, and confirm saved state and startup behavior.
- [x] Finalize project and shared-memory handoffs.

Current state: [Presence 1.11.0](https://github.com/remriel/presence-windows/releases/tag/v1.11.0) is published from merge commit `b1d85dee83209215d6873fd7b653c5107ea67c52`; PR and release Windows CI passed. Its official ZIP matches its checksum and GitHub digest, and the packaged fictional preview passed. This PC runs the matching 1.11.0 executable from `C:\Presence` with startup unchanged. The consistent post-update profile passes integrity, saved device/person/network/settings and in-retention history comparisons. Local native timeline checks passed at 144 DPI (203 checks) and simulated 96 DPI (201), along with the other local regressions.

Blockers: none. Windows denied app input, so the previous process was stopped directly after a recent SQLite write; a few seconds of buffered observations may have been lost. Installed timeline interaction and physical reconnect/sleep behavior remain unverified.

Next: no required steps. Optional acceptance is to inspect the installed timeline and compare its marker times with an actual device departure and rejoin.

# Previous objective — Presence 1.10.0 timeline and release, 2026-11-05

Progress: **60%** — intervals, stronger colors, version and combined release notes updated; focused timeline regression and packaged preview pass. PR/CI, release and installs remain.

- [x] Re-read project/shared instructions, state, Git status, memory and current release metadata.
- [x] Set 1h/2h/3h timelines to five-minute intervals; preserve 4h/6h and hourly/daily ranges.
- [x] Increase occupied/away color contrast in light/dark and add visual contrast regression checks.
- [x] Pass focused timeline regression: 145 checks, zero findings, 20 fictional captures.
- [ ] Pass all core/native regressions and inspect fictional timeline previews.
- [x] Pass self-contained Windows x64 publish and packaged fictional launch (1.10.0.0, exit 0).
- [ ] Push a PR and pass Windows CI; merge with release title `Release Presence 1.10.0`.
- [ ] Verify release assets/checksum; back up/update REMY and NOVA; finalize project/shared handoffs.

Current base: `main` at `ca7bb02`; branch is `release/presence-1.10.0`. PR #12's reset, vendor CSV and stale person-assignment fixes are already on `main`. Latest release is v1.9.0; v1.10.0 is not published. REMY's `C:\Presence` v1.9.0 and profile are located, unchanged. NOVA now pings and RDP/SMB service ports respond; its admin share denies access, so installation/profile are not inspected yet.

Blockers: none. Physical LAN reconnect/sleep behavior remains unverified.

Ordered next steps: push PR and wait for full Windows CI; merge with release title; verify assets/hash; safely back up and update REMY; inspect NOVA over RDP without automating login; have the user authenticate if prompted; back up/update NOVA; finalize handoffs.

# Current objective — 1.9.0 timeline update

Progress: 100% — implemented, installed and published; GitHub release workflow passed.

- [x] Inspect project and previous handoff.
- [x] Verify requested intervals and UI.
- [x] Publish GitHub release v1.9.0; local 1.9.0 is installed and running.

Next: optional live UI acceptance. Release workflow 41718088957 passed; https://github.com/remriel/presence-windows/releases/tag/v1.9.0

# Presence progress

## Current objective — shorter timeline ranges, 2026-11-03

Progress: **100%** — requested timeline views are merged and installed; PR Windows checks and profile-preservation checks passed.

- [x] Read user instructions, shared memory, repository instructions and handoffs; inspect GitHub, source, tests and the installed app.
- [x] Add rolling 6h / 30-minute, 3h / 15-minute and 1h / 5-minute timeline views while preserving existing ranges.
- [x] Verify interval reconstruction, boundary behavior, accessibility and light/dark native layouts; show a fictional screenshot.
- [x] Build a self-contained Windows executable and synchronize the reviewed changes with GitHub.
- [x] Back up the installed program and SQLite profile, update this computer, restart the tray app, and verify the preserved profile and startup entry.
- [x] Complete project handoffs, wisdom and shared-memory pointer.

Current state: core checks pass; fictional timeline preview passes 107 checks/16 captures at 144 DPI and 105 checks/16 captures in simulated 96-DPI mode, with no findings. Full [PR Windows CI](https://github.com/remriel/presence-windows/actions/runs/41677879466) passed and [PR #11](https://github.com/remriel/presence-windows/pull/11) merged. Local version 1.8.0 is running from the existing startup target and matches the built executable. Database integrity, settings, networks, saved device/person configuration and existing event IDs passed comparisons to both pre-update and stopped backups.

Blockers: none for the requested work. Windows UI automation returned `GetCursorPos: Access is denied`, so installed live UI interaction was not verified; fictional native previews and packaged preview passed. Process-level stopping could discard a few seconds of unflushed scan observations. Main CI was in progress at handoff; the already-passed PR CI tested the same implementation. No public 1.8.0 release was requested or created.

Next steps: no required work remains. Optionally check the main CI result, inspect the new ranges in the live app, and publish a public 1.8.0 release if the user requests one.

## Current objective — 12-hour timeline, app UI and automatic known option, 2026-10-29

Progress: **100%** — implementation, publication, install, profile preservation checks, and handoffs are complete.

- [x] Inspect shared memory, project instructions, source, tests and clean repository baseline.
- [x] Add a rolling 12-hour timeline with one-hour cells and exact local/UTC tooltips.
- [x] Refresh shared light/dark UI, show direct device navigation, and add searchable inventory.
- [x] Add opt-in automatic known classification for new or returning unidentified devices; preserve ignored/person choices and notification semantics.
- [x] Pass core tests, 28 interface checks, timeline 51, category 112, saved networks 11, device visibility 16, reliability 16, bulk 24 and 26-screen broad smoke; zero findings.
- [x] Merge PR #10 after Windows CI passes; publish 1.7.0 after main/release CI passes.
- [x] Download the official ZIP; verify its SHA-256 against both the checksum asset and GitHub digest.
- [x] Back up the installed program/profile, replace the requested `C:\Presence` install, and restart the tray app.
- [x] Confirm installed executable identity, running process, startup entry, SQLite integrity, retained saved state and pre-update event IDs.
- [x] Record and preserve current live settings; export project handoffs and update shared memory.

Blockers: none. The live profile has the new auto-known setting enabled. Existing unidentified devices convert on a later join; ignored and person-assigned choices remain. Three other settings differ from the pre-update backup; the source is undetermined, so the current live settings were kept.

Next steps: none required. Optional acceptance: confirm LAN reconnect/sleep behavior and additional physical display/DPI combinations.

Release: [v1.7.0](https://github.com/remriel/presence-windows/releases/tag/v1.7.0). PR: [#10](https://github.com/remriel/presence-windows/pull/10). Release commit: `a93b2f543900d1894983c6d3267574f7ff820242`. PR CI [40970814478](https://github.com/remriel/presence-windows/actions/runs/40970814478); release CI [40971271791](https://github.com/remriel/presence-windows/actions/runs/40971271791). ZIP SHA-256: `060d1100cb4674c8ddf69f7f450dad150f428ca5adcb53e48a5049474a310dce`.

## Previous progress

## Current objective — Presence 1.6.0 device categories (complete, 2026-10-21)

Progress: **100%** — category checkboxes and themed colors are published and installed. The user confirmed one category per device and explicitly authorized the release and PC update.

- [x] Inspect instructions, shared memory, source, tests and clean GitHub baseline.
- [x] Persist one manual category with Automatic fallback; retain existing profiles and save rollback.
- [x] Add adaptive, accessible category checkboxes and themed colors to details/home/inventory/timeline.
- [x] Pass core behavior and 112 category/theme checks; verify persistence, cancel, reopen, failed-save retry and tracking independence.
- [x] Pass all required native regressions and stabilize history sizing for previous-date timestamps.
- [x] Merge PR #9 and publish Presence 1.6.0 after Windows CI passed.
- [x] Verify the official ZIP against its checksum and GitHub asset digest; launch the packaged fictional preview.
- [x] Back up both program folders and consistent online/stopped SQLite profiles; update both installed copies and restart the original startup target.
- [x] Verify one running 1.6.0 instance, matching installed executable hashes, unchanged startup, database integrity and preserved saved state/history.
- [x] Finalize state/progress, shared memory, WISDOM and delivery exports.

Release: https://github.com/remriel/presence-windows/releases/tag/v1.6.0

PR: https://github.com/remriel/presence-windows/pull/9

Release/tag target: `3e51dddbb759ba2bbef40659610c799ccc5cf0af`. PR CI [39706566156](https://github.com/remriel/presence-windows/actions/runs/39706566156) and main/release CI [39706867928](https://github.com/remriel/presence-windows/actions/runs/39706867928) succeeded. The official ZIP is 57,496,097 bytes; SHA-256 `14f734c327f466f012cf29569421570f85adcceb51b0011ad8129acd66bd1f7c`. EXE version: 1.6.0.0; packaged fictional preview exit 0.

Verification: category suite 112 checks/zero findings/10 captures across both themes; networks 11, timeline 39, visibility 16, reliability 16, bulk/reset/pause 24; broad UI 26 captures/zero findings. Category text/fill and text/surface contrast is at least 4.5:1. Core checks include legacy snapshots, SQLite reopen, automatic fallback and discovery independence. A 96-DPI/150% local smoke also passed before the overnight-history fix; the final overnight fixture passed in both PR and release CI.

CI discovery: broad UI originally stalled while scaling activity history. Previous-date timestamps were wider than clock-only values; automatic column width and wrapped row height depended on each other. Disable wrapping for the AllCells When/Event columns, keep device names wrapping, and always include overnight timestamps in fictional fixtures. CI now logs captures, throws UI exceptions instead of displaying unseen dialogs, uploads fictional diagnostics on failure and bounds broad smoke to one minute. No further broad local run was required after the complete final CI pass.

PC verification: both installed executables match the official package, one active 1.6.0 instance runs from the original startup target, and startup configuration is unchanged. SQLite integrity, saved device configuration/categories/associations, people, saved networks, settings and in-retention event history are preserved. No reset or schema migration occurred. Machine-specific paths and profile baselines remain private and are not committed.

Blockers: none. Required next steps: none. Optional future acceptance: use the category selector on additional physical displays and compare LAN inference with real reconnect/sleep behavior. Do not move the release tag for documentation-only changes.

## Previous objective — completed 1.5.1 release

Current objective: make the timeline open large enough to show more devices without scrollbars, publish Presence 1.5.1, and replace both portable copies on this PC without resetting the profile.

Progress: **100%** — the maximized, viewport-fitted timeline is published and installed; the existing profile is preserved.

Checklist:

- [x] Re-read project instructions/state/progress and inspect the clean release baseline, relevant UI and preview fixture.
- [x] Open the timeline maximized, allow restore/maximize, and provide a larger restored size.
- [x] Fit timeline columns and rows to the rendered grid after first display and resize; keep full names in tooltips.
- [x] Expand the fictional preview to 20 devices and verify daily/hourly ranges in light and dark themes.
- [x] Pass 39 focused timeline checks with zero findings; all 20 devices fit without scrollbars on this PC's 144-DPI display.
- [x] Replace the public fictional daily and 24-hour timeline screenshots with the larger previews.
- [x] Update version metadata, README, release notes, project state and progress to 1.5.1.
- [x] Merge PR #8 (`f6813837d00e77ca9fca09a846a6b057abfd30c0`) after Windows CI passed; workflow run [39299052655](https://github.com/remriel/presence-windows/actions/runs/39299052655) published release [v1.5.1](https://github.com/remriel/presence-windows/releases/tag/v1.5.1).
- [x] Verify official ZIP SHA-256 `9091e23de7b350bf79d8c8e33d4f1fce952168fdb382a574988b8ce9e2c0d55c` against the checksum file and GitHub asset digest; the EXE is 1.5.1.0 and the packaged fictional preview exported successfully.
- [x] Back up both 1.5.0 folders and the SQLite profile, replace both copies, preserve the startup entry, and restart the active copy.
- [x] Confirm post-update SQLite integrity and retention of saved devices/associations, people, saved networks, settings and in-retention event history.
- [x] Finalize project and shared-memory handoffs, reflection notes and task outputs.

Release ZIP: 57,489,917 bytes. PR: [#8](https://github.com/remriel/presence-windows/pull/8). Release commit: `f6813837d00e77ca9fca09a846a6b057abfd30c0`; tag `v1.5.1`.

Verification note: the focused timeline preview passed 39 checks with 0 findings and 6 captures; 20 fictional device rows fit at 1942×1056, 144 DPI. GitHub CI passed all regression and packaging jobs. Both installed EXEs match the official package, startup configuration stayed the same, and the existing SQLite profile passed integrity checks before/after with saved state retained. No blockers or required next steps remain. Larger device lists or smaller displays may still need scrolling.
