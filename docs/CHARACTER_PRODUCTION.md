# Karakterproduktion — udestående M5-arbejde

De fem referenceark i [images](images/) bestemmer udseendet. Originalerne er
bevaret; [CHARACTER_ASSETS.json](CHARACTER_ASSETS.json) registrerer kilder,
runtimekopier og SHA-256. Menuens portrætter bruger et UV-udsnit af de uændrede
ark. De nuværende fuldkropsmodeller er programmerede **pladsholdere**.

## Aftalt proces og budget

Der er 0 kr. ekstra budget. Ingen eksterne konti, betalte abonnementer eller
AI-credits er bestilt. En gratis pilot er endnu ikke udført; der er ingen
tilsluttet Meshy-konto i projektet.

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
