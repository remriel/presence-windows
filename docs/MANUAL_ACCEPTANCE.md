# Manual acceptance

Production publication deliberately skipped testing at the user's request. These steps are for the owner, on the normal home Wi-Fi network.

1. Extract the whole portable ZIP to a permanent local folder and run Presence.exe. Wait for the first full silent sweep; the status line reports coverage.
2. Open an unknown phone. Set Name, choose Person / presence device, enter the person's name and select the primary phone checkbox. Save.
3. Wait for two successful detections. Confirm the person is under Home Now. A first-launch baseline should produce no burst of alerts.
4. Disconnect that phone's Wi-Fi for longer than the configured departure threshold while this computer remains awake and connected. Confirm one left event/notification. Reconnect and wait for two detections; confirm one arrived event and no repeated arrival alerts during subsequent scans.
5. Let the phone briefly sleep or miss replies for less than the threshold. Confirm no left event. Sleeping longer may require a higher threshold on that phone/network; the app cannot read an access point association table.
6. Change its DHCP address. Confirm its MAC/person mapping remains intact. For a changed private MAC, open the new device, choose the existing person, mark it primary and save. Confirm the previous primary no longer decides presence.
7. Quit from the tray and relaunch. Confirm saved names, assignments and settings persist; inspect Activity.
8. In Settings, use Test alert. Click the Windows toast and confirm it opens the device. Windows Focus Assist can suppress the banner.
9. Enable/disable arrival, departure and unknown alerts separately. Configure quiet hours; confirm suppressed alerts still have history records.
10. Put the computer to sleep or temporarily disconnect its network. Confirm Presence pauses monitoring and does not infer departures from unmonitored time.

Known boundaries: IPv4 LAN discovery only; no IPv6-only devices, no router credentials/integration, no certainty of human presence, no automatic private-MAC identity guesses. Wi-Fi isolation/proxy ARP/sleeping clients can affect visibility.
