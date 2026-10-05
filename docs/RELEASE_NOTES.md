# Presence 1.0.3

Windows 10 x64 portable, self-contained C# tray utility.

- Local-network scan every 3 seconds by default with a manual Refresh button. Fresh observations update the interface as they arrive.
- First fresh response tracks each non-ignored device. One successful scan forms the silent baseline. Departure grace defaults to 30 seconds and is configurable from 10 seconds to one hour.
- Known addresses are prioritized; /24 and /23 subnets are swept each pass while larger networks rotate through bounded batches.
- Local ARP, ICMP and mDNS discovery, no port scanning or router credentials.
- Optional person associations and primary phone assignment. All device history stays in local SQLite.
- Floating icon alerts and bundled chime work with Windows notification banners disabled.
- A compact bottom-bar Speed test button immediately measures internet download/upload speeds, median latency and jitter to Cloudflare. It uses only short test transfers and stops after 35 seconds.
- Speed test is entirely on demand. Cloudflare receives your public IP and test traffic; it collects test measurements for aggregated internet connection insights. Presence does not send device, person or history data and does not store test results.
- Bold, high-contrast light/dark colors, readable device lists and persistent system-tray operation.

No physical Wi-Fi or live internet speed acceptance was performed for this package. Manual acceptance instructions are in MANUAL_ACCEPTANCE.md.
