# Presence progress

## Current objective — device categories (2026-10-21)

Add a saved category per device, checkbox selection, and category colors that follow the light/dark theme. The user confirmed one category per device and explicitly authorized publishing 1.6.0 and updating this PC while preserving saved devices/history. Work is on `feature/device-categories`, based on clean public main `9d6f701`.

Progress: **80%** toward the expanded release/install objective — implementation and all local regressions pass; PR #9 and a tested portable preview are ready. Release CI, public package verification, profile backup and PC update remain.

- [x] Load instructions/shared memory; reconcile the clean checkout with GitHub main and inspect source/tests.
- [x] Confirm one category per device with the user.
- [x] Persist an optional manual category; preserve automatic hints for existing devices and save rollback.
- [x] Add accessible category checkboxes and themed colors to details, home, inventory and timeline labels.
- [x] Verify persistence, cancel/failure behavior, category switching, automatic fallback and light/dark layouts using fictional fixtures.
- [ ] Build a portable Windows milestone, sync a reviewable GitHub PR and verify CI.
- [ ] Publish the verified 1.6.0 release and verify its official ZIP/checksums.
- [ ] Back up app/profile, update installed copies, restart and verify preserved saved state/startup.
- [ ] Finalize state/progress, shared memory and WISDOM; export deliverables.

Verification: core checks pass, including legacy JSON, SQLite reopen, override and hostname-enrichment behavior. Native category suite: 112 checks, zero findings, 10 fictional captures in Light/Dark at 144 DPI, default/minimum widths. All category text/fill and text/surface pairs meet 4.5:1 contrast. Existing suites pass: saved networks 11; timeline 39; visibility 16; reliability 16; bulk/reset/pause 24; broad native UI 26 captures, zero layout findings. Self-contained Windows x64 publish succeeds, file version 1.6.0.0, packaged fictional preview exits 0. No live profile was used.

Blockers: none. Exact next steps: pass final PR CI, merge with the release trigger, verify main CI and official assets, back up/replace installed copies, verify restart/profile and finalize handoffs. No installation has occurred yet.

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
