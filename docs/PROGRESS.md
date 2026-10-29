# Presence progress

## Current objective — 12-hour timeline, app UI and automatic known option, 2026-10-29

Progress: **75%** — implementation and local checks pass; GitHub CI and publication remain.

- [x] Inspect shared memory, project instructions, source, tests and clean repository baseline.
- [x] Add a rolling 12-hour timeline with one-hour cells and exact local/UTC tooltips.
- [x] Refresh shared light/dark UI, show direct device navigation, and add searchable inventory.
- [x] Add opt-in automatic known classification for new or returning unidentified devices; preserve ignored/person choices and notification semantics.
- [x] Pass core tests, 28 interface checks, timeline 51, category 112, saved networks 11, device visibility 16, reliability 16, bulk 24 and 26-screen broad smoke; zero findings.
- [ ] Push PR and pass GitHub Windows CI; merge with the release commit title.
- [ ] Wait for main CI to publish 1.7.0; download and verify release ZIP and checksum.
- [ ] Finalize repository state, shared-memory pointer, wisdom and exported handoffs.

Blockers: none. Existing unidentified devices are converted on their next join, not immediately; existing ignored or person-assigned choices remain. No changes requested to installed app or live profile.

Next steps: refresh fictional README screenshots, verify diff, submit release PR, check CI and official package.

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
