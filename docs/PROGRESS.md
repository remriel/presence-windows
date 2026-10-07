# Presence progress

Current objective: release v1.1.1 so network changes automatically clear and repopulate the active device list, and update the installed PC copy.
Progress: **85%** [#################---]

- [x] Read repository guidance, project state, actual source and the linked original chat/brief.
- [x] Fix reconnect lifecycle, obsolete scan cancellation and network-scoped list rendering.
- [x] Preserve device identities/history while resetting presence and the quiet baseline.
- [x] Complete one Windows x64 self-contained production publish.
- [x] Capture three fictional before/reconnecting/after screens.
- [x] Package portable ZIP/checksum in task outputs.
- [ ] Push completed source/docs and publish v1.1.1.
- [ ] Replace C:\Presence and restart the tray app.
- [ ] Record final evidence and deliver continuity files.

Implementation: address and availability events cancel the active scan, invalidate its epoch, reset discovery/presence, clear the network label and debounce reconnect for 750 ms. Obsolete results and failures cannot overwrite newer state. A changed source IP restarts background discovery even on the same stable LAN identity. Main lists use only the active network; saved inventory remains accessible from Settings.

Blockers: none.
Verification: the production publish exited 0, and fictional screenshots were rendered. No automated tests, physical Wi-Fi switching or speed transfers were run in the carried-forward single-build release workflow. Screenshots demonstrate UI states, not live network behavior.

Exact next steps:
1. Push this release commit to GitHub main and publish ZIP/checksum as v1.1.1.
2. Back up the installed executable, replace it, restart the tray app and record its version.
3. Fast-forward the original clean checkout, then copy PROJECT_STATE/PROGRESS to outputs.
