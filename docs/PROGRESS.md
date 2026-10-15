# Presence progress

Current objective: deliver the finished audit patch, verified portable build, and reviewable GitHub PR.
Progress: **100% of the finished patch delivered** [####################]

- [x] Reconcile instructions and clean main 531ad4e; isolate concurrent checkout work.
- [x] Reproduce and fix replayed sightings, newest-sample selection and backwards-clock recovery.
- [x] Fix transaction-local observation throttling, exclude identity-only metadata, and resume writes after a backwards clock change.
- [x] Omit computed display/type properties from persisted snapshots.
- [x] Repair the broader preview's icon resource; add broader native smoke to Windows CI.
- [x] Fix repeated application-context disposal and repeated Exit; verify explicit repeated-disposal regression.
- [x] Original and added core tests pass. Release native build and self-contained publish: zero warnings/errors.
- [x] Visibility/shutdown: 17 checks/zero findings/6 captures; bulk/reset/pause: 24 checks/zero findings/12 captures; broader smoke: 26 captures/zero layout findings.
- [x] Inspect fictional screenshots; verify extracted archive binary equals publish; packaged preview exits with code 0.
- [x] Push both implementation commits, create/attach/update PR #4, and export final handoffs.

PR: https://github.com/remriel/presence-windows/pull/4 (open; not merged).
Code/package commit: 71e11c92f04da38bbfbc3abedfe664499bd5ab4f
Checkout: C:/Drive/2026-10-14/fix-all-bugs-and-optimize/work/presence-audit
Branch: fix/audit-reliability-20261014
Initial implementation CI passed: https://github.com/remriel/presence-windows/actions/runs/38693354265
Final shutdown-fix CI: https://github.com/remriel/presence-windows/actions/runs/38693668802 (passed).
Portable audit archive: outputs/Presence-audit-71e11c9-win-x64.zip
SHA-256: 2a7ac2f9eaae8021f0b5c815337373cdf07c1279062bb96e53ecabd46ca68c57
File version: 1.2.1.0; informational version: 1.2.1+71e11c92f04da38bbfbc3abedfe664499bd5ab4f. This is a branch audit build, not a published release.
Packaged capture: outputs/verification/packaged-smoke-final.png

Known unresolved work: editor save-failure rollback gaps and a mismatch between documented bounded discovery and full-subnet background sweeps remain follow-up work after the user's request to finish. Physical LAN, sleeping-device and suspend/resume behavior needs manual acceptance. This patch does not establish that all conceivable application bugs are fixed. The installed app and real profile were not changed.

Exact ordered next steps:
1. Local verification, final Windows CI, and package delivery are complete; review the linked evidence.
2. Review/merge PR #4 when ready. No release or installation has been performed.
3. Future work: add failure-injection tests for editor saves and deterministic bounded-discovery tests before changing those paths.
4. For physical LAN acceptance, follow docs/MANUAL_ACCEPTANCE.md using a backed-up profile.
