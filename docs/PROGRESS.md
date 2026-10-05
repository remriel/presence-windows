# Presence progress

Current objective: publish Presence 1.0.3 with responsive local device tracking, the bottom-bar internet speed test, and the bold high-contrast UI.

Verified progress: **88%** `[██████████████████░░]` toward GitHub publication.

- [x] Merge the latest scanner updates from origin/main and reconcile source/docs.
- [x] Add a 3-second scan loop with streamed local observations, faster first results and responsive main-list rendering.
- [x] Add bottom-bar Speed test for Cloudflare download/upload/latency/jitter; on click only, cancellable, limited to 35 seconds.
- [x] Document Cloudflare public-IP/test-traffic disclosure and that Presence device/history data is never sent or speed results saved.
- [x] Restyle main view and speed test with bold neo-brutalist light/dark colors and responsive rows.
- [x] Run the single Windows x64 self-contained production publish; fixed its one blocking missing-namespace compile error and the corrected build succeeded.
- [x] Export the native main-window preview with fictional devices.
- [x] Package the portable v1.0.3 ZIP with app files and docs.
- [ ] Commit/push merged source and final docs to the private GitHub repository.
- [ ] Create GitHub release v1.0.3 and upload portable/source/docs/screenshot.

Build completed once for this final code state. No local tests, review, cleanup, or live speed/network acceptance were run. Upstream scanner CI had already passed for the earlier scanner-only code.

No app data was used in the speed test and no test request was made during publishing. The current local installation/database remains unchanged.

Next: commit/push, create release v1.0.3, attach source archive and progress/state docs, then hand off. Do not rerun the production build or test suite.
