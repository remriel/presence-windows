# Presence progress

Current objective: release Presence 1.2.0 with full reset, main-window bulk Ignore/Set known, pause/resume, and a larger highlighted primary-phone checkbox.

Progress: **90%** [##################--]

- [x] Read repository instructions and handoffs; reconcile clean GitHub main with source.
- [x] Implement transactional reset and main-window device selection.
- [x] Add pause/resume with cancellation and quiet baseline.
- [x] Make primary phone opt-in, 20% larger, warm orange/yellow.
- [x] Pass core inference and reset rollback/persistence tests.
- [x] Build self-contained Windows x64 app without warnings/errors.
- [x] Finish focused screenshot verification: 24 behavior checks, zero layout findings, 12 captures.
- [ ] Commit, push, create and merge PR; verify CI.
- [ ] Publish 1.2.0 ZIP/checksum and verify release asset digest.
- [ ] Export final project state and progress.

Implementation: source complete, version 1.2.0. Reset removes all profile tables in one transaction and cancels obsolete scans, queued observations/events and alerts. Defaults come from Settings. Bulk selection uses MAC identities scoped to the current network; person groups expand to devices; bulk classification removes associations. Pause is runtime-only and blocks automatic/explicit scans until resumed. Existing primary phones remain checked.

Verification: core checks and transactional reset checks pass. Initial focused UI run passed 22 behavior checks with zero findings. Expanded persistence checks passed, but minimized preview windows caused invalid tiny captures; preview now restores forms before capture. This affected the harness, not the production build. Final rerun passed 24 behavior checks with zero layout findings; 12 captures visually reviewed. All test devices are fictional. No real profile was reset or changed.

Checkout: C:\Drive\2026-10-11\ao\work\presence-windows
Branch: feat/reset-and-bulk-devices
Build: %LOCALAPPDATA%\PresenceBuild\ResetBulk\portable
Screenshots: C:\Drive\2026-10-11\ao\outputs\presence-verification

Blockers: none.

Exact next steps:
1. Run corrected focused preview, inspect reset and bulk captures, resolve any findings.
2. Package the already-published executable with README/notices and checksum.
3. Commit/push, create PR, wait for CI, merge and publish 1.2.0 targeting full merge SHA.
4. Verify release digest and export both handoffs to outputs.
