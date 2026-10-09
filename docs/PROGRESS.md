# Presence progress

Current objective: show devices excluded from notifications in an **Ignored devices** section on the main window, using the existing Ignore option.

Progress: **95%** [###################-]

- [x] Reconcile AGENTS.md, handoffs and the actual clean v1.1.1 repository at 3daa9f5.
- [x] Add counted, selectable ignored rows and an explanatory empty state; retain the existing current-network boundary and silent Ignore behavior.
- [x] Update device details, README, release notes and manual acceptance.
- [x] Pass the existing deterministic core checks and 15 focused fictional UI/core/persistence checks with zero findings.
- [x] Capture and visually inspect light, dark, compact and device-details screenshots.
- [x] Complete one Windows x64 self-contained production publish and package the portable ZIP/checksum.
- [x] Back up the installed 1.1.1 files, replace C:\Presence, and confirm the 1.1.2 tray process, startup command, binary hash and original data path.
- [ ] Push the completed change and create/attach its GitHub pull request.
- [ ] Check GitHub CI and export final PROJECT_STATE.md and PROGRESS.md to task outputs.

Implementation: MainWindow.Render includes every ignored device in the active network irrespective of presence state. Rows read Ignored and open device details; changing Track as restores normal tracking. Existing Ignore event suppression and serialized DeviceKind persistence are used without a new setting or schema change. During reconnect and network changes the section clears with the other live lists; the full saved inventory remains in Settings > Devices.

Verification: Windows x64 publish exited 0 with no warnings/errors. The existing Presence.Core executable checks passed. A scratch preview harness referenced the already-built assemblies, so the production app was not rebuilt. Fifteen checks covered present/away ignored rows, no duplicates, other-network exclusion, row-click editing, Save/Cancel behavior, reopening saved SQLite state, reconnect/new-network clearing, an empty state, suppression of first/new/arrival/departure events, and restoration of normal events. Native 150% DPI captures were visually inspected in light/dark and minimum-size layouts. No live Wi-Fi transitions, popup sound or internet speed transfer were tested.

Source checkout: C:\Drive\2026-10-09\gi\work\presence-windows
Git branch: codex/show-ignored-devices
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
1. Review the final diff; commit/push codex/show-ignored-devices and create/attach a pull request against main.
2. Update these handoff files with the confirmed PR URL and final synchronization state; push the documentation.
3. Confirm the final PR CI result and copy both handoff files to task outputs.
4. Physical LAN transitions remain optional owner acceptance in docs/MANUAL_ACCEPTANCE.md. Do not claim those were proven by fictional screenshots.
