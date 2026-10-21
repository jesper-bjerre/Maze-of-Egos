# Valg af touchstyring — 21. oktober 2026

Brugeren har overladt valget til implementeringen efter at have gjort opmærksom
på Apples virtuelle controller. Vi bruger et spiltilpasset Unity-overlay med
Input System til joystick, kameratræk og genstandsknap.

## Begrundelse

Spillet kombinerer analog bevægelse med frit kameratræk, en genstandsafhængig
knap og et spejlet venstrehåndslayout. Disse handlinger skal kunne bruges
samtidigt og være ens på iOS 17+, iPad og desktop under udvikling. Den eksisterende
inputvej er allerede automatisk afprøvet på fysisk iPhone 13, og brugeren har
bekræftet joystick og kamera med to fingre.

Apples [GCVirtualController](https://developer.apple.com/documentation/gamecontroller/gcvirtualcontroller)
er en brugbar iOS-mulighed. Den stiller systemkontroller til rådighed og kan
også kobles til appens egne kontroller. Apples
[vejledning](https://developer.apple.com/documentation/gamecontroller/adding-virtual-controls-to-games-that-support-game-controllers-in-ios)
beskriver også egne berøringsområder og egen tegning.
Det nyere [Touch Controller](https://developer.apple.com/documentation/touchcontroller)
kan tilpasses mere og integreres i Metal, men kræver iOS 26. Den installerede
Xcode 27.1-SDK angiver dette i `TCTouchController.h`.

For dette spil vælger vi én samlet inputvej i Unity. Den giver direkte kontrol
over berøringsområder, kameratræk, symbolerne for banan/olie/boost og indstillingerne
uden forskellige kontrolsystemer afhængigt af iOS-version. Dette er en
projektvurdering; Apples native controllere er fortsat relevante i andre layouts.

## Konkret udformning

- Fast, rund joystickbase med analogt udslag og samme startplacering som den
  manuelt bekræftede prototype. Berøringsområdet er 300 × 300 UI-enheder; den
  synlige base er 220 × 220 og tommelfingerens vandring er 76 enheder.
- Joysticket er diskret i hvile og tydeligere, mens det bruges. Slip stopper straks.
- Kameratræk har sin egen pointer. Genstandsknappen har rund hit-test og viser
  banan-, olie- eller boosts-symbol efter inventaret.
- Venstrehåndslayout spejler bevægelse og genstand, mens kameraområdet flyttes.
- Pause, fokusændringer og ændret safe area rydder aktive bevægelsesdata.
- Ingen netværkstjeneste eller ekstra abonnement er involveret.

## Verifikation

Build 2 er automatisk kontrolleret med tre samtidige syntetiske touchpunkter
på Mac og fysisk iPhone 13, inklusive spejlet layout. Alle kontroller består.
Den tidligere manuelle bekræftelse vedrører build 1; ny manuel accept registreres særskilt.
Se [testrapporten](TEST_REPORT.md) og [implementeringsplanen](IMPLEMENTATION_PLAN.md).
