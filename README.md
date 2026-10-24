# Maze-of-Egos

Et 3D-arkadespil til iPhone og iPad. Det aftalte nye design er alle mod alle:
én menneskestyret og fire computerstyrede figurer konkurrerer om hjerter i
runder på højst 2 minutter med et lokalt, skråt kamera. Runden slutter også,
når sidste hjerte er taget. Først til tre pokaler vinder.
**Build 17:** Foto og navn er ét klikbart karakterkort. Vandballonen rammer alle
ubeskyttede modstandere inden for ét banefelt fra landingen; den synlige ring
viser samme radius. Rundens guld/sølv/bronze viser portræt med navn og point
under. Samlet stilling med guldpokaler står nedenunder.
Tilfældige rundestarter, central KO-genstart og fælles minikort bevares.
122 spilchecks og native tests i tre Mac-formater samt begge iPhone-landscapes består. Normal Release 0.2.0 (17) er installeret og startet på iPhone 13.
Manuel gameplayaccept står åben. Se [testrapporten](docs/TEST_REPORT.md).

Start med [implementeringsplan og aktuel status](docs/IMPLEMENTATION_PLAN.md).
Planen indeholder aftalte beslutninger, milepæle og instruktioner til overdragelse
mellem computere. Se også [spildesignspecifikationen](docs/Labyrint_3D_Spildesign_og_Codex_specifikation.md)
og [karakterreferencerne](docs/images/).

Projektet bruger **Unity 6000.3.26f1 (6.3 LTS)** med URP. Reglerne er adskilt fra
Unity og kan testes med .NET 10 på Windows/macOS uden at starte editoren.

Læs [opsætning og fortsættelse på en anden computer](docs/SETUP.md), herunder
Unity Hub og aktivering af Unity Personal.

## Start spillet i Unity

1. Tilføj repositoryets rodmappe som projekt i Unity Hub med den fastlåste version.
2. Kør **Tools → Maze of Egos → Configure project** første gang.
3. Åbn `Assets/Scenes/Maze.unity`, og tryk Play.

Figuren går automatisk midt i gangen. Peg joysticket mod den ønskede gang
**på skærmen**; retningen huskes, indtil svinget er muligt. Slip fortsætter
gangen, og en væg stopper figuren. Peg modsat for at vende om. Kameraet følger
spilleren med fast høj vinkel og roterer ikke ved sving. Udsnittets højde er
9 meter, så figuren fylder cirka 1,9 gange mere end i build 13. Knappen til højre
bruger genstanden i gangretningen. WASD/piletaster eller controllerens venstre
stick vælger også skærmretning; mellemrum/nederste controllerknap bruger
genstanden, og Escape/Start åbner pause.

Touch-overlayet har et rundt joystick, større berøringsområde og symboler for
banan, olie, boost, vandballon og lagkage. Venstrehåndslayout kan vælges i Indstillinger.
Se [valget af touchstyring](docs/INPUT_DECISION.md) for vurderingen af Apples
native controllere og vores samlede Unity-inputvej.

Bananen har en kort kastbue og kasteanimation på den synlige figur. Den aktiveres ved landing og
lever derefter i 20 sekunder. Pause og gemt spil bevarer et igangværende kast;
reduceret bevægelse dæmper animationen.

Alle fem figurer har importerede, riggede FBX-modeller
fra lokale Blender-kilder med referencebaserede ansigter og animationer.
Endelig karakterkvalitet udestår. Redigerbare Blender-kilder, FBX og
[produktionsvejledning](docs/CHARACTER_PRODUCTION.md) følger repoet; Blender
kræves kun for at ændre modellerne. Den valgte figur vises i fuld størrelse med
en gul markering og YOU over hovedet. Den gamle
førstepersonshånd bevares som kildeasset, men vises ikke i spillet.

## Kontroller

```sh
dotnet run --project Tests/MazeOfEgos.Checks.csproj
dotnet run --project Tests/MazeOfEgos.CompetitionChecks.csproj
dotnet run --project Tests/MazeOfEgos.ArenaChecks.csproj -- Logs/arena-matches.json
python3 Tools/verify_repository.py
```

Med den fastlåste editor installeret kan C#-integrationen og den faktiske
Newtonsoft-serialisering også kontrolleres uden at starte Unity:

```sh
python3 Tools/unity.py compile-check
python3 Tools/unity.py serialization-check
```

N2-kontrollen gennemfører fem kampe med scriptede inputs. Arena-kontrollen
har 19 checks, ti stressrunder og fem hele kampe med den færdige lokale AI,
angreb og genstarter. Den vælger hver figur som mennesket, hvis input leveres
af samme lokale AI under testen. Rapporten angiver simuleret tid; fysisk
Release-performance dokumenteres særskilt.

Unity-validering og builds kræver en aktiveret Unity-licens:

```sh
python3 Tools/unity.py validate
python3 Tools/unity.py build-mac
python3 Tools/unity.py smoke-mac
python3 Tools/unity.py build-ios
```

På Windows bruges normalt `python` i stedet for `python3` og `build-windows`
til en lokal prototype. iOS-eksport til Xcode foretages på en Mac med iOS Build
Support. Resultater ligger i `Builds/`; logfiler ligger i `Logs/`.

`smoke-mac` starter udviklingsbuilden og kontrollerer konkurrencens normale
skærme: billedmenu, figurvalg, alle fem modeller, projicerede retninger og
samtidigt joystick/genstandsinput i begge layouts, alle fem genstande,
pause/indstillinger, save/resume midt i kast, KO/genstart og delte runde-/kampsejre.
Testdata bruger særskilt cache og rører ikke normale gemte kampe. Billeder ligger
i `.artifacts/smoke-mac/`. Se [testrapporten](docs/TEST_REPORT.md) og
[overdragelsen](docs/COMPETITION_HANDOVER.md). Automatisk touch erstatter ikke
en håndført kamp på iPhone.

Til måling af billedtider, hukommelse og længere stabilitet findes en særskilt
Release-testbuild. Se [performance og gentagelse](docs/PERFORMANCE.md).
