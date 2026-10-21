# Maze of Egos — implementeringsplan og overdragelse

Senest opdateret: 21. oktober 2026. Arbejdssprog og spillets brugerflade: dansk.

## Læs først

Denne fil er den vedvarende plan og status for arbejdet. Læs derefter
[spildesignspecifikationen](Labyrint_3D_Spildesign_og_Codex_specifikation.md).
Beslutningerne nedenfor erstatter specifikationens modstridende forslag; øvrige
regler og acceptkriterier i specifikationen gælder fortsat.

Referencer til filer er relative, så planen kan bruges på både Windows og macOS.
Referencearkene i [images](images/) bevares som originale kilder.

## Aftalte produktbeslutninger

- Førstepersonsarkadespil til iPhone/iPad, begge landscape-retninger, offline.
- Teenagere og voksne er den primære målgruppe. Tonen er varm politisk satire
  med harmløs tegnefilmskomik.
- Zelenskyj er helten. Putin, Xi, Kim og Trump er forfølgerne med henholdsvis
  Chaser-, Ambusher-, Flanker- og Wanderer-adfærd.
- Alle fem er hele karakterer med ben, baseret på de fem PNG-referenceark.
  Spøgelsesunderkroppe erstattes af løbende karakterer.
- Verden er en mekanisk eventyrhave med afrundede materialer, grønt og messing.
- Venstre virtuelle joystick giver fri analog bevægelse frem/bak og sidelæns
  i kameraets vandrette koordinater. Slip for at stoppe. Diagonal input
  normaliseres; joystickets udslag bestemmer farten.
- Træk på højre skærmside styrer kameraet. Lodret kig begrænses til ±20 grader
  som startværdi. Genstandsknappen er en særskilt touchhandling, der ikke drejer
  kameraet. Flere fingre skal kunne bevæge, kigge og bruge en genstand samtidig.
- Der er ingen hop, automatisk fremdrift eller swipebaseret svingkø.
- En farlig forfølger spærrer en smal gang. Fri bevægelse må ikke gøre det muligt
  at smutte forbi ved væggen; synlige kropsmål og kollisionsmål skal stemme.
- Uventet synlig nærkontakt kan udløse et kort komisk chok med lyd og reaktion.
  Det låser ikke input. Der er mindst otte sekunder mellem chok. Effekten kan
  dæmpes via indstillingerne.
- Rumlig lyd og en diskret, grov retningsindikator giver hjælp ved fare. Ingen
  præcise fjendepositioner eller udsyn gennem vægge.
- Referencebanen er fortsat 28 × 31 med 240 små og fire store hjerter. Alle 244
  skal samles. Ingen udgang, leveltimer eller fast aftalt rundelængde.
- Krafthjerter giver altid en kraftperiode: `max(2 sekunder, tabelværdien)`.
  På sene levels bruges den sidste definerede sårbare hastighed fra tabellen.
- Banan kastes i kameraets vandrette retning. Kastet starter straks og flyver
  i 0,36 sekund; fælden aktiveres ved landing og lever derefter i 20 sekunder.
  Styring og brug af nye genstande låses ikke. Pause/baggrund/genoptagelsens
  nedtælling fryser kastet. Olie lægges på det nylige bevægelsesspor.
  Boost ændrer fart, ikke kamerahøjde eller FOV.
- En genvej holdes åben, mens et kast krydser den, eller en levende fælde
  ligger i væggens felt. Den kan lukke efter træf eller udløb.
- Første reviewmilepæl er en komplet grundrunde med alle fire AI-roller,
  hjerter, krafttilstand, score, chancer, levelskifte og Game Over.
- De tre genstande og bevægelige vægge tilføjes efter grundrunden. Den færdige
  hovedoplevelse har begge dele aktiveret.
- Arbejdsnavnet er fortsat Maze of Egos.

## Teknik og data

- Nyt Unity 6.3 LTS-projekt med URP, C# og Input System. Lås editorversionen i
  `ProjectSettings/ProjectVersion.txt` og pakkeversioner i `Packages/manifest.json`.
- Rene C#-regler adskilles fra Unity-præsentation, input og animation. Fast
  simulationstid, seedet tilfældighed og reproducerbare regeltests.
- Spilleren har kontinuerlig position, bevægelsesvektor og kameraretning.
  Fjender navigerer på banens ganggraf med aktørspecifik adgang til basen.
- Samme topologi bestemmer bevægelse, vægpassager og fjendernes ruter.
  Kontinuerlige kontakter sikrer, at boost ikke springer hjerter eller fangst over.
- Versioneret snapshot gemmer også fri position og kameraretning. Pause og
  baggrund forbruger ingen effekt- eller vægtid.
- Ingen backend, konto, annoncer, køb i appen eller netværksafhængighed i spillet.
- Touchstyringen bruger et spiltilpasset, rundt Unity-overlay med Input System.
  Valget, vurderingen af Apples alternativer og kontrolmål er gemt i
  [inputbeslutningen](INPUT_DECISION.md). Brugeren har overladt valget til implementeringen.

## Milepæle og accept

- [x] **M0 — projekt og værktøjer.** Reproducerbart Unity-projekt, fastlåste
  versioner, banedata, testkommandoer og tidlig fysisk iPhone-build. En konkret
  værktøjsblokering dokumenteres, hvis enhedsbuild ikke er mulig.
- [ ] **M1 — bane og fri styring.** Rigtig 3D-geometri, joystick, kamera,
  vægkontakt og sidetunnel. Alle hjertepositioner kan besøges uden vægpassage.
- [ ] **M2 — komplet grundrunde.** Alle fire AI-roller, 244 hjerter,
  krafttilstand, base/retur, score, chancer, HUD, pause, Game Over og levelskifte.
  Midlertidige 3D-modeller er tilladt på dette trin.
- [ ] **M3 — tre genstande.** Ét inventarfelt, fire spørgsmålstegn,
  banan/olie/boost og de aftalte interaktioner. Hjerteantallet påvirkes ikke.
- [ ] **M4 — bevægelige vægge.** Varsling, sikker reservation, åbning/lukning
  og opdateret AI-navigation. Ingen aktør klemmes eller strander.
- [ ] **M5 — visuel og lydmæssig kvalitet.** Genkendelige animerede
  karakterer, gennemarbejdet eventyrhave, komiske reaktioner, lyd, haptik,
  hjælp og indstillinger. Vurder faktisk gameplay på enhed.
- [ ] **M6 — udgivelseskandidat.** Save/resume, iPhone/iPad-test,
  performanceoptagelse, TestFlight og specifikationens udgivelsesleverancer.

Milepæle afkrydses kun med verificerbar evidens, ikke alene fordi kode findes.
Fysisk enhedstest og mobilperformance skal fremgå særskilt fra automatiske tests.

## Karakterproduktion og budget

Der er **0 kr. ekstra budget nu** ud over brugerens eksisterende Codex-abonnement.
Ingen betalte eksterne tjenester eller abonnementer bestilles.

Afprøv en gratis billed-til-3D-pilot med Putin først, hvor gratis generering og
eksport er tilgængelig. Meshy er pilotkandidat, Blender bruges til efterbearbejdning,
og model/rig/animation eksporteres til Unity. Bevar referencearkene og dokumentér
kilder, licens og eksportindstillinger. Test proportioner og bevægelse på én figur
før de øvrige fire.

En gratis pilot garanterer ikke fem færdige figurer i den aftalte kvalitet.
Manglende 3D-assets eller utilstrækkelig kvalitet registreres som udestående.
Grundspillet kan udvikles med tydeligt markerede pladsholdere; de opfylder ikke M5.
Spillet må ikke være afhængigt af AI-tjenesten under afvikling.

## Testplan

- Importér og kontrollér 28 × 31 felter, præcis 240 + 4 hjerter og adgang til alle.
- Test tunnelovergang, analog diagonal fart, vægge og passage tæt ved forfølgere.
- Test alle AI-roller, kraftudløb, returruter, pointkæde og én ekstra chance.
- Test fangst samtidig med kraft-/sidste hjerte samt Game Over og levelskifte.
- Test inventar, kast mod væg, olie, fælles glideimmunitet og boostkontakter.
- Test vægkombinationer og udsat lukning under en reserveret passage.
- Test pause/baggrund og versioneret lagring/genoptagelse.
- Test multi-touch, kamera og UI fysisk på iPhone og iPad i begge retninger.
- Mål mod 60 fps på iPhone 13 og mindst 15 minutters sammenhængende spil.

## Overdragelse mellem computere

1. Læs denne fil og README; fortsæt fra den aktuelle status nedenfor.
2. Brug den fastlåste Unity-version og de versionsstyrede pakker. Lokale caches,
   byggemapper, signeringsidentiteter og credentials skal ikke versionsstyres.
3. Unity/C#-arbejdet kan udføres på Windows eller macOS. Lokal Xcode-baseret
   iOS-build og signering udføres på en Mac. Brug relative projektstier.
4. Opdatér status med ændringer, præcise testkommandoer, resultater, blokeringer
   og næste skridt efter hver afsluttet arbejdsblok.
5. For overdragelse via Git skal kildekode, dokumenter og referencebilleder være
   committet og synkroniseret. En lokal fil er ikke i sig selv en ekstern backup.

## Aktuel status

Implementering påbegyndt 21. oktober 2026. Planen blev gemt før kodearbejdet.

| Område | Implementeret | Verificeret |
| --- | --- | --- |
| Projekt | Unity 6000.3.26f1, URP, scene, materialer, metadata, package lock og portable scripts | Native import/validering, Mac-build, iOS-eksport, signeret Xcode-build, installation og automatisk afprøvning på iPhone 13 lykkedes |
| Bane/styring | Dokumentets JSON, fri kollisionsbevægelse, tunnel, touchjoystick, kameratræk, tastatur/controller | Regeltests samt faktisk Mac/iPhone-player med tre syntetiske touchpunkter består; brugeren har manuelt bekræftet samtidig joystickbevægelse og kameratræk på iPhone 13 |
| Grundrunde | Fire AI-roller, base/retur, hjerter, kraft, score, chancer, fangst, levels og Game Over | Regeltests og traversal af 244 hjerter består; native sidste-hjerte, levelskifte, Game Over og genstart består på Mac/iPhone med kontrollerede snapshots |
| Genstande/vægge | Ét inventarfelt, banankast med bue/håndbevægelse, olie, boost, tydelige fælder, varsling, vægcyklus og sikre passager | Regeltests samt brug/visning af alle genstande og varsling/åbning/reservation/lukning består i Mac/iPhone-app |
| Lagring | Atomisk snapshot/settings, backup, validering, profil/RNG/trail, kasttilstand og pauset genoptagelse; snapshot v1 migreres til v2 | .NET/Newtonsoft-tests og faktisk Mac/iPhone save/resume består |
| Præsentation | Prototypemodeller, volumetriske hjerter, mekanisk have, HUD/menu, egne synteselyde og indstillinger | Mac/iPhone-menu, gameplay og indstillinger er renderet og skærmbilleder visuelt kontrolleret; 15 minutters prototypeperformance er målt; slutkvalitet og måling med endelige modeller udestår |
| Performance | Opt-in Release-måler, billedintervaller, samplet hukommelse, iOS-temperatur/batteri og automatisk spiller med normale regler | 900,04 sekunder på fysisk iPhone 13: 59,98 fps, p95 17,25 ms, ingen runtime-fejl; alle temperaturprøver nominal |
| Karakterreferencer | Fem uændrede runtimekopier, portrætter og SHA-256-manifest | Kopierne er kontrolleret byte-for-byte; endelige 3D-assets mangler |

**Kontroller:** 53 C#-tests (50 spilregeltests og tre måletests) bestået, separat Unity C#-kompilering med 0 fejl/
advarsler, Newtonsoft save/resume-test, repositoryvalidering og native Mac-
runtimekontrol bestået. iOS-eksport samt signeret Debug-build/installation på
iPhone 13 med iOS 26.6.2 er gennemført. Den samme automatiske runtimekontrol
består på den fysiske iPhone, og faktiske skærmbilleder er kontrolleret.
Build 2 er kontrolleret med det udvidede scene-/inputforløb, inklusive begge
layouts, genstande, vægge, porte og centrale spilfaser. Build 3 består også på
Mac og fysisk iPhone, inklusive banankast, hånd, pause/gem/genoptagelse midt i
luften og reduceret bevægelse. Der gemmes nu 13 faktiske skærmbilleder pr. kontrol.
Se [testresultater](TEST_REPORT.md) for kommandoer og grænserne for denne evidens.

**Værktøjer:** Den tidligere Unity-licensblokering er løst via Unity Hub.
Editor og iOS Build Support findes under `.tools/`; Xcode 27.1 og .NET 10.0.401
er tilgængelige. Blender er ikke installeret. Computer-use er blokeret af en
ChatGPT-klient/server-versionsforskel; den grafiske Mac-kontrol bruger derfor
en automatisk udviklingsbuild og faktiske skærmbilleder.

**iPhone-status:** Version 0.1.0 (4) er installeret og startet normalt efter
bestået udvidet automatisk kontrol. Native IL2CPP-stripping er rettet via `Assets/link.xml`.
Aktuelle skærmbilleder og funktionsresultat ligger lokalt i `.artifacts/smoke-iphone-build4/`.
Den gennemførte Release-performancekørsel ligger i `.artifacts/performance-iphone-build4/`;
uændret JSON/CSV følger repoet under `docs/verification/`. Tidligere billeder er bevaret.
Brugeren har 21. oktober 2026 bekræftet, at joystickbevægelse og kameratræk
fungerer samtidigt med rigtige fingre på iPhone 13. Manuel brug af genstand
som tredje touch samt de øvrige spil- og enhedskontroller er fortsat udestående.

**Næste skridt:** Afprøv manuelt genstand samtidig med bevægelse/kamera, begge landscape-retninger og faktisk spil
mod AI. Kør `python3 Tools/unity.py smoke-mac` for reproducerbar Mac-kontrol.
Der må ikke erklæres fuld M1–M4-accept, før de relevante runtimekontroller er
gennemført. Performanceinstrumentering og den 15 minutter lange prototypekontrol
er nu udført. Næste implementeringsområde er karakterproduktion og visuel kvalitet;
iPad og manuel afprøvning følger fortsat. Den gratis karakterpilot kræver
adgang til en tjeneste med gratis generering/eksport. Mål igen efter endelige assets.

**Aktuel arbejdsblok — build 4:** Performanceinstrumentering, automatisk
stabilitetstest med aktive AI-roller og cache af HUD-tekst. 53 C#-tests,
Newtonsoft, repositoryvalidering, kompilering og udvidet Mac-kontrol består.
Den fysiske iPhone gennemførte en Unity/Xcode Release-kørsel med 15 sekunders
opvarmning og 900,04 sekunders måling: 59,98 fps, p95 17,25 ms, p99 17,75 ms,
ét interval over 50 ms, ingen runtime-fejl og alle temperaturprøver nominal.
Spillet var aktivt i Playing i 830,90 sekunder; resten er normale fangster,
nedtællinger og genstarter. Alle fire AI-roller er set, og der blev opsamlet
1.140 hjerter over syv startede runder. De endelige karaktermodeller indgår ikke.

Rapport og CSV er bevaret uændret i `docs/verification/`. GPU-tider og GC-rate
var ikke tilgængelige; USB-testen dokumenterer ikke uopladet batteriforbrug.
Se [målemetode og gentagelse](PERFORMANCE.md) og [testresultater](TEST_REPORT.md).
Den normale Debug-build 4 består også den udvidede kontrol på fysisk iPhone,
inklusive korrekt synlig HUD-tekst efter opsamling, Game Over og genstart.
Appen er startet normalt igen uden testflag; normale gemte spil er bevaret.

**Tidligere arbejdsblok — build 3:** Bananen flyver i en synlig bue og lander som
fælde. En midlertidig Zelenskyj-hånd med olivengrønt ærme vises under kastet og
spejles med håndlayoutet. Reduceret bevægelse slår snurret og håndsvinget fra og
giver en lavere bue. Kameraets højde og FOV ændres ikke.

Snapshotformatet er nu v2. Build 1/2's v1-spil kan fortsættes; deres bananer
bevares som allerede landede fælder med oprindelig udløbstid. Filnavnet
`session-v1.json` bevares for at finde eksisterende spil; indholdets `version`
bestemmer formatet. V2-spil kræver build 3 eller nyere. Nye feltdata valideres
før gendannelse og kan ikke ændre en aktiv session ved afvisning.

50 regeltests, faktisk Newtonsoft-serialisering, repositoryvalidering og C#-
kompilering med 0 fejl/advarsler består. Native Mac-build/runtime, iOS-eksport,
signeret Xcode Debug-build og samme runtimekontrol på fysisk iPhone 13 består.
Appen er efterfølgende startet normalt uden testflag. Se testresultaterne.

**Udestående grafik:** Gratis AI-karakterpilot, endelige modeller, rigs og
animationer er ikke udført. Følg [karakterproduktionen](CHARACTER_PRODUCTION.md).
Primitive fuldkropsmodeller er prototyper og opfylder ikke M5.

**Bevidste prototypeforenklinger:** Hånden og forfølgernes reaktioner bruger
fortsat enkel prototypegeometri/-animation. De udgør ikke de endelige riggede
karakterer. Banan-/oliefælder har særskilt geometri og bananen en faktisk kastbue. Belysningen bruger ambientlys
og én dynamisk sol. Der er ingen lysbagning; performance med endelige modeller
skal måles igen. Haptik er
den simple platformsvibration, ikke et færdigafstemt haptikdesign.

**Versionsstyring:** Build 3 findes i repoets commit `9e30e93` ved starten af
denne arbejdsblok. Build 4-ændringerne gemmes på disken; der er ikke udført
commit eller push i denne arbejdsblok. For overdragelse
via kloning skal ændringerne også committes og synkroniseres. Lokale downloads,
licenser, caches og builds er bevidst ignoreret.

Den oprindelige banes rettighedsstatus er fortsat `unverified`, som angivet i
specifikationen. Dette er en udgivelsesafhængighed og blokerer ikke kodearbejdet.
