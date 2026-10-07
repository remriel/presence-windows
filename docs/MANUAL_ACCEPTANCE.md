# Manual acceptance

Automated core checks and the Windows publish build run in CI. These steps cover the real-LAN behavior CI cannot prove.

1. Extract the whole portable ZIP to a permanent local folder and run Presence.exe. Wait for the first successful silent scan; the status line reports coverage.
2. Devices are tracked automatically with no approval. Optionally open a phone, set Name, enter/select a person and choose the primary phone checkbox. Save.
3. A first fresh detection is enough to mark the device present. Confirm an associated person is under Home Now. A first-launch baseline should produce no burst of alerts.
4. Disconnect that phone's Wi-Fi for longer than the configured departure threshold while this computer remains awake and connected. Confirm one left event/floating popup. Reconnect and wait for the first fresh detection; confirm one arrived event and no repeated arrival alerts during subsequent scans.
5. Let the phone briefly sleep or miss replies for less than the threshold. Confirm no left event. Sleeping longer may require a higher threshold on that phone/network; the app cannot read an access point association table.
6. Change its DHCP address. Confirm its MAC/person mapping remains intact. For a changed private MAC, open the new device, choose the existing person, mark it primary and save. Confirm the previous primary no longer decides presence.
7. Quit from the tray and relaunch. Confirm saved names, assignments and settings persist; inspect Activity.
8. In Settings, use Test alert. Confirm the custom floating icon popup appears above the taskbar and plays the bundled chime even with Windows notification banners disabled. Click it to open Presence. Test alert follows the saved sound setting and bypasses quiet hours. Real device alerts open the device; app/system audio mute can silence playback.
9. Enable/disable arrival, departure and unknown alerts separately. Configure quiet hours; confirm suppressed alerts still have history records.
10. Put the computer to sleep or temporarily disconnect its network. Confirm Presence pauses monitoring and does not infer departures from unmonitored time.

Known boundaries: IPv4 LAN discovery only; no IPv6-only devices, no router credentials/integration, no certainty of human presence, no automatic private-MAC identity guesses. Wi-Fi isolation/proxy ARP/sleeping clients can affect visibility.

Internet speed test: click **Speed test** in the main window's bottom bar. It starts automatically and displays download/upload speed, median HTTPS latency and jitter. The request is on-demand, sends test traffic and your public IP to Cloudflare, and sends no Presence device IDs or history. Cloudflare states it collects test measurements for aggregated connection insights. Results are displayed by Presence and not stored locally.

Network change acceptance for v1.1.1:
1. Switch the PC from Wi-Fi network A to B with Presence running. Confirm the old live rows and network label clear during reconnect, then B devices populate without clicking Refresh. Stored A devices remain in Settings > Devices.
2. Disconnect and reconnect to the same network. Confirm fresh baseline population without a burst of arrival/left events, and names/person assignments stay intact.
3. Switch while a scan is running and switch rapidly twice. Confirm no obsolete scan result or error replaces the final network list.
4. Let DHCP change the local PC address; confirm discovery resumes with the new source address. Sleep/resume and unavailable-gateway time must not accumulate departures.
5. If an explicit adapter is selected in Settings, Presence continues to honor that choice; choose Automatic for automatic Wi-Fi/Ethernet adapter selection. IPv4-only and physical-adapter discovery constraints remain unchanged.
