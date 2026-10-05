# Presence progress

Current objective: publish and deliver Presence v1.0.3 with responsive local device detection, a bottom-bar internet speed test and the refreshed native UI.

Verified progress: **100%** `[████████████████████]` for the requested build/package/GitHub-release handoff. Runtime speed and LAN acceptance were not tested.

- [x] Reconcile latest GitHub source/metadata and release branch.
- [x] Update LAN discovery to stream fresh observations on a three-second cadence; keep known-device checks prioritized and timely.
- [x] Add the user-triggered internet download/upload/latency/jitter speed test at the bottom of the main window.
- [x] Add Cloudflare disclosure; no Presence device data is transmitted and no test results are saved.
- [x] Apply the bold high-contrast palette and responsive, lower-churn main UI.
- [x] Publish the final self-contained Windows x64 build once; fix the missing `Presence.Core` namespace identified by that build.
- [x] Export the native main-window screenshot with fictional devices.
- [x] Package the portable archive and SHA-256 checksum.
- [x] Push the merged source and release docs to the private GitHub repository.
- [x] Create/publish GitHub release v1.0.3 and upload Windows, source, documentation and screenshot assets.
- [x] Publish final progress/state/manual-acceptance docs.

Current release: https://github.com/remriel/presence-windows/releases/tag/v1.0.3
Repository: https://github.com/remriel/presence-windows (private).

Verification: the production publish completed. This release workflow did not run local tests or live speed-test/Wi-Fi acceptance. Cloudflare test behavior, audio playback, phone sleep and router isolation remain unverified runtime behavior; see `MANUAL_ACCEPTANCE.md` for owner steps.

The speed test runs only after clicking **Speed test**. Cloudflare sees the public IP and speed-test traffic and states it collects results for aggregated connection insights. Presence sends no MAC, device name, person mapping or history and stores no test result.

## RESUME HERE
The build, portable/source packaging, GitHub synchronization and v1.0.3 release upload are complete. Do not rebuild or add validation unless requested. Owner can launch Presence and click **Speed test** in the bottom bar; the window reports download/upload, latency and jitter. Live WAN and LAN behavior requires owner acceptance.
