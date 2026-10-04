# Presence

A quiet Windows 10 tray utility that alerts when any device joins or disappears from your own local network. Optionally assign phones to people to infer who is home. All device mappings, observations and event history stay on this computer.

## Start

1. Extract the portable archive to a permanent folder on a local disk.
2. Launch **Presence.exe**. No administrator account or .NET runtime installation is required.
3. Allow the silent initial network sweep to complete. All discovered devices are tracked automatically; nothing needs approval. Optionally open a device, enter a name or associate a person, and choose their primary phone.
4. Close the window. Presence keeps running in the system tray. **Start quietly with Windows** is enabled by default; disable it in Settings if desired. Use **Quit Presence** in the tray menu to stop the background process.

Windows 10 x64 version 1809 or later is the minimum target. The release is unsigned; signing requires an owner's signing certificate. Runtime data is under `%LOCALAPPDATA%\Presence\presence.db`. The portable binary does not carry your device data.

## How it works

Presence chooses a connected physical Wi-Fi adapter first, or you can choose an adapter in Settings. It detects its local IPv4 subnet and gateway. Fresh ARP resolution supplies the principal signal, supplemented by ICMP and local mDNS replies. A cached ARP entry only supplies an address to probe; it is not counted as evidence of presence.

The default interval is **two minutes**. The main window's **Refresh** button starts an immediate scan; Settings permits intervals from 30 to 600 seconds. Each cycle prioritizes discovered addresses and scans 128 additional addresses. A /24 is swept in about two cycles; larger networks take longer. The first full sweep is silent. The app supports physical IPv4 LANs from /16 through /30. It does not enumerate IPv6-only devices. It pauses departure inference when the gateway is unreachable or the computer suspends. There is no router-specific integration. One fresh discovery response is enough to track a device and infer arrival; no confirmation or second detection is required.

An arrival is inferred from the first fresh response. A departure requires five minutes of absence during healthy monitoring, configurable from two to sixty minutes. Brief missed replies leave the device probably home. Every non-ignored device generates its own arrival/departure events, including unassigned devices and phones. An unidentified device's first join uses a single “New device joined” alert; subsequent reconnects use arrival alerts. A primary phone determines the person's presence; without a primary phone, any assigned presence device can keep them home. Person grouping does not add duplicate notifications. Known devices appear in Home Now and Away; unidentified devices appear in Unknown Devices. Ignored devices never notify. Associating a person does not require a separate device-type confirmation: enter/select the person and save.

Private MAC changes cannot safely be matched automatically. Open the new device, select the existing person, and mark it as their primary phone. This replaces the old primary while retaining its history. Manufacturer lookup uses a bundled offline IEEE OUI table; private MACs do not reveal a reliable manufacturer.

## Privacy and limitations

No cloud account, telemetry, web server, subscriptions, login, port scanning, vulnerability scanning, packet interception or runtime external requests. Discovery uses ARP, ICMP and a local mDNS query. Run it only on a network you own or have permission to monitor. History is a local SQLite database protected by the Windows user profile, not application-level encryption. Event retention defaults to 90 days; detailed observations to seven days. Ignored devices remain in local records but never produce alerts.

**Device presence is a proxy for human presence.** Sleeping phones may not respond even while associated with Wi-Fi. Client isolation, proxy ARP, private MAC changes and roaming can affect detection. A router association table could improve certainty, but Presence deliberately does not require router access. Quiet hours suppress notifications while preserving events.

Alerts are **custom floating icon popups above the taskbar**, with a bundled chime matching the Codex Usage Counter. They do not use Windows toast or balloon notifications, so disabled Windows notification banners do not suppress them. Popups stay above ordinary windows without taking keyboard focus; click one to open its device, or click × to dismiss. They close after ten seconds by default (3–60 seconds in Settings), pausing that countdown while hovered. Sound is enabled by default and can be disabled in Settings; normal Windows/app audio volume still applies. Quiet hours and the independent arrival/departure/unknown switches apply to real events. **Test alert** displays immediately even during quiet hours; it follows the saved sound setting. A bounded queue handles event bursts, with excess changes summarized while all history remains stored.

Activity shows recorded events. Settings → Devices includes known, absent and ignored devices. There is one process per user/profile; launching again opens the existing window. The tray process stays alive independently of the main window.

## Build and modes

Requires .NET SDK 10 on Windows:

```powershell
dotnet publish src/Presence.App/Presence.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o artifacts/portable
```

Do not trim WinForms/WinRT. `Presence.exe --tray` starts hidden. `--demo` uses fictional devices in a separate profile and disables discovery and notifications. `--preview-image C:\path\preview.png` exports the native client window with fictional devices and exits. `--preview-alert-image C:\path\alert.png` exports the custom popup with fictional text and exits without sound. `--diagnose --output C:\path\report.json` performs one local scan and writes only sanitized counts/timings, with no MAC/IP/hostname export. `--startup-trace` writes local sanitized startup phase markers, without device identifiers. `--unregister` removes legacy toast registrations; use it only when uninstalling after quitting the tray app. Disable startup in Settings before removing the portable folder.

The source separates `Presence.Core` (identity, inference, persistence), `Discovery.cs` (LAN discovery), `PresenceContext.cs` (tray, notification service, lifecycle), and `Windows.cs` (native UI).

## Release verification boundary

This release follows the requested **build once and publish** workflow. The production build is the validation gate. Automated tests, live discovery tests, phone reconnect/sleep tests, popup click/sound tests and restart persistence tests were not run. Screenshots show the built native UI with explicitly fictional preview devices.

Manual acceptance after launch:

- Confirm your phone appears on home Wi-Fi and assign it as primary.
- Reconnect it and confirm a single arrival after its first fresh detection.
- Leave it briefly asleep and confirm no departure within the configured threshold.
- Quit and relaunch; confirm person/device mappings persist.
- Send a test alert in Settings; check the floating popup/chime and click it to open Presence.

## Sources / third-party components

- [Microsoft ResolveIpNetEntry2 documentation](https://learn.microsoft.com/en-us/previous-versions/windows/hardware/device-stage/drivers/ff570686(v=vs.85)) explains fresh ARP resolution.
- [Microsoft notification compatibility API](https://learn.microsoft.com/en-us/dotnet/api/communitytoolkit.winui.notifications.toastnotificationmanagercompat) describes native desktop toast activation.
- [IEEE OUI database](https://standards-oui.ieee.org/oui/oui.csv), bundled as a local vendor lookup snapshot downloaded October 4, 2026. The CSV remains unmodified; no runtime download occurs.
- Microsoft.Data.Sqlite / SQLitePCLRaw / SQLite and Windows Community Toolkit retain their upstream licenses. NuGet package metadata supplies license details; see THIRD_PARTY_NOTICES.md.
- Floating alert icon/chime reused from the owner's [Codex Usage Counter](https://github.com/remriel/codex-usage-counter), at `assets/usage-orbit-64.png` and `assets/milestone-alert.wav`, downloaded October 4, 2026 and embedded locally.
