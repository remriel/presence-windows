# Wisdom

## Presence 1.5.0 — 2026-10-19

1. **Least confident:** Hour-by-hour estimates can look more precise than the underlying LAN signals. Sleeping or isolated devices may not answer probes. I would compare the hourly graph with real reconnects and clearly treat it as recorded network evidence.
2. **Biggest missing point:** Device presence is only a proxy for whether a person is home. I would keep that limit visible in the timeline copy and avoid using the graph as proof of human presence.
3. **Highest-impact assumption:** “24 hour view” means the rolling last 24 elapsed hours divided into 24 one-hour bins. If the user meant a selected calendar day or a different time grouping, I would adapt the selector and labels before release.

## Presence 1.5.1 — 2026-10-19

1. **Least confident:** The no-scroll result was verified for 20 fictional rows on this PC's 144-DPI, 1942×1056 display. I would also check common lower-resolution and scaling combinations before making a general capacity claim.
2. **Biggest missing point:** A maximized window has a finite readable capacity. Lists larger than the viewport can still scroll, and users with more than 20 devices may need another way to navigate. I would gather the normal device count and add filtering or grouping if it regularly exceeds the tested capacity.
3. **Highest-impact assumption:** Twenty rows are enough for the user's usual timeline. If that is wrong, I would design around the actual count with filtering or grouped sections instead of shrinking text further.
