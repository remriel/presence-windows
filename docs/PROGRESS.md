# Presence progress

Current objective: add the supplied per-device presence heatmap, publish Presence 1.4.0, and update the installed app without resetting its profile.

- [x] Inspect existing UI, event persistence, release workflow, and local install paths.
- [x] Implement a current-network device heatmap with 7/14/21/28-day ranges and a 14-day default.
- [x] Reconstruct daily estimates from saved arrival/departure history and state timestamps.
- [x] Add date-bounded, MAC-scoped event retrieval; keep existing history retention unchanged.
- [x] Add 15 timeline preview checks for light/dark themes, ranges, tooltips, and empty state.
- [x] Capture and inspect fictional-device screenshots; add the 14-day preview to project docs.
- [x] Update 1.4.0 release notes, application version, README, and CI workflow.
- [x] Pass all local regression suites: core, timeline (15), saved networks (11), visibility (16), reliability (16), broad UI (26 captures), and bulk/reset/pause (24 checks).
- [x] Build without warnings; publish the self-contained x64 app, verify package contents/checksum, and launch its fictional preview.
- [ ] Commit and open the release PR; wait for CI and merge through the repository release workflow.
- [ ] Verify the public release assets and package checksum/version.
- [ ] Update `%LOCALAPPDATA%\\Programs\\Presence` without touching `%LOCALAPPDATA%\\Presence\\presence.db`; resolve the existing startup target carefully.
- [ ] Launch/verify the updated installation and finalize these handoffs.

Local profile location: `%LOCALAPPDATA%\\Presence\\presence.db`. A running Presence process currently uses a build-output path and the startup entry points there. Preserve the profile. Never commit device data, real-device screenshots, credentials, or diagnostics.
