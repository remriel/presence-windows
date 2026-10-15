# Presence progress

Current objective: finish the reproduced presence/persistence fixes and preview repair, verify them, and deliver a portable audit build and GitHub PR.
Progress: **90%** [##################--]

- [x] Reconcile instructions, source and clean main at 531ad4e.
- [x] Isolate work in work/presence-audit after concurrent edits appeared in the original checkout.
- [x] Reproduce stale/replayed sightings, oldest-sample selection, clock recovery and observation-write defects with failing checks.
- [x] Fix session observation freshness, newest-sample selection, clock recovery, transaction-local throttling and metadata filtering.
- [x] Stop serializing computed display properties and repair the obsolete UI-preview icon resource.
- [x] Pass original and added core checks; Release native build has zero warnings/errors.
- [x] Pass 16 visibility checks (6 captures), 24 bulk/reset/pause checks (12 captures), and broader UI smoke (26 captures, zero findings).
- [x] Add broader UI smoke to Windows CI; inspect a fictional native screenshot.
- [ ] Complete self-contained portable publish and packaged smoke check.
- [ ] Commit/push, create PR, inspect CI, and export final handoffs.

Implementation: focused audit patch on branch fix/audit-reliability-20261014 in C:/Drive/2026-10-14/fix-all-bugs-and-optimize/work/presence-audit. No UI redesign. No real profile or installed application changed.

Known unresolved work: the source review also identified editor save-failure rollback gaps and a mismatch between documented bounded discovery and full-subnet background sweeps. Those broader changes were not implemented after the user requested finishing up. Real LAN, sleeping devices and Windows suspend/resume remain manual acceptance. This patch does not establish that every possible application bug is fixed.

Exact ordered next steps:
1. Finish portable publish and fictional packaged smoke check.
2. Commit and push the patch; open and attach a PR; inspect Windows CI.
3. Update this checklist with exact PR/artifact/check references and export both handoffs to outputs.
4. Future work: reproduce editor save failures and bounded discovery in dedicated native tests before changing those paths.
