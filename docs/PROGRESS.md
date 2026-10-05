# Presence progress

Current objective: make successful internet speed tests measure for at least five seconds, publish and update the local app.
Progress: **100%** [████████████████████]

- [x] Replace short byte-limited samples with at least 2.5 seconds of timed measurement per direction.
- [x] Preserve cancellation and the 35-second deadline; use actual wall-clock duration for throughput.
- [x] Build once, package and publish v1.0.5.
- [x] Update the running local app and startup path to Downloads\Presence-1.0.5-win-x64.

No tests or live speed transfers run. Older installed-folder deletion remains blocked by the previously reported filesystem policy; do not retry through alternative mechanisms.

## RESUME HERE
Completed: v1.0.5 is published and running in the tray. Release: https://github.com/remriel/presence-windows/releases/tag/v1.0.5. No further build or testing is required for this change.


