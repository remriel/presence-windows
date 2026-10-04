# Presence project state

## Architecture and rationale
- C# / .NET 10, Windows Forms. Native Windows 10 utility; no browser runtime or server.
- Core library separates identity, inference, and SQLite storage. Windows app separates discovery, notifications, and UI.
- Self-contained Windows x64 portable distribution; no runtime installation needed.
- MAC is identity. Private MAC changes require explicit reassignment to the existing person; no speculative identity matching.
- Fresh ARP resolution plus ICMP and local multicast DNS responses. Cached ARP entries provide candidates only; stale cache is never presence evidence.
- Native toast activation via CommunityToolkit.WinUI.Notifications. No router credentials.
- Explicit brief overrides generic bold visual design instructions: neutral native styling, light/dark themes, simple lists.

## Discoveries and constraints
- Host is Windows 10 build 19045; .NET SDK 10.0.401 is installed.
- Physical Wi-Fi has a /22 subnet. Probe scheduling must cover larger networks without excessive concurrent requests; known devices have priority.
- VPN and virtual interfaces must be excluded from automatic home-network choice.
- User-owned local LAN only. No port scanning, packet capture, cloud, or runtime internet requests.
- Human presence is an inference from device presence. AP client isolation and sleeping/private-MAC phones limit certainty.
- Offline monitoring time must not count toward departures. First startup scans form a silent baseline.

## Relevant files
- src/Presence.Core: models, identity/inference, SQLite repository.
- src/Presence.App: Windows discovery, notification service, native forms/tray.
- tests/Presence.Tests: unused project skeleton; automated tests were not authored or run after the user's build-once instruction.
- src/Presence.App/Diagnostic.cs: opt-in single-scan sanitized counts for future manual investigation; not run for this release.

## Implementation decisions
- PresenceEngine.Apply requires two consecutive detections; per-device missing time accumulates only during healthy monitoring. Primary phones take precedence over auxiliary devices. Unknown is never a departure.
- SQLite snapshot, event and observation records save in one transaction. Events retain 90 days by default; observations at most seven days. Existing data is preserved on startup failure.
- ResolveIpNetEntry2 flushes a matching neighbor entry and performs fresh ARP resolution. GetIpNetTable2 is candidate-only; fresh mDNS replies can use a cached MAC for identity.
- /16 through /30 physical IPv4 LANs: 128 new addresses per cycle, known devices and neighbor candidates prioritized, bounded 48-worker discovery. Large subnets take multiple cycles for a full sweep. First full sweep stays silent.
- Adapter key includes physical interface, subnet and gateway MAC. VPN/virtual adapters are excluded.
- mDNS uses a local UDP socket and QU replies; names are parsed only from local IPv4 A records. No reverse-DNS internet lookups, packet capture, router credentials or port scanning.
- Offline IEEE OUI CSV is bundled. Locally administered MACs are labelled private. Existing person selection plus primary-phone replacement links a new private MAC without inventing identity matches.
- CommunityToolkit.WinUI.Notifications 7.1.2 provides native toasts and click activation; per-user named pipe handles additional launches. Startup defaults to enabled via HKCU Run, following the user's persistent-background request; Settings can disable it.
- SQLite package initially restored a vulnerable native library. Upgraded Microsoft.Data.Sqlite to 10.0.12 and SQLitePCLRaw.bundle_e_sqlite3 to 3.0.5; restore has no vulnerability warning.
- Initial production build blocked on WinForms WFO1000 for AllowClose. Explicit designer serialization attribute is the fix.
- Publishing on the Google Drive-backed source directory then failed in CreateAppHost with a user-mapped file section. Use a local-disk artifacts path for obj/bin/publish and copy finished binaries to outputs; do not repeat synced-drive apphost generation.
- Latest feature steering: automatic scans default to 120 seconds; main header has a manual Refresh button. Settings permits 30–600 seconds. Two detections may therefore take four minutes without manual refresh.
- Persistent background/tray request: closing hides the main window; Quit Presence ends the process. Startup registration refreshes the stable executable path on normal launch. Install locally under LocalAppData/Programs/Presence and run --tray for this user.
- Final scope steering: any device joins/leaves the local network. PresenceEngine now emits one event per non-ignored device transition. First unknown confirmation uses only a new-device event, later reconnects use arrivals. Person grouping drives UI without extra person-level notifications. Known devices appear in Home Now/Away alongside people.
- Windows Computer Use capture failed with FrameArrived/window capture timeouts after one recovery attempt. --preview-image exports the actual native client window through WinForms DrawToBitmap with fictional devices, without scanning. --startup-trace optionally writes sanitized launch-stage markers under LocalAppData/PresenceBuild.
- Latest user instruction: build once and publish; fix only blocking errors. Skip tests, review, cleanup and repeated validation. Existing test project is an unused skeleton; no automated tests authored or run.

## Unresolved / proof boundary
- A physical phone disconnect/reconnect/sleep test requires user participation; deterministic tests cannot establish radio behavior.
- Native preview export completed; Windows Computer Use screenshots timed out twice. Live LAN discovery and phone/restart/toast acceptance were explicitly skipped under the build-once instruction.
- Release is unsigned. Source and a self-contained portable ZIP were published privately; startup/persistence across a real Windows logon and long-running CPU/RAM behavior are not verified.
- This release is IPv4-only. Sleeping clients/AP isolation/proxy ARP can make inferred joins/leaves differ from router association state. A /22 full baseline takes multiple two-minute cycles; recognized-device addresses are probed each cycle.
- No runtime errors were established by functional testing because none was requested after the build-once steering. Known uncertainty must not be presented as passed acceptance.

## Publication and local installation
- Private GitHub repository: https://github.com/remriel/presence-windows.
- Release v1.0.0: https://github.com/remriel/presence-windows/releases/tag/v1.0.0.
- Local stable install: %LOCALAPPDATA%/Programs/Presence. Started --tray; startup defaults to enabled via HKCU Run. Data: %LOCALAPPDATA%/Presence/presence.db; preview is a separate Presence-Demo profile.
- Production build: dotnet publish with win-x64, self-contained, single-file, IncludeNativeLibrariesForSelfExtract, DebugType=None and local --artifacts-path. No trimming. OUI CSV accompanies the executable.
- GitHub initially rejected a shortened release target SHA with Release.target_commitish is invalid. Using full git rev-parse HEAD resolved the publication blocker.

## RESUME HERE
The requested build-and-publish workflow is complete. Stop without further tests/review/cleanup. Owner identifies phones/devices and performs optional manual acceptance. On a future explicit fix request, read these docs and inspect actual source/git state, then make only the requested change; prefer local-disk build artifacts to avoid Google Drive apphost locking.
