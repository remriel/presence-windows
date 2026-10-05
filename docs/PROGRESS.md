# Presence progress

Objective: finish publishing Presence 1.0.3 with fast local-device tracking, bold responsive UI and a click-to-run internet speed test.

Verified progress: **90%** `[██████████████████░░]` toward GitHub publication.

- [x] Read and merge current GitHub changes into the release source; reconcile actual files and docs.
- [x] Implement 3-second scan defaults, streamed observations, evaluation only for actually probed devices and bounded background discovery.
- [x] Add bottom-bar internet speed test for download/upload Mbps, median HTTPS latency and jitter; starts after click and cancels if its dialog closes.
- [x] Add Cloudflare disclosure, use no Presence device identifiers, persist no speed metrics.
- [x] Apply the specified high-contrast neo-brutalist light/dark theme, responsive device rows and render caching.
- [x] Fix the first blocking error by importing `Presence.Core` in `InternetSpeedTest.cs`.
- [x] Publish one Windows x64 self-contained single-file production build. It succeeded with no diagnostics.
- [x] Export the native main-window screenshot using the built app with fictional devices.
- [x] Build the versioned portable app ZIP, docs and preview assets.
- [ ] Commit and push final merged source/docs to the private GitHub repository.
- [ ] Publish GitHub v1.0.3 and upload portable/source/docs/screenshot assets.

Test traffic is never started during normal monitoring or publishing. When manually run, Cloudflare sees the public IP and test traffic and may retain measurements for aggregated connection-quality insights. Presence does not upload device or person data.

Checks: successful production publish for the final source. No tests or live speed/Wi-Fi acceptance were run; they remain manual. Upstream CI evidence run 37237103022 predates this release.

Exact remaining steps:
1. Resolve Git merge and push the prepared v1.0.3 source to main.
2. Publish the private GitHub v1.0.3 release and attach Windows ZIP, source ZIP, preview and progress/state docs.
3. Set docs/PROGRESS and docs/PROJECT_STATE to released state, upload docs/source snapshot, and hand off without more checks.

Repository: https://github.com/remriel/presence-windows (private).
Previous release: https://github.com/remriel/presence-windows/releases/tag/v1.0.2.
