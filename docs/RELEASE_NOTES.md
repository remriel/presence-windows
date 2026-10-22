# Presence 1.6.0 (upcoming)

- Categorize each device with one checkbox choice: Phone, Laptop, Desktop, Tablet, TV / streaming, Game console, Printer, Speaker, Smart home, Camera, Wearable, NAS / storage, Network equipment or Other.
- Keep Automatic detection available; unchecking a manual category restores the latest hostname hint. Distinguish laptops from desktops in recognizable hostname hints.
- Use labeled category colors in the home list and inventory, plus themed device-name colors and category tooltips in daily/hourly timelines. Presence heatmap colors retain their existing meaning.
- Adapt category colors to light/dark/system appearance and reflow checkbox choices at narrow window sizes.
- Preserve category choices across restarts, hostname enrichment and tracking changes. Cancel and failed saves preserve the previous state; failed saves remain retryable.

Existing profiles retain their devices, settings, people and history without a reset or schema migration. Category, tracking behavior and primary-phone selection remain independent.

# Presence 1.5.1

- Open the device timeline maximized with a large restored size and an enabled maximize control.
- Fit timeline rows and columns to the available window area so more devices are visible without scrollbars. A 20-device fictional preview fits on the development PC's 144-DPI display.
- Keep full device names available in tooltips when compact rows are needed to fit the screen.

Existing profiles and the 24-hour and daily timeline ranges are preserved.

# Presence 1.5.0

- Add a rolling 24-hour timeline with one color-coded hour per column, alongside the existing 7-, 14-, 21- and 28-day views.
- Show exact local time intervals, UTC offsets, presence duration and recorded-state coverage in hourly tooltips, including across daylight-saving transitions.
- Keep unknown time neutral and preserve the existing local-only history and retention behavior.

Existing profiles are preserved without a reset. The hourly view estimates device presence from recorded transitions; it does not guarantee that a person or device was physically home.

# Presence 1.4.0

- Add a per-device daily presence timeline to the main window, with 7-, 14-, 21- and 28-day ranges.
- Estimate each day's presence from recorded arrival/departure transitions and saved state timestamps; use tooltips to inspect daily presence and recorded-state coverage.
- Show away and unrecorded time in neutral gray, keep the graph local, and retain the existing event-history retention policy.

Existing profiles are preserved without a reset. The timeline is an estimate based on recorded presence transitions, not a guarantee that a person or device was physically home.

# Presence 1.3.0

- Add Networks from the main window to remember/save the connected network and rename it later.
- Default the name to the selected adapter's Wi-Fi SSID. If Windows restricts SSID access or the connection is wired, use the adapter/subnet description as an editable fallback.
- Recognize saved names by the existing adapter/subnet/gateway identity when reconnecting; identical names do not merge separate networks.
- List saved networks and allow forgetting a name while preserving devices, people and activity.
- Persist names locally, roll back failed saves/forgets, reject saving after the network changes, and clear saved names with Reset everything.

Existing profiles are preserved without a reset. SSID availability and real Wi-Fi reconnect behavior depend on Windows permissions and the local network.

# Presence 1.2.2

- Prevent replayed observations, older samples and monitoring gaps from producing false presence changes. Resume accepting fresh sightings after backwards clock correction.
- Restore prior settings and device/person assignments when saving fails, including displaced primary phones and newly created people. Reject stale editors after a reset.
- Preserve a selected disconnected adapter when saving unrelated settings.
- Rotate background ARP and ICMP discovery through batches of at most 512 addresses, respecting the scan interval. Reduce ICMP concurrency while retaining independent known-device priority probes.
- Start ARP workers only when needed, isolate obsolete network lanes and defer inventory rendering while the window is hidden or minimized.
- Reduce snapshot serialization and observation writes; metadata-only hostname enrichment is not stored as physical presence evidence.
- Fix repeated shutdown/disposal and the broader native preview's embedded icon reference.

Verified with deterministic core checks, 11 added core reliability checks, 56 focused native checks, a 26-capture broader UI smoke suite, and self-contained Windows x64 packaging. Physical LAN, sleeping devices and suspend/resume remain manual acceptance. Bounded sweeps may increase initial discovery time on large networks. Existing local data is preserved; no reset is required when updating.

# Presence 1.2.1

- Keep known and unidentified devices visible in the main window. Uncertain recognized devices appear under Checking presence, and unidentified devices retain their actual Home/Recent/Away/Unknown label.
- Establish Away silently for previously observed saved devices that remain absent after restart or reconnect, once healthy evaluated monitoring reaches the departure threshold.
- Show orphaned person-device records individually rather than dropping them from the dashboard.
- Try background reverse DNS alongside mDNS, with bounded concurrency, timeouts, retries and matching-address validation. Hostname lookup cannot create devices, revive absent devices, change last-seen timestamps or trigger arrival alerts.
- Show likely device types from recognizable hostnames in device details and unidentified-device rows. Ambiguous devices remain Unknown; tracking choices and custom names remain user-controlled.
- Discover service-only mDNS response senders even when the packet has no hostname A record.

Verified with deterministic core/persistence checks, injected DNS success/failure/timeout/cancellation fixtures, native light/dark UI visibility checks, existing bulk/reset/pause checks and self-contained Windows publishing. Real network hostname availability and physical LAN timing require manual acceptance.
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
