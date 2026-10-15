# Presence progress

Current objective: remember/save network with SSID as default name, and publish release.
Progress: **100%** [####################]

- [x] Inspect instructions, project notes, source/tests and actual Git state.
- [x] Implement Networks dialog: SSID default, editable save/rename, persisted list and forget.
- [x] Preserve existing profile/device isolation; support old snapshots, stale editors, failed-save rollback and reset.
- [x] Pass core tests for Unicode, fallback, validation, identity isolation, legacy load, SQLite reopen and reset.
- [x] Pass 11 saved-network native checks and five captures with zero findings, including save-button flow and reconnect label.
- [x] Pass all existing local checks: 16 reliability, 16 visibility/DNS, 24 bulk/reset/pause; broad 26 captures, zero findings.
- [x] Build without warnings/errors; inspect fictional screenshots and correct name-field width.
- [x] Push release commit 717586a1b8683e2a79d602e179b82fe1f8be4b7c to main.
- [x] Pass Windows CI and publish v1.3.0: https://github.com/remriel/presence-windows/actions/runs/38808885693.
- [x] Download public ZIP and checksum; match SHA-256 to checksum file and GitHub asset digest.
- [x] Verify tag target and package contents, launch public packaged preview with exit code 0.
- [x] Export final PROJECT_STATE.md and PROGRESS.md to task outputs.

Release: https://github.com/remriel/presence-windows/releases/tag/v1.3.0
ZIP: Presence-1.3.0-win-x64.zip (57,474,482 bytes)
SHA-256: 3e72a3b026ec8665377c9e1e5de777b29fce6c43fa7cb0daf2720ec1b0d9c484
Output directory: C:/Drive/2026-10-15/x/outputs
Current implementation state: released and verified. Blockers: none.

Exact ordered next steps:
1. No implementation or publication work remains.
2. Installation is a separate action, not requested this session. Preserve the existing local profile if updating.
3. For physical acceptance, check SSID permissions and reconnect behavior on an authorized real Wi-Fi LAN. Network identity remains adapter/subnet/gateway; changing these may require saving again.
4. Future work must read applicable instructions and handoffs, then reconcile with Git/source/tests.
