# Presence progress

Current objective: replace disabled Windows toast delivery with a custom floating icon notification/chime, automatically track every non-ignored LAN device without confirmation, retain optional associations with people, and publish/install 1.0.1.

Verified progress: **85%** `[█████████████████░░░]` toward this update's build-and-publish handoff. Functional acceptance is deferred under the user's build-once instruction.

- [x] Read current AGENTS, state/progress, git state and affected source.
- [x] Inspect the live Codex Usage Counter reference popup/sound implementation.
- [x] Implement app-owned popup, embedded reference icon/chime, queue, click/dismiss, hover pause, sound/duration settings.
- [x] Route real events and Test alert through custom notification delivery.
- [x] Remove two-detection/approval requirement; first fresh discovery automatically tracks arrival.
- [x] Keep people association optional and make it directly available in device details.
- [x] Publish Windows x64 production build once; no blocking compiler errors.
- [x] Export requested native popup progress image with fictional content.
- [ ] Replace stable local install, preserve mappings/settings and restart background/tray process.
- [ ] Publish portable/source packages and private GitHub release; provide state/progress docs.

Completed previous release: 1.0.0 source and portable published; installed locally and started --tray. This update is prompted by user-reported Test alert failure with Windows notifications disabled.

Current implementation: self-contained x64 production publish succeeded. Native popup image exported with fictional text. Packaging, local replacement and GitHub 1.0.1 publication are next. Automatic interval stays two minutes with manual Refresh; five-minute absence tolerance and silent first baseline remain. No OS notification settings changed.

Blockers: none currently. No tests, linting, code review, cleanup, repeated successful builds or live-network verification authorized under build-once steering.

Checks performed: source/reference inspection and asset fetch needed for implementation; production publish succeeded. Requested native popup bitmap export completed. No automated or functional tests, audio listening tests or live network verification performed for 1.0.1.

Exact ordered next steps:
1. Publish the changed Windows x64 code once using local-disk artifacts.
2. Export native popup preview, package portable files and preserve source.
3. Replace local installed executable and launch --tray.
4. Commit/push source, publish v1.0.1, deliver docs and stop.

Repository: https://github.com/remriel/presence-windows (private).

Manual acceptance for owner: Settings > Test alert should display the custom icon popup/chime with Windows notification banners off. Check a real device arrival, click-to-open, quiet hours, optional association, and a tolerated departure when desired.
