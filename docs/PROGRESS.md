# Presence progress

Current objective: release v1.1.1 so network changes automatically clear and repopulate the active device list, and update the installed PC copy.
Progress: **100%** [####################]

- [x] Read repository guidance, project state, actual source and the linked original chat/brief.
- [x] Fix reconnect lifecycle, obsolete scan cancellation and network-scoped list rendering.
- [x] Preserve device identities/history while resetting presence and the quiet baseline.
- [x] Complete one Windows x64 self-contained production publish.
- [x] Capture three fictional before/reconnecting/after screens.
- [x] Package portable ZIP/checksum in task outputs.
- [x] Push completed source and publish v1.1.1 on GitHub.
- [x] Back up the previous installed binary, replace C:\Presence and restart the tray app.
- [x] Confirm installed file version 1.1.1.0, running process path, startup command and existing data file.
- [x] Record final evidence and prepare deliverable continuity files; synchronize the original checkout as the final documentation step.

Implementation: Windows address and availability events cancel the active scan, invalidate its epoch, reset discovery/presence, clear the network label and debounce reconnect for 750 ms. Obsolete results and failures cannot overwrite newer state. A changed source IP restarts background discovery even on the same stable LAN identity. Main lists use only the active network; saved inventory remains accessible from Settings. Explicit adapter selection is respected; Automatic follows available physical adapters.

Release: https://github.com/remriel/presence-windows/releases/tag/v1.1.1
Source release commit: ec5564a2a93f27ec3a7dc79a28ff22c2b55e4b65
Archive SHA-256: a9e67e044e4857c0dc417f3ef7be0abd801f0fba8b018b070959f55d27fea4a5
Installed app: C:\Presence\Presence.exe, file version 1.1.1.0; Run entry remains "C:\Presence\Presence.exe" --tray.
Data: existing %LOCALAPPDATA%\Presence\presence.db remains at its original location. Binary backup: %LOCALAPPDATA%\PresenceBuild\NetworkRefresh-1.1.1\previous-install.

Blockers: none. No implementation/publication/install step remains.
Verification: the production publish exited 0. Three fictional UI states were rendered. GitHub reports the portable archive uploaded with the matching digest, and the new executable process is running from the installed location. No automated tests, physical Wi-Fi switching, sleep/resume or speed transfers were run in the carried-forward single-build release workflow. Screenshots demonstrate UI states, not live network behavior.

RESUME HERE / exact ordered next steps:
1. No release work remains. Use docs/PROJECT_STATE.md for architecture and rationale.
2. If real-LAN acceptance is requested, follow the v1.1.1 network-change steps in docs/MANUAL_ACCEPTANCE.md: A-to-B switch, same-network reconnect, rapid switching during a scan, DHCP rebinding and sleep/resume.
3. Do not infer physical network acceptance from the fictional screenshots or successful compile.
