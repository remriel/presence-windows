# Presence progress

Current objective: audit Presence 1.2.1, repair reproducible reliability bugs, reduce unnecessary work, and deliver a verified portable build and GitHub pull request.
Progress: **70%** [##############------]

- [x] Read instructions and handoffs; reconcile clean main at 531ad4e with source and GitHub.
- [x] Create branch `fix/reliability-performance`; confirm no open issues or PRs.
- [x] Baseline core tests and Release build pass without warnings or errors.
- [x] Add failing regression coverage for discovered defects (seven initial failures).
- [x] Implement presence and persistence fixes; all eight new core checks pass.
- [x] Implement save rollback, adapter preservation, bounded discovery, lazy workers, hidden-window rendering and preview resource fixes.
- [ ] Run native fixture suites, inspect screenshots, and publish a portable build.
- [ ] Push branch, create PR, verify Windows CI, and export final handoffs.

Current findings under investigation:
- Device/settings editors mutate live state before persistence and do not roll back on save failure.
- Discovery continuously enumerates the full subnet for ARP and up to 4,096 ICMP addresses per pass, contradicting the documented bounded sweep.
- The full UI preview suite references the obsolete `Presence.AlertIcon` resource.
- Review ongoing for stale observations, cancellation, background failures, database hot paths, and idle UI work.

Verification: original core suite and all eight added reliability checks pass. Native regression/build and package verification are in progress. Current build artifacts use `%LOCALAPPDATA%/PresenceBuild/Reliability/artifacts`. A slow local shared compiler was bypassed with `UseSharedCompilation=false`; do not mistake a running build for a passing result.
Blockers: none. Physical LAN, sleeping-device behavior, and Windows suspend/resume require real-world acceptance; fixture testing cannot prove all possible bugs absent.

Exact ordered next steps:
1. Finish source audit and reproduce the identified defects in deterministic core/native tests.
2. Implement focused fixes and record design choices in PROJECT_STATE.md.
3. Run core and native regressions, inspect captures, publish and smoke-test the package.
4. Synchronize changes to GitHub via a PR and verify CI.
5. Export PROGRESS.md and PROJECT_STATE.md with the final deliverables.
