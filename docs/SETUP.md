# Opsætning og overdragelse

Start altid med [implementeringsplanens status](IMPLEMENTATION_PLAN.md).
Kildekode, banedata, grafikreferencer, Unity-metadata og projektindstillinger er
samlet i repositoryet. Unity-editoren, caches og builds er lokale værktøjer.

## Aktuel videreførelse — konkurrencen i build 17

Build 17 tilføjer klikbare karakterkort, vandballonradius på ét banefelt, tidlig rundeslut ved sidste hjerte og portrætter/navne/point i afgørelsen. Snapshot V4 migrerer V1/V2/V3 uden at flytte igangværende spillere og gemmer faktiske korte runders tid. Native tests og normal installation/opstart af build 17 består. Manuel gameplayaccept står åben. Se [testrapporten](TEST_REPORT.md) og den øverste status i
[implementeringsplanen](IMPLEMENTATION_PLAN.md) for faktisk installeret build.

Kør fra rodmappen på en ny pc/Mac med .NET 10:

```sh
dotnet run --project Tests/MazeOfEgos.Checks.csproj
dotnet run --project Tests/MazeOfEgos.CompetitionChecks.csproj -- --report Logs/competition-matches.json
dotnet run --project Tests/MazeOfEgos.ArenaChecks.csproj -- Logs/arena-matches.json
python3 Tools/verify_repository.py
python3 Tools/unity.py compile-check
python3 Tools/unity.py serialization-check
python3 Tools/unity.py validate
```

Arena-suiten gennemfører fem fulde kampe med produktions-AI og hver valgbare
figur. Den samme fælles kort-/lokale kampobservationsmodel leverer menneskets automatiske input
i testen. Dette er simuleret tid, ikke fysisk enhedsperformance.

`CompetitionApp` starter nu spillet; det gamle `GameApp`-flow har ingen
bootstrap. Classic Core og dens regressionstests er bevaret. Redigér balancen i
`Assets/Resources/Data/competition_rules.json`. Kameraets delte udsnitsmodel
ligger i `LocalArenaView` og bruges både af præsentation og AI-observationer.

`competition-v1.json` er den selvstændige fil, nu med **snapshot-version 2**:
combat, KO, navigation, alle seedstrømme, AI-hukommelse og regnskab. V1 fra N2
migreres i hukommelsen. Klassiske `session-v1.json` og dens backup bevares.
`settings-v1.json` bevarer eksisterende indstillinger og gammel highscore;
highscore blandes ikke ind i kampens point. CONTINUE åbner i pause.

Testflag er udeladt fra normal Release. Opt-in-smoke/lifecycle/performance
bruger egne mapper under appens cache og rører ikke normale gemte kampe.
Fysisk iPad er fravalgt denne omgang; ingen Apple-upload indgår.

## Unity Hub og licens

1. Installér [Unity Hub](https://unity.com/download), og log ind med din egen
   Unity-konto. En konto kan oprettes i dette flow.
2. Unity Personal aktiveres normalt ved login. Hvis ikke, vælg **Settings →
   Licenses → Add license → Get a free personal license**. Læs og accepter selv
   betingelserne, hvis du opfylder dem. Der er ikke aftalt et betalt abonnement.
3. Kontrollér, at en aktiv licens fremgår under Licenses.

Se [Unitys officielle licensvejledning](https://docs.unity.com/en-us/hub/manage-license).
Licensaktivering er knyttet til computeren/kontoen; credentials og licensfiler
skal ikke lægges i Git eller sendes i chatten.

På den oprindelige Mac er editoren og iOS Build Support allerede hentet til den
ignorerede `.tools/`-mappe. Hjælpescriptet finder denne installation automatisk.
Den skal ikke downloades igen alene for at aktivere licensen. Aktivér via Hub,
og kør derefter `python3 Tools/unity.py validate`.

Hvis Hub skal registrere den lokale editor, brug Installs/Locate og vælg:

```text
<repository>/.tools/unity-expanded/Unity.pkg.tmp/Payload/Unity/Unity.app
```

Finder kan åbne en skjult mappe via Cmd+Shift+G. Den lokale installation flytter
ikke automatisk med Git; på en anden computer installeres versionen nedenfor.

## Ny pc eller Mac

1. Klon/kopiér det komplette repository med `Assets/`, `Packages/`,
   `ProjectSettings/`, `Tests/`, `Tools/` og `docs/`, inklusive `.meta`-filer.
2. Installér og aktivér Unity Hub, og installér præcis **6000.3.26f1**.
   Versionen er også angivet i `ProjectSettings/ProjectVersion.txt`.
3. På Mac, tilføj **iOS Build Support**, hvis der skal eksporteres til Xcode.
   Windows kan bruges til kode, regelsimulation og editorafprøvning; lokal
   Xcode-bygning og signering foregår på Mac.
4. Tilføj repositoryets rodmappe som projekt i Hub. Opret ikke et nyt projekt
   oven i filerne. Unity genskaber selv `Library/` og henter pakkerne.
5. Kør **Tools → Maze of Egos → Configure project**, åbn
   `Assets/Scenes/Maze.unity`, og tryk Play.
6. Installér .NET 10 SDK for regeltests og Python 3 for repositoryets hjælpescripts.
   På Windows bruges normalt `python` i kommandoerne nedenfor.

Pakker er fastlåst direkte i `Packages/manifest.json`: URP 17.3.0, Input System
1.20.1, uGUI 2.0.0 og Newtonsoft JSON 3.2.1. `Packages/packages-lock.json` er nu
genereret ved en vellykket Unity-import og medtages i kildekoden til overdragelse.
En licensblokering må ikke omgås eller rapporteres som en build.

## Kommandoer

Kør fra repositoryets rod:

```sh
dotnet run --project Tests/MazeOfEgos.Checks.csproj
python3 Tools/verify_repository.py
python3 Tools/unity.py compile-check
python3 Tools/unity.py serialization-check
python3 Tools/unity.py validate
python3 Tools/unity.py build-mac
python3 Tools/unity.py smoke-mac
python3 Tools/unity.py build-ios
```

Hvis editoren ligger et andet sted, tilføj `--editor /sti/til/Unity` til
`Tools/unity.py`. På Mac kan også en `.app` angives. Alternativt kan den lokale
variabel `MAZE_UNITY_EDITOR` pege på editoren; den gemmes ikke i projektet.

`compile-check` kompilerer mod editorens faktiske managed biblioteker og URP-
skabelonens cache med C# 9/.NET Standard 2.1. Det starter ikke editoren og beviser
ikke, at rendering, sceneimport eller multi-touch fungerer. `serialization-check`
bruger editorens Newtonsoft-bibliotek til et faktisk save/resume-rundløb.

`smoke-mac` kræver den grafiske udviklingsbuild og starter kun med et udtrykkeligt
flag. Den bruger syntetisk touch i begge håndlayouts og kontrollerede kampfixtures:
menu, figurvalg, lokalt kamera, alle fem genstande, pause/indstillinger,
save/continue midt i kast, KO/genstart samt delte runde- og kampresultater.
Der gemmes 14 faktiske framebuffer-PNG'er pr. Mac-format; iPhone har desuden
gameplay i begge landscape-retninger og i alt 16 PNG'er. Testkoden bruger
særskilt cache og rører ikke normale gemte kampe. Fuld AI og 900-sekunders
performance er særskilte checks; syntetisk touch erstatter ikke fingerspil.

`CompetitionAutomation` findes kun i editor-, udviklings- og den særlige
MAZE_PERFORMANCE-Release-build. Normal Release udelader klassen helt.
`competition_rules.json` indeholder den nye balance. Den klassiske `rules.json`
og `session-v1.json` med internt snapshot v3 er historiske, bevarede data;
de er ikke konkurrenceflowets profil eller kampfil.

## Bygning til iPhone

Efter `build-ios` åbnes `Builds/iOS/Unity-iPhone.xcodeproj` i Xcode. Vælg dit
Apple-udviklerteam og en fysisk enhed i Xcode. Standard-bundle-id'et
`dk.mazeofegos.game` er en lokal startværdi; endelig App Store-identitet er ikke
oprettet. Ingen signeringsidentiteter eller adgangsoplysninger er indbygget.

Tilfældigt valgte signeringsprofiler fra andre projekter må ikke kopieres ind.
En build tæller først som enhedstest, når installation og afvikling er verificeret.

### Automatisk kontrol på fysisk iPhone

Efter installation af udviklingsbuilden kan den samme runtimekontrol startes
på en oplåst, tilsluttet telefon. Erstat `DIN_IPHONE` med enhedsnavnet fra
`xcrun devicectl list devices`:

```sh
xcrun devicectl device process launch --device "DIN_IPHONE" --terminate-existing --console --environment-variables '{"MAZE_SMOKE_TEST":"1"}' dk.mazeofegos.game
xcrun devicectl device copy from --device "DIN_IPHONE" --domain-type appDataContainer --domain-identifier dk.mazeofegos.game --source Library/Caches/maze-smoke-captures --destination .artifacts/smoke-iphone
```

Kontrollér både **RUNTIME SMOKE PASSED** i loggen og `PASS` i det hentede
`result.txt`; iOS kan afslutte processen med kode 0 også ved en testfejl.
Testen sender syntetiske touchhændelser og gemmer 16 faktiske framebuffer-PNG'er.
Den gemmer særskilte testdata og lukker processen efter kontrollen.
Start derefter appen normalt fra hjemmeskærmen for manuel afprøvning.
De første dokumenterede iPhone-skærmbilleder findes under `docs/verification/`.

## Performance og lang afprøvning

Se [performancevejledningen](PERFORMANCE.md) for den særskilte Release-build,
automatisk gennemspilning, isolerede testdata og JSON med den fulde måleserie.
En kort Mac-kørsel erstatter ikke den planlagte 15-minuttersmåling på iPhone.

## Redigér karakterpiloten

FBX, materialer, importerindstillinger og redigerbar `.blend` følger repoet.
Unity behøver ingen lokal Blender-installation. Ved ændring af geometri/rig
bruges Blender 4.5 LTS og [karakterproduktionsvejledningen](CHARACTER_PRODUCTION.md).
Generatoren og assets har relative projektstier; referencebilledet er pakket
ind i Blender-filen og bevares også som uændret PNG. `.tools/` er lokal og
skal installeres på ny på en anden computer.

## Hvis arbejdet afbrydes

- Læs status og næste skridt i `IMPLEMENTATION_PLAN.md`.
- Kør kontrollerne; se dokumenterede resultater i `TEST_REPORT.md`.
- Ret de åbne blokeringer før nye funktioner tilføjes.
- Opdatér plan/status og relevante testresultater efter næste arbejdsblok.
- Lokale kildefiler overlever en afbrudt chat. Overdragelse via kloning kræver
  også et Git-commit og push. `.tools/`, `Library/`, `Logs/` og `Builds/` er
  tilsigtet ignoreret; de skal ikke med i kildekodeoverdragelsen.


## Build 12 — illustreret startskærm

`docs/images/Jagten gennem egoernes labyrint.png` kopieres uændret til
`Assets/Resources/Brand/MenuBackground.png`. Unity konfigurerer billedet som
UI-tekstur med oprindelige proportioner. Startmenuen har PLAY, CONTINUE SAVED
GAME og HELP i højre side; Settings findes via pausemenuen. Bredformat bruger
en mørk udvidelse med blød overgang til billedet. Kopiér begge PNG-filer ved
manuel overdragelse; repositoryvalideringen kræver, at de er identiske.

## Build 11 — engelsk produkt

Alle spillertekster, verdensskilte og udgivelsesudkast er engelske. Appen
angiver kun `en` som iOS-sprog. Score og effekttider bruger engelsk talformat
uafhængigt af enhedens sprog. Spil-/settingsfilernes format og navne er uændrede.
Den native kontrol tvinger `da-DK` kultur, så engelsk tekst og formatering også
kontrolleres på en dansk enhed. Ved hver optagelse kontrolleres tekstens højde.

## Aktuel test og perspektivprototype fra build 14

Alle figurer er nu importerede FBX-modeller; Unity kræver stadig ikke Blender.
Læs den seneste status i planen, da ældre Build 1–6-beskrivelser ovenfor er
historiske. N1 viser hele spilleren fra et lokalt, højt kamera og bruger
joysticket efter gangene på skærmen. Build 14 har en markant tættere visning:
9 meter i billedhøjde mod build 13's 17 meter. Det er stadig den gamle regelsimulation;
konkurrence-Core er nu implementeret i N2; angreb følger N3 og figurvalg/
integration følger N4. Den udvidede N1-kontrol producerer
43 PNG'er pr. Mac-format og 47 på iPhone.
Fire separate `*-character-check.json` indeholder fodmålingerne. Begge fysiske
landscape-retninger og safe area testes på iOS; desktop springer netop den
fysiske rotationskontrol over.

```sh
python3 Tools/unity.py smoke-mac --width 1600 --height 1200
python3 Tools/unity.py smoke-mac --width 2400 --height 1080
# Efter installation af den signerede Debug-app på din egen enhed:
python3 Tools/ios_test.py smoke --device DIN_IPHONE --label build14
python3 Tools/ios_test.py lifecycle --device DIN_IPHONE --label build14
# Efter installation af den særskilte signerede Release-performanceapp:
python3 Tools/ios_test.py performance --device DIN_IPHONE --seconds 60 --label build14
# Normal Release-eksport uden testharness:
python3 Tools/unity.py build-ios-release
```

`ios_test.py lifecycle` åbner systemets Indstillinger, venter otte sekunder og
aktiverer den samme kørende app igen. Den kræver faktiske OS-callbacks og
kontrollerer frosne timere, disk-checkpoint og resume-nedtælling. Kun den
udtrykkeligt startede test bruger dette forløb; normal gameplay gør ikke.
Runneren kræver en frisk lokal outputmappe og bevarer tidligere evidens.
Brug en ny `--label`/`--output` ved gentagelser. Fra build 13 kontrolleres desuden
fast kameravinkel, hel spiller, projektion af alle fire retninger, begge
tunnelovergange og gameplay i begge landscape-retninger. Der er ikke optaget
en ny performancekørsel; en performancekommando kræver først en frisk,
særskilt performanceeksport og signering. De gamle build 10-performancefiler
må ikke bruges som måling af build 14.

En normal Release-eksport har hverken `DevelopmentSmoke`, `DevelopmentLifecycle`
eller `DevelopmentPerformance` i den genererede spilassembly. Den signerede
lokale archive ligger under `Builds/Archives/`, som er ignoreret. Dens signering
er til lokal enhedsafprøvning; App Store-distribution og upload udføres særskilt
efter review. Brug dit eget udviklerteam ved signering på en ny Mac.

Normal archive for den tættere visning: `Builds/Archives/MazeOfEgos-0.2.0-14.xcarchive`.
Audit: `python3 Tools/verify_ios_release.py --build 14`.
Luk Xcode før Unitys iOS-eksport, hvis Unity bliver stående ved
`Closing open Unity projects in Xcode`; genåbn først efter eksporten. Xcode
kan derefter bygge det genererede `Builds/iOS-Release/Unity-iPhone.xcodeproj`.
I build 14 lykkedes eksporten også med Xcode åbent ved først at flytte de
tidligere genererede `Builds/iOS` og `Builds/iOS-Release` til en lokal backup
under `.artifacts/build14-previous-exports/` og eksportere til de nu tomme
placeringer. Bevar gamle eksportmapper frem for at slette eventuelle lokale
Xcode-ændringer. Unitys forsøg på at lukke et eksisterende projekt blev derved
undgået; kildeprojektet og de tidligere archives blev bevaret.

Mac 4:3 og bredformat validerer layout og input i de faktiske renderstørrelser;
de erstatter ikke fysisk iPad-accept. Fuld testdokumentation og afgrænsninger
står i [testrapporten](TEST_REPORT.md) og [udgivelsesstatus](release/READINESS.md).
