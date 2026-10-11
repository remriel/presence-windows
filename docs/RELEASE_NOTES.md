# Presence 1.2.0

- Select multiple devices directly on the main window and apply Ignore or Set known across sections. Person groups expand into individual devices; selection clears on network changes. Both actions remove person associations from the selected devices.
- Pause/resume scanning with an accessible icon button beside Settings. Pausing cancels outstanding scans, and resuming establishes a quiet baseline.
- Reset everything from Settings after confirmation: restore default settings and erase devices, people, assignments, event history and observations in a transaction. Cancel preserves data; failed database resets roll back.
- Leave the primary-phone checkbox unchecked for newly assigned devices, keep saved primary phones checked, and make the text 20% larger with a warm orange/yellow highlight.

Verified with core behavior and transactional persistence checks, focused fictional WinForms checks, visual inspection of light/dark and compact layouts, and a Windows x64 self-contained publish. Physical LAN transitions and real pause/resume timing remain manual acceptance.

# Presence 1.1.2

Devices excluded from notifications now remain visible in a counted **Ignored devices** section on the main window, below Unknown devices. Click a row to open its details and change **Track as** to restore normal tracking.

- Show all ignored devices on the current network, including devices that were previously marked away.
- Continue suppressing arrival, departure and new-device notifications for ignored devices.
- Keep saved device records and the existing current-network boundary during reconnects.
- Explain how to ignore and restore a device in the empty section and device details.

Verified with a Windows x64 self-contained publish, existing deterministic core checks, and fictional UI checks for visibility, editing, persistence, alert suppression and network changes. Light, dark and compact layouts were visually inspected. Real-LAN behavior and popup sound remain manual acceptance.

# Presence 1.1.1

Presence now automatically refreshes the active device list when Windows reports a network address or availability change. Switching networks clears the old live list and repopulates it as devices respond, without requiring Refresh or an app restart.

- Cancel obsolete scans, including gateway discovery, and reject their queued results/errors.
- Coalesce connection event bursts for 750 ms, then rescan automatically; normal scan intervals retry while DHCP/gateway access is unavailable.
- Restart background discovery when the local source address changes.
- Reset the quiet presence baseline on reconnect, including reconnecting to the same LAN, without false departure/arrival bursts.
- Show only devices and associated people on the current network. Saved names, assignments, inventory and activity history are retained.

Self-contained Windows 10 x64 and later portable build. Build and fictional transition screenshots are the verification boundary; physical Wi-Fi switching, sleep/resume and live network timing remain manual acceptance.
