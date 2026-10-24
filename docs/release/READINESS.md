# Release readiness — Maze of Egos 0.2.0 (17), competition candidate

October 24, 2026. Build 17 adds a single selectable picture/name card, a one-cell
water-balloon splash radius with matching warning and landing effect, immediate
round completion after the last heart, and round portraits with names/points below.
Snapshot V4 migrates V1/V2/V3 and persists actual short-round timing and result reason.
122 game checks, native tests in three Mac formats/both iPhone landscapes, real OS lifecycle and normal Release 17 installation/startup pass.
All five retain the shared map and local combat information policy from build 16.
Manual gameplay acceptance remains open. No upload or distribution is performed.

Build 16's 300-second Release measurement is retained as historical evidence;
it is not repeated or presented as a build 17 performance measurement.

## Historical build 15 evidence

October 24, 2026. N2–N4 are implemented in the normal player flow. N5's fresh
900-second physical iPhone Release measurement, final native QA and normal
installation pass. Normal build 15 is running on iPhone 13; manual gameplay
acceptance remains open. See the [test report](../TEST_REPORT.md).
No App Store Connect submission or TestFlight distribution is included.

PLAY opens a separate character selection. Any of the five characters can be
the human; the remaining four are local AI with the same movement and item
rules. Hearts score points during two-minute rounds. Tied leaders share the
round trophy. The match ends at three trophies, with total points breaking
simultaneous third-trophy ties and equal totals sharing the match.

The close local 3D camera and screen-oriented automatic movement remain.
Water balloons and cream pies have heading-based, wall-blocked paths and
visible warnings. Banana and oil only delay; boost speeds up movement. KO
retains points/inventory, waits five active seconds and returns the player to
the centre with three seconds of protection. Contact is harmless.

| Deliverable | Current evidence / acceptance |
| --- | --- |
| Competition rules, resources and settlement | 24 competition checks, 19 arena checks and 65 classic regressions pass |
| Production AI and combat stress | Five complete matches, one per human character: 26 rounds / 187,200 active ticks, 7,132 hearts, 293 items, 48 KO; ten additional seeded stress rounds / 72,000 ticks / 120 checkpoints |
| Local information policy | Observation-only brain, persisted memory and counterfactual tests for unseen actor/resource changes |
| Native Mac | PASS at 1600×900, 2400×1080 and 1600×1200; 14 new captures per format |
| Physical iPhone 13 | PASS final check, 16 fresh captures including gameplay/controls and safe areas in both landscape orientations |
| Actual iOS background/foreground | PASS: 9.51 seconds in final background check, identical frozen combat/AI/disk snapshot, resume countdown and Playback audio policy |
| Versioned storage | V2 combat/AI snapshot in independent competition-v1.json; V1 migration, atomic backup and actual Unity JSON continuation pass; classic files and old highscore remain separate |
| Normal Release archive | Local signature/configuration audit passed; normal build 15 installed/launched with no test flags and same PID after 8 s; classic files/settings byte-preserved; QA classes absent |
| Fresh long Release measurement | PASS 900.003 s, 59.964 fps, p95 18 ms, worst 46.12 ms; no frames >50 ms; all 181 thermal samples nominal; resident peak 279.64 MiB |
| English UI and metadata | New HUD/help/selection/results and updated English store/privacy drafts |
| Illustrated main menu | Original supplied image; PLAY / CONTINUE SAVED GAME / HELP on the right; no added title or character overlays |
| Manual gameplay acceptance | Open: readability, reaction time, tactics, fingers on controls, and a complete hand-played match on iPhone |
| Physical iPad | Not performed; user declined this round |
| Art/animation quality | Existing review models retained. All five have seven common clips; the four non-Zelenskyy models use their Surprise gesture for throws |
| Public release | Support/privacy publication, asset/maze rights, final owner/store forms, screenshots and distribution remain separate review work |

## Local deliverables and scope

- `Builds/macOS/Maze of Egos.app`: development app with opt-in native checks.
- `Builds/iOS`: development Xcode export, separate Debug DerivedData.
- `Builds/iOS-Performance`: dedicated Release profile with local performance recorder.
- `Builds/iOS-Release`: normal Release export without automation.
- `Builds/Archives/MazeOfEgos-0.2.0-15.xcarchive`: audited development-signed local archive; not an App Store distribution export.
- `docs/verification/2026-10-24-build15-*`: new reports, selected actual framebuffer captures and source hashes.
- [Competition handover](../COMPETITION_HANDOVER.md): exact code roles, balance, storage and reproduction on PC/Mac.

Normal and QA saves use separate locations. All builds, logs and caches are
ignored by Git. The implementation and documentation have not been committed
or pushed in this work session; copying the workspace preserves local changes,
while a fresh clone only includes commits.

The local archive audit checks iOS 17 minimum, iPhone/iPad device family,
English localization, both landscape orientations, permission keys, RGB icon,
privacy manifests, signature and absence of test code. It does not replace
Apple validation or manual gameplay/art acceptance. Unity services/analytics,
ads, accounts and in-app purchases remain disabled. The manifest declares no
tracking or collected data types.

The support URL is the user's chosen existing endpoint. Public website content
has not been updated or revalidated as part of this build. The [English metadata](APP_STORE_EN.md)
and [privacy policy](PRIVACY_POLICY_DRAFT.md) are local review drafts.
