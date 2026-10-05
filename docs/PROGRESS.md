# Presence progress

Current objective: make successful internet speed tests measure for at least five seconds, publish and update the local app.
Progress: **90%** [██████████████████░░]

- [x] Replace short byte-limited samples with at least 2.5 seconds of timed measurement per direction.
- [x] Preserve cancellation and the 35-second deadline; use actual wall-clock duration for throughput.
- [x] Build once and package v1.0.5; GitHub publication is next.
- [x] Update the running local app and startup path to Downloads\Presence-1.0.5-win-x64.

No tests or live speed transfers run. Older installed-folder deletion remains blocked by the previously reported filesystem policy; do not retry through alternative mechanisms.

## RESUME HERE
The single production publish succeeded and v1.0.5 is running in the tray. Commit and publish the release assets next.

