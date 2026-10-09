# Presence progress

Current objective: show devices excluded from notifications in an **Ignored devices** section on the main window, using the existing Ignore option.

Progress: **100%** [####################]

- [x] Reconcile AGENTS.md, handoffs and the actual clean v1.1.1 repository at 3daa9f5.
- [x] Add counted, selectable ignored rows and an explanatory empty state; retain the existing current-network boundary and silent Ignore behavior.
- [x] Update device details, README, release notes and manual acceptance.
- [x] Pass the existing deterministic core checks and 15 focused fictional UI/core/persistence checks with zero findings.
- [x] Capture and visually inspect light, dark, compact and device-details screenshots.
- [x] Complete one Windows x64 self-contained production publish and package the portable ZIP/checksum.
- [x] Back up the installed 1.1.1 files, replace C:\Presence, and confirm the 1.1.2 tray process, startup command, binary hash and original data path.
- [x] Push the completed change and create/attach GitHub PR #1.
- [x] Confirm successful GitHub CI and prepare final PROJECT_STATE.md and PROGRESS.md exports.

Implementation: MainWindow.Render includes every ignored device in the active network irrespective of presence state. Rows read Ignored and open device details; changing Track as restores normal tracking. Existing Ignore event suppression and serialized DeviceKind persistence are used without a new setting or schema change. During reconnect and network changes the section clears with the other live lists; the full saved inventory remains in Settings > Devices.

Verification: Windows x64 publish exited 0 with no warnings/errors. The existing Presence.Core executable checks passed. A scratch preview harness referenced the already-built assemblies, so the production app was not rebuilt. Fifteen checks covered present/away ignored rows, no duplicates, other-network exclusion, row-click editing, Save/Cancel behavior, reopening saved SQLite state, reconnect/new-network clearing, an empty state, suppression of first/new/arrival/departure events, and restoration of normal events. Native 150% DPI captures were visually inspected in light/dark and minimum-size layouts. No live Wi-Fi transitions, popup sound or internet speed transfer were tested.

Source checkout: C:\Drive\2026-10-09\gi\work\presence-windows
Git branch: codex/show-ignored-devices
Pull request: https://github.com/remriel/presence-windows/pull/1 (open, not merged)
Verified feature commit: 3e53b471eb31ef7d492133b4692fedd66ade4d38
Successful feature CI: https://github.com/remriel/presence-windows/actions/runs/37976713773
Target app version: 1.1.2
Current published release remains v1.1.1; this change has a verified local portable package and installation.

Package: C:\Drive\2026-10-09\gi\outputs\Presence-1.1.2-win-x64.zip
Archive SHA-256: b2f60f709f9e90c6d99d7f2c91c3379460b286dcc9ebe873154112d2a05d789b
Installed app: C:\Presence\Presence.exe, file version 1.1.2.0, running with --tray. Startup remains "C:\Presence\Presence.exe" --tray.
Data: original %LOCALAPPDATA%\Presence\presence.db remains in place. No profile data was copied, moved or deleted.
Binary backup: %LOCALAPPDATA%\PresenceBuild\IgnoredDevices-1.1.2\previous-install.
Build intermediates/output: %LOCALAPPDATA%\PresenceBuild\IgnoredDevices-1.1.2.
Fictional screenshots, verification.json and installation.json: task outputs/ignored-devices.

Blockers: none.

RESUME HERE / exact ordered next steps:
1. No feature, build, package or install work remains. The completed source and handoff documentation are synchronized in PR #1 and the local tray app is 1.1.2.
2. PR review/merge and publication of a v1.1.2 GitHub release are separate owner follow-ups; do not imply the PR was merged or a release published.
3. If physical LAN acceptance is requested, use the ignored-devices steps in docs/MANUAL_ACCEPTANCE.md. Fictional checks do not prove physical Wi-Fi behavior.
4. Before future work, read AGENTS.md and both handoffs, inspect git status/diff, and reconcile source/tests with actual state.
