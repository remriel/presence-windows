# Presence progress

Current objective: package the current Presence feature set as v1.0.2 and publish the portable app, source, docs and popup preview on the private GitHub release.

Verified progress: **75%** `[███████████████░░░░░]` toward v1.0.2 GitHub publication. Functional acceptance remains deferred under the user's build-once instruction.

- [x] Read current AGENTS, state/progress, git state and affected source.
- [x] Inspect the live Codex Usage Counter reference popup/sound implementation.
- [x] Implement app-owned popup, embedded reference icon/chime, queue, click/dismiss, hover pause, sound/duration settings.
- [x] Route real events and Test alert through custom notification delivery.
- [x] Remove two-detection/approval requirement; first fresh discovery automatically tracks arrival.
- [x] Keep people association optional and make it directly available in device details.
- [x] Update assembly/package and release notes to v1.0.2.
- [x] Publish one Windows x64 production build to local-disk artifacts.
- [x] Package portable app ZIP.
- [ ] Package current source with final progress/state docs.
- [ ] Publish v1.0.2 portable/source packages and fictional popup preview on GitHub.

Completed previous release: 1.0.0 source and portable published; installed locally and started --tray. This update is prompted by user-reported Test alert failure with Windows notifications disabled.

Current implementation: v1.0.2 self-contained x64 production publish succeeded. Portable ZIP is packaged. Source/docs and GitHub release publication remain.

Blockers: none currently. No tests, linting, code review, cleanup, repeated successful builds or live-network verification authorized under build-once steering.

Checks performed: project/documentation and GitHub history read; v1.0.2 production publish succeeded. No automated or functional tests requested.

Exact ordered next steps:
1. Copy portable ZIP/preview and state/progress docs into task outputs.
2. Commit/push package version and docs, create GitHub release v1.0.2 and upload portable/preview assets.
3. Update progress/state to published, create the source snapshot and upload source/docs assets.
4. Report the release URL and hand off without tests or review.

Repository: https://github.com/remriel/presence-windows (private).

Previous published release: https://github.com/remriel/presence-windows/releases/tag/v1.0.1.

Manual acceptance for owner: Settings > Test alert should display the custom icon popup/chime with Windows notification banners off. Check a real device arrival, click-to-open, quiet hours, optional association, and a tolerated departure when desired.
