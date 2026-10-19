# Presence progress

Current objective: make the timeline open large enough to show more devices without scrollbars, publish Presence 1.5.1, and replace both portable copies on this PC without resetting the profile.

Progress: **45%** — implementation and fictional UI preview are complete; release and installation remain.

Checklist:

- [x] Re-read project instructions/state/progress and inspect the clean release baseline, relevant UI and preview fixture.
- [x] Open the timeline maximized, allow restore/maximize, and provide a larger restored size.
- [x] Fit timeline columns and rows to the rendered grid after first display and resize; keep full names in tooltips.
- [x] Expand the fictional preview to 20 devices and verify daily/hourly ranges in light and dark themes.
- [x] Pass 39 focused timeline checks with zero findings, including no vertical or horizontal scrollbars for the 20-device fixture on the development PC's 144-DPI display.
- [x] Replace the public fictional daily and 24-hour timeline screenshots with the larger previews.
- [x] Update version metadata, README, release notes, and current project handoffs to 1.5.1.
- [ ] Build and inspect the self-contained x64 package.
- [ ] Push the feature branch, create and attach a PR, wait for Windows CI, and merge using the workflow trigger title `Release Presence 1.5.1`.
- [ ] Verify the public release ZIP, checksum, executable version, and packaged fictional preview.
- [ ] Back up both 1.5.0 portable folders, replace them from the verified package, and confirm startup configuration plus profile integrity/state preservation.
- [ ] Finalize project/shared-memory records and copy the release package, checksum, screenshots, and handoffs to task outputs.

Current verification: `dotnet run --project tools/Presence.UiPreview/Presence.UiPreview.csproj -c Release -- <preview-dir> --timeline` completed successfully with 39 checks, 0 findings, and 6 captures. The screenshots are fictional and use the local 1942×1056 display at 144 DPI. Release build/CI/package and PC replacement are not yet complete.

Current blocker: none.

Exact next steps: (1) build/package version 1.5.1; (2) inspect package version and fictional preview; (3) push branch/create PR/attach; (4) wait for CI and merge with the release-trigger title; (5) verify official assets; (6) replace both PC copies after making backups and verify the existing profile; (7) finalize handoffs and outputs.
