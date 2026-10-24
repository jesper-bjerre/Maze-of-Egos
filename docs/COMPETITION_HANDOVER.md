# Konkurrenceversionen — overdragelse, 24. oktober 2026

Build 17 samler karakterfoto/navn i én klikbar boks, giver vandballonen en radius på ét banefelt med præcis visuel markering og kort landingsplask, afslutter runden ved sidste hjerte og viser portrætter/navne/point i rundeafgørelsen. 122 spilchecks, native tests i tre Mac-formater/begge iPhone-landscapes og faktisk OS-livscyklus består. Normal Release 0.2.0 (17) er installeret og startet på iPhone 13. Se [testrapporten](TEST_REPORT.md). Build 16's 300-sekunders performance-måling er historik, ikke en ny måling af build 17. Manuel spilleglæde og slutgrafik er fortsat ikke accepteret.

## Spil og arkitektur

PLAY åbner figurvalg mellem Kim, Putin, Trump, Xi og Zelenskyy. Alle fem bruger
samme navigation, genstande og genstart. Fire computere konkurrerer indbyrdes
og mod mennesket. De kender banens faste layout og får samme minikortinformation som mennesket: resterende hjerter, aktive positioner og genvejsstatus. Angreb, modstanderes inventar og beskyttelse observeres kun lokalt.
Den faste kameramodel deles med AI'ens projektion af synsfeltet.

`CompetitionSession` er en ren C#-simulation med 60 aktive ticks/sekund.
`CompetitionState` definerer regelprofil, deltagere og snapshot. De øvrige
partial-filer opdeler kamp, AI og valideret lagring:

| Fil | Ansvar |
| --- | --- |
| `CompetitionSession.cs` | Runder, point/pokaler, ressourcer, navigation, automatiske genveje og settlement én gang |
| `CompetitionCombat.cs` | Fem genstande, sigte, varsling/baner, KO, genstart, beskyttelse og vægreservation |
| `CompetitionBrain.cs` | Delte kamerakonstanter, fælles kort, lokale kampobservationer og seedet AI med vedvarende hukommelse |
| `CompetitionPersistence.cs` | V1/V2/V3-migrering og atomisk validering af V4, før en kørende kamp ændres |
| `CompetitionApp.cs` | Eneste normale bootstrap; valg, input, fast simulation, pause/OS-livscyklus og gemt kamp |
| `CompetitionView.cs` | Lav 3D-have, alle fem importerede modeller, hjerter, kasser, sigte og angrebsvarsler |
| `CompetitionHud.cs` | Engelsk menu, figurvalg, minikort, fem stillinger, timer, medaljer og samlet pokaltabel |
| `CompetitionMinimap.cs` | Nordvendt banekort, resterende hjerter og farve-/nummerkodede aktive spillere |
| `CompetitionAudio.cs` | Lokal/egen hændelseslyd og eksisterende musik; iOS Playback-politik bevares |
| `CompetitionAutomation.cs` | Opt-in QA i særskilt cache; udeladt fra normal Release |

Core-filerne ligger i `Assets/Scripts/Core`; app-/visningsfilerne i
`Assets/Scripts/Runtime`. Det gamle `GameApp` og de gamle QA-klasser har ikke
længere bootstrap. Classic Core og dens regressionstests er bevaret.

## Første balanceprofil

`Assets/Resources/Data/competition_rules.json` er den versionerede profil:
Højst 120 sekunder pr. runde, eller indtil sidste hjerte er taget; tre pokaler; 1,6 felter/sekund. 240 hjerter ved rundestart, ingen genopfyldning i runden. Alle fem starter på tilfældige, adskilte gangfelter uden for midten med en lovlig gangretning. Kun KO genstarter centralt. En særskilt gemt RNG gør næste rundes fordeling reproducerbar efter restore. Kasser genopfyldes efter ti sekunder og kræver, at man går ud og ind igen. Den seneste opsamlede genstand erstatter den gamle.

Banan giver 0,8 sekunders glid; olie giver 55 % fart i 2,5 sekunder; boost giver
1,6× fart i fire sekunder. Lagkage varsles i 0,35 sekunder og flyver højst fire
felter med fire felter/sekund. Vandballon varsles i 0,35 sekunder, flyver 0,6
sekunder højst tre felter og har et plaskområde med radius ét banefelt.
Vægge stopper angreb. Ejeren rammes ikke af egne genstande. Banan/olie forbruges
ved første træf. KO giver ingen pointstraf, men fem sekunders ventetid. Man
beholder sin genstand og får tre sekunders beskyttelse fra midten; eget angreb
ophæver den, mens boost bevarer den. Alle kan passere hinanden.

AI beslutter normalt hvert 18. aktive tick. Den opdaterer globale korthjerter/positioner og husker lokalt sete kasser, modstandere og genveje. Indsamlede korthjerter fjernes straks fra hukommelsen; lokale ressourceminder ældes efter ét minut. Den vælger
mellem kortets resterende hjerter, lokalt kendte kasser og udforskning. Rivalers kortpositioner bruges til at nedprioritere hjerter, de kan nå først. AI bruger genstande mod lokalt synlige modstandere og reagerer på lokalt synlige angreb. Profilen er teknisk afprøvet, men sjov, sværhedsgrad og
varslingernes læsbarhed kræver stadig menneskelig vurdering.

Alle fem har de eksisterende syv fælles animationer. Zelenskyy har desuden
en særskilt Throw-animation; de øvrige bruger deres Surprise-gestus ved kast.
Modellerne er fortsat de tidligere reviewmodeller, ikke ny godkendt slutgrafik.

## Gemte data og flytning

`competition-v1.json` er filens stabile navn, mens formatets **version er 4**.
Det gemmer alle fem deltagere, runderegnskab, aktive kast/fælder, timere,
genveje, randomstrømme, faktisk rundestarttick, slutårsag og AI-hukommelse. V2/V3 migreres uden at ændre aktuelle positioner, ressourcer eller bankede point, og aktiverer de nye regler fremover. En gammel N2-V1 migreres i hukommelsen
med den hidtidige kommandostyrede aktørtilstand. Nye kampe aktiverer altid fire
computere. CONTINUE åbner i pause; resume-nedtællingen bruger ikke rundetid.

`session-v1.json` og dens backup bevares som klassiske saves og læses ikke ind
som en konkurrence. Eksisterende `settings-v1.json` bevarer lyd, musik, haptik,
venstrehåndslayout, grafik og gammel highscore. Det gamle highscore-tal bruges
ikke i kampregnskabet.

Kopiér den aktuelle arbejdsmappe for at overtage **ikke-committede** ændringer.
En Git-klon får kun allerede committede filer. Installér den fastlåste Unity
6000.3.26f1 og .NET 10; [SETUP](SETUP.md) beskriver pc/Mac-forløbet. `Builds`,
`Library`, `.tools`, `.artifacts` og `Logs` er lokale og ignorerede. Apple-signering
kræver brugerens egen konto og maskinens provisioning. Credentials følger ikke
med repoet.

## Gentag verifikationen

```sh
dotnet run --project Tests/MazeOfEgos.Checks.csproj
dotnet run --project Tests/MazeOfEgos.CompetitionChecks.csproj -- --report Logs/competition-matches.json
dotnet run --project Tests/MazeOfEgos.ArenaChecks.csproj -- Logs/arena-matches.json
python3 Tools/verify_repository.py
python3 Tools/unity.py compile-check
python3 Tools/unity.py serialization-check
python3 Tools/unity.py validate
python3 Tools/unity.py build-mac
python3 Tools/unity.py smoke-mac
python3 Tools/unity.py smoke-mac --width 2400 --height 1080
python3 Tools/unity.py smoke-mac --width 1600 --height 1200
```

iOS-eksporterne er `build-ios` (Debug), `build-ios-performance` (Release med
MAZE_PERFORMANCE) og `build-ios-release` (normal Release uden QA). Brug separate
DerivedData-mapper. Byg færdigt og kontrollér xcodebuild-exitkode, **før** appen
installeres. En ældre app kan stadig ligge i Products under en igangværende build.

`Tools/ios_test.py smoke|lifecycle|performance --device <id> --label <nyt-navn>
--output .artifacts/<ny-mappe>` bruger den allerede installerede tilsvarende
build. Performance kræver `--seconds 900`; lifecycle kræver, at den eksterne
runner faktisk sender appen i baggrunden og tilbage. Testene må aldrig
installeres/startes oven i en kørende måling. Installér til sidst normal
Release uden testflag og verificér version og proces. Ved tab af konsolforbindelsen kræver
performance-runneren stadig en frisk, fuld og godkendt enhedsrapport med
monotone samples; transportstatus gemmes separat. `verify_ios_release.py`
auditerer det lokale archive, ikke en Apple-indsendelse.

Unity kan hænge på “Closing open Unity projects in Xcode”, hvis en tidligere
eksport ligger åben. Bevar eksportmappen ved at flytte den til en ignoreret
`.artifacts`-undermappe, og eksportér til en frisk mappe. Overskriv ikke
brugerens eventuelle manuelle Xcode-ændringer. Kun én Unity-editor ad gangen.

Den resterende produktvurdering er en håndført hel kamp på iPhone: kan man
læse trusler, vælge ruter, undvige og konkurrere om hjerterne? Fysisk iPad er
fravalgt denne omgang. Offentlige support-/privacytekster, rettigheder og
Apple-distribution er ikke en del af denne lokale implementering.
