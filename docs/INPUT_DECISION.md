# Valg af touchstyring — opdateret 24. oktober 2026, build 14

## Aktuel styring — N1 i build 14

N1 erstatter førsteperson med et lokalt, højt ortografisk kamera med fast
retning. Hele spilleren vises; lave vægge afslører nabogange. Kameraet følger
positionen, men roterer ikke med figuren. Aktuelle værdier: 55° hældning, 25° yaw,
ortografisk halv højde 4,5 meter, væghøjde 1,15 meter. Udsnittet følger
skærmformatet og viser et område omkring spilleren, ikke hele banen.

Brugeren bad efter build 13 om kameraet **markant tættere på karakteren**.
Build 14 reducerer derfor billedhøjden fra 17 til 9 meter. Det giver cirka
1,89 gange større figurer på skærmen. Kameraet er ortografisk, så denne
ændring laves med udsnittets størrelse frem for blot at flytte kameraet.

- Peg mod den ønskede gang **på skærmen**. Input sammenlignes med kameraets
  faktiske projektion af banens fire retninger og gemmes som en verdensretning.
- Automatisk gang på midterlinjen bevares. Retningen huskes indtil næste
  lovlige sving, også efter slip eller vægstop. En modsat retning vender straks.
- Ét udslag giver én kommando; et holdt udslag gentager ikke sving. Slip
  nulstiller inputlåsen, men annullerer ikke gang eller et ventende sving.
  En anden retning erstatter det ventende ønske. Peg langs den nuværende gang
  for at erstatte et ventende sidesving med fortsættelse ligeud.
- Kameraet bliver fast ved alle fire retninger og tunnelpassage. Intet frit
  kameratræk, lodret kig eller sidelæns gang. Gemt verdensretning ændres ikke
  ved et andet skærmformat.
- Joystick og genstandsknap virker samtidigt i begge håndlayouts. Tastatur
  og controller bruger samme skærmorienterede valg. Genstande følger figurens
  gangretning; lydlytteren følger spilleren med kameraets vandrette retning.
- Pause/baggrund/genoptagelsesnedtælling fryser simulation og figurens animation.
  Gemte spil beholder navigationen i det eksisterende snapshotformat 3.

N1 beholder de gamle regler med liv, kraft og fire forfølgere. Ens regler
for fem deltagere, nye angrebsmarkeringer og konkurrencesnapshot kommer i
N2–N5; se [regler og implementeringstrin](IMPLEMENTATION_PLAN.md).

## Historisk rettelse — implementeret i build 12

Efter fysisk afprøvning var fri analog bevægelse og frit kamera for svært at
styre. Brugeren valgte eksplicit **relativt joystick med automatisk gang** blandt
alternativerne relativt joystick, absolut firevejsstyring og swipe.
Denne beslutning erstatter build 1–9's frie bevægelse og kameratræk.

- Figuren går automatisk efter startnedtællingen og bliver på gangens midterlinje.
  Ingen sidelæns bevægelse eller hastighedsstyring med joystickets udslag.
- **Venstre/højre:** bestil ét sving ved næste lovlige mulighed i den valgte side.
  Svinget huskes, indtil det kan udføres, også hvis en væg standser figuren først.
- **Op:** fortsæt ligeud og erstat et eventuelt gemt sving.
- **Ned:** vend 180 grader, også mellem to feltcentre.
- Ét udslag giver én kommando. At holde venstre giver ikke flere venstresving.
  Slip og vælg igen, eller skift direkte til en anden retning, for en ny kommando.
- Slip fortsætter automatisk gang. En væg stopper ved sidste gangfelts centrum.
  En ny lovlig retning eller en åbnet genvej kan sætte figuren i gang igen.
- Kameraet følger gangretningen vandret. Ingen lodret kamerastyring eller
  frit kameratræk. Normalt tager et 90-graders kameraskift ca. 0,125 sekund;
  reduceret bevægelse bruger øjeblikkeligt skift. Pause fryser kamera og simulation.
- Pause, baggrund og genoptagelsesnedtælling fryser bevægelse og effekter.
  Gemte spil bevarer retningen og det bestilte sving; pointerdata ryddes.
- Genstandsknappen er separat og kan bruges, mens joysticket holdes.
  Venstrehåndslayout spejler joystick og genstand. Desktop: WASD/piletaster med
  samme relative betydning; gamepad bruger venstre pind.

## Teknik og videreførelse

Det spiltilpassede Unity-overlay og Input System bevares. Det giver samme
kommandoer, touchmål, genstandssymboler og spejlede layout på iOS 17+, iPad og
desktop. Bevægelsen følger den samme ganggraf som banens regler, men med
kontinuerlig position og hastighed langs gangen. Sving sker ved feltcentre.

Snapshotformat 3 gemmer automatisk bevægelse og det ventende sving.
Version 1 og 2 kan fortsat læses: positionen justeres til en lovlig midterlinje,
retningen afrundes til nærmeste af de fire gangretninger, og lodret kig nulstilles.
Score, hjerter, effekttider, genstande og tilfældighed bevares. Gemte spil åbner i
pause. Filnavnet er uændret, så den eksisterende lokale save kan findes.

Apples [GCVirtualController](https://developer.apple.com/documentation/gamecontroller/gcvirtualcontroller)
blev vurderet ved det tidligere valg. Et native overlay ændrer ikke i sig selv
bevægelsesmodellen. Unity-overlayet bruges fra build 13 til skærmorienterede retningsvalg.
Ingen netværkstjeneste eller ekstra abonnement er involveret.

## Lyd og liv

- Fra build 11 er alle spillertekster på engelsk. HUD viser **Lives:** og op til fire tegnede hjertesymboler. Resterende samlehjerter
  vises separat. Et mistet liv fjerner et symbol; ekstra liv tilføjer et.
- iOS bruger AVAudioSession Playback med MixWithOthers, så Ring/Silent-knappen
  ikke slukker spillets musik og effekter. Spillets egne lydindstillinger og
  enhedens medielydstyrke gælder fortsat. Ingen mikrofon eller baggrundsafspilning.
  Politikken genetableres ved forgrund og afsluttet lydafbrydelse.
  Se [Apples Playback-dokumentation](https://developer.apple.com/documentation/avfaudio/avaudiosession/category-swift.struct/playback).

## Verifikation

Build 10 testes særskilt i [testrapporten](TEST_REPORT.md). Build 9's tidligere
skærmbilleder og 15-minutters måling er historisk evidens for det build.
Brugeren har 21. oktober bekræftet både musik og effekter med fysisk
Ring/Silent-knap på lydløs som svar på build 10-spørgsmålet. Den præcise build
under lytningen er ikke særskilt verificeret. Se [manuelt lydbevis](verification/2026-10-21-manual-silent-switch-audio.json).
Brugeren har 24. oktober svaret **“Styringen fungerer godt”** på spørgsmålet om
automatisk gang og relative sving i build 12 på iPhone. Styringsoplevelsen er
dermed manuelt accepteret for build 12; [registreret tilbagemelding](verification/2026-10-24-build12-manual-controls.json).
Det er ikke samtidig accept af en hel runde, startmenuens design eller den
endelige visuelle og lydmæssige kvalitet.
Build 14's tættere kamera og skærmstyring kræver en ny manuel vurdering;
tidligere accept af relative sving overføres ikke. Se den aktuelle automatiske
verifikation i [testrapporten](TEST_REPORT.md).
