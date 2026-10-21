# Verifikation — 21. oktober 2026

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

## Fortsat ikke verificeret

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
