# Wisdom — timeline and 1.11.0 release, 2026-11-06

1. **Least confident:** Live network arrival/departure timing has only been checked with fictional fixtures, not a physical device disconnect. I would compare the installed timeline with a real device's reconnect and last response.
2. **Biggest missing point:** Last seen is the last successful observation, while an away transition is confirmed later after a grace period; timeline cells estimate presence rather than prove physical presence. I would use the exact-time tooltip and avoid treating a missed response as definitive proof of departure.
3. **Highest-impact assumption:** The request means highlighting one latest state-relevant time per device: the last join while present or the last observation while away. If every historical transition should remain marked, I would extend the timeline to show all recorded transitions instead.

# Wisdom — Presence 1.10.0, 2026-11-05

1. **Least confident:** Five-minute cells estimate presence from the same stored transitions; they do not provide more frequent observations. I would describe them as estimates and inspect the release preview.
2. **Biggest missing point:** Publishing doesn't update either installed copy by itself. I will back up the profile on each computer and verify its own target before installation.
3. **Highest-impact assumption:** “Altogether” means ship the 1h/2h/3h interval changes, stronger timeline colors and already merged bug fixes in v1.10.0. If the second computer is offline, I can publish and update REMY but must wait for NOVA to be reachable before claiming both are updated.

# Wisdom — bug audit, 2026-11-04

1. **Least confident:** Physical LAN reconnect/sleep behavior and the installed app were not exercised. I would next compare real reconnect timing with observed events; automated fixtures cannot establish that every bug is gone.
2. **Biggest missing point:** The merged GitHub source does not automatically update the installed 1.9.0 executable. I would publish and install a versioned package when an app update is requested.
3. **Highest-impact assumption:** The request targets the GitHub repository. If the intended result is an updated running app, I would additionally back up its profile, install a verified build and check preserved settings/history.

# Wisdom — Presence 1.9.0

1. Least confident: Live UI interaction was not verified; inspect the installed timeline when Windows UI control is available.
2. Biggest missing point: Minute cells do not increase the frequency of underlying presence measurements; compare against a real reconnect if needed.
3. Highest-impact assumption: Ranges are rolling snapshots; add automatic refresh if continuous updates are intended.

# Wisdom

## Short timeline ranges — 2026-11-03

1. **Least confident:** The installed live UI could not be clicked because Windows computer control returned an access-denied error. I would inspect the new choices interactively when that access is available; fictional native layouts passed on this PC.
2. **Biggest missing point:** Smaller time cells do not add new presence measurements; they subdivide estimates reconstructed from the same recorded arrivals, departures and saved state. I would compare a real device's reconnect with the timeline if actual network timing is important.
3. **Highest-impact assumption:** The requested durations refer to rolling intervals ending at the time the timeline is rendered. If calendar-aligned or continuously refreshing ranges were intended, I would adjust their time boundaries or refresh behavior.

## 12-hour timeline and interface refresh — 2026-10-29

1. **Least confident:** The live profile's scan interval, departure grace and retention settings differed from the pre-update backup while Presence was running, and the source of those changes is unclear. I would compare immediately before and after app restart and ask before restoring any earlier values; preserving the live state was the safest choice.
2. **Biggest missing point:** The auto-known option affects devices as they join, not unidentified devices already online. Ignore and person assignments stay explicit. I would keep that scope clear in the setting and only bulk-convert existing devices if the user asks.
3. **Highest-impact assumption:** “12-hour chart” means a rolling 12 elapsed hours, and “make all devices known automatically when joining” means unknown devices become known on discovery/next join while explicit Ignore/person choices remain. A calendar-day chart or retroactive reclassification would change the behavior, so I would make the selection and scope explicit if requested.


## Device categories — 2026-10-21

1. **Least confident:** The category UI passed native and CI checks, but other physical displays still need hands-on use. I would test another display/DPI setup. The CI history stall also showed that fixtures must cover previous-date timestamps, not depend on the runner's current timezone.
2. **Biggest missing point:** Category is independent of tracking behavior and primary-phone selection. Categorizing a laptop or phone does not change presence inference or notifications. I would keep these controls separate and their effect explicit.
3. **Highest-impact assumption:** Category color belongs on category labels and timeline device names, while timeline heatmap cells keep showing recorded presence. If the user intended entire rows or a new heatmap meaning, I would extend the styling after confirming that meaning. One category per device is user-confirmed, not an assumption.

## Presence 1.5.0 — 2026-10-19

1. **Least confident:** Hour-by-hour estimates can look more precise than the underlying LAN signals. Sleeping or isolated devices may not answer probes. I would compare the hourly graph with real reconnects and clearly treat it as recorded network evidence.
2. **Biggest missing point:** Device presence is only a proxy for whether a person is home. I would keep that limit visible in the timeline copy and avoid using the graph as proof of human presence.
3. **Highest-impact assumption:** “24 hour view” means the rolling last 24 elapsed hours divided into 24 one-hour bins. If the user meant a selected calendar day or a different time grouping, I would adapt the selector and labels before release.

## Presence 1.5.1 — 2026-10-19

1. **Least confident:** The no-scroll result was verified for 20 fictional rows on this PC's 144-DPI, 1942×1056 display. I would also check common lower-resolution and scaling combinations before making a general capacity claim.
2. **Biggest missing point:** A maximized window has a finite readable capacity. Lists larger than the viewport can still scroll, and users with more than 20 devices may need another way to navigate. I would gather the normal device count and add filtering or grouping if it regularly exceeds the tested capacity.
3. **Highest-impact assumption:** Twenty rows are enough for the user's usual timeline. If that is wrong, I would design around the actual count with filtering or grouped sections instead of shrinking text further.
