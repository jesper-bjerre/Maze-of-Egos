# Build 12 — October 21, 2026

Illustrated start screen using the supplied cover as background. PLAY,
CONTINUE SAVED GAME, and HELP form a right-side column in the dark area.
Extra character overlays and text are removed; Settings is available from pause.
Original image proportions and button safe-area bounds are preserved.
See [the current plan](IMPLEMENTATION_PLAN.md) and [test report](TEST_REPORT.md).

# Build 11 — October 21, 2026

English product language requested for international release. Menus, HUD,
tutorials, events/errors, world signs, and character display names are English.
iOS declares `en`; number formatting is independent of the device locale.
English App Store and privacy drafts are in [release](release/APP_STORE_EN.md).
Native checks exercise Danish culture while verifying English text and layout.
See [the current plan](IMPLEMENTATION_PLAN.md) and [test report](TEST_REPORT.md).

# Build 10 — 21. oktober 2026

Efter brugerens konkrete iPhone-feedback og valg af relativt joystick:
automatisk midterlinjegang med ét gemt sving, ingen fri kamera-/sidebevægelse,
`Liv:` med hjerter og iOS Playback-lyd. Snapshot v3 bevarer svingkø og læser v1/v2.
Se [den aktuelle plan](IMPLEMENTATION_PLAN.md), [inputbeslutningen](INPUT_DECISION.md)
og [testrapporten](TEST_REPORT.md). Tidligere builds nedenfor er historik.

# Historik til og med build 6

Historiske resultater; aktuel status står i [planen](IMPLEMENTATION_PLAN.md).

## Aktuel status

Implementering påbegyndt 21. oktober 2026. Planen blev gemt før kodearbejdet.

| Område | Implementeret | Verificeret |
| --- | --- | --- |
| Projekt | Unity 6000.3.26f1, URP, scene, materialer, metadata, package lock og portable scripts | Native import/validering, Mac-build, iOS-eksport, signeret Xcode-build, installation og automatisk afprøvning på iPhone 13 lykkedes |
| Bane/styring | Dokumentets JSON, fri kollisionsbevægelse, tunnel, touchjoystick, kameratræk, tastatur/controller | Regeltests samt faktisk Mac/iPhone-player med tre syntetiske touchpunkter består; brugeren har manuelt bekræftet samtidig joystickbevægelse og kameratræk på iPhone 13 |
| Grundrunde | Fire AI-roller, base/retur, hjerter, kraft, score, chancer, fangst, levels og Game Over | Regeltests og traversal af 244 hjerter består; native sidste-hjerte, levelskifte, Game Over og genstart består på Mac/iPhone med kontrollerede snapshots |
| Genstande/vægge | Ét inventarfelt, banankast med bue/håndbevægelse, olie, boost, tydelige fælder, varsling, vægcyklus og sikre passager | Regeltests samt brug/visning af alle genstande og varsling/åbning/reservation/lukning består i Mac/iPhone-app |
| Lagring | Atomisk snapshot/settings, backup, validering, profil/RNG/trail, kasttilstand og pauset genoptagelse; snapshot v1 migreres til v2 | .NET/Newtonsoft-tests og faktisk Mac/iPhone save/resume består |
| Præsentation | Forbedret Putin-reviewpilot med ansigtsovergang, profil/hår, vægtning og fartstyrede skridt; øvrige prototypemodeller | Build 6: Mac- og fysisk iPhone-kontrol af skelet, fodkontakt, skala/retning, pause, glid, power, retur og hvile består |
| Performance | Opt-in Release-måler, billedintervaller, samplet hukommelse, iOS-temperatur/batteri og automatisk spiller med normale regler | Build 4: 900,04 sekunder / 59,98 fps. Build 6 med forbedret Putin-pilot: 60,01 sekunder / 59,97 fps, p95 18,00 ms. Fysisk iPhone 13, ingen runtime-fejl, temperaturprøver nominal |
| Karakterreferencer | Fem uændrede runtimekopier, portrætter, Putin Blender/FBX/rig/animation og SHA-256-manifester | Kopier samt pilotens source/FBX-hashes kontrolleres; endelige 3D-assets mangler |

**Kontroller:** 55 C#-tests (50 spilregeltests, tre måletests og to tests af animationsuret) bestået, separat Unity C#-kompilering med 0 fejl/
advarsler, Newtonsoft save/resume-test, repositoryvalidering og native Mac-
runtimekontrol bestået. iOS-eksport samt signeret Debug-build/installation på
iPhone 13 med iOS 26.6.2 er gennemført. Den samme automatiske runtimekontrol
består på den fysiske iPhone, og faktiske skærmbilleder er kontrolleret.
Build 2 er kontrolleret med det udvidede scene-/inputforløb, inklusive begge
layouts, genstande, vægge, porte og centrale spilfaser. Build 3 består også på
Mac og fysisk iPhone, inklusive banankast, hånd, pause/gem/genoptagelse midt i
luften og reduceret bevægelse. Build 6 udvider til 19 skærmbilleder samt
`character-check.json` med målt fodkontakt og ekstra profil-/bagfra-visninger.
Se [testresultater](TEST_REPORT.md) for kommandoer og grænserne for denne evidens.

**Værktøjer:** Den tidligere Unity-licensblokering er løst via Unity Hub.
Editor og iOS Build Support findes under `.tools/`; Xcode 27.1 og .NET 10.0.401
er tilgængelige. Blender 4.5.14 LTS ligger lokalt under `.tools/blender/`;
officiel ARM64-download er kontrolleret mod SHA-256. Meshy-adgang er afvist af
Chrome-værktøjets sikkerhedskontrol; ingen upload, kontoændring eller køb er udført.
Computer-use er blokeret af en
ChatGPT-klient/server-versionsforskel; den grafiske Mac-kontrol bruger derfor
en automatisk udviklingsbuild og faktiske skærmbilleder.

**iPhone-status:** Version 0.1.0 (6) er installeret og startet normalt efter
bestået udvidet automatisk kontrol. Native IL2CPP-stripping er rettet via `Assets/link.xml`.
Aktuelle 19 skærmbilleder, fodkontakt og funktionsresultat ligger lokalt i `.artifacts/smoke-iphone-build6/`.
Den korte Release-performancekørsel med piloten ligger i `.artifacts/performance-iphone-build6/`;
uændret JSON/CSV følger repoet under `docs/verification/`. Tidligere billeder er bevaret.
Brugeren har 21. oktober 2026 bekræftet, at joystickbevægelse og kameratræk
fungerer samtidigt med rigtige fingre på iPhone 13. Manuel brug af genstand
som tredje touch samt de øvrige spil- og enhedskontroller er fortsat udestående.

**Næste skridt:** Afprøv manuelt genstand samtidig med bevægelse/kamera, begge landscape-retninger og faktisk spil
mod AI. Kør `python3 Tools/unity.py smoke-mac` for reproducerbar Mac-kontrol.
Der må ikke erklæres fuld M1–M4-accept, før de relevante runtimekontroller er
gennemført. Performanceinstrumentering og den 15 minutter lange prototypekontrol
er nu udført. Profil/ansigt, hår, teksturovergange, vægtning og fodkontakt er
forbedret i build 6. Næste implementeringsområde er Xi med egen silhuet og
bevægelse via den teknisk afprøvede pipeline.
Kim, Trump og Zelenskyjs hånd følger derefter; iPad og manuel afprøvning
udestår. AI-piloten kræver fungerende, autoriseret adgang og gratis eksport.
Den lokale Blender-pilot gør videre assetarbejde muligt uden denne tjeneste.
Mål igen efter endelige assets.

**Aktuel arbejdsblok — build 6:** Forbedret Putin-pilot. 17.404 importerede
trekanter, ti materialer, formet profil, hår med striber, blød teksturkant,
sammenhængende ærmer og stoflag tilpasset jakkens krumning. Jagt/retur bruger
bagte, løste benstillinger. Normal jagt følger 0,88 meter pr. skridtcyklus;
returens miniature har et læsbart særskilt tempo. Animationstilstande caches.
Overraskelse fortsætter ikke ind i base/hvile.

55 C#-tests, Newtonsoft, repositorykontrol og kompilering med 0 fejl/advarsler
består. Den endelige native Mac-kontrol består med 19 PNG'er; 23 støtteprøver
har højst 0,54 mm afvigelse i den plantede ankelhøjde. Blender-generatoren
kontrollerer desuden skosålernes faktiske deformerede vertices gennem alle
40 nøglebilleder i jagtcyklussen. Front, profil, bagfra, trekvart og jagt er renderet og
visuelt kontrolleret; uændrede Blender-renders og geometrirapport følger repoet.
En separat Mac Release-måling gennemførte 30,02 sekunder: 59,63 fps, p95
17,25 ms og ét langt interval på 65,20 ms. Rapport/CSV og native Mac-billeder
følger repoet. Det er ikke en mobilmåling.

iOS-eksport og signerede Xcode Debug-/Release-builds af 0.1.0 (6) er klar i
`Builds/iOS-DerivedData/Build/Products/Debug-iphoneos/MazeofEgos.app` og
`Builds/iOS-Performance-DerivedData/Build/Products/Release-iphoneos/MazeofEgos.app`.
Den tidligere CoreDevice 4016-forbindelsesfejl er løst efter gentilslutning og
oplåsning. Begge builds er installeret og afprøvet på fysisk iPhone 13.
Den korte Release-måling gennemførte 60,01 sekunder efter ti sekunders
opvarmning: 59,97 fps, p95 18,00 ms, p99 18,50 ms, værste interval 20,05 ms
og ingen intervaller over 33,33 ms. Alle 13 temperaturprøver var nominal.
Der var 71 opsamlede hjerter, én fangst og én brugt genstand. JSON/CSV følger
repoet. Det er en kort pilotmåling, ikke en ny 15-minuttersmåling.

Den samme udvidede Debug-kontrol består på iPhone: både friske logs
`RUNTIME SMOKE PASSED` og resultatfilens `PASS` er kontrolleret. Alle 19 PNG'er
er 2532 × 1170. Fodkontrollen måler højst 0,54 mm afvigelse over 23 prøver og
observerer en løftet svingfod. Jagt, profil og bagfra er visuelt kontrolleret;
uændrede billeder og kontrolrapport følger repoet. Debug-build 6 er startet
normalt igen uden testflag. Se [testresultater](TEST_REPORT.md).

**Tidligere arbejdsblok — build 5:** Første lokale riggede karakterpilot. Putin
bruger en importeret FBX med 17 knogler, 10.796 importerede trekanter, ni
URP-materialer og Idle/Chase/Surprise/Slip/Return-klip. Spillets ur sampler
animationerne; pause fryser både skelet og position. Power farver tøjet blåt,
reduceret bevægelse giver fast glidepose, og retur bruger miniature/boble.
Redigerbar Blender-kilde og generator er med i repoet. Originalbillederne
er uændrede. Den udvidede Mac-kontrol består med 17 framebuffer-billeder.
53 C#-tests, Newtonsoft og kompilering med 0 fejl/advarsler består.
iOS-eksport, signeret Xcode Debug-build og samme udvidede kontrol på fysisk
iPhone 13 består. Både den friske logs `RUNTIME SMOKE PASSED` og resultatfilens
`PASS` er kontrolleret. Alle 17 PNG'er er 2532 × 1170; jagt, glid, power og
retur er visuelt kontrolleret og uændrede evidenskopier følger repoet.
Appen er startet normalt igen uden testflag; normale gemte spil er bevaret.

En separat Unity/Xcode Release-build med piloten gennemførte 60,02 sekunder
efter 10 sekunders opvarmning: 59,97 fps, p95 17,00 ms, p99 17,25 ms,
værste interval 21,69 ms og ingen intervaller over 33,33 ms. Alle
temperaturprøver var nominal; 71 hjerter, én fangst og én genstand blev
registreret. Det er en kort kontrol og erstatter ikke en ny 15-minuttersmåling
med endelige assets. JSON/CSV og billeder følger repoet under
`docs/verification/`; se [testrapporten](TEST_REPORT.md).

**Tidligere arbejdsblok — build 4:** Performanceinstrumentering, automatisk
stabilitetstest med aktive AI-roller og cache af HUD-tekst. 53 C#-tests,
Newtonsoft, repositoryvalidering, kompilering og udvidet Mac-kontrol består.
Den fysiske iPhone gennemførte en Unity/Xcode Release-kørsel med 15 sekunders
opvarmning og 900,04 sekunders måling: 59,98 fps, p95 17,25 ms, p99 17,75 ms,
ét interval over 50 ms, ingen runtime-fejl og alle temperaturprøver nominal.
Spillet var aktivt i Playing i 830,90 sekunder; resten er normale fangster,
nedtællinger og genstarter. Alle fire AI-roller er set, og der blev opsamlet
1.140 hjerter over syv startede runder. De endelige karaktermodeller indgår ikke.

Rapport og CSV er bevaret uændret i `docs/verification/`. GPU-tider og GC-rate
var ikke tilgængelige; USB-testen dokumenterer ikke uopladet batteriforbrug.
Se [målemetode og gentagelse](PERFORMANCE.md) og [testresultater](TEST_REPORT.md).
Den normale Debug-build 4 består også den udvidede kontrol på fysisk iPhone,
inklusive korrekt synlig HUD-tekst efter opsamling, Game Over og genstart.
Appen er startet normalt igen uden testflag; normale gemte spil er bevaret.

**Tidligere arbejdsblok — build 3:** Bananen flyver i en synlig bue og lander som
fælde. En midlertidig Zelenskyj-hånd med olivengrønt ærme vises under kastet og
spejles med håndlayoutet. Reduceret bevægelse slår snurret og håndsvinget fra og
giver en lavere bue. Kameraets højde og FOV ændres ikke.

Snapshotformatet er nu v2. Build 1/2's v1-spil kan fortsættes; deres bananer
bevares som allerede landede fælder med oprindelig udløbstid. Filnavnet
`session-v1.json` bevares for at finde eksisterende spil; indholdets `version`
bestemmer formatet. V2-spil kræver build 3 eller nyere. Nye feltdata valideres
før gendannelse og kan ikke ændre en aktiv session ved afvisning.

50 regeltests, faktisk Newtonsoft-serialisering, repositoryvalidering og C#-
kompilering med 0 fejl/advarsler består. Native Mac-build/runtime, iOS-eksport,
signeret Xcode Debug-build og samme runtimekontrol på fysisk iPhone 13 består.
Appen er efterfølgende startet normalt uden testflag. Se testresultaterne.

**Udestående grafik:** AI-pilot og endelige modeller/rigs/animationer er ikke
udført. Putin har en lokal rigget reviewpilot; de øvrige fuldkropsmodeller og
hånden er primitive prototyper. Følg [karakterproduktionen](CHARACTER_PRODUCTION.md).
M5 er fortsat åben og kræver visuel accept på enhed.

**Bevidste prototypeforenklinger:** Putin har fast vred ansigtstekstur uden
ansigts-blendshapes. Profilen har begrænsede teksturdetaljer, og håret er
stiliseret; endelig genkendelighed og bevægelse kræver visuel accept. Hånden og
de øvrige forfølgere bruger enkel
prototypegeometri/-animation. Banan-/oliefælder har særskilt geometri og bananen
en faktisk kastbue. Belysningen bruger ambientlys
og én dynamisk sol. Der er ingen lysbagning; performance med endelige modeller
skal måles igen. Haptik er
den simple platformsvibration, ikke et færdigafstemt haptikdesign.

**Versionsstyring:** Build 5 findes i repoets commit `380456d` ved starten af
denne arbejdsblok. Build 6-ændringerne gemmes på disken; der er ikke udført
commit eller push i denne arbejdsblok. For overdragelse
via kloning skal ændringerne også committes og synkroniseres. Lokale downloads,
licenser, caches og builds er bevidst ignoreret.

Den oprindelige banes rettighedsstatus er fortsat `unverified`, som angivet i
specifikationen. Dette er en udgivelsesafhængighed og blokerer ikke kodearbejdet.
