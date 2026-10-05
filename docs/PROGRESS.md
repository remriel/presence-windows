# Presence progress

Current objective: publish v1.1.0 with one Presence icon across the executable, title bars, tray and floating alerts, while keeping the installed PC copy current.
Progress: **95%** [███████████████████░]

- [x] Finish the v1.1 native UI, on-demand speed test and Fing-style README with fictional screenshots.
- [x] Make `remriel/presence-windows` public.
- [x] Create a production Presence icon and replace the unrelated alert image and drawn tray lettermark.
- [x] Publish one self-contained Windows x64 build and package the updated archive.
- [x] Replace `C:\Presence\Presence.exe` and confirm it runs from the startup location with the existing data file intact.
- [ ] Push the icon/source/docs commit and tag `v1.1.0` at that commit.
- [ ] Replace the old draft ZIP and checksum with the new package; publish and verify the public GitHub release.

The production publish succeeded. The new archive SHA-256 is `6069ee29aa682aea7eac0bb19c833a7af9c1498d3eb8a12b3d69e022fe5f716b`. The app runs from `C:\Presence`; the user database remains in `%LOCALAPPDATA%\Presence\presence.db` and the Run key points to `C:\Presence\Presence.exe --tray`. No lint, automated tests, code review or repeated validation were run, as requested. The floating alert screenshot from the earlier icon was removed from README so it does not misrepresent this build.

## RESUME HERE
Commit and push the current icon/UI documentation changes, tag the resulting commit `v1.1.0`, replace the existing draft release's old Windows ZIP and checksum, then publish the draft. After publication, record the release URL and final status in this file and `PROJECT_STATE.md`, and push that documentation update.
