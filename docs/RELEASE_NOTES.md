# Presence 1.0.0

Windows 10 x64 portable, self-contained C# tray utility.

- Automatic local-network scans every two minutes and manual Refresh.
- Arrival/departure events for every device except explicitly ignored devices.
- Silent initial baseline, two consecutive detections for arrivals, five-minute default departure tolerance.
- Fresh ARP + ICMP + local mDNS discovery, offline vendor lookup, no port scanning/cloud/telemetry.
- Optional person assignments and primary-phone replacement for private MAC changes.
- Local SQLite mappings, history, observations and settings.
- Native Windows toasts with click activation; independent notification switches and quiet hours.
- Light, dark and system appearance; native minimal lists, details, activity and settings.
- Persistent tray operation, close-to-tray and automatic Windows startup enabled by default.

Production publish succeeded. Under the owner's build-once instruction, automated tests, live LAN/phone tests, toast activation and restart acceptance were not performed. The native preview uses fictional devices. Manual acceptance is documented in MANUAL_ACCEPTANCE.md.
