# Verifikation — senest opdateret 24. oktober 2026

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
