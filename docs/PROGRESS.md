# Presence progress

Current objective: fix reproducible bugs and optimize Presence, with verified source, portable build and durable handoffs.
Progress: **100%** [####################]

- [x] Reconcile instructions, handoffs, Git and source; baseline main 531ad4e.
- [x] Reproduce/fix stale observations, batch ordering, monitoring gaps and backwards clock correction.
- [x] Deduplicate observation writes, exclude metadata and reduce snapshot JSON.
- [x] Roll back failed settings/device edits, including displaced primary phones and new people.
- [x] Preserve disconnected adapters and reject stale editors after reset.
- [x] Bound discovery, reduce ICMP concurrency, start workers lazily and isolate network lanes.
- [x] Skip hidden-window rendering; fix preview resource and repeated shutdown/disposal.
- [x] Pass original core suites and 11 added reliability checks.
- [x] Pass 16 native reliability, 16 visibility and 24 bulk/reset/pause checks; zero findings.
- [x] Pass broad UI smoke: 26 captures, zero layout findings; inspect light/dark fixtures.
- [x] Build/publish self-contained Windows x64 with zero warnings/errors.
- [x] Verify ZIP digest, extracted binary equality, version and packaged fictional preview.
- [x] Push PR #5, verify implementation Windows CI and export handoffs.

Branch: fix/reliability-performance. PR is open; no merge, new tagged release, installation or production-profile reset was performed.
PR: https://github.com/remriel/presence-windows/pull/5
Verified implementation: afc87a0f6334dff22b06f8129a19673ecfa1be14
Passing CI: https://github.com/remriel/presence-windows/actions/runs/38694066264
Latest checks: https://github.com/remriel/presence-windows/pull/5/checks

Package: outputs/Presence-1.2.1-fixes-afc87a0-win-x64.zip (57,473,271 bytes).
SHA-256: 0320a1416a0ccf6e9348298356e0025220e92575356c0952c024e434fd888fe6
File version 1.2.1.0; product version 1.2.1+afc87a0f6334dff22b06f8129a19673ecfa1be14.
This is a PR preview; published release remains v1.2.1.

Evidence: outputs/verification has 47 native fixture captures/JSON reports plus packaged preview. All devices/databases are fictional and isolated. No real device data or captures were committed.
Blockers: none. Real LAN reconnect/sleep, OS suspend/resume and long-term resource use remain physical acceptance. Bounded discovery can increase first discovery time on large networks; known-device priority probes remain independent. Tests do not prove every possible bug absent.

Exact ordered next steps:
1. Review PR #5 and its latest checks; implementation/package verification is complete.
2. Merge/publish a tagged release only when requested.
3. If installing, quit the tray app and extract the whole package while preserving %LOCALAPPDATA%/Presence/presence.db.
4. Follow docs/MANUAL_ACCEPTANCE.md for physical network checks.
5. For future work, read AGENTS.md and both handoffs, then reconcile actual Git/source/tests.
