# Release readiness — Maze of Egos 0.2.0 (14), closer N1 camera

Product direction changed on October 24: the completed design quiz specifies a
local five-player free-for-all with a high camera, timed rounds, and trophies.
See the [new implementation plan](../IMPLEMENTATION_PLAN.md). Build 13
implements N1: a local overhead 3D camera, full player avatar and
screen-oriented controls. Build 14 responds to user feedback with a much
closer view: orthographic half-height 4.5 m instead of 8.5 m, approximately
1.89 times larger characters in each screen direction. It retains the classic rules as a perspective
prototype. N2–N5 remain unimplemented; this is not a release-ready version of
the competitive game.

Verified October 24, 2026. New Mac and physical iPhone checks, source inventory,
screenshots and signed normal Release archive audit are recorded for build 14.
No App Store Connect submission or TestFlight distribution has been performed.
The supplied cover art and three-action menu remain intact. Product copy is
English; store drafts still describe the classic rules and must be rewritten
after the competitive implementation. Product acceptance and public release
requirements remain separate from this local implementation.

| Deliverable | Status |
| --- | --- |
| Local 3D camera and screen-oriented movement | N1 implemented; build 14 closer view passes native checks. Manual assessment of the new zoom and controls pending |
| Illustrated start screen | Original supplied artwork, PLAY / CONTINUE SAVED GAME / HELP on the right; no extra title, character overlays, or start Settings action |
| English product copy | Menus, HUD, tutorials, events/errors, 3D signs, and character display names translated |
| English formatting | Score grouping and decimal timers use invariant English formatting on all device languages |
| iOS language declaration | English (`en`) development region and supported localization verified in the signed archive |
| 65 rule/measurement/animation tests and Unity JSON | Passed for build 13, including three new screen-input checks |
| Native functional and text layout checks | Passed again for build 14 on Mac 16:9/wide/4:3 (43 captures each) and physical iPhone 13 (47 captures), including gameplay in both landscape directions and safe areas |
| Actual iOS background/resume | Historical build 13 pass: frozen timers/checkpoint/countdown and native Playback policy after foreground, 9.31 s background interval; not repeated for camera zoom |
| Normal Release without test harness | Build 14 signed archive/audit passed; normal Release installed/launched on physical iPhone without test flags; installed version and running process verified |
| Historical build 10 simulation stress | 10 seeds / 540,000 ticks; 7,564 hearts, 235 captures, 145 items, 702 wall transitions, 900 checkpoints |
| Historical build 10 short Release measurement | 60.0021 s, 60.00 fps, p95 18.00 ms, worst 20.40 ms; no frames over 33.33 ms |
| Historical build 9 long Release measurement | 900.01 s, 59.95 fps; applies to build 9. No new build 14 performance measurement |
| App icon | Original SVG and 1024px RGB PNG in Xcode's AppIcon catalog |
| English store metadata and privacy policy | Review drafts in this folder |
| Public support/privacy website | Existing URL concerns Donkey Trump 3D; game-specific English content still needs publication |
| Physical iPad test | Not performed; user chose to continue without it |
| Manual controls/camera | User requested a much closer camera after build 13; build 14 zoom awaits review. Screen-oriented controls still await acceptance. Previous relative turns were accepted in build 12; [historical feedback](../verification/2026-10-24-build12-manual-controls.json) |
| Complete hand-played round against AI | Awaiting confirmation; controls feedback does not confirm a complete round |
| Sound with physical Ring/Silent switch set to silent | Native Playback policy verified in build 10; user confirmed both music/effects audible on October 21 in reply to the build 10 question (not a separately identified build 12 listening test) |
| Final art/audio acceptance | Review models, faces/hair/materials, garden, and sound awaiting acceptance |
| Sources and rights | Inventory available; character reference images and classic maze topology still require final review |
| App Store Connect / TestFlight | English primary listing language, territories, metadata/age/privacy forms, release screenshots, and distribution require review/setup |

## Local deliverables

- `Builds/macOS/Maze of Egos.app`: development app with opt-in functional checks.
- `Builds/iOS/`: Xcode project for development checks.
- `Builds/iOS-Release/`: normal Release without the three test harness classes.
- `Builds/Archives/MazeOfEgos-0.2.0-14.xcarchive`: local development-signed normal archive, audited for the closer N1 view.
- `Builds/iOS-Performance/`: the previous build 10 performance export.
- `docs/verification`: immutable reports and selected framebuffer captures by build.
- `art/characters`: editable Blender sources and SHA-256 manifests for all five characters.

Builds and machine-local logs are ignored by Git and can be reproduced. Source,
assets, metadata, scripts, and documentation have not been committed or pushed
in this work session.

`Tools/verify_ios_release.py` checks the actual signed archive: version,
iPhone/iPad device family, iOS 17 minimum, English language declarations,
landscape orientations, permission keys, RGB icon, bundled privacy manifests,
and absence of DevelopmentSmoke/DevelopmentLifecycle/DevelopmentPerformance.
This is a local technical audit, not App Store validation.

Unity Connect, Analytics, Ads, cloud diagnostics, and Purchasing are disabled.
No analytics, advertising, or account SDK was added. The generated manifest
declares SystemBootTime 35F9.1, DiskSpace E174.1, UserDefaults CA92.1, and
FileTimestamp 0A2A.1/C617.1. It declares no tracking or collected data types.
The final App Privacy submission must match the actual submitted build.

See [asset sources](ASSET_SOURCES.md), [English metadata](APP_STORE_EN.md),
[privacy draft](PRIVACY_POLICY_DRAFT.md), and [test report](../TEST_REPORT.md).
