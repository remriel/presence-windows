# Third-party notices

Presence depends on:

- Microsoft.Data.Sqlite 10.0.12: MIT, [dotnet/efcore](https://github.com/dotnet/efcore).
- SQLitePCLRaw.bundle_e_sqlite3 3.0.5 and its native/runtime dependencies: Apache-2.0 for SQLitePCLRaw, [SQLitePCL.raw](https://github.com/ericsink/SQLitePCL.raw); SQLite is public domain, [SQLite copyright](https://www.sqlite.org/copyright.html).
- CommunityToolkit.WinUI.Notifications 7.1.2: MIT, [Windows Community Toolkit](https://github.com/CommunityToolkit/WindowsCommunityToolkit).
- Microsoft .NET runtime and Windows SDK projections: licenses supplied by their upstream packages, [.NET license](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT).
- Offline manufacturer lookup: unmodified [IEEE public OUI CSV](https://standards-oui.ieee.org/oui/oui.csv), downloaded October 4, 2026. IEEE is the source of the assignments; no endorsement is implied.
- Floating notification icon and chime: reused at the owner's request from [remriel/codex-usage-counter](https://github.com/remriel/codex-usage-counter), `assets/usage-orbit-64.png` and `assets/milestone-alert.wav`, downloaded October 4, 2026. Embedded in Presence; no runtime downloads occur.

The source package retains this notice. Runtime components are included solely to make the Windows portable application self-contained.
