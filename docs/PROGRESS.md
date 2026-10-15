# Presence progress

Current objective: publish Presence 1.2.2 from the consolidated fixes in PR #5, as explicitly requested on 2026-10-15.
Progress: **35%** [#######-------------]

- [x] Reconcile instructions, Git, source and both handoffs.
- [x] Discover PR #4 is superseded/closed; verify consolidated PR #5 is mergeable and its latest Windows CI passed.
- [x] Review the consolidated changes and test coverage; prepare version 1.2.2, README links and release notes.
- [ ] Push release metadata and pass PR Windows checks.
- [ ] Merge PR #5 with the release-trigger commit title; verify main CI publishes v1.2.2.
- [ ] Download the published ZIP/checksum; verify tag, digest, binary version and packaged fictional preview.
- [ ] Synchronize final handoffs with GitHub and export them to outputs.

Active checkout: C:/Drive/2026-10-14/fix-all-bugs-and-optimize/work/presence-audit
Local branch: release/presence-1.2.2; source PR branch: fix/reliability-performance.
PR: https://github.com/remriel/presence-windows/pull/5
Pre-release-metadata CI passed: https://github.com/remriel/presence-windows/actions/runs/38694662855

The consolidated implementation includes the earlier audit plus save rollback, disconnected-adapter retention, bounded discovery and hidden-window work reductions. Prior 1.2.1 preview archives are historical. The installed application and real profile are outside this release task.
Blockers: none. Physical LAN/sleeping-device/suspend behavior remains manual acceptance, and large-network first discovery may take longer with bounded batches.

Exact ordered next steps:
1. Commit/push the release metadata to PR #5 and wait for its Windows checks.
2. Merge with title Release Presence 1.2.2 to trigger the existing CI release workflow.
3. Download and independently verify the released package, then record/export final handoffs.
