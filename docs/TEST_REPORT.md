# Verifikation — senest opdateret 24. oktober 2026

## Build 17 — selectable character cards, splash hits, early rounds and portrait results

October 24, 2026. All functional checks and normal Release installation/startup pass. No commit, push or Apple upload.

| Verification | Result |
| --- | --- |
| Classic / foundation / arena | **65 + 34 + 23 = 122 passed**, zero failures |
| Character selection | Native taps on the actual photo and name for every character select the correct human; picture/name share one Button rectangle |
| Balloon landing radius | Radius = 1 map cell; all active unprotected opponents inside/on boundary hit, none outside; wall/protection rules retained; same radius for warning and hit test; 0.3 s visible impact hits only once |
| Balloon persistence | Restore on last airborne tick gives identical hits/cleanup; entering the visible post-impact splash does not cause a later hit |
| Early round finish | Last heart awards its point before settlement; empty board ends immediately; reason recorded as HeartsCollected; timeout remains 120 s |
| Short-round clock / settlement | Real cumulative active ticks continue into following rounds; save/restore preserves start tick and reason; trophies/points settle exactly once; corrupt timing/phase headers reject atomically |
| Results | Actual character portraits, names and round points stacked below each picture for gold/silver/bronze; all shared places shown; at least three shared gold winners expand across width; overall trophy table below |
| Production AI matches | Five full matches / 34 rounds / 133,356 active ticks (2,222.6 simulated seconds), 8,160 hearts, 250 items and 42 KO |
| Serialization | Actual Unity Newtonsoft V1/V2/V3→V4 migration and deterministic combat/AI continuation pass; normal legacy files remain separate |
| Managed / assets | 0 managed warnings/errors; original maze, supplied images/model hashes and 152 unique metadata GUIDs pass |
| Native Mac | PASS at 1600×900, 2400×1080 and 1600×1200; 17 fresh captures per format |
| Physical iPhone 13 | PASS with 19 fresh captures; actual photo/name taps, splash KO, last-heart settlement + save/continue, result portraits, both landscapes/safe areas and simultaneous controls |
| Actual OS background/foreground | PASS 9.384301 s; frozen combat/AI/timers and matching disk checkpoint; Playback audio policy after foreground |
| Normal Release archive | PASS local signature/configuration/privacy audit; normal 0.2.0 (17), QA harness absent |
| Final normal startup | PASS 0.2.0 (17) installed and launched without test flags; same PID after at least 8 s; installed version/executable match; classic saves/settings and backups byte-preserved |

[Character cards](verification/2026-10-24-build17-iphone-03-selection.png),
[landing splash](verification/2026-10-24-build17-iphone-08b-balloon-splash.png),
[last-heart result](verification/2026-10-24-build17-iphone-11b-last-heart-results.png),
[round portraits](verification/2026-10-24-build17-iphone-12-round-results.png),
[shared winners](verification/2026-10-24-build17-iphone-13-match-results.png),
[complete matches](verification/2026-10-24-build17-arena-full-matches.json),
[real lifecycle](verification/2026-10-24-build17-lifecycle.json),
[normal startup and preserved files](verification/2026-10-24-build17-normal-launch.json).

Native checks use controlled fixtures and synthetic touch through the actual
application UI. Full Core matches use the normal shared-map/local-combat AI
for all five slots. Core match time is simulated, not a physical frame measurement.
Build 16's 300-second Release performance run below is historical; no fresh long
performance run is claimed for build 17. Manual fun/balance/final art remain open.
Physical iPad remains excluded at the user's request.

## Historical build 16 — random starts, finite hearts, shared minimap and medals

October 24, 2026. Final Core/shared-map AI and physical functional checks pass.
Fresh Release performance and final normal installation/startup pass. Build 15 results
below remain historical and are not substituted for this build's measurements.
No commit, push or Apple upload.

| Verification | Result |
| --- | --- |
| Classic / foundation / arena | 65 + 31 + 21 = **117 passed**, zero failures |
| New random starts | 100 seeds: unique legal cells outside centre, at least six cells apart, legal initial headings; human-independent allocation and persisted next-round RNG |
| Finite round hearts | Full production round never increases its heart count; exhausted board stays empty until 120 s; next round restores 240 hearts |
| Shared map and local combat | All five receive identical remaining hearts and active map positions; AI routes toward distant hearts and removes collected hearts from memory; hidden inventory/protection changes leave observations unchanged |
| Medals / overall rankings | Round points decide shared gold/silver/bronze with skipped occupied ranks (1, 1, 3); only gold grants a trophy; overall rank uses trophies then total points |
| Production AI complete matches | Five matches / 34 rounds / 244,800 active ticks (4,080 simulated seconds), 8,160 hearts, 474 item uses and 74 KO |
| Additional AI stress | Ten seeded rounds / 72,000 ticks, 120 checkpoint roundtrips |
| Persistence | Actual Unity Newtonsoft continuation and V1/V2→V3 migrations pass; current positions/resources/points preserved; separate legacy files and atomic backups retained |
| Compilation / repository | 0 managed warnings/errors; 152 unique metadata GUIDs; source maze, supplied images and model hashes preserved |
| Native Mac | PASS final shared-map build at 1600×900, 2400×1080 and 1600×1200; 15 fresh captures per format |
| Physical iPhone 13 | Final shared-map build passes with 17 captures, both actual landscape safe areas, simultaneous controls, all five items, KO/respawn, continue, new round and shared match results |
| Actual iOS background/foreground | PASS: 9.501879 s; identical frozen combat/AI state, verified disk checkpoint and Playback policy after foreground |
| Fresh Release performance | PASS 300.0039011 s / 18,000 frames: 59.9994 fps; p95 18 ms, p99 19.5 ms, worst 46.30 ms; 4 frames over 33.33 ms, none over 50 ms |
| Normal archive / install | PASS normal signed 0.2.0 (16) archive installed and launched without test flags; same PID after 8 s; installed version/executable verified; legacy saves/settings and backups byte-preserved; QA harness absent |

The new Release measurement includes 289.934 s of active play, 5.983 s of countdown and 4.086 s of results. Three rounds were started in one match. The measured window collected 621 hearts, used 38 items and produced 8 KO. Uses by canonical actor: 8 / 9 / 6 / 10 / 5. All 61 thermal samples are nominal (0). Peak sampled resident memory is 284.06 MiB; CPU main-thread average is 2.60 ms. GPU timing and GC allocation counters were unavailable. USB charging does not establish battery drain. This is a fresh 300-second build 16 run; the historical 900-second build 15 run is not repeated or reused as current evidence.

[Build 16 performance](verification/2026-10-24-build16-performance.json), [normal archive audit](verification/2026-10-24-build16-release-audit.json), [verified normal launch and preserved files](verification/2026-10-24-build16-normal-launch.json).

The current rules restore hearts only at a new round. Timers, question-box refill,
five-second central KO respawn and three-second protection remain unchanged.
All five receive the minimap's active positions, hearts and shortcut state;
combat information stays local. Round medals do not change the agreed gold-only
trophy award. The UI uses explicit vector medals/trophies and English labels.

An intermediate Mac smoke run lost a synthetic HELP tap while other build tools
took focus. The opt-in smoke harness now ignores desktop focus and yields an
input frame before and after release; the final native suite is rerun. Normal
Release excludes this harness. The first phone runs before the final shared-map
answer are retained privately; only the final shared-map evidence is linked here.

[Physical minimap](verification/2026-10-24-build16-iphone-03b-random-start-minimap.png),
[round medals and overall trophies](verification/2026-10-24-build16-iphone-12-round-results.png),
[shared final victory](verification/2026-10-24-build16-iphone-13-match-results.png),
[production matches](verification/2026-10-24-build16-arena-full-matches.json),
[actual lifecycle](verification/2026-10-24-build16-lifecycle.json).

Manual fun/balance and final character art remain unaccepted. Physical iPad is
excluded at the user's request. A Mac 4:3 layout check is not a physical iPad test.

## Historical N3–N5 — competition build 15, October 24, 2026

Status: gameplay implementation and automatic N5 verification are complete.
Normal Release 0.2.0 (15) is installed and launched on the physical iPhone 13.
Manual gameplay/art acceptance remains open.
No Apple upload, commit or push was performed.

| Verification | Result |
| --- | --- |
| Classic regressions | 65 passed / 0 failed |
| Competition foundation | 24 passed / 0 failed |
| New combat/AI/integration Core checks | 19 passed / 0 failed |
| Complete matches with production local-view AI | 5 matches, one per selected human character; 26 rounds, 187,200 active ticks / 3,120 simulated seconds, 7,132 hearts, 293 items and 48 KO |
| Additional live AI stress | 10 seeded rounds / 72,000 ticks / 1,200 simulated seconds, 120 exact checkpoint roundtrips |
| Actual Unity Newtonsoft | Deterministic combat/AI continuation, version migration, corruption/backup recovery, classic-file isolation and exactly-once settlement pass |
| Host evidence guards | 5 Python checks pass: incomplete, failed, stale, wrong-model and inactive reports reject; durable fresh completion does not require a live console |
| Managed compilation | 0 warnings / 0 errors against pinned Unity libraries |
| Repository/assets | Preserved source maze and all supplied PNG copies; FBX/source hashes, rigs and 151 unique metadata GUIDs pass |
| Unity editor validation | Content, six imported rigs including the retained hand asset, real competition profile and V2 JSON roundtrip pass |
| Native macOS | PASS: 1600×900, 2400×1080 and 1600×1200; 14 fresh framebuffer captures per format |
| Physical iPhone 13 | PASS final functional check: 16 fresh captures, gameplay, controls and safe areas in both actual landscape orientations |
| Actual iOS lifecycle | PASS: 9.509317 s in final background check (first pass: 9.515799 s); identical frozen combat, round and AI state; disk checkpoint matches; resume countdown freezes active time; Playback audio policy survives foreground |
| Normal archive | Development-signed local 0.2.0 (15) archive passes signature/configuration/privacy audit; all opt-in QA classes absent |
| Fresh Release performance | PASS durable 900.0032182 s / 53,968 frames: 59.9643 fps; p95 18 ms, p99 18.5 ms, worst 46.1224 ms; 14 frames over 33.33 ms, none over 50 ms |
| Final normal installation | PASS normal archive installed, version/build and same running PID verified after 8 s, no test arguments/environment; classic save/settings and backups byte-identical |

The measurement includes **864.6914 seconds of active play**, 20.8957 seconds
of countdown and 14.4145 seconds of results. Eight rounds and two matches were
started (one match completed and the next continued). During the measured
window, five actors collected 1,979 hearts, used 79 items and received 16 KO.
Uses per actor in canonical order: 18 / 13 / 19 / 9 / 20. All 181 thermal
samples are nominal (0). Peak sampled resident memory is 279.64 MiB; first/last
samples are 278.02 / 244.92 MiB. CPU main-thread average is 2.57 ms. GPU timing
and GC allocation counters were unavailable; zero reported values do not mean
zero GPU work or allocation. USB charging does not establish battery drain.

CoreDevice invalidated its console connection on termination, losing the final
completion token. The original host runner therefore returned failure despite
native PASS files. The completed Release report and all 181 monotonic samples
were independently validated, including duration, phase-time coverage, all five
actors, no error/interruption and a UTC timestamp newer than this launch log.
The runner now accepts this strictly validated fresh durable completion for
performance runs and retains transport status separately. Native failure,
stale/partial reports and incomplete samples still reject. No second 900-second
measurement is claimed.

[Performance report](verification/2026-10-24-build15-performance.json),
[transport record](verification/2026-10-24-build15-performance-transport.json),
[final OS lifecycle](verification/2026-10-24-build15-lifecycle-final.json),
[normal startup and preserved files](verification/2026-10-24-build15-normal-launch.json).

The native smoke check uses the actual menu, separate character choice, five
models, projected direction selection and simultaneous joystick/item touches
in both hand layouts. It exercises all five items, mid-item save/continue,
KO with retained points, respawn, settings, next round and shared round/match
victories. Controlled fixtures and synthetic touch are distinct from the live
AI measurement and from a human-played match. Text height is checked against
its real native layout when capturing.

New Core coverage includes all five item users, warned cake/balloon timing,
nonlethal banana/oil, ownership/protection, reciprocal KO, point/inventory
retention, occupied-centre respawn, round deadline during combat, projectile
shortcut reservations, atomic malformed-snapshot rejection, frozen timers,
AI observation boundaries, seeded AI continuation and V1 migration. Full-match
reports use the production observation-based brain for all five participants;
only the human input is supplied automatically.

The first device smoke attempts exposed iOS's filename screenshot behavior.
The harness now captures the real framebuffer texture and writes the PNG into
its cache directly. One retry installed before its native rebuild completed;
that repeated the old screenshot failure. Subsequent installation used the
completed build and passed. A Mac 4:3 run under build load also exposed a
wall-clock assumption in the harness's respawn wait. It now waits for the
actual state with a bounded timeout; Core separately checks exactly 300 active
ticks. All three formats passed after that test correction. Production rules
were unchanged by these harness fixes.

Performance profile source hashes are retained separately because later
smoke-only changes clean stale QA captures, strengthen gameplay rotation
checks and remove the fragile respawn wait. Normal Release excludes the entire
QA class. Production game code is identical between those profiles.

The development player's shutdown still reports two persistent native
allocations, as in earlier builds. This is not a claim that every native
allocation leak has been diagnosed. The new long measurement reports sampled
memory and thermal behavior separately. Native builds also emit Unity/libc++
and script-phase warnings; the zero-warning result above concerns managed
compilation only.

Manual acceptance still needed: play a complete match using real fingers,
judge local visibility, reaction opportunities, tactics, difficulty, animation
and sound quality. Prior build 12 relative controls and build 10 audible-Silent
feedback do not establish acceptance of the new competitive gameplay. Physical
iPad testing remains declined for this round.

New evidence uses `verification/2026-10-24-build15-*`; older N2/build 9–14
reports below are preserved as historical records. Selected Mac captures:
[character selection](verification/2026-10-24-build15-mac-1600x900-03-selection.png),
[water balloon warning](verification/2026-10-24-build15-mac-1600x900-07-item-4.png),
[shared match result](verification/2026-10-24-build15-mac-1600x900-13-match-results.png).


## N2 — competition Core and independent storage, October 24, 2026

N2 is implemented as the agreed rules foundation, separate from the classic
`GameSession`. Five stable participant IDs use identical corridor/base access,
speed, pickup, inventory and scoring rules. Commands identify the actor and
current simulation tick. The core covers ordinary regenerating hearts,
replenishing/re-entry crates, replacement inventory, automatic safe shortcuts,
120-second rounds, round/banked scores, shared round trophies, first-to-three
match results and total-score/shared-win tie handling.

**Scope:** Normal `GameApp` still uses the N1 classic simulation in the installed
build 14. N2 has no new phone build or player-facing competition screen. Attack
effects/KO/respawn are N3; production AI, character selection and runtime flow
integration are N4. The five item kinds can be held in N2; they cannot yet be
used as competition attacks. Scripted routing is a test fixture with global
state access, not the local-observation AI specified for N4.

| Verification | Result |
| --- | --- |
| New competition executable | **24 passed, 0 failed** |
| Classic regression executable | **65 passed, 0 failed**, including original navigation/save migration |
| Managed Unity compilation | **0 warnings, 0 errors** |
| Actual Unity Newtonsoft JSON | Both classic and competition checks passed |
| Competition disk storage | Atomic primary/backup roundtrip, malformed-primary fallback, old save/settings preservation, invalid-file rejection without live-state mutation, and exactly-once settlement after disk reload passed |
| Pinned Unity 6000.3.26f1 batch validation | Passed actual five-actor Core/JSON exercise alongside existing asset/scene validation |
| Repository | Passed, **142 unique GUIDs**, original maze/reference/menu/model assets preserved |

Competition checks exercise all five actor IDs, equal base exits, screen-derived
world-heading queues, tunnel/corner paths, contact order, deterministic seeded
ties without human/array-order priority, all five inventory types, full-inventory
replacement, crate camping/re-entry, resource limits/locations, strict round
deadline, pause/countdown, shared trophies, simultaneous third trophies,
full shared match victory, frozen results, corrupted-snapshot rejection and
continued seeded simulation. Restore normalizes actor order by stable ID.

Five additional full-match fixtures run through every active tick and restore
each round result before continuing. All five human ID choices are covered:

| Human ID / seed | Rounds | Simulated seconds | Hearts collected by all five | Match winner |
| --- | --- | --- | --- | --- |
| kim / 100 | 8 | 960 | 760 | kim |
| putin / 101 | 8 | 960 | 731 | putin |
| trump / 102 | 8 | 960 | 827 | trump |
| xi / 103 | 10 | 1200 | 850 | putin |
| zelenskyy / 104 | 7 | 840 | 649 | xi |

Total: **41 rounds, 295,200 active ticks, 4,920 simulated seconds and 3,817
collected hearts**. Each actor collects hearts in every match. These are
headless rules runs, not 82 minutes of real-device gameplay or a frame/memory/
thermal performance measurement. No new N5 performance claim is made.

Persistence uses `competition-v1.json` with a required format/version marker,
maze fingerprint and stored rule profile. It preserves navigation, independent
random streams, inventory, resource/timer/entry-latch state, wall state and
settled-round/winner ledgers. Restore validates before commit and opens paused.
The classic `session-v1.json` and its backup/highscore/settings remain separate.
Future N3/N4 data requires a versioned extension/migration, not legacy enum
reinterpretation. Item kinds use a separate enum from classic `ItemKind`.

The first seed-coverage run exposed correlated first draws from adjacent raw
seeds. Competition stream initialization now mixes each seed/salt before
xorshift; streams then advance independently and persist exactly. Seed
coverage now reaches all five item kinds and all five simultaneous-pickup
winners, while changing the human ID or snapshot array order leaves a given
seed's result unchanged.

Evidence: [competition checks](verification/2026-10-24-n2-core-competition-checks.txt),
[full matches](verification/2026-10-24-n2-core-full-matches.json),
[classic regression](verification/2026-10-24-n2-core-classic-checks.txt),
[Unity serialization](verification/2026-10-24-n2-core-serialization.txt),
[editor validation](verification/2026-10-24-n2-core-unity-validation.json) and
[source inventory](verification/2026-10-24-n2-core-source-inventory.json).
No commit/push, phone installation or Apple upload was performed for N2.

## Closer N1 camera — 0.2.0 (14), October 24, 2026

User feedback on build 13: **“Kameraet skal markant tættere på karakteren”.**
Build 14 reduces the orthographic half-height from 8.5 to 4.5 m, giving a
9 m image height instead of 17 m. Characters and world details appear about
**1.89 times larger in each screen direction**. Pitch 55°, yaw 25°, full-player
rendering, fixed rotation and screen-oriented corridor input are retained.
This is a zoom change; moving an orthographic camera closer alone would not
enlarge the character. The visible local area is smaller and needs manual
assessment for the balance between character size and warning time.

Fresh verification for this change:

- Managed Unity compilation passed with **0 warnings and 0 errors**.
- Repository validation passed with **139 unique GUIDs**, unchanged reference
  art, menu artwork, models/manifests, maze and rig/clip budgets.
- Native Mac checks passed at **1600×900, 2400×1080 and 1600×1200**, with
  **43 PNGs per format**. Full player, fixed camera, all four screen directions,
  queued/released turns, tunnel wraps, both hand layouts and item use pass.
- Physical iPhone 13 Debug checks passed with **47 PNGs at 2532×1170**,
  including both actual landscape directions, safe area and gameplay views.
- The first run found a review NPC placed outside the smaller camera frame.
  The character-review fixture now reframes closer after its gait measurement,
  and the local-view example places the neighbour two cells away. Visibility
  assertions were retained; this changes test staging, not AI/game rules.
- Core/input rules and serialization are unchanged from the earlier build 13
  run (**65/65** Core tests and actual Unity Newtonsoft checks). These counts
  are retained baseline evidence, not a newly executed rule suite for this zoom.

The Mac [16:9 view](verification/2026-10-24-build14-mac-16x9-45-local-garden.png)
and [4:3 view](verification/2026-10-24-build14-mac-4x3-45-local-garden.png)
were visually inspected. Reports and new captures use the
`2026-10-24-build14-` prefix; build 13 evidence is preserved separately.
The [iPhone view](verification/2026-10-24-build14-iphone13-view-LandscapeLeft.png)
was also inspected for the larger full player, nearby opponent, corridors and
HUD clearance. The [opposite orientation](verification/2026-10-24-build14-iphone13-view-LandscapeRight.png)
is captured separately.

The normal **0.2.0 (14)** Release archive passed signature, version, English
language, orientations, icon/privacy and test-harness-absence audit. It was
installed and launched on the physical iPhone without test flags, preserving
normal player storage. The installed version and running process were verified
after eight seconds. See [normal launch evidence](verification/2026-10-24-build14-iphone13-normal-launch.json)
and [archive audit](verification/2026-10-24-build14-release-audit.json).

Both iOS exports succeeded with Xcode still open by preserving the previous
generated export folders under `.artifacts/build14-previous-exports/` and
exporting into fresh locations. This avoided Unity's attempt to inspect/close
an existing Xcode project when the GUI tool could not connect. No permanent
build-tool change or game-code workaround was needed.

This remains the N1 classic-rule perspective prototype; N2–N5 are pending.
There is no new long-run performance/stress measurement or physical iPad test.
User feedback requests the closer camera; it does not yet accept build 14.

## N1 — local 3D camera and screen-oriented controls, 0.2.0 (13)

Implemented October 24, 2026. This is the perspective prototype requested for
N1. It retains the classic simulation, including lives, power hearts, levels,
four pursuers and the existing save format. Five equal competitors, character
selection, timed rounds, trophies and new attacks remain N2–N5 work.

The camera is orthographic, follows the interpolated player position, and
keeps pitch 55°, yaw 25° and half-height 8.5 m. Walls are 1.15 m high; low
garden dressing and open tunnel roofs expose neighbouring corridors. Wall
tiles no longer overlap at the top, and coping is separated from the wall
surface to remove the z-fighting found in the first visual check. Zelenskyy
is a full rigged character with a gold marker and YOU label. Movement drives
the gait; the full-body Throw animation replaces the visible first-person
hand. The audio listener stays at the player, with the camera's horizontal
orientation. Shadow distance/resolution now cover the overhead view.

Joystick input is matched to the actual pixel projections of the four maze
directions. It stores a world heading once per deflection. Release retains
automatic movement and a pending turn; a changed direction replaces it.
Keyboard/gamepad use the same conversion. Help/tutorial and MOVE label are
English; the ineffective perspective FOV buttons are removed.

| Check | Actual result |
| --- | --- |
| Core rules/navigation/measurement/animation | **65 passed, 0 failed**; three new tests cover projected directions across angles/scales, command latching, and a world-heading queue through a junction |
| Unity managed compilation | Passed, 0 errors and 0 warnings |
| Actual Unity Newtonsoft JSON | Passed roundtrip, paused restore, queued heading, airborne banana/landing and v1 migration |
| Repository/assets | Passed, 139 unique Unity GUIDs; maze equality, original reference copies, menu artwork, model/source hashes and rig budgets preserved |
| Mac 1600×900 | Native PASS, 43 framebuffer PNGs |
| Mac 2400×1080 | Native PASS, 43 framebuffer PNGs |
| Mac 1600×1200 | Native PASS, 43 framebuffer PNGs |
| Physical iPhone 13 Debug | Native PASS, 47 PNGs at 2532×1170; both actual landscape orientations, safe area and gameplay captures |
| Physical iPhone background/resume | Passed actual OS callbacks, 9.31 s background interval, frozen timers, disk checkpoint, resume countdown and native Playback policy after foreground |
| Normal Release archive | Signed 0.2.0 (13) audit passed; English language, iPhone/iPad family, iOS 17 minimum, orientations, privacy declarations and absence of all three test harness classes |

New native checks exercise all four projected headings while facing the
opposite direction, a queued turn after release, fixed camera rotation,
local floor footprint, full-player visibility, player-local audio listener,
both tunnel wraps, full-player throwing/frozen pose and both gameplay
orientations on iPhone. Existing functional checks for the supplied menu,
mirrored multitouch, settings, old rules/items/walls, imported NPC rigs,
save/resume mid-flight, level completion and replay also pass. These are
controlled fixtures with synthetic touch, not a manually played round.

Selected captures were inspected for composition, local visibility, full
player, low walls, stable wall tops and tunnel rendering. See the
[iPhone view](verification/2026-10-24-build13-iphone13-view-LandscapeLeft.png),
[opposite orientation](verification/2026-10-24-build13-iphone13-view-LandscapeRight.png),
[Mac 4:3 view](verification/2026-10-24-build13-mac-4x3-45-local-garden.png),
[Mac wide view](verification/2026-10-24-build13-mac-wide-45-local-garden.png),
[queued turn](verification/2026-10-24-build13-iphone13-41-automatic-turn.png)
and [tunnel wrap](verification/2026-10-24-build13-iphone13-43-tunnel-wrap.png).
PASS reports, lifecycle JSON and the signed archive audit are saved beside
these captures with the `2026-10-24-build13-` prefix.

The audited normal Release 0.2.0 (13) was then installed and launched on the
physical iPhone without test arguments or MAZE test environment variables.
The installed app list confirms the version; process 39848 remained running
after eight seconds. Normal player storage was not reset. See the
[normal launch evidence](verification/2026-10-24-build13-iphone13-normal-launch.json).
The matching source inventory excludes generated bin/obj/cache/build files.

The first iOS export crashed in Unity/Burst's Mono child domain during target
import; the next stalled while closing the open Xcode project. After stopping
that batch process and the user closing Xcode, both final Debug and Release
exports succeeded. The local diagnostic is retained in
`Logs/build13-ios-import-crash.log`. No game-code workaround was required.
Xcode emits generated Unity/libc++ platform and script-phase warnings.
The Debug iPhone smoke also emits a persistent allocation warning at shutdown;
the same warning is present in the build 12 log. The functional PASS does not
establish absence of memory leaks or long-run stability.

Manual acceptance of this new camera and input is pending. The user's
build 12 controls feedback applies to relative first-person turns, not N1.
No new performance/stress run or physical iPad test is claimed. Final art,
camera balance and the competitive gameplay still require their planned
review. Build 9/10 measurements remain historical evidence for those builds.

## Existing build 12 redeployed — October 24, 2026

Opened `Builds/iOS-Release/Unity-iPhone.xcodeproj` in Xcode 27.1 (27A9275),
rebuilt its normal Release configuration successfully, and verified the app's
signature, version 0.2.0 (12), English locale and absence of test harnesses.
Installed it on the physical iPhone 13; the device's installed-app listing
confirms version/build. No uninstall or data reset was performed.

After the user unlocked the phone, the normal Release was launched and
activated in the foreground without test arguments or MAZE test flags. Process
39725 was still running after an 8-second startup wait. No new gameplay run or
performance test is claimed. This is the existing first-person version, not the
planned competition.
[Deployment record](verification/2026-10-24-build12-iphone13-redeploy-mdtcgwa.json).

## Manual controls acceptance — October 24, 2026

The user answered **“Styringen fungerer godt”** when asked how automatic walking
and relative turns work in the latest installed build 12 on their iPhone.
The manual acceptance of the new control feel is now recorded as passed.
[Recorded feedback](verification/2026-10-24-build12-manual-controls.json).

This confirms the control experience. A complete hand-played round, start-menu
design, final art/audio quality, and physical iPad acceptance remain separate.
No new build or automated/device run was performed to record this feedback.

## Resume check — October 24, 2026

All 59 entries in the recorded build 12 source inventory still match (49
non-generated files and 10 generated build files). The executable in the local
build 12 archive matches its previous audit hash. Repository validation passes:
the maze, original reference/runtime copies, all five character FBX/source
manifests, editable sources, rig budgets, and 138 unique Unity GUIDs.
`git diff --check` also passes.

This checks continuity of the local handover. No new build, device installation,
gameplay run, or performance measurement was performed. The previous device
installation/launch evidence remains dated October 21. This check did not record
manual product acceptance; the subsequent controls confirmation is recorded above.
[Recorded check](verification/2026-10-24-build12-resume-check.json).

## Manual sound confirmation — October 21, 2026

The user confirmed that **both music and sound effects are audible** with the
physical iPhone Ring/Silent switch set to silent, responding to the build 10
question requesting in-game sound/music on and media volume above zero.
Exact reply: “Ja, begge dele kan høres”.

This closes the pending manual check of the Silent-switch audio fix. The reply
is linked to the build 10 question; the build running during listening was not
independently identified. No separate build 12 listening test or full audio/
haptics quality acceptance is claimed.
[Recorded feedback](verification/2026-10-21-manual-silent-switch-audio.json).

## Illustrated start screen — 0.2.0 (12), verified

The supplied `docs/images/Jagten gennem egoernes labyrint.png` is copied byte
for byte to `Assets/Resources/Brand/MenuBackground.png`. The start screen shows
only PLAY, CONTINUE SAVED GAME, and HELP in the dark area on the right. Extra
portraits, 3D hero, title/subtitle, best-score text, prototype notice, and the
start-menu Settings action are removed. Settings remain available from pause.
The hero celebration at level clear remains. Empty-save Continue is disabled.

- 62/62 Core checks passed; managed compile passed with no errors/warnings.
- Native checks now require three menu texts/actions in order, all buttons in
  the right side and safe area, original cover proportions, no character
  overlays, and no unused hero camera rendering. Help opens through touch.
- Both empty-save and saved-game start screens are captured; save/resume and
  the existing gameplay/settings/rig checks remain in the native suite.
- Mac native checks passed at 1600×900 during layout development and at
  2400×1080 after the final edge blend/button styling. The final wide capture
  preserves the complete cover and blends its empty edge into the dark extension.
  Both empty-save and saved-game menus pass. [Result](verification/2026-10-21-build12-mac-wide-smoke.txt),
  [menu](verification/2026-10-21-build12-mac-wide-01-menu.png),
  [saved menu](verification/2026-10-21-build12-mac-wide-42-menu-saved.png).
- Final Mac 4:3 (1600×1200) passed with 40 PNGs. The complete cover is fitted
  inside the narrower screen, with dark margins above/below, while the right
  actions remain in the image's dark region. This is layout evidence, not a
  physical iPad test. [Result](verification/2026-10-21-build12-mac-4x3-smoke.txt),
  [menu](verification/2026-10-21-build12-mac-4x3-menu.png).
- Physical iPhone 13 Debug build 12 passed with **42 PNGs at 2532×1170**,
  including both landscape directions/safe areas. Help touch, Play, saved-game
  Continue, pause Settings, all gameplay fixtures, and hero celebration pass.
  The start screen has only the three requested English action labels.
  [Result](verification/2026-10-21-build12-iphone13-smoke.txt),
  [start](verification/2026-10-21-build12-iphone13-01-menu.png),
  [saved game](verification/2026-10-21-build12-iphone13-42-menu-saved.png).
- Signed normal Release 0.2.0 (12) archive passed signature, English language,
  iPhone/iPad family, landscape, icon, permission/privacy, and absence-of-test-
  harness checks. [Audit](verification/2026-10-21-build12-release-audit.json).
  The normal Release was installed from this archive and launched on iPhone
  without test flags. Normal user storage was preserved. No Apple upload was performed.
  Development-build watermarks in QA captures are absent in normal Release.

Earlier builds below are historical evidence.

## English product — 0.2.0 (11), verified

All player-facing menus, HUD labels, tutorials, notifications/errors, and 3D
signs are English. The hero's English display name is Zelenskyy. Life icons
remain, now labeled `Lives:`. English score grouping and decimal timers are
explicit, independent of the device locale. iOS declares `en` as the default
and only supported app language. Store metadata and privacy drafts are English.

- 62/62 Core tests passed again. The actual Unity Newtonsoft roundtrip passed.
- Managed compile: zero errors/warnings. Repository/assets validation passed.
- Native checks force `da-DK` culture, assert `Best: 12,345` and decimal-point
  effect timers, and check text height within its layout at every capture.
- Native Mac checks passed at 1600×900 and 1600×1200 with 39 framebuffer
  captures per run. Menu, gameplay, help, settings, and Game Over were inspected.
  [Result](verification/2026-10-21-build11-mac-smoke.txt),
  [4:3 result](verification/2026-10-21-build11-mac-4x3-smoke.txt).
- Signed Debug build 11 passed on physical iPhone 13 with 41 framebuffer PNGs
  at 2532×1170, including both landscape orientations and safe areas. English
  menu/settings/HUD/help/world sign text and all existing functional fixtures
  passed under forced `da-DK` culture. Input is synthetic, not manual acceptance.
  [Result](verification/2026-10-21-build11-iphone13-smoke.txt),
  [menu](verification/2026-10-21-build11-iphone13-01-menu.png),
  [HUD](verification/2026-10-21-build11-iphone13-02-gameplay.png),
  [settings](verification/2026-10-21-build11-iphone13-03-settings.png),
  [help](verification/2026-10-21-build11-iphone13-20-help.png),
  [base sign](verification/2026-10-21-build11-iphone13-08-base.png).
- Both iOS exports declare `en` and contain exactly one localization key.
  The signed normal Release archive 0.2.0 (11) passed signature, language,
  device family, orientations, icon, permission/privacy, and test-harness checks.
  [Audit](verification/2026-10-21-build11-release-audit.json).
  The normal Release from this archive was installed and launched on iPhone
  without test flags after QA. Existing normal user storage was preserved.
  No App Store upload has been performed.

Build 10 and earlier evidence below remains historical; no new long performance
measurement is claimed for this text-only update.

## Rettelser efter brugerafprøvning — 0.2.0 (10)

Brugeren valgte relativt joystick: automatisk gang midt i gangen, venstre/højre
bestiller ét gemt sving, op fortsætter ligeud, ned vender om, slip fortsætter.
Kameraet følger retningen uden lodret kig. HUD viser `Liv:` med tegnede hjerter.
iOS bruger Playback/MixWithOthers for lyd uafhængigt af Ring/Silent-knappen;
spillets lydvalg og medielydstyrken gælder fortsat.

- **62/62** Core-tests består. De omfatter sving før kryds, forbrug af den
  resterende bevægelse efter svinget, ét sving pr. udslag, erstatning af køen,
  vending mellem feltcentre, stop ved væg, tunnel, alle 244 hjerter, pause og
  v1/v2-migration til snapshot v3. [Resultat](verification/2026-10-21-build10-core-checks.txt).
- Kompilering mod Unity: **0 fejl og 0 advarsler**. Unitys faktiske Newtonsoft
  serializer bevarer også et bestilt sving og giver identisk videre simulation.
- **540.000 ticks / 10 seeds** består med midterlinjeinvariant efter hvert tick:
  7.564 hjerter, 235 fangster, 145 genstande, 702 vægskift, 900 checkpoints og
  alle fire aktive AI-roller. [JSON](verification/2026-10-21-build10-core-stress.json).
- Stresstesten fandt en reservation, der sluttede før sidste feltcenter ved en
  genvej. Begge ender af den aktuelle gangkant reserveres nu, indtil spilleren
  er helt ude. En målrettet regressionstest består sammen med hele stresstesten.
- Native Mac-kontrol består med syntetisk sving før et kryds, fortsat gang efter
  slip, stop ved væg, fast kamera og kontrol af 0/2/3/4 livssymboler. De øvrige
  genstands-, rig-, menu- og save/resume-kontroller består også.
- **iPhone 13:** signeret Debug-build 10 består med **41 PNG'er på 2532×1170**,
  begge landscape-retninger/safe area og samme native kontrol. Et venstreudslag
  før et kryds udfører ét sving efter slip. Livrækken tælles ved 0/2/3/4 liv.
  [Resultat](verification/2026-10-21-build10-iphone13-smoke.txt),
  [HUD](verification/2026-10-21-build10-iphone13-02-gameplay.png),
  [sving](verification/2026-10-21-build10-iphone13-41-automatic-turn.png).
- **Reel OS-baggrund:** 9,36 sekunder i Indstillinger og tilbage i samme proces.
  Timere/disk-checkpoint/nedtælling består; den faktiske iOS-lydsession har
  fortsat Playback/MixWithOthers efter forgrund. [JSON](verification/2026-10-21-build10-iphone13-lifecycle.json).
- **Mac 4:3:** den endelige build med genvejsrettelsen består ved 1600×1200 med
  39 PNG'er; hjælpetekst og livrækken er visuelt kontrolleret. Ingen fysisk iPad-test.
- **Normal Release-archive:** signatur, version 0.2.0 (10), dansk locale,
  iPhone/iPad, iOS 17, ikon, tilladelsesnøgler, manifest og fravær af testharness
  består. [Audit](verification/2026-10-21-build10-release-audit.json).
  Ingen Apple-upload er foretaget.
- **Kort Release-måling på iPhone:** 60,0021 s efter 15 s opvarmning, 60,00 fps,
  p95 18,00 ms, p99 18,50 ms, værst 20,40 ms; ingen frames over 33,33 ms.
  71 hjerter, én fangst og én brugt genstand med alle fire AI-roller.
  Samplet peak resident 263,42 MiB, alle 13 temperaturprøver nominal.
  Ingen fejl/baggrundsskift; GPU-/GC-tællere utilgængelige. USB/haptik-afgrænsning
  som i [performancebeskrivelsen](PERFORMANCE.md).
  [JSON](verification/2026-10-21-build10-iphone13-performance.json),
  [CSV](verification/2026-10-21-build10-iphone13-performance.csv).
  Dette erstatter ikke build 9's 15-minutters måling med en ny lang test.
- Den normale signerede Release 0.2.0 (10) er installeret og startet på iPhone
  uden testflag efter målingen; normal brugerlagring er bevaret.
- Den automatiske lydtest kontrollerer den faktiske iOS-session. Brugeren har
  senere bekræftet både musik og effekter med den fysiske knap på lydløs som
  svar på build 10-spørgsmålet; se den manuelle bekræftelse øverst.

Build 9 nedenfor er historisk evidens. Dets 900-sekunders måling og tidligere
manuelle bekræftelse af frit kamera er ikke validering af den nye styring.

## Samlet build 7–9 — 0.2.0 (9), 21. oktober 2026

Denne sektion erstatter historiske udsagn om, at kun Putin er importeret.
Alle fem figurer og heltens arm er nu eksporteret fra lokale Blender-kilder.

### Kontroller og faktisk evidens

- **56/56** C#-tests, 0 fejl/advarsler ved kompilering mod Unitys managed
  biblioteker, Unity Newtonsoft-rundløb og repositoryvalidering består.
- **Stresstest:** 10 seeds × 900 nominelle simulerede sekunder = 540.000 faste
  ticks. 5.641 hjerter, 228 fangster, 111 brugte genstande, 712 vægskift og
  900 JSON-checkpoints/genoptagelser. Alle fire AI-roller var aktive. Ingen
  ulovlige spillerpositioner, tællerdrift eller ugyldige snapshot-/aktørtilstande.
  Dette er Core-simulation, ikke 150 minutters fysisk enhedstid.
  [Uændret JSON](verification/2026-10-21-build9-core-stress.json).
- **Mac:** Den udvidede native kontrol består med 38 framebuffer-PNG'er ved
  1600×900, 1600×1200 og 2400×1080. Det kontrollerer touch, håndlayouts,
  genstande/vægge, alle fire rigs, menuavatar, hjælp, grafikprofil og spilfaser.
  4:3-formatet er ikke en fysisk iPad-test.
- **iPhone 13 / iOS 26.6.2:** Debug-build 9 er signeret, installeret og består
  samme kontrol. 40 faktiske PNG'er på 2532×1170 inkluderer tvungen fysisk
  LandscapeLeft/LandscapeRight og kontrol af safe area i begge retninger.
  Inputtet er syntetiske Input System-events, ikke en ny håndført tretouchtest.
  [Resultat](verification/2026-10-21-build9-iphone13-smoke.txt).
- **Rigs:** Fire native fodmålinger med 22 støtteprøver pr. figur. Højst
  0,537 mm afvigelse i plantet ankelhøjde; svingfoden løftes. Skala/retning,
  deformerende jagt, frossen pause, fast reduceret glidepose, power, miniature-
  retur samt profil/bagfra består. Unity importerer 16.806–17.994 trekanter
  pr. hel figur og 1.896 i heltens arm. Kilder har 17 knogler, syv fælles
  klip, helten desuden Throw/Cheer. Ansigternes kunstneriske slutkvalitet er åben.
- **Faktisk OS-livscyklus:** Runneren åbnede Indstillinger og aktiverede samme
  spilproces igen. 9,24 sekunder i baggrunden; simulationstid, kraft-, boost-
  og vægvarslingstimer var uændrede. Et pauset disk-checkpoint stemte, og
  genoptagelsens nedtælling forbrugte ingen simulationstid.
  [Uændret JSON](verification/2026-10-21-build9-iphone13-lifecycle.json).
- **Mac Release, kort kontrol:** 30,01 målte sekunder efter 3 sekunders
  opvarmning: 59,80 fps, p95 17,25 ms, p99 17,75 ms, værste interval 19,79 ms,
  ingen intervaller over 50 ms. 39 hjerter, én fangst og én genstand. Mac'en
  byggede andre lokale artifacts samtidig; dette er ikke en mobilmåling.
  [JSON](verification/2026-10-21-build9-mac-performance.json),
  [CSV](verification/2026-10-21-build9-mac-performance.csv).

Efter den lange måling er den seneste Debug-build installeret og hele
funktionskontrollen gentaget med de sidste UI-rettelser. Den består igen;
frisk log og PASS er kontrolleret, og alle 40 PNG'er er 2532×1170.
De kopierede build 9-billeder og fodrapporter ovenfor stammer fra denne sidste
kørsel i `.artifacts/smoke-iphone-build9-final`. Spilregler og 3D-assets er
uændrede gennem de sidste UI-rettelser.

### Lang fysisk Release-måling — bestået

**900,0107 målte sekunder efter 15 sekunders opvarmning på fysisk iPhone 13.**
Release (`developmentBuild=false`), 2532×1170, alle aktuelle 3D-assets og normal
AI. Både frisk `PERFORMANCE COMPLETED` og det hentede `PASS`/JSON er kontrolleret.
`completed=true`, `stabilityPassed=true`, ingen fejl eller baggrundsafbrydelser.

| Måling | Resultat |
| --- | ---: |
| Frames / gennemsnit | 53.954 / 59,9485 fps |
| p95 / p99 | 18,00 / 18,25 ms |
| Værste interval | 48,80 ms |
| Over 33,33 ms / over 50 ms | 21 / 0 |
| CPU main thread, gennemsnit / p95 | 2,95 / 5,50 ms |
| Samplet peak resident / Unity / managed | 271,56 / 100,46 / 5,57 MiB |
| Temperaturprøver | 181, alle nominal |
| Aktiv Playing-tid | 830,90 sekunder |
| Startede runder / hjerter / fangster / genstande | 7 / 1.124 / 19 / 19 |
| Aktive AI-roller | Alle fire, maske 15 |

[Uændret rapport](verification/2026-10-21-build9-iphone13-performance.json) og
[CSV](verification/2026-10-21-build9-iphone13-performance.csv) følger repoet.
De langsommere intervaller er samlet omkring 686–711 sekunder; der er ingen
registreret OS-baggrund eller temperaturændring på det tidspunkt. Årsagen er
ikke fastslået; de konkrete langsomme intervaller er bevaret i CSV til videre profilering.
GPU-frame time og GC-rate var ikke tilgængelige og rapporteres derfor ikke som
nul. Haptik er slået fra under automatisk langtidskørsel. USB-opladning betyder,
at der ikke kan drages en konklusion om uopladet batteriforbrug.

Automaten spiller med normale regler, fangster og genstarter; den fuldførte
ikke et level i denne måling (`maxLevel=1`). Fuld 244-hjertetraversal og
levelskifte er verificeret særskilt i regler/native fixtures. Dette er ikke
manuel spilaccept eller slutaccept af de anvendte reviewmodeller. Efter
performance-eksporten er kun tastaturhjælpetekst og dansk bundle-locale rettet;
spilregler og 3D-assets er uændrede. Final normal Release har ingen måler.

### Udgivelseskandidat og resterende accept

Normal Release er adskilt fra Debug/performance. Lokal archive er bygget og auditeret med
udviklersignering, dansk sprogangivelse, begge landscape-retninger og iOS 17
som minimum. Audit kontrollerer også fravær af de tre testharness-klasser,
1024px RGB-ikon uden alfa og Unitys privatlivsmanifest. [Uændret audit](verification/2026-10-21-build9-release-audit.json). Det er ikke en
App Store-validering eller upload.

Den afsluttende normale Release-build er installeret fra den auditerede archive
og startet uden testflag på iPhone. Opstarten er bekræftet i
`Logs/iphone-build9-normal-launch.json`; en efterfølgende procesliste bekræfter,
at processen kører. Brugerens normale gemte spil er ikke ændret af testharness.
Der er ikke lavet manuel gameplayaccept af denne Release-build.

Brugeren har valgt at fortsætte uden fysisk iPad-test i denne omgang. Manuel
hel runde mod AI, tredje finger, endelig grafik, lyd/haptik og rettighedsafklaring
står fortsat åbne. Den tidligere Unity Debug-meddelelse om én persistent
allocation ved procesafslutning ses stadig; den er ikke erklæret løst.

Gentag stresstesten med:

```sh
dotnet run --project Tests/MazeOfEgos.StressChecks.csproj
python3 Tools/ios_test.py lifecycle --device DIN_IPHONE
python3 Tools/ios_test.py performance --device DIN_IPHONE --seconds 900
python3 Tools/verify_ios_release.py
```

Se [udgivelsesstatus](release/READINESS.md) og [planen](IMPLEMENTATION_PLAN.md).


## Historik — build 1–6


## Bekræftede resultater

Tabellen nedenfor indeholder det oprindelige build 1-forløb. Den efterfølgende
build 2-kontrol er beskrevet særskilt, så manuel accept af build 1 ikke forveksles
med nye automatiske kontroller.

| Kontrol | Kommando | Resultat |
| --- | --- | --- |
| Regelsimulation | `dotnet run --project Tests/MazeOfEgos.Checks.csproj` | 40 tests bestået, 0 fejl |
| Unity C#-API/kompilering | `python3 Tools/unity.py compile-check` | Bestået mod editorens biblioteker, 0 fejl/advarsler |
| Faktisk Unity JSON-bibliotek | `python3 Tools/unity.py serialization-check` | Snapshot-rundløb, pauset genoptagelse og fortsat determinisme bestået |
| Kildedata/assets | `python3 Tools/verify_repository.py` | Bane lig kildedokument, fem uændrede billedkopier, unikke GUID'er og gyldige assemblyreferencer |
| Native Unity-import | `python3 Tools/unity.py validate` | Exit 0; 244 hjerter, snapshot-rundløb, fem referencer og scene bestået |
| macOS-player | `python3 Tools/unity.py build-mac` | Udviklingsbuild lykkedes; universal ARM64/x86_64-app |
| Grafisk macOS-afvikling | `python3 Tools/unity.py smoke-mac` | Menu, tre samtidige touchpunkter, opsamling, boost, slip, pause, indstillinger og save/resume bestået |
| iOS-eksport | `python3 Tools/unity.py build-ios` | IL2CPP/Xcode-projekt genereret uden fejl |
| Xcode/iPhone | Debug-build med automatisk signering mod fysisk iPhone 13 | Build og installation af 0.1.0 (1) lykkedes |
| Grafisk afvikling på fysisk iPhone | Udviklingsbuild med `MAZE_SMOKE_TEST=1` via `devicectl` | Automatisk runtimekontrol bestået; tre faktiske 2532 × 1170-skærmbilleder kontrolleret |
| Manuel samtidig touchstyring | Brugeren afprøvede venstre joystick og højre kameratræk på iPhone 13 | Bekræftet 21. oktober 2026: begge fungerer samtidigt med rigtige fingre |

Værktøjer: macOS ARM64, .NET SDK 10.0.401, Unity-editorbiblioteker fra
6000.3.26f1 og Xcode 27.1. iPhone 13 bruger iOS 26.6.2. Installation og opstart er
udført med `devicectl`; den automatiske afviklingskontrol på telefonen består.
Manuel samtidig bevægelse og kamerastyring er også bekræftet af brugeren.
De øvrige manuelle kontroller og performance er fortsat udestående.

Regeltests omfatter referencebanens 28 × 31 felter, 240 + 4 hjerter, tilgængelighed,
udelukket base/tomrum, tunnel og frie bevægelser, kollisionskontakt ved væggen,
kraftprioritet og udløb, pointkæder og ufarlig retur, fangst og sidste hjerte,
ekstra chance, fyldt inventar, banan/olie/immunitet, boost/swept-kontakter,
sikker væglukning, alle otte stabile vægkombinationer, aktuel retur-navigation,
pause/lagring, korrupt snapshot og backup, ændret banemetadata, seeded replay,
30/60/120-fps-renderingsplaner med samme simulation samt level 256 og videre.

En test bevæger spilleren fysisk gennem den logiske bane, samler alle 244
hjerter og udløser level-afslutning med 3.600 point. Forfølgerne holdes i ro i
denne traversaltest for at isolere navigation og den samlede opsamlingsrunde.
Dette er en **simulationstest**, ikke en optagelse fra en fysisk iPhone.

## Native afvikling og rettelser

Den tidligere licensblokering (exit 198) er løst, efter brugeren installerede
Unity Hub og aktiverede licensen. Native import, validering og builds virker nu.
`Packages/packages-lock.json` er genereret af Unity og medtaget i kildekoden.

Native kontroller afslørede og førte til rettelser af:

- Portrætark blev importeret som cubemaps; de konfigureres nu udtrykkeligt som 2D.
- Mobilvibration var refereret i desktop-playeren; kaldet er nu platformsafgrænset.
- Prototypetekst, pauseknap og HUD overlappede; placering og paneler er rettet.
- Verdenslabels brugte skrifttypens overlaymateriale og kunne ses gennem vægge;
  de bruger nu en URP-shader med normal dybdetest.
- IL2CPP fjernede collider-typer, som Unity opretter internt ved `CreatePrimitive`;
  de bevares nu via `Assets/link.xml`, selv om præsentationen bagefter fjerner
  de ubrugte colliders og bruger spillets egne bevægelsesregler.
- De to .NET-testprogrammer delte mellemoutput/apphost; de har nu separate
  outputmapper, og begge programmer er kørt med deres korrekte resultat.

Den grafiske runtimekontrol afvikler den faktiske Mac-player med syntetiske
Input System-touchhændelser. Tre fingre bevæger, drejer og bruger boost samtidigt.
Kontrollen bekræfter også stop ved slip, opsamling, frosset pausetid, lydindstilling
og gendannelse på samme position/tid. Et kontrolleret snapshot giver boost og
udsætter forfølgerne under inputtesten; den beviser ikke en komplet runde mod AI.
Testflaget findes kun i udviklingsbuild/editor, bruger isoleret midlertidig
lagring og er inaktivt ved normal opstart.

Faktiske skærmbilleder fra Mac-appens menu, gameplay og indstillinger er gemt og
visuelt kontrolleret i `.artifacts/smoke-mac/`. Native logs ligger i `Logs/`.
Disse lokale buildartefakter er ignoreret i Git.

## Fysisk iPhone 13

Den signerede Debug-app `dk.mazeofegos.game`, version 0.1.0 (1), er installeret
på brugerens iPhone 13, og native opstart/Metal-initialisering er bekræftet.
En låst telefon blokerede først udviklertjenesterne; oplåsning løste dette.
Brugeren har bekræftet, at menuen vises. Efter rettelsen af IL2CPP-stripping er
opstarten uden de tidligere collider-fejl. Testflaget på iOS gives via
miljøvariablen `MAZE_SMOKE_TEST=1`; normal opstart bruger ingen testvariabel.
En platformsspecifik screenshot-sti i testværktøjet er rettet til direkte
PNG-skrivning fra framebufferen.

Den endelige automatiske kontrol meldte **RUNTIME SMOKE PASSED** på den fysiske
iPhone med Apple A15/Metal. Menu, tre samtidige syntetiske touchpunkter,
opsamling, boost, stop ved slip, pause, indstillinger og save/resume består.
Der er ingen exceptions eller manglende komponenttyper i denne kørsel.
Resultat og faktisk framebuffer fra menu, gameplay og indstillinger er hentet
til `.artifacts/smoke-iphone-final/` og visuelt kontrolleret. Skærmbillederne
er 2532 × 1170 pixels. Log: `Logs/iphone-smoke.log`.

Tre uændrede kopier er gemt sammen med rapporten, så evidensen følger kildekoden:
[menu](verification/2026-10-21-iphone13-menu.png),
[gameplay](verification/2026-10-21-iphone13-gameplay.png) og
[indstillinger](verification/2026-10-21-iphone13-settings.png).
Gameplaybilledet viser testens kontrollerede boost-situation, ikke en hel runde.

Unity logger ved testens procesafslutning én resterende persistent allocation.
Det er ikke undersøgt eller målt som en voksende læk under gameplay; vurdering
af længere afvikling og hukommelse indgår fortsat i performancearbejdet.

Appen er derefter startet normalt uden testflag eller testmiljøvariabel, og
`devicectl` bekræfter opstarten. Testens gemte tilstand er isoleret fra normale
spildata. Brugeren har 21. oktober 2026 bekræftet, at bevægelse med venstre
joystick og kameratræk på højre side fungerer samtidigt med rigtige fingre
på iPhone 13. Dette bekræfter to samtidige touchhandlinger; genstand som tredje
touch, begge landscape-retninger og en komplet spilrunde er ikke dermed godkendt.

## Begrænsninger ved build 1

- Manuel brug af genstand samtidig med bevægelse/kamera, lyd/haptik, safe areas
  og begge landscape-retninger på iPhone; manuel styring på iPad.
- En komplet grafisk spilrunde mod alle fire forfølgere på fysisk enhed.
- Windows-playerbuild.
- 60 fps, mindst 15 minutters spil, varme og batteriforbrug på iPhone 13.
- Endelige riggede 3D-karakterer, animationskvalitet og visuel M5-accept.
- TestFlight eller App Store-indsendelse.

## Build 2 — touchstyring og tydeligere spilpræsentation

Valget af et spiltilpasset Unity-overlay og vurderingen af Apples native
alternativer er gemt i [inputbeslutningen](INPUT_DECISION.md).

Build 2 har rundt joystick med et 300 × 300-berøringsområde, diskret visning i
hvile og en rund genstandsknap med banan-, olie- og boosts-symboler. Fælder vises
som tre krumme bananskræller eller en oliepøl. Genveje har gule signaler ved
varsling og grønne ved åbning; reduceret bevægelse giver fast lys. Signalerne er
vendt mod adgangsretningen. Basen har synlige, animerede porte og et skilt.
Navnelabels skjules helt tæt på kameraet.

Den udvidede grafiske Mac-kontrol er bestået og omfatter:

- Tre samtidige touchpunkter i både standard- og venstrehåndslayout.
- Stop ved slip og uafhængig brug af boost og banan, plus brug af olieknappen.
- Aktivering af de særskilte banan- og oliefælders renderere.
- Varslingslys, nedsænkning af væggen, åben passage, udsat lukning med en spiller
  i passagen og korrekt lukning af både geometri og navigation bagefter.
- Åbning af baseporten for en udgående forfølger og efterfølgende lukning.
- Opsamling af sidste hjerte, korrekt bonus, næste level med 244 nye hjerter,
  Game Over ved sidste chance og genstart med nul point og tre chancer.
- De eksisterende menu-, pause-, indstillings- og save/resume-kontroller.

Baner, inventar, placeringer, vægtilstande og sidste-hjerte/Game Over-situationer
opsættes med kontrollerede snapshots. Det er faktiske renderede sceneforløb og
Input System-hændelser; der hævdes ikke en hel normalt spillet runde mod AI.
Ti skærmbilleder gemmes i `.artifacts/smoke-mac/`.

40 regeltests, repositoryvalidering og separat Unity C#-kompilering med 0 fejl/
advarsler består fortsat. macOS-build, iOS-eksport og signeret Xcode Debug-build
er gennemført. Version 0.1.0 (2), iOS-minimum 17.0, er installeret på den samme
fysiske iPhone 13, og **hele det udvidede forløb ovenfor består også på telefonen**.

Fysisk runtime-log: `Logs/iphone-smoke-build2.log`. Resultat og ti uændrede
2532 × 1170-framebuffer-PNG'er: `.artifacts/smoke-iphone-build2/`. Seks kopier er
gemt sammen med kildekoden og visuelt kontrolleret:
[styring](verification/2026-10-21-build2-iphone13-controls.png),
[venstrehåndslayout](verification/2026-10-21-build2-iphone13-left-handed.png),
[genstande](verification/2026-10-21-build2-iphone13-items.png),
[varsling](verification/2026-10-21-build2-iphone13-warning.png),
[åben genvej](verification/2026-10-21-build2-iphone13-open.png) og
[Game Over](verification/2026-10-21-build2-iphone13-game-over.png).

Der er ingen runtime-exceptions eller manglende komponenttyper i den afsluttede
kontrol. Unitys allerede noterede afslutningsmeddelelse om én persistent
allocation er fortsat til stede og er ikke undersøgt som gameplay/performance.
Appen er derefter åbnet normalt uden testvariablen; opstart er bekræftet af
`devicectl`. Normale spildata er bevaret. Manuel accept af de nye kontrolflader,
begge skærmretninger, iPad og længere spil mod AI er fortsat udestående.

## Build 3 — banankast, håndbevægelse og gendannelse midt i kast

Bananen starter ved ét tryk, flyver i 0,36 sekund langs kameraets vandrette
retning og bliver først en aktiv fælde ved landing. Levetiden på 20 sekunder
regnes fra landingen. Kamera og bevægelse kan fortsat styres under kastet.
Kastet stopper før væggen med plads til skrællens synlige størrelse; tunnellens
wrap bruges også af animationen. Genveje reserveres langs den resterende
kastebane og af levende fælder i væggens felt, så væggen ikke skjuler dem.

En midlertidig hånd med olivengrønt ærme vises i den nederste del af billedet og
spejles i venstrehåndslayout. Den ændrer ikke kamerahøjde eller FOV. Reduceret
bevægelse giver lavere bue, ingen snurren og intet håndsving. Hånden er fortsat
prototypegeometri og tæller ikke som en færdig rigget karakter.

Snapshot v2 gemmer kastets startsted og tidsstempler. V1-snapshots fra build 1/2
migreres med fælderne som allerede landede; udløbstider, point og runden bevares.
Det eksisterende filnavn bevares for at finde gamle spil. En build 3-fil kræver
build 3 eller nyere. Pause, baggrund og genoptagelsens nedtælling fryser
simulationstid, kastebue og hånd. Ugyldige kastdata afvises før sessionen ændres.
Genoptagelsens hjælpetekst fjernes nu, når spilleren faktisk trykker Fortsæt.

**Bestået:** 50 regeltests (40 eksisterende + 10 nye), faktisk Unity/Newtonsoft-
serialisering af et igangværende kast, deterministisk landing efter gendannelse
og migration af ældre JSON uden de nye felter. C#-kompilering med 0 fejl/
advarsler, repositoryvalidering, native import/indholdsvalidering og den endelige
grafiske Mac-kontrol består. Den udvidede grafiske kontrol omfatter nu yderligere kast,
visning af hånd, pause, gem/fortsæt midt i luften, landing, konstant FOV og
reduceret bevægelse i det modsatte håndlayout. Den skifter layouts via de rigtige
indstillingsknapper. I alt gemmes 13 skærmbilleder.

**Fysisk iPhone:** iOS-eksport, signeret Xcode Debug-build og installation af
version 0.1.0 (3), minimum iOS 17.0, er gennemført. Hele det udvidede forløb
består på samme fysiske iPhone 13: både `RUNTIME SMOKE PASSED` i den friske log
og `PASS` i testresultatet er kontrolleret. Der er ingen runtime-exceptions eller
manglende komponenttyper. Den tidligere noterede meddelelse om én persistent
allocation ved procesafslutning er fortsat til stede; længere performancearbejde
udestår.

Log: `Logs/iphone-smoke-build3.log`. Resultat og 13 faktiske framebuffer-PNG'er:
`.artifacts/smoke-iphone-build3/`. Følgende tre billeder er kontrolleret visuelt
og kopieret uændret, 2532 × 1170 pixels, til repoet:
[kast med højre hånd](verification/2026-10-21-build3-iphone13-banana-flight.png),
[landet skræl](verification/2026-10-21-build3-iphone13-banana-landed.png) og
[reduceret bevægelse med venstre hånd](verification/2026-10-21-build3-iphone13-reduced-motion.png).

Appen er derefter startet normalt uden testmiljøvariablen, bekræftet af
`devicectl` i `Logs/iphone-build3-normal-launch.json`. Testdata bruger fortsat
et særskilt midlertidigt lager; normale spildata er bevaret. De nye kastforløb
opsættes kontrolleret, og touchhændelserne er syntetiske. Tidligere manuelle,
iPad-, fuld-runde- og performance-forbehold gælder derfor fortsat.

## Build 4 — performanceinstrumentering og stabilitet

HUD-tekst for point, level, chancer, hjerter, rekord og Game Over genbygges nu
ved dataændring. Effekttimere opdateres pr. tiendedel sekund; fareteksten sættes
én gang pr. billede. Det fjerner unødvendig formatering og gentagne tom/fuld-
ændringer i samme billede. Der hævdes ingen procentvis forbedring uden en
sammenlignelig førmåling.

En opt-in Release-player måler billedintervaller, CPU/GPU-tællere når de findes,
samplet hukommelse og på iPhone batteri-/temperaturtilstand. Automatisk styring
bruger normale regler, aktive forfølgere, fangst, genstande og genstart efter
Game Over. Se [målemetode og gentagelse](PERFORMANCE.md).

53 regeltests, Newtonsoft-kontrol, repositoryvalidering og C#-kompilering med
0 fejl/advarsler består. Tre nye tests verificerer tidsvægtet FPS, percentiler,
lange stalls, ugyldige målinger og nulstilling. Den grafiske kontrol er udvidet
med faktisk HUD-tekst efter opsamling, Game Over og genstart.

Den endelige korte Mac Release-kørsel gennemførte 30,02 sekunder efter tre
sekunders opvarmning: 59,73 fps, p95 17,25 ms, p99 17,75 ms og værste interval
36,36 ms. Der var 44 opsamlede hjerter, en fangst og en brugt genstand.
[Uændret Mac-rapport](verification/2026-10-21-build4-mac-performance.json).
Mac-kørsel beviser værktøjets funktion, ikke iPhone-performance. Testværktøjet
fortsætter nu ved tab af Mac-vinduesfokus; normal spilpause og iPhone-baggrund
har fortsat deres almindelige beskyttelse.

Den udvidede grafiske Mac-kontrol med faktisk HUD-tekst består, og den normale
iOS-eksport og signerede Xcode Debug-build er gennemført.

**Fysisk performancekørsel:** Unity Release-player, Xcode Release, version
0.1.0 (4), iPhone 13 / Apple A15 / iOS 26.6.2, 2532 × 1170, mål 60 fps.
15 sekunders opvarmning er udeladt. Målingen gennemførte **900,04 sekunder**,
udtrykkeligt bekræftet af både log, resultatfil og rapportens completed/status.
Ingen runtime-fejl eller baggrundsafbrydelser er registreret.

| Måling | Resultat |
| --- | --- |
| Antal målte billeder | 53.985 |
| Gennemsnit | 59,98 fps / 16,67 ms |
| p50 / p95 / p99, histogrammets øvre grænser | 16,75 / 17,25 / 17,75 ms |
| Værste billedinterval | 50,77 ms |
| Intervaller over 33,33 / 50 ms | 3 / 1 |
| CPU-hovedtrådens frame-tæller, gennemsnit / p95 | 1,82 / 3,00 ms |
| GPU-tider og GC-allokering pr. billede | Ikke tilgængelige i denne Release-kørsel |
| Samplet peak resident / Unity-allokeret / managed | 188,50 / 79,25 / 5,24 MiB |
| Temperaturtilstand, 181 prøver | Alle 0: nominal |
| Batteri / Low Power Mode | 100 %, full via USB / deaktiveret |
| Aktiv Playing / countdown / Caught | 830,90 / 45,91 / 23,09 sekunder |
| Runder / hjerter / fangster / brugte genstande | 7 / 1.140 / 19 / 21 |
| Aktive AI-roller set / højeste level | Alle fire / level 1 |

Dette er 15 minutters sammenhængende afvikling inklusive almindelig fangst,
nedtælling og automatisk genstart. Det er ikke 15 minutter udelukkende i
Playing og ikke en manuelt gennemført hel runde med alle 244 hjerter mod AI.
Der er ét mærkbart langt interval, selv om gennemsnittet ligger nær 60 fps.
Der er ikke fastlagt en særskilt endelig udgivelsesgrænse for hitch-frekvens.

Unity-allokeret hukommelse var ca. 79,11 MiB ved første prøve og 79,25 MiB ved
sidste. Managed heap var ca. 4,63 → 4,93 MiB med peak 5,24 MiB. Resident memory
var ca. 182,64 → 91,66 MiB og varierede mellem 84,52 og 91,66 MiB i de sidste
fem minutter. Dette viser ingen stor vedvarende vækst i Unity-allokeret
hukommelse under denne måling; det beviser ikke fravær af alle lækager.
Den tidligere noterede Unity-meddelelse om én persistent allocation ved
procesafslutning er fortsat til stede. Alle nye ProfilerRecorder-ressourcer
frigives udtrykkeligt. Der hævdes ingen målt allokeringsrate, når GC-tælleren
ikke er tilgængelig, og intet uopladet batteriforbrug udledes af USB-testen.

Log: `Logs/iphone-performance-build4.log`. Fuld lokal eksport:
`.artifacts/performance-iphone-build4/`. Uændrede evidenskopier følger repoet:
[rapport med alle prøver](verification/2026-10-21-build4-iphone13-performance.json)
og [CSV-tidsserie](verification/2026-10-21-build4-iphone13-performance.csv).
Rapporten indeholder build-GUID, miljø, målemetode og manglende tællere.
Resultaterne gælder den aktuelle prototype med midlertidige karaktermodeller.
Der skal måles igen med endelige karakterer og grafik.

Den normale Unity-udviklingsbuild, signeret Xcode Debug, version 0.1.0 (4), er
installeret bagefter. Hele den udvidede runtimekontrol består på samme fysiske
iPhone, inklusive de nye kontroller af faktisk HUD-tekst efter opsamling,
Game Over og genstart. Både `RUNTIME SMOKE PASSED` og resultatfilens `PASS` er
kontrolleret. Alle 13 framebuffer-PNG'er er hentet og deres 2532 × 1170-format
kontrolleret; gameplay og Game Over er også kontrolleret visuelt.

Log: `Logs/iphone-smoke-build4.log`, lokale billeder/resultat:
`.artifacts/smoke-iphone-build4/`. Appen er derefter startet normalt uden
miljøvariabler for test, bekræftet i `Logs/iphone-build4-normal-launch.json`.
Normale gemte spil er bevaret. Manuel tredje finger, begge landscape-retninger,
iPad, en hel normalt spillet runde mod AI og endelige karaktermodeller er
fortsat udestående. Release-performance og Debug-funktionskontrol er separate
builds med samme spilændringer; målerens senere håndtering af Mac-vinduesfokus
ændrer ingen iPhone-spilregel eller den målte renderkode.

## Build 5 — rigget Putin-reviewpilot

Putins programmerede primitive krop er erstattet af en lokal Blender-pilot,
baseret på det oprindelige referenceark. Den har én skinnet mesh, 17 knogler,
10.796 importerede trekanter, ni URP-materialer og fem animationer. Geometri,
rig og klip er lokalt modelleret; der er ikke gennemført AI-generering.
Redigerbar kilde, FBX, generator og kilde-/runtime-SHA-256 følger repoet.
Se [karakterproduktionen](CHARACTER_PRODUCTION.md) for fremgangsmåde og grænser.

Unitys indbyggede animationsmodul er nu udtrykkeligt med i manifest/lock.
Importerens validering kontrollerer skelet, klip, meter-skala, trekantbudget
og URP-materialer. Runtime sampler klippene med simulationstiden, mens
spillereglerne fortsat bestemmer position og retning. Power farver tøjet blåt;
ansigtet bruger referencearket. Reduceret bevægelse holder glideposen fast.

**Bestået:** 53 C#-tests, faktisk Newtonsoft-serialisering, repositorykontrol
inklusive pilotens hashes, C#-kompilering med 0 fejl/advarsler, native
Unity-validering, Mac-build og hele det udvidede native Mac-forløb.
Det kontrollerer også fremadretning, faktisk højde, fem tilgængelige klip,
deformerede meshvertices under jagt, fuldstændigt frosset skelet/root under
pause, glidepose, fast glidepose ved reduceret bevægelse, blå power-materialer
og miniature/boble under retur. I alt gemmes 17 framebuffer-PNG'er.

Den første power-fixture manglede et nyt `powerId` og aktiverede derfor korrekt
ingen ny kraftperiode. Testopsætningen blev rettet; spilreglen er uændret.
Det endelige Mac-forløb består inklusive kontrollen af det faktiske materiale.

**Fysisk funktionskontrol:** iOS-eksport, signeret Xcode Debug-build,
installation og hele det samme udvidede forløb består på iPhone 13.
`RUNTIME SMOKE PASSED` i `Logs/iphone-smoke-build5.log` og `PASS` i det hentede
resultat er begge kontrolleret. De 17 PNG'er er alle 2532 × 1170; de fire
karakterbilleder er visuelt kontrolleret og kopieret uændret til repoet:
[jagt](verification/2026-10-21-build5-iphone13-putin-chase.png),
[glid](verification/2026-10-21-build5-iphone13-putin-slip.png),
[power](verification/2026-10-21-build5-iphone13-putin-power.png) og
[retur](verification/2026-10-21-build5-iphone13-putin-return.png).
Det er kontrollerede snapshots og syntetisk input, ikke manuel spilaccept.

Der er ingen runtime-exceptions eller manglende komponenttyper. Den tidligere
noterede Unity-meddelelse om én persistent allocation ved procesafslutning
forekommer stadig i Debug-forløbet. Appen er derefter startet normalt uden
testflag, bekræftet i `Logs/iphone-build5-normal-launch.json`. Normale gemte
spil bruger fortsat et andet lager end testdata. Lokale resultater/billeder:
`.artifacts/smoke-iphone-build5/`.

**Kort fysisk performancekørsel:** Unity Release/Xcode Release, version 0.1.0 (5),
iPhone 13 / A15 / iOS 26.6.2, 2532 × 1170. Efter 10 sekunders opvarmning er
60,02 sekunder målt: 3.599 billeder, **59,97 fps**, p95 **17,00 ms**, p99
17,25 ms og værste interval 21,69 ms. Ingen intervaller oversteg 33,33 ms.
Alle 13 temperaturprøver var nominal; ingen runtime-fejl eller
baggrundsafbrydelser. 56,81 sekunder var Playing, 71 hjerter blev opsamlet,
én fangst og én genstand registreret, og alle fire AI-roller var aktive.

Samplet peak resident/Unity/managed: 226,33 / 82,96 / 4,78 MiB. GPU-tider og
GC-rate var ikke tilgængelige; 100 % batteri via USB måler ikke batteriafladning.
`PERFORMANCE COMPLETED`, resultatfil og rapportens completed/stabilityPassed
er kontrolleret. [Uændret rapport](verification/2026-10-21-build5-iphone13-performance.json)
og [CSV](verification/2026-10-21-build5-iphone13-performance.csv) følger repoet.
Log: `Logs/iphone-performance-build5.log`.

Dette er en kort kontrol med den nye pilot, ikke en ny 15-minuttersmåling.
Build 4's lange måling gjaldt de tidligere primitive modeller. Piloten er ikke
endelig M5-grafik: ansigtet har et fast projiceret udtryk uden blendshapes;
profil, hår, overgange, vægtning og fodkontakt kræver videre arbejde. Xi, Kim,
Trump og hånden er stadig pladsholdere. Fuld runde, manuel tredje touch,
begge landscape-retninger, iPad og endelig visuel accept udestår fortsat.

## Build 6 — profil, teksturovergang, vægtning og fodkontakt

Putin-pilotens anden revision har et tættere hoved med formet næse/bryn,
en blød vertexstyret overgang fra referenceark til hud, tilpasset hårgrænse
og hårstriber. Skjorte, revers og slips følger nu den krumme jakke, og ærmer
og knæ har mere sammenhængende geometri/vægtning. Shaderens overgang bruger
det oprindelige uændrede referenceark; ingen rastergrafik er redigeret.

Der er 17.404 importerede trekanter, ti materialer, 17 knogler og fem klip.
Importer-valideringen kontrollerer også overgangsvægtene og referencens
shader/tekstur. Redigerbar Blender-kilde, generator og FBX er opdateret.
[Front](verification/2026-10-21-build6-blender-putin-front.png),
[profil](verification/2026-10-21-build6-blender-putin-profile.png),
[trekvart](verification/2026-10-21-build6-blender-putin-three-quarter.png),
[bagfra](verification/2026-10-21-build6-blender-putin-back.png) og
[jagt](verification/2026-10-21-build6-blender-putin-chase.png) er faktiske
Blender-renders, kopieret uændret og visuelt kontrolleret. De erstatter ikke
afprøvning af modellen i spillet.

En analytisk to-knogle-løsning bager benstillinger, støttefod og løftet
svingfod i jagt/retur. Generatoren måler de faktiske deformerede skosåler på
40 støtteprøver. Maksimal kontaktfejl er under 1 mm i denne diskrete
Blender-kontrol; det er ikke en garanti for alle interpolerede tidspunkter
eller terræn. [Uændret geometrirapport](verification/2026-10-21-build6-blender-geometry.json).

`GaitClock` integrerer fart i verdensmeter fra simulationstiden. Normal jagt
bruger én cyklus pr. 0,88 meter, også når hastigheden ændres. Pause og
nedtælling fryser fasen; en tidligere gendannet tidslinje nulstiller den.
AnimationState-referencer caches. Returens lille figur bruger et særskilt
læsbarhedstempo. En tidligere overraskelsesreaktion kan ikke fortsætte ind i
base/hvile; den rettelse kontrolleres i det native forløb.

**Bestået:** 55 C#-tests, faktisk Newtonsoft-serialisering, kildebilleder/FBX-
hashes, unikke GUID'er, C#-kompilering med 0 fejl/advarsler, Unity-import og
native Mac-build/-forløb. To nye tests kontrollerer ganguret ved fartskift,
frosset tid, stop, reset/gendannelse og 30/60/120 opdelinger pr. sekund.

Den endelige grafiske Mac-kontrol gemmer 19 framebuffer-PNG'er og
`character-check.json`. Den måler støttefodens ankelhøjde efter Unity-import
og kræver samtidig løftet svingfod. 23 prøvebilleder har maksimalt 0,54 mm
afvigelse; testgrænsen er 15 mm. Dette måler ankelhøjde, mens Blender-kontrollen
måler skosåler. [Uændret Mac-kontrolrapport](verification/2026-10-21-build6-mac-character.json).
Den omfatter også de eksisterende touch-, pause-, glid-, kraft-, retur-,
lagrings- og spilfasekontroller samt nye profil-/bagfra-visninger i scenen.
De ekstra modelvisninger bruger kontrollerede snapshots med en ryddet
hjertekorridor og holdt hviletilstand, ikke en almindelig spilrunde.

Uændrede billeder fra den endelige Mac-player følger repoet:
[jagt](verification/2026-10-21-build6-mac-putin-chase.png),
[profil](verification/2026-10-21-build6-mac-putin-profile.png) og
[bagfra](verification/2026-10-21-build6-mac-putin-back.png).

**Kort Mac Release-måling:** 30,02 sekunder efter tre sekunders opvarmning,
59,63 fps, p95 17,25 ms, p99 17,75 ms og værste interval 65,20 ms. Ét interval
oversteg 50 ms. 44 hjerter, én fangst og én genstand er registreret; ingen
runtime-fejl. [Uændret rapport](verification/2026-10-21-build6-mac-performance.json)
og [CSV](verification/2026-10-21-build6-mac-performance.csv) følger repoet.
Dette er en kort desktopmåling og dokumenterer ikke iPhone-performance,
lang stabilitet eller fravær af mærkbare hak.

**Kort fysisk Release-måling:** Efter gentilslutning og oplåsning er
forbindelsesfejlen CoreDevice 4016 løst. Unity/Xcode Release, version 0.1.0 (6),
er installeret på iPhone 13 / A15 / iOS 26.6.2 ved 2532 × 1170. Efter ti
sekunders opvarmning gennemførte målingen 60,01 sekunder: 3.599 billeder,
**59,97 fps**, p95 **18,00 ms**, p99 18,50 ms og værste interval 20,05 ms.
Ingen intervaller oversteg 33,33 ms. Alle 13 temperaturprøver var nominal;
ingen runtime-fejl eller baggrundsafbrydelser er registreret.

56,81 sekunder var Playing; 71 hjerter, én fangst og én genstand blev
registreret, og alle fire AI-roller var aktive. CPU-hovedtrådens frame-tæller
var i gennemsnit 2,11 ms. Samplet peak resident/Unity/managed: 208,88 / 83,50 /
4,87 MiB. GPU-tider og GC-rate var ikke tilgængelige. USB-testen ved 100 %
batteri måler ikke batteriafladning.

`PERFORMANCE COMPLETED`, resultatfilens `PASS`, completed/stabilityPassed og
`developmentBuild=false` er kontrolleret. Uændret
[rapport](verification/2026-10-21-build6-iphone13-performance.json) og
[CSV](verification/2026-10-21-build6-iphone13-performance.csv) følger repoet;
log og lokal eksport ligger i `Logs/iphone-performance-build6.log` og
`.artifacts/performance-iphone-build6/`. Dette er en kort kontrol med den
forbedrede pilot, ikke en ny 15-minuttersmåling eller endelig grafikaccept.

**Fysisk Debug-funktionskontrol:** Version 0.1.0 (6) er installeret efter
Release-målingen og består hele det udvidede forløb på samme iPhone 13.
`RUNTIME SMOKE PASSED` i den friske `Logs/iphone-smoke-build6.log` og `PASS`
i den hentede resultatfil er begge kontrolleret. Alle 19 framebuffer-PNG'er
har 2532 × 1170 pixels. Fodkontrollen har 23 prøver, højst 0,54 mm afvigelse
i støttefodens ankelhøjde og en observeret løftet svingfod.
[Uændret fysisk kontrolrapport](verification/2026-10-21-build6-iphone13-character.json).

Faktiske iPhone-billeder er visuelt kontrolleret og kopieret uændret:
[jagt](verification/2026-10-21-build6-iphone13-putin-chase.png),
[profil](verification/2026-10-21-build6-iphone13-putin-profile.png) og
[bagfra](verification/2026-10-21-build6-iphone13-putin-back.png).
Det er syntetisk input og kontrollerede snapshots, ikke manuel spilaccept.
Der er ingen runtime-exceptions eller manglende komponenttyper. Den tidligere
noterede Unity-meddelelse om én persistent allocation ved procesafslutning
forekommer fortsat i Debug-forløbet.

Appen er startet normalt igen uden testflag, bekræftet i
`Logs/iphone-build6-normal-launch.json`. Testdata bruger fortsat et særskilt
midlertidigt lager. Det fulde lokale resultat og alle billeder ligger i
`.artifacts/smoke-iphone-build6/`.

Dette er fortsat en reviewpilot. Faste ansigtsudtryk, begrænset profiltekstur,
stiliseret hår og de øvrige pladsholderfigurer kræver mere grafisk arbejde og
accept. Manuel tredje touch, begge landscape-retninger, iPad og en hel runde
mod AI er fortsat udestående. M5 er ikke afsluttet.

## Gentagelse og næste trin

```sh
python3 Tools/unity.py validate
python3 Tools/unity.py build-mac
python3 Tools/unity.py smoke-mac
python3 Tools/unity.py build-ios
```

Afprøv manuelt genstand samtidig med joystick/kamera, pause, baggrund, begge retninger og en
hel runde. Følg [opsætning](SETUP.md)
for signering på en anden Mac. Signeringskonto, certifikater, profiler og
enhedsidentifikatorer gemmes ikke som projektkonfiguration i Git.

Computer-use via ChatGPT var utilgængelig på grund af versionsforskel mellem
klient og server. Det er årsagen til den automatiske playerkontrol; manuel
kontrol af appvinduet er ikke hævdet gennemført. Genstart af ChatGPT er foreslået
af værktøjet, hvis denne UI-adgang senere skal bruges.
