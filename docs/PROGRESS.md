# Presence progress

Current objective: run Presence v1.0.4 locally and remove the previous installed version after publishing the Windows x64 release.

Verified progress: **90%** `[██████████████████░░]`. The new tray app is installed and running, and the previous process is stopped. Removing the old extracted folder is blocked by the environment file-operation policy.

- [x] Read the user brief and reconcile the repo state with the durable handoff docs.
- [x] Identify that nested/default Settings labels retained Windows black text under the dark theme.
- [x] Apply the selected theme recursively while preserving deliberately muted labels.
- [x] Resolve stale version conflict markers to version 1.0.4.
- [x] Update release and project documentation for the UI correction.
- [x] Publish one self-contained Windows x64 build without running tests.
- [x] Export the fictional main-window screenshot and package portable archive and checksum.
- [x] Push the source and docs to GitHub and publish v1.0.4 with the build and documentation assets.
- [x] Record final release links and progress state.
- [x] Install and start v1.0.4 in the system tray; preserve the existing local database.
- [x] Stop the older v1.0.3 tray process.
- [ ] Remove the old v1.0.3 extracted folder from Downloads.

Release: https://github.com/remriel/presence-windows/releases/tag/v1.0.4
Portable build: `outputs/Presence-1.0.4-win-x64.zip` (SHA-256 `95291ce7e44ed4ae70b6bb4b71c07404c376a89c0019bf43e759a44819e7e270`). The main-window screenshot uses fictional devices. An off-screen Settings screenshot exported a blank form and was excluded; the theme fix is in the dialog source.

No tests, lint, review, cleanup or repeated validation were run. Runtime Wi-Fi and internet-speed behavior remains owner acceptance. The `[skip ci]` marker prevented the repository workflow from starting its test job.

## RESUME HERE
The unreadable labels came from nested controls retaining their default black foreground. `Ui.Theme` now applies the selected theme recursively and keeps intentionally muted label colors. The source, portable build, SHA-256, main preview and docs are published in GitHub release v1.0.4. The app is installed at `Downloads\Presence-1.0.4-win-x64` and is running in the tray; the existing `%LOCALAPPDATA%\Presence\presence.db` remains. The old 1.0.3 process is stopped, but its folder remains because deletion was rejected by automatic filesystem policy.
