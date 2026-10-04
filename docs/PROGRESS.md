# Presence progress

Current objective: publish Presence 1.0.2 as the reliability release for responsive LAN join/leave detection.

Release progress: **100% source-ready** `[████████████████████]`. The release commit triggers Windows CI, packages the self-contained x64 build, writes a SHA-256 checksum, and publishes the GitHub release after the build succeeds.

## 1.0.2 scope
- [x] Reduce default scan interval from 120 seconds to 10 seconds.
- [x] Reduce default departure grace from five minutes to 45 seconds.
- [x] Migrate old persisted default timing without discarding intentional custom settings.
- [x] Probe known devices before broad discovery on every cycle.
- [x] Sweep /24 and /23 networks in one cycle; rotate larger subnets in 512-address chunks.
- [x] Remove target truncation that could starve active discovery.
- [x] Keep cached neighbor entries candidate-only and require fresh ARP evidence.
- [x] Filter gateway/local MAC answers on unrelated targets to reduce proxy-ARP false positives.
- [x] Add deterministic core checks for baseline, departure, reconnect, and unknown-device semantics.
- [x] Add Windows CI publish, portable ZIP packaging, artifact upload, checksum generation, and release automation.
- [x] Update version metadata and 1.0.2 release notes.
- [ ] Physical device Wi-Fi off/on acceptance on the owner's actual LAN. This remains a manual environment check and does not block publication.

## Verification boundary
GitHub Actions verifies the core state machine and the self-contained Windows x64 publish. It cannot prove radio sleep behavior, AP client isolation, mesh/proxy-ARP behavior, local popup audio/click behavior, startup persistence across a real Windows logon, or long-running CPU/RAM behavior.

Repository: https://github.com/remriel/presence-windows

Release target: https://github.com/remriel/presence-windows/releases/tag/v1.0.2
