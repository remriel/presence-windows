# Presence project state

## Architecture and rationale
- C# / .NET 10, Windows Forms. Native Windows 10 utility; no browser runtime or server.
- Core library separates identity, inference, and SQLite storage. Windows app separates discovery, notifications, and UI.
- Self-contained Windows x64 portable distribution; no runtime installation needed.
- MAC is identity. Private MAC changes require explicit reassignment to the existing person; no speculative identity matching.
- Fresh ARP resolution plus ICMP and local multicast DNS responses. Cached ARP entries provide candidates only; stale cache is never presence evidence.
- App-owned floating icon notifications and embedded WAV playback, independent of Windows notification banners. No router credentials. Legacy toast activation/unregistration compatibility remains for the initial release.
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
- PresenceEngine.Apply accepts the first fresh discovery response, with no approval or second scan required (1.0.1 steering). Per-device missing time accumulates only during healthy monitoring. Primary phones take precedence over auxiliary devices. Unknown is never a departure.
- SQLite snapshot, event and observation records save in one transaction. Events retain 90 days by default; observations at most seven days. Existing data is preserved on startup failure.
- ResolveIpNetEntry2 flushes a matching neighbor entry and performs fresh ARP resolution. GetIpNetTable2 is candidate-only; fresh mDNS replies can use a cached MAC for identity.
- /16 through /30 physical IPv4 LANs: 128 new addresses per cycle, known devices and neighbor candidates prioritized, bounded 48-worker discovery. Large subnets take multiple cycles for a full sweep. First full sweep stays silent.
- Adapter key includes physical interface, subnet and gateway MAC. VPN/virtual adapters are excluded.
- mDNS uses a local UDP socket and QU replies; names are parsed only from local IPv4 A records. No reverse-DNS internet lookups, packet capture, router credentials or port scanning.
- Offline IEEE OUI CSV is bundled. Locally administered MACs are labelled private. Existing person selection plus primary-phone replacement links a new private MAC without inventing identity matches.
- 1.0.0 used CommunityToolkit.WinUI.Notifications 7.1.2 for native toasts; 1.0.1 replaces delivery with FloatingAlerts. The toolkit remains solely for compatibility with legacy toast activation/unregistration. Per-user named pipe handles additional launches. Startup defaults to enabled via HKCU Run; Settings can disable it.
- SQLite package initially restored a vulnerable native library. Upgraded Microsoft.Data.Sqlite to 10.0.12 and SQLitePCLRaw.bundle_e_sqlite3 to 3.0.5; restore has no vulnerability warning.
- Initial production build blocked on WinForms WFO1000 for AllowClose. Explicit designer serialization attribute is the fix.
- Publishing on the Google Drive-backed source directory then failed in CreateAppHost with a user-mapped file section. Use a local-disk artifacts path for obj/bin/publish and copy finished binaries to outputs; do not repeat synced-drive apphost generation.
- Automatic scans default to 120 seconds; main header has a manual Refresh button. Settings permits 30–600 seconds. 1.0.1 removes the two-detection arrival requirement; first fresh discovery is sufficient.
- Persistent background/tray request: closing hides the main window; Quit Presence ends the process. Startup registration refreshes the stable executable path on normal launch. Install locally under LocalAppData/Programs/Presence and run --tray for this user.
- Final scope steering: any device joins/leaves the local network. PresenceEngine now emits one event per non-ignored device transition. First unknown confirmation uses only a new-device event, later reconnects use arrivals. Person grouping drives UI without extra person-level notifications. Known devices appear in Home Now/Away alongside people.
- Windows Computer Use capture failed with FrameArrived/window capture timeouts after one recovery attempt. --preview-image exports the actual native client window through WinForms DrawToBitmap with fictional devices, without scanning. --startup-trace optionally writes sanitized launch-stage markers under LocalAppData/PresenceBuild.
- Latest user instruction: build once and publish; fix only blocking errors. Skip tests, review, cleanup and repeated validation. Existing test project is an unused skeleton; no automated tests authored or run.

## 1.0.1 notification / automatic tracking change
- User reports Test alert produces no banner because all Windows notifications are disabled. Replace actual/test delivery with app-owned borderless topmost windows; no OS notification setting changes.
- Reference inspected live: remriel/codex-usage-counter, TrayMilestonePopup and _play_sound_alert. Popup above tray, application icon and bundled async WAV chime. Reuse owner's assets/usage-orbit-64.png and assets/milestone-alert.wav as embedded local resources, with provenance in notices.
- FloatingAlerts.cs separates notification lifecycle, bounded FIFO (20), overflow summary, sound and native popup. All presence events remain stored even if popup queue overflows.
- Popup doesn't steal keyboard focus (ShowWithoutActivation, WS_EX_NOACTIVATE / TOOLWINDOW, MA_NOACTIVATE); click opens device; × dismisses; 10-second default duration (3–60); hover pauses dismissal.
- WinMM PlaySound plays pinned embedded WAV bytes asynchronously. Sound defaults enabled; Settings can disable it. Windows/system audio volume still applies. No runtime audio download.
- Real events retain notification category/quiet-hour preferences. Test alert bypasses quiet hours/categories and uses the saved sound setting; it does not create device history.
- Devices automatically become present on first fresh ARP/mDNS response. Unknown section says Details, not Confirming. Person field is optional and enabled on any non-ignored device; entering a person automatically sets the association without a separate device-type approval.
- Screenshot export: --preview-alert-image produces the real native popup client bitmap with fictional text, without sound or discovery.
- Production publish succeeded for 1.0.1; native floating alert preview exported. No testing/review/cleanup added.

## Unresolved / proof boundary
- A physical phone disconnect/reconnect/sleep test requires user participation; deterministic tests cannot establish radio behavior.
- Native preview export completed; Windows Computer Use screenshots timed out twice. Live LAN discovery and phone/restart/toast acceptance were explicitly skipped under the build-once instruction.
- Release is unsigned. Source and a self-contained portable ZIP were published privately; startup/persistence across a real Windows logon and long-running CPU/RAM behavior are not verified.
- This release is IPv4-only. Sleeping clients/AP isolation/proxy ARP can make inferred joins/leaves differ from router association state. A /22 full baseline takes multiple two-minute cycles; recognized-device addresses are probed each cycle.
- No runtime errors were established by functional testing because none was requested after the build-once steering. Known uncertainty must not be presented as passed acceptance.

## Publication and local installation
- Private GitHub repository: https://github.com/remriel/presence-windows.
- Latest release v1.0.1: https://github.com/remriel/presence-windows/releases/tag/v1.0.1. Previous v1.0.0 remains available historically.
- Local stable install: %LOCALAPPDATA%/Programs/Presence. Updated to 1.0.1 and started --tray; startup defaults to enabled via HKCU Run. Data: %LOCALAPPDATA%/Presence/presence.db; existing database/settings were left intact. Preview is a separate Presence-Demo profile.
- Production build: dotnet publish with win-x64, self-contained, single-file, IncludeNativeLibrariesForSelfExtract, DebugType=None and local --artifacts-path. No trimming. OUI CSV accompanies the executable.
- GitHub initially rejected a shortened release target SHA with Release.target_commitish is invalid. Using full git rev-parse HEAD resolved the publication blocker.

## RESUME HERE
1.0.1 is built, privately published, installed locally and launched --tray. The native floating popup preview was exported with fictional content; audible sound/click/live network acceptance is unverified. Stop without tests/review/cleanup under build-once steering. The owner can use Settings > Test alert, with Windows notification banners still disabled. Devices auto-track with no approval; optional person association remains in Details. On a future request, read actual source/git state; preserve local database/settings and prefer local-disk build artifacts to avoid Google Drive apphost locking.
