# Presence progress

Current objective: release Presence 1.2.0 with full reset, main-window bulk Ignore/Set known, pause/resume, and a larger highlighted primary-phone checkbox.

Progress: **100%** [####################]

- [x] Read repository instructions and handoffs; reconcile clean GitHub main b646e151ac4c4b1fcc6f5035ad733b0c6c7cd6b0 with source.
- [x] Implement transactional reset and main-window selection across device sections.
- [x] Add pause/resume with cancellation and quiet baseline.
- [x] Make primary phone opt-in, 20% larger, warm orange/yellow; preserve saved primary flags.
- [x] Pass core inference and reset rollback/persistence/reopen/cache tests.
- [x] Pass 24 focused UI checks, zero layout findings; visually inspect 12 fictional captures.
- [x] Publish self-contained Windows x64 app without warnings/errors; launch packaged app in demo mode.
- [x] Push source, create/attach/merge PR #2; verify passing GitHub Windows CI.
- [x] Publish 1.2.0 ZIP/checksum and verify release asset digest matches local archive.
- [x] Export final project state and progress to task outputs.

Implementation: Reset removes all profile tables in one transaction and cancels obsolete scans, queued observations/events and alerts. Defaults come from Settings. Bulk selection uses MAC identities scoped to the current network; person groups expand to devices; Ignore/Set known removes associations. Pause is runtime-only and blocks automatic/explicit scans until resumed. New primary assignments are unchecked; saved primary phones remain checked.

Verification: deterministic core and transactional reset tests pass, including injected failure rollback, empty profile reopen and cleared observation throttle. Focused fictional native WinForms suite passed 24 behavior checks with zero layout findings and 12 captures. Light/dark, compact selection, reset, primary control and pause icon were visually inspected. The packaged executable launched in demo mode, file version 1.2.0.0. Real LAN pause/resume timing, sleep/network switching and sound remain manual acceptance. No real profile was reset or changed, and no installed app was replaced.

Repository: https://github.com/remriel/presence-windows
Checkout: C:\Drive\2026-10-11\ao\work\presence-windows
Branch: main
Feature commit: 459899a
Merged PR: https://github.com/remriel/presence-windows/pull/2
Release: https://github.com/remriel/presence-windows/releases/tag/v1.2.0
Release target: 02afe990131156b989422e93e87b9d88c3e03242
Passing CI: https://github.com/remriel/presence-windows/actions/runs/38179914500
Package: C:\Drive\2026-10-11\ao\outputs\Presence-1.2.0-win-x64.zip
ZIP SHA-256: 01af0efb2eee95d5ad87f559ef3638d73cc5acee302128f4842692d099a84c19
Executable SHA-256: e85635decc424ccf96a7665a23b0edae1c30a4dbfd8a8bcad3ebd0d9be512fb2
Build: %LOCALAPPDATA%\PresenceBuild\ResetBulk\portable
Screenshots and verification.json: C:\Drive\2026-10-11\ao\outputs\presence-verification

Blockers: none.

Exact ordered next steps:
1. No requested implementation or release work remains. Release 1.2.0 is published with verified ZIP/checksum.
2. If installation is requested, close the old app, back up binaries, replace the installation with this package, and preserve %LOCALAPPDATA%\Presence\presence.db. Installing does not invoke reset.
3. Use docs/MANUAL_ACCEPTANCE.md if physical LAN validation is requested. Never reset the real profile for tests.
4. Before future work, read AGENTS.md and both handoffs and reconcile git/source/tests.
