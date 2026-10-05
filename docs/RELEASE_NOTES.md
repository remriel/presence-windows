# Presence 1.0.4

Windows 10 x64 portable, self-contained C# tray utility.

- Fast local device presence with a 3-second scan default, streamed observations, first-response tracking and 30-second departure grace.
- Fresh ARP/ICMP/mDNS, prioritized known devices, full /24 and /23 scans, bounded larger subnet rotation, proxy-ARP filtering, silent startup baseline, no ports or router credentials.
- Local SQLite events, mappings and settings. Person associations remain optional.
- Floating icon alerts and the bundled chime work when Windows banners are off.
- The new bottom-bar **Speed test** opens on demand for download/upload, median HTTP latency and jitter using Cloudflare's nearest edge; the popup auto-starts and can be closed to cancel.
- Speed test has a 35-second cap and does not save results. Cloudflare receives the public IP and test traffic; its documented service collects test measurements for aggregated internet-quality insights. Presence never sends discovered-device identifiers, mappings or history.
- Settings and device dialog labels now receive the selected theme throughout nested controls, fixing black text on the dark background while preserving muted label contrast.
- The main window stays compact and readable, with responsive device rows, persistent tray and Windows startup.

The Windows x64 self-contained production publish completed. No automated tests or live Wi-Fi/speed-test acceptance are part of this release. Manual instructions are in MANUAL_ACCEPTANCE.md.
