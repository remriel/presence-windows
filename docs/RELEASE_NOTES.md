# Presence 1.0.1

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

Under the owner's build-once instruction, automated tests, live LAN/phone tests, popup click/sound and restart acceptance are handed to the owner. The native preview uses fictional devices. Manual acceptance is documented in MANUAL_ACCEPTANCE.md.
