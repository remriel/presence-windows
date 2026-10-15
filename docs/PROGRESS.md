# Presence progress

Current objective: deliver the finished audit patch, portable build, and reviewable GitHub PR.
Progress: **95%** [###################-]

- [x] Reconcile instructions and clean main 531ad4e; isolate concurrent checkout work.
- [x] Reproduce and fix replayed sightings, newest-sample selection and backwards-clock recovery.
- [x] Fix transaction-local observation throttling, exclude identity-only metadata, and resume writes after a backwards clock change.
- [x] Omit computed display/type properties from persisted snapshots.
- [x] Fix the broader preview's missing icon resource and add that suite to Windows CI.
- [x] Original and added core tests pass; Release native build has zero warnings/errors.
- [x] Visibility: 16 checks/zero findings/6 captures; bulk/reset/pause: 24 checks/zero findings/12 captures; broader smoke: 26 captures/zero layout findings.
- [x] Push implementation fcb1880; open and attach PR #4.
- [x] Windows CI passes for fcb1880: https://github.com/remriel/presence-windows/actions/runs/38693354265.
- [x] Initial portable archive integrity verified; packaged smoke exposed a second-disposal shutdown exception.
- [x] Make application-context disposal idempotent, guard repeated Exit, and add an explicit repeated-disposal native regression.
- [ ] Rebuild/archive the shutdown fix, verify packaged exit, synchronize final documentation, and export handoffs.

PR: https://github.com/remriel/presence-windows/pull/4 (open; not merged).
Checkout: C:/Drive/2026-10-14/fix-all-bugs-and-optimize/work/presence-audit
Branch: fix/audit-reliability-20261014
Build root: %LOCALAPPDATA%/PresenceBuild/Audit20261014

Known unresolved work: editor save-failure rollback gaps and a mismatch between documented bounded discovery and full-subnet background sweeps remain follow-up work after the user's request to finish. Physical LAN, sleeping-device and suspend/resume behavior needs manual acceptance. This patch does not establish that all conceivable application bugs are fixed. The installed app and real profile were not changed.

Exact ordered next steps:
1. Complete the shutdown-fix portable rebuild and package verification.
2. Update/push PR #4 and inspect final CI status; export both handoffs.
3. Review/merge the PR when ready; no release or installation has been performed.
4. Future work: add failure-injection tests for editor saves and deterministic tests for bounded discovery before changing those paths.
