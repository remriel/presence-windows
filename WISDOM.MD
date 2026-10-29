# Wisdom

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
