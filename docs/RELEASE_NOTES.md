# Presence 1.0.2

Windows 10 x64 portable, self-contained C# tray utility.

- Automatic local-network scans every two minutes and manual Refresh.
- Arrival/departure events for every device except explicitly ignored devices.
- Silent initial baseline, first fresh response for arrivals with no confirmation, five-minute default departure tolerance.
- Fresh ARP + ICMP + local mDNS discovery, offline vendor lookup, no port scanning/cloud/telemetry.
- Optional person assignments directly from device details, plus primary-phone replacement for private MAC changes. Unassigned devices continue automatic tracking/alerts.
- Local SQLite mappings, history, observations and settings.
- Custom floating icon popups and the Codex Usage Counter chime, independent of Windows notification banners. Click opens the device; × dismisses. Ten-second default duration, hover pause, optional sound, independent notification switches and quiet hours. Test alert uses this same delivery path.
- Light, dark and system appearance; native minimal lists, details, activity and settings.
- Persistent tray operation, close-to-tray and automatic Windows startup enabled by default.

This release packages the current Presence app behavior as Windows x64 single-file self-contained v1.0.2. Under the owner's build-once workflow, automated tests, live LAN and phone checks, popup click/sound checks and restart acceptance are handed to the owner. The included preview has fictional content; see MANUAL_ACCEPTANCE.md.
