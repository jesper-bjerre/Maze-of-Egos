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
