# Presence progress

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
