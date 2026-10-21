# Performance og stabilitet

Målet er 60 fps på iPhone 13 og mindst 15 minutters sammenhængende afvikling.
En afsluttet stabilitetstest er ikke i sig selv accept af billedhastighed,
batteriforbrug, manuel styring eller de endelige karaktermodellers performance.

## Målemetode

En særskilt **Unity Release-player** indeholder kun måleværktøjet via
`MAZE_PERFORMANCE`. Det starter udelukkende med `MAZE_PERFORMANCE_TEST=1`.
Udviklingsbuilds indeholder også værktøjet, men målinger fra dem mærkes særskilt
med `developmentBuild=true`. Almindelige Release-builds indeholder det ikke.
Der ændres ingen signering, konti eller netværksadgang i spillet.

Standardforløbet er 15 sekunders opvarmning efterfulgt af mindst 900 sekunders
måling. En automatisk spiller går ad lovlige gangfelter mod de resterende hjerter,
bruger opsamlede genstande og starter en ny runde efter Game Over. Alle fire
AI-roller, fangst, point og vægge bruger normale regler. Der gives ingen
usårlighed eller kunstig genopfyldning, og der gendannes ingen test-snapshots.
Kameraet drejer undervejs. Haptik er slået fra i testens særskilte indstillinger;
lyd, musik, FOV og grafik bruger standardindstillingerne.

Testen bruger en separat midlertidig save-mappe, aldrig spillerens normale filer.
På iPhone afslutter baggrund, pause eller runtime-fejl med fejl frem for at
skjule en uafsluttet måling. På Mac fortsætter den udtrykkeligt valgte
performancekørsel ved tab af vinduesfokus, så kompilering eller arbejde i et
andet vindue ikke stopper testen. Normale spils fokus/pause-adfærd bevares. Skærmen holdes tændt af spillets eksisterende opsætning.

Billedinterval måles med `Stopwatch` mellem `LateUpdate`-kald; lange pauser
forkortes ikke til simulationens maksimale tidsstep. FPS beregnes som antal
billeder divideret med samlet tid. Et fast histogram giver p50/p95/p99 med
0,25 ms opløsning; værdier over histogrammets område giver den faktiske værste
tid som konservativ øvre grænse. Der registreres også antal intervaller over
33,33 og 50 ms. Der allokeres ikke en ny samling for hvert billede.

CPU-hovedtråd og GPU måles med Unitys ProfilerRecorder, hvis tællerne findes.
Manglende data vises med nul målinger og må ikke fortolkes som nul arbejde.
CPU-hovedtrådens frame-tæller kan indeholde ventetid og er ikke et mål for
ren script-CPU-belastning.
GC-allokering rapporteres kun som målt, når tælleren er tilgængelig. Alle
ProfilerRecorder-ressourcer frigives ved afslutning.

Hvert femte sekund måles Unity-allokeret hukommelse, managed heap og på iPhone
processens resident memory via `task_info`. Toppene er **samplede** topværdier;
de forskellige hukommelsesbegreber kan ikke lægges sammen. iOS' offentlige API
leverer temperaturtilstand (0 nominal, 1 fair, 2 serious, 3 critical), Low Power
Mode og batteriniveau/-tilstand. Der hævdes ingen temperatur i grader.
USB-opladning betyder, at testen ikke kan afgøre batteriforbrug uden opladning.

Måler, automatisk styring, femsekunders checkpoints og logning har selv en
omkostning og er med i resultatet. Der tages ikke screenshots under målingen.

## Gentag på Mac

```sh
python3 Tools/unity.py build-mac-performance
python3 Tools/performance.py mac --seconds 30 --warmup 3
# Lang måling: udelad --seconds/--warmup for standarden 900/15.
python3 Tools/performance.py report .artifacts/performance-mac
```

Rapport og CSV ligger i `.artifacts/performance-mac/`, log i
`Logs/performance-mac.log`. Mac-kørsel kontrollerer værktøjet; den dokumenterer
ikke mobilperformance. Samtidig kompilering kan påvirke Mac-tiderne.

## Gentag på iPhone

```sh
python3 Tools/unity.py build-ios-performance
```

Signér `Builds/iOS-Performance/Unity-iPhone.xcodeproj` med eget team i Xcodes
**Release**-konfiguration, installér på en oplåst fysisk telefon og start:

```sh
xcrun devicectl device process launch --device "DIN_IPHONE" --terminate-existing --console --environment-variables '{"MAZE_PERFORMANCE_TEST":"1","MAZE_PERFORMANCE_SECONDS":"900","MAZE_PERFORMANCE_WARMUP_SECONDS":"15"}' dk.mazeofegos.game
xcrun devicectl device copy from --device "DIN_IPHONE" --domain-type appDataContainer --domain-identifier dk.mazeofegos.game --source Library/Caches/maze-performance --destination .artifacts/performance-iphone
python3 Tools/performance.py report .artifacts/performance-iphone
```

Kontrollér både `PERFORMANCE COMPLETED` i den friske log, `PASS` i `result.txt`
og `completed`, `stabilityPassed`, `developmentBuild=false`, varighed og faktisk
aktivitet i `report.json`. Enhedsprocessens exitkode alene er utilstrækkelig.
`checkpoint.json` indeholder seneste status, hvis testen afbrydes. `samples.csv`
er tidsserien; `report.json` indeholder målemetode, miljø og samlet statistik.
Efter testen åbnes appen uden testvariablen. Opvarmningens indsamlinger tælles
ikke med i den målte periodes aktivitet. Testen fejler ved 45 sekunder uden
spilprogression. Et stabilitetspas kræver også mindst halvdelen af tiden i
aktiv spilfase og mindst ét opsamlet hjerte pr. ti målte sekunder.

En ny resultatfil overskriver lokale testartefakter, ikke normale gemte spil.
Bevar sammenlignelige rapporter under forskellige navne før en ny kørsel.

## API-grundlag

- [Unity ProfilerRecorder](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html): måling i Release og korrekt frigivelse.
- [Unity Frame Timing Manager](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/FrameTimingManager.html): forsinkede CPU/GPU-data og Metal-forbehold; GPU-tider kan på Apple-hardware overstige billedintervallet.
- [Apple thermalState](https://developer.apple.com/documentation/foundation/processinfo/thermalstate-swift.property) og [batteriovervågning](https://developer.apple.com/documentation/uikit/uidevice/isbatterymonitoringenabled): offentlige systemmålinger.

Aktuelle resultater og grænser dokumenteres i [testrapporten](TEST_REPORT.md).
