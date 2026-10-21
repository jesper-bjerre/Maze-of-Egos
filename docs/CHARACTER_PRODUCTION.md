# Karakterproduktion — udestående M5-arbejde

De fem referenceark i [images](images/) bestemmer udseendet. Originalerne er
bevaret; [CHARACTER_ASSETS.json](CHARACTER_ASSETS.json) registrerer kilder,
runtimekopier og SHA-256. Menuens portrætter bruger et UV-udsnit af de uændrede
ark. Putin bruger nu en **lokal rigget reviewpilot**. Xi, Kim, Trump og
førstepersonshånden er fortsat programmerede pladsholdere. Ingen figur har
endelig M5-godkendelse.

## Aftalt proces og budget

Der er 0 kr. ekstra budget. Ingen eksterne konti, betalte abonnementer eller
AI-credits er bestilt. Der er ikke gennemført AI-generering eller upload.

### Build 6: forbedret Putin-pilot

Anden revision forbedrer det eksisterende lokale asset:

- Hovedet har tættere, sammenhængende geometri til næseryg/-tip, bryn,
  øjenhuler, kinder, mund og hage. UV'er sampler stadig det originale ark.
- Vertexvægte blander ansigtsteksturen over i hudfarven ved tindinger og hals.
  Blender-materialet og den nye mobile URP-shader bruger samme vægte; ingen
  ny eller redigeret PNG er nødvendig. Shaderen følger scenens sol, ambientlys,
  skygger og tåge.
- Håret følger kraniet med vigende hårgrænse og seksten formede, fejede striber.
  Ærmerne har sammenhængende geometri og blød vægtning ved albuerne. Knæene
  har flere løkker og bredere vægtning. Skjorte/revers/slips følger jakkens
  krumning med afstand mellem stoflagene.
- Jagt/retur har nye bagte benbevægelser, løft af svingfoden og plantet
  støttefod. En analytisk to-knogle-løsning køres ved assetgenerering;
  Unity behøver ingen IK-pakke. 40 støtteprøver af de faktiske deformerede
  skosåler kontrolleres mod gulvet under genereringen.
- Normal jagt bruger en cyklus pr. 0,88 verdensmeter. Tempoet følger farten,
  også ved krafttilstand og tunnel. Pause/nedtælling ændrer ingen fase.
  Miniature-retur har et særskilt læsbart tempo på 2,6 cyklusser/sekund.
  En gammel overraskelsesreaktion fortsætter ikke ind i base/hvile.

Modellen har fortsat 17 knogler og fem klip. Der er nu **17.404 importerede
trekanter** (18.224 i Blender), ti materialer og et budget på 20.000 trekanter.
Importer-valideringen kontrollerer også vertexvægtene til teksturovergangen og
referencematerialets shader/tekstur. Kilder, runtimehash, stride og den bagte
geometrikontrol er gemt i [manifestet](../art/characters/putin/manifest.json).

Generatoren skriver sammenlignelige front-, profil-, bagfra-, trekvart- og
jagt-renders samt `geometry-check.json` i `.artifacts/character-pilot/`.
Den native kontrol er udvidet med fodhøjder, løftet svingfod, hvilepose efter
reaktion og profil-/bagfra-billeder. Afprøvningsresultater fremgår af
[testrapporten](TEST_REPORT.md).

Build 6 består nu samme native forløb på fysisk iPhone 13 med 19 billeder.
Fodkontrollen har højst 0,54 mm afvigelse i støttefodens ankelhøjde over
23 prøver. Den separate korte Release-måling gennemførte 60,01 sekunder ved
59,97 fps, p95 18,00 ms og normal temperaturtilstand. Debug-build 6 er
efterfølgende startet normalt igen. Resultater, billeder og målegrænser følger
repoet i testrapporten.

Det er fortsat en reviewpilot. Profilen mangler selvstændige teksturdetaljer,
ansigtet har fast udtryk uden blendshapes, og håret er stiliseret. De nye
kontroller beviser teknisk funktion; endelig genkendelighed og animationsstil
skal fortsat vurderes under faktisk spil på enhed. Næste asset er Xi med egen
silhuet og bevægelse; M5 forbliver åben.

### Build 5: første lokale Putin-pilot

Meshy-forsøget 21. oktober blev afvist af Chrome-værktøjets sikkerhedskontrol:
`The admin-enforced policy could not be verified`. Konto, ledige credits og
eksport kunne derfor ikke kontrolleres. Ingen alternativ adgang blev brugt.
Meshys officielle [gratisplan](https://help.meshy.ai/en/articles/15696428-what-is-included-on-the-free-plan)
beskriver 100 månedlige credits, 10 Meshy 6 Lite-downloads og CC BY 4.0 med
kreditering; det er ikke bevis for adgang på brugerens konto.

Arbejdet fortsætter med lokalt modelleret geometri, skelet og animation i
**Blender 4.5.14 LTS** (gratis, officiel ARM64-download kontrolleret mod SHA-256).
Den første revision havde skræddersyet jakke, revers, skjorte, slips, hænder, adskilte ben,
sko, volumetrisk hoved, 17 knogler og fem in-place-klip: `Idle`, `Chase`,
`Surprise`, `Slip` og `Return`. Build 5 havde 11.636 Blender-trekanter og
10.796 efter Unity-import. De aktuelle filer indeholder revisionen fra build 6.

- Redigerbar kilde: [putin-pilot.blend](../art/characters/putin/putin-pilot.blend).
- Runtime: [putin.fbx](../Assets/Resources/CharacterModels/putin.fbx) med ni URP-materialer.
- Kilder, SHA-256, geometri og begrænsninger: [manifest](../art/characters/putin/manifest.json).
- Genskabelse: [build_putin_pilot.py](../Tools/build_putin_pilot.py).

Ansigtets UV'er sampler det uændrede referenceark. Der er ingen redigeret PNG
eller ansigts-blendshapes. Fronten har fast vredt udtryk. Build 6 forbedrer
profil, hår, teksturens overgange, vægtning og fodkontakt. Det er fortsat en
reviewpilot, ikke en færdig skulptureret karakter.

Unity importerer klippene som Legacy-animation og sampler dem udtrykkeligt med
spillets simulationstid. Position og retning kommer fortsat fra spillereglerne.
Pause, nedtælling og gemt genoptagelse bruger derfor ingen animationstid.
Power farver jakkens/buksernes materialer blåt; ansigtet beholder sin tekstur.
Reduceret bevægelse giver en fast glidepose uden wobble. Retur bruger miniature
og den eksisterende harmløse boble. Tunnelkopien bruger samme model og ur.

Build 5 består native Mac- og fysisk iPhone 13-kontrol med 17 skærmbilleder,
inklusive faktisk skeletbevægelse og frosset pause. Den korte fysiske
Release-måling viser 59,97 fps over 60,02 sekunder. Det er teknisk verifikation,
ikke endelig grafisk accept. Se [evidens og begrænsninger](TEST_REPORT.md).

Blender kræves kun ved redigering/genskabelse, ikke for at åbne eller bygge
Unity-projektet på en anden pc/Mac. Med Blender 4.5 LTS installeret:

```sh
blender --background --python Tools/build_putin_pilot.py
python3 Tools/unity.py validate
python3 Tools/unity.py build-mac
python3 Tools/unity.py smoke-mac
```

Generatoren erstatter pilotens `.blend`, FBX og manifest og skriver et faktisk
3D-preview i `.artifacts/character-pilot/`. Gem bevidste manuelle Blender-ændringer
før generatoren køres igen. Lokale Blender-installationer ligger i `.tools/` og
følger ikke med Git. Originale reference-PNG'er forbliver byte-for-byte identiske.

### Resterende produktion

1. Afprøv **Putin** først, da han er den første aktive forfølger.
2. Forbered separate, konsistente front-, profil- og bagreferencer fra arket;
   bevar originalen. Modellen skal være en hel karakter med tydeligt adskilte
   arme/ben og en egnet neutral riggingpose.
3. Brug gratis billed-til-3D/eksport, hvis det er tilgængeligt i Meshy. Stop før
   enhver betaling. Hvis gratis eksport eller kvalitet er utilstrækkelig,
   dokumentér det som en udestående afhængighed.
4. Efterbearbejd geometri, ansigt, materialer, proportioner og bevægelighed i
   Blender. Billedernes kvalitet er ikke bevis for den resulterende models kvalitet.
5. Rig og afprøv gang/jagt, overraskelse, banan-/olieglid og den harmløse pause-
   og returreaktion. Brug generiske komiske lyde, ingen politikeroptagelser.
6. Eksportér model/rig/animation som FBX til Unity. Kontrollér skala, retning,
   fodkontakt og bevægelse i den faktiske scene på iPhone før resten af figurerne.
7. Gentag for Xi, Kim og Trump. Zelenskyj skal have menuavatar og relevante
   førstepersonshænder; selve spillet forbliver i hans øjenhøjde.

Meshy dokumenterer [flere referencevinkler](https://docs.meshy.ai/en/api/multi-image-to-3d)
og [rigging/animation/FBX-eksport](https://help.meshy.ai/en/articles/16231707-how-to-create-3d-animation-with-auto-rigging).
Gratis muligheder og begrænsninger skal verificeres på produktionstidspunktet.

## Accept

Figurerne er genkendelige fra flere vinkler, har hele kroppe og fungerer i den
varme animationsstil under faktisk jagt. De skal kunne skelnes på silhuet, hår,
tøj og bevægelse. Assetkilder, licens og eksportindstillinger registreres sammen
med modellerne. Automatisk generering eller primitive pladsholdere opfylder ikke
den endelige grafiske accept alene. Performance skal måles på fysisk enhed.

## Grafiske projektindstillinger

De serialiserede URP-renderer-, pipeline- og globalindstillinger er baseret på
Unity 6000.3.26f1-editorens indbyggede `com.unity.template.3d-cross-platform-17.0.14`
skabelon, tilpasset med mobilbudget, eget materialevalg og en tom volumenprofil.
Ingen skabelonscene eller sample/testkomponenter er importeret. Ubrugte, uløste
APV-bageressourcer fra skabelonen er nulstillet; adaptiv probe-bagning er ikke
konfigureret eller verificeret i prototypen.
