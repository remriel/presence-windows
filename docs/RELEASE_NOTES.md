# Presence 1.0.2

Reliability release for local-network join and leave detection.

- Default monitoring interval reduced from 120 seconds to **10 seconds**.
- Default departure grace reduced from five minutes to **45 seconds**, configurable from 15 to 600 seconds.
- Known devices are re-probed first on every cycle so departure detection does not wait behind subnet discovery.
- Typical /24 and /23 home networks are actively swept in one cycle; larger networks rotate through bounded 512-address chunks.
- Removed target truncation that could let a large neighbor table starve active discovery.
- Initial silence now lasts one successful scan instead of a complete multi-cycle subnet sweep.
- Cached neighbor-table entries remain candidates only; fresh ARP resolution is still required as presence evidence.
- Gateway/local MAC responses for unrelated target addresses are ignored to reduce proxy-ARP false positives.
- Existing 1.0.1 timing settings migrate safely: the old 120-second / five-minute defaults become the new responsive defaults while intentional custom values are preserved.
- Added deterministic regression checks for baseline silence, departure grace, reconnect arrival, and first-seen unknown devices.
- Added Windows CI packaging and SHA-256 output for the portable x64 build.
- Retains 1.0.1 custom floating alerts/chime, automatic device tracking, optional person assignment, local SQLite history, and close-to-tray behavior.

Verification for 1.0.2: deterministic core checks and the self-contained Windows x64 publish build run in GitHub Actions. A physical phone Wi-Fi off/on test, sleeping-client behavior, mesh/AP isolation, and long-running resource behavior remain environment-dependent manual acceptance items.
