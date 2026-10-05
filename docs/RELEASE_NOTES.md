# Presence 1.0.3

Windows 10 x64 portable, self-contained C# tray utility.

- Fast local device presence with a 3-second scan default, streamed observations, first-response tracking and 30-second departure grace.
- Fresh ARP/ICMP/mDNS, prioritized known devices, full /24 and /23 scans, bounded larger subnet rotation, proxy-ARP filtering, silent startup baseline, no ports or router credentials.
- Local SQLite events, mappings and settings. Person associations remain optional.
- Floating icon alerts and the bundled chime work when Windows banners are off.
- The new bottom-bar **Speed test** opens on demand for download/upload, median HTTP latency and jitter using Cloudflare's nearest edge; the popup auto-starts and can be closed to cancel.
- Speed test has a 35-second cap and does not save results. Cloudflare receives the public IP and test traffic; its documented service collects test measurements for aggregated internet-quality insights. Presence never sends discovered-device identifiers, mappings or history.
- Bold high-contrast light/dark controls, responsive device rows, persistent tray and Windows startup.

Production build passed for Windows x64 self-contained v1.0.3. No new automated tests or live Wi-Fi/speed-test acceptance was run. Manual instructions are in MANUAL_ACCEPTANCE.md.
