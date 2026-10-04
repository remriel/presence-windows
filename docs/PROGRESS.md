# Presence progress

Current objective: replace disabled Windows toast delivery with a custom floating icon notification/chime, automatically track every non-ignored LAN device without confirmation, retain optional associations with people, and publish/install 1.0.1.

Verified progress: **100%** `[████████████████████]` for this update's requested build-and-publish handoff. Functional acceptance is deferred under the user's build-once instruction; this percentage does not claim tested notification/audio/network behavior.

- [x] Read current AGENTS, state/progress, git state and affected source.
- [x] Inspect the live Codex Usage Counter reference popup/sound implementation.
- [x] Implement app-owned popup, embedded reference icon/chime, queue, click/dismiss, hover pause, sound/duration settings.
- [x] Route real events and Test alert through custom notification delivery.
- [x] Remove two-detection/approval requirement; first fresh discovery automatically tracks arrival.
- [x] Keep people association optional and make it directly available in device details.
- [x] Publish Windows x64 production build once; no blocking compiler errors.
- [x] Export requested native popup progress image with fictional content.
- [x] Replace stable local install, preserve the existing database/settings and restart background/tray process.
- [x] Publish portable package and private GitHub release v1.0.1; commit/push completed source and documentation.
- [x] Provide source snapshot and state/progress files with the release handoff.

Completed previous release: 1.0.0 source and portable published; installed locally and started --tray. This update is prompted by user-reported Test alert failure with Windows notifications disabled.

Current implementation: self-contained x64 production publish succeeded; native popup image exported with fictional text; portable release v1.0.1 published. Local installed copy at %LOCALAPPDATA%/Programs/Presence was replaced and launched --tray. Automatic interval stays two minutes with manual Refresh; five-minute absence tolerance and silent first baseline remain. All non-ignored devices auto-track on first fresh response; person association is optional. No OS notification settings changed.

Blockers: none currently. No tests, linting, code review, cleanup, repeated successful builds or live-network verification authorized under build-once steering.

Checks performed: source/reference inspection and asset fetch needed for implementation; production publish succeeded. Requested native popup bitmap export completed. No automated or functional tests, audio listening tests or live network verification performed for 1.0.1.

Exact ordered next steps:
1. Owner opens Presence from the tray and uses Settings > Test alert for manual popup/chime acceptance.
2. Optionally associate devices with people from Details; no device approval is needed for tracking or alerts.
3. Further tests/review/changes await the owner's next request. No additional validation scheduled.

Repository: https://github.com/remriel/presence-windows (private).

Latest published release: https://github.com/remriel/presence-windows/releases/tag/v1.0.1.

Manual acceptance for owner: Settings > Test alert should display the custom icon popup/chime with Windows notification banners off. Check a real device arrival, click-to-open, quiet hours, optional association, and a tolerated departure when desired.
