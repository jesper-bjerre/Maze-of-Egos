# Maze-of-Egos

Et 3D-arkadespil til iPhone og iPad. Det aftalte nye design er alle mod alle:
én menneskestyret og fire computerstyrede figurer konkurrerer om hjerter i
2-minutters runder med et lokalt, skråt kamera. Først til tre pokaler vinder.
Designquizzen og implementeringsplanen er færdige; den nye model er endnu ikke
implementeret. Seneste dokumenterede build 12 er stadig førstepersonsversionen.

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

I build 12 går figuren automatisk midt i gangen. Venstre/højre på joysticket
bestiller et relativt sving, op fortsætter ligeud, og ned vender om. Slip
fortsætter gangen; der er intet frit kameratræk. Knappen til højre bruger
genstanden. WASD/piletaster eller controllerens venstre stick giver samme
relative kommandoer; mellemrum/nederste controllerknap bruger genstanden,
og Escape/Start åbner pause. Den nye models skærmorienterede styring er planlagt.

Touch-overlayet har et rundt joystick, større berøringsområde og symboler for
banan, olie og boost. Venstrehåndslayout kan vælges i Indstillinger.
Se [valget af touchstyring](docs/INPUT_DECISION.md) for vurderingen af Apples
native controllere og vores samlede Unity-inputvej.

Bananen har en kort kastbue og håndbevægelse. Den aktiveres ved landing og
lever derefter i 20 sekunder. Pause og gemt spil bevarer et igangværende kast;
reduceret bevægelse dæmper animationen.

Alle fem figurer og førstepersonshånden har importerede, riggede FBX-modeller
fra lokale Blender-kilder med referencebaserede ansigter og animationer.
Endelig karakterkvalitet udestår. Redigerbare Blender-kilder, FBX og
[produktionsvejledning](docs/CHARACTER_PRODUCTION.md) følger repoet; Blender
kræves kun for at ændre modellerne. Den nye model skal vise den valgte figur
i fuld størrelse i verden.

## Kontroller

```sh
dotnet run --project Tests/MazeOfEgos.Checks.csproj
python3 Tools/verify_repository.py
```

Med den fastlåste editor installeret kan C#-integrationen og den faktiske
Newtonsoft-serialisering også kontrolleres uden at starte Unity:

```sh
python3 Tools/unity.py compile-check
python3 Tools/unity.py serialization-check
```

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

`smoke-mac` starter den grafiske udviklingsbuild og kontrollerer den nuværende
spilmodel: startmenu, syntetisk touch i begge layouts, automatisk gang/sving,
genstande, pause, indstillinger, save/resume midt i kast, reduceret bevægelse,
vægskift, importerede figurer/animationer, levelskifte, Game Over og genstart.
Disse checks skal udvides til den nye konkurrence; de er ikke evidens for den
planlagte model. Testdata er isoleret fra spillerens gemte spil. Billeder gemmes i
`.artifacts/smoke-mac/`. Se [testresultater og aktuelle begrænsninger](docs/TEST_REPORT.md).
En automatisk touchtest erstatter ikke manuel afprøvning på en fysisk iPhone.

Til måling af billedtider, hukommelse og længere stabilitet findes en særskilt
Release-testbuild. Se [performance og gentagelse](docs/PERFORMANCE.md).
