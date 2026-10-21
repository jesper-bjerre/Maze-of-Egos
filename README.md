# Maze-of-Egos

Et førstepersonsarkadespil til iPhone og iPad.

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

På touch: venstre joystick styrer bevægelsen, træk på højre side styrer kameraet,
og knappen til højre bruger genstanden. På computer: WASD til bevægelse,
træk med musen på højre skærmhalvdel eller brug piletasterne til kamera,
mellemrum til genstand og Escape til pause. Controllerens to sticks, nederste
knap og Start bruger samme handlinger.

Touch-overlayet har et rundt joystick, større berøringsområde og symboler for
banan, olie og boost. Venstrehåndslayout kan vælges i Indstillinger.
Se [valget af touchstyring](docs/INPUT_DECISION.md) for vurderingen af Apples
native controllere og vores samlede Unity-inputvej.

Figurerne i 3D er **midlertidige fuldkropsmodeller**. De originale karakterark
bruges til menuportrætter; endelige modeller, rigging og animationer er udestående.

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

`smoke-mac` starter den grafiske udviklingsbuild, afprøver menu, tre samtidige
syntetiske touchpunkter i begge layouts, genstande, pause, indstillinger,
save/resume, vægskift, baseporte, levelskifte, Game Over og genstart og lukker
appen igen. Testdata er isoleret fra spillerens gemte spil. Skærmbilleder gemmes i
`.artifacts/smoke-mac/`. Se [testresultater og aktuelle begrænsninger](docs/TEST_REPORT.md).
En automatisk touchtest erstatter ikke manuel afprøvning på en fysisk iPhone.
