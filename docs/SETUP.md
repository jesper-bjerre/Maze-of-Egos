# Opsætning og overdragelse

Start altid med [implementeringsplanens status](IMPLEMENTATION_PLAN.md).
Kildekode, banedata, grafikreferencer, Unity-metadata og projektindstillinger er
samlet i repositoryet. Unity-editoren, caches og builds er lokale værktøjer.

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

`smoke-mac` kræver den grafiske macOS-udviklingsbuild. Den starter appen med et
udtrykkeligt testflag, sender syntetiske touchhændelser gennem Input System og
kontrollerer menu, bevægelse/kamera/genstand samtidigt i begge layouts, slip,
pause, indstillinger, lagring/genoptagelse (også midt i banankast), reduceret
bevægelse, genstande, sikre vægskift, baseporte,
levelskifte, Game Over og genstart. Flere forløb opsættes med kontrollerede
snapshots; testen er ikke en normal hel spilrunde mod AI.
Testdata gemmes i en særskilt midlertidig mappe; brugerens
gemte spil ændres ikke. Resultat og 13 faktiske skærmbilleder ligger i
`.artifacts/smoke-mac/`; runtime-loggen ligger i `Logs/runtime-smoke-mac.log`.
Testkoden er kun med i editor- og udviklingsbuilds og kører kun med testflaget.

Grundprofilen kan afprøves ved at sætte de tre uafhængige flag i
`Assets/Resources/Data/rules.json` til false. Hovedoplevelsen har alle tre true.
Et gemt spil bevarer sin egen regelprofil ved genoptagelse. Fra build 3 er
snapshotindholdets version 2; tidligere version 1 migreres ved indlæsning.
Filnavnet `session-v1.json` bevares med vilje, så eksisterende spil findes.
Version 2 kan ikke åbnes i ældre builds. Kastets position og tider gemmes
sammen med resten af runden; pause og nedtælling forbruger ingen kasttid.

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
Testen sender syntetiske touchhændelser og gemmer 13 faktiske framebuffer-PNG'er.
Den gemmer særskilte testdata og lukker processen efter kontrollen.
Start derefter appen normalt fra hjemmeskærmen for manuel afprøvning.
De første dokumenterede iPhone-skærmbilleder findes under `docs/verification/`.

## Performance og lang afprøvning

Se [performancevejledningen](PERFORMANCE.md) for den særskilte Release-build,
automatisk gennemspilning, isolerede testdata og eksport af JSON/CSV.
En kort Mac-kørsel erstatter ikke den planlagte 15-minuttersmåling på iPhone.

## Hvis arbejdet afbrydes

- Læs status og næste skridt i `IMPLEMENTATION_PLAN.md`.
- Kør kontrollerne; se dokumenterede resultater i `TEST_REPORT.md`.
- Ret de åbne blokeringer før nye funktioner tilføjes.
- Opdatér plan/status og relevante testresultater efter næste arbejdsblok.
- Lokale kildefiler overlever en afbrudt chat. Overdragelse via kloning kræver
  også et Git-commit og push. `.tools/`, `Library/`, `Logs/` og `Builds/` er
  tilsigtet ignoreret; de skal ikke med i kildekodeoverdragelsen.
