# Release readiness — Maze of Egos 0.2.0 (12)

Product direction changed on October 24: the completed design quiz specifies a
local five-player free-for-all with a high camera, timed rounds, and trophies.
See the [new implementation plan](../IMPLEMENTATION_PLAN.md). It is not yet
implemented or verified. Build 12 and the table below are the historical
first-person baseline; they do not establish readiness of the new game model.

Reviewed October 24, 2026; build 12 verification remains dated October 21.
Source hashes, the local archive executable hash, and repository assets still
match the recorded evidence. No new device run or build was performed on resume.
Historical local review candidate. No App Store Connect submission or
TestFlight distribution has been performed. Build 12 replaces the start screen with the supplied cover art and a three-action
column in its dark right area. The product and release copy remain English. Product acceptance and public release requirements
remain separate from the local implementation.

| Deliverable | Status |
| --- | --- |
| Illustrated start screen | Original supplied artwork, PLAY / CONTINUE SAVED GAME / HELP on the right; no extra title, character overlays, or start Settings action |
| English product copy | Menus, HUD, tutorials, events/errors, 3D signs, and character display names translated |
| English formatting | Score grouping and decimal timers use invariant English formatting on all device languages |
| iOS language declaration | English (`en`) development region and supported localization verified in the signed archive |
| 62 rule/measurement/animation tests and Unity JSON | Passed again for build 12 |
| Native functional and text layout checks | Passed on Mac wide/4:3 and physical iPhone 13, including empty/save menus, all three actions, both landscape directions and safe areas (42 iPhone captures) |
| Normal Release without test harness | Build 12 signed archive/audit passed; normal Release installed and launched on iPhone without test flags |
| Historical build 10 simulation stress | 10 seeds / 540,000 ticks; 7,564 hearts, 235 captures, 145 items, 702 wall transitions, 900 checkpoints |
| Historical build 10 short Release measurement | 60.0021 s, 60.00 fps, p95 18.00 ms, worst 20.40 ms; no frames over 33.33 ms |
| Historical build 9 long Release measurement | 900.01 s, 59.95 fps; applies to build 9, not a new build 12 measurement |
| App icon | Original SVG and 1024px RGB PNG in Xcode's AppIcon catalog |
| English store metadata and privacy policy | Review drafts in this folder |
| Public support/privacy website | Existing URL concerns Donkey Trump 3D; game-specific English content still needs publication |
| Physical iPad test | Not performed; user chose to continue without it |
| Manual controls | User accepted automatic walking and relative turns in build 12 on October 24: “Styringen fungerer godt”; [recorded feedback](../verification/2026-10-24-build12-manual-controls.json) |
| Complete hand-played round against AI | Awaiting confirmation; controls feedback does not confirm a complete round |
| Sound with physical Ring/Silent switch set to silent | Native Playback policy verified in build 10; user confirmed both music/effects audible on October 21 in reply to the build 10 question (not a separately identified build 12 listening test) |
| Final art/audio acceptance | Review models, faces/hair/materials, garden, and sound awaiting acceptance |
| Sources and rights | Inventory available; character reference images and classic maze topology still require final review |
| App Store Connect / TestFlight | English primary listing language, territories, metadata/age/privacy forms, release screenshots, and distribution require review/setup |

## Local deliverables

- `Builds/macOS/Maze of Egos.app`: development app with opt-in functional checks.
- `Builds/iOS/`: Xcode project for development checks.
- `Builds/iOS-Release/`: normal Release without the three test harness classes.
- `Builds/Archives/MazeOfEgos-0.2.0-12.xcarchive`: local development-signed archive, audited and installed.
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
