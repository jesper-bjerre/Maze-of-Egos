# Maze of Egos — implementeringsplan og overdragelse

Senest opdateret: 24. oktober 2026. Build 17 tilføjer klikbare karakterkort, større og tydeligt afgrænset vandballonplask, tidlig rundeslut ved sidste hjerte og portrætter i rundeafgørelsen. 122 spilchecks og native tests består; normal Release 0.2.0 (17) er installeret og startet på iPhone 13. Manuel gameplayaccept står åben. Arbejdssprog: dansk. Spillets brugerflade og udgivelsestekster: engelsk.

## Læs først

Denne fil er den vedvarende plan og status for arbejdet. Læs derefter
[spildesignspecifikationen](Labyrint_3D_Spildesign_og_Codex_specifikation.md).
Beslutningerne nedenfor erstatter specifikationens modstridende forslag; øvrige
regler og acceptkriterier i specifikationen gælder fortsat.
Det aftalte alle-mod-alle-design og den reviderede implementeringsplan nedenfor
har forrang. Afsnit markeret som build 12-grundlag eller historik beskriver den
tidligere spilmodel og er ikke krav om førsteperson, krafttilstand eller liv.

Referencer til filer er relative, så planen kan bruges på både Windows og macOS.
Referencearkene i [images](images/) bevares som originale kilder.

## Aktuel ændring — build 17

Brugerens seneste rettelser har forrang for build 16's faste ventetid ved tom bane:

- Valg af karakter: foto og navn ligger i samme synlige kort og samme knap. Hele kortet kan vælges. Native testen trykker separat på foto og navn for alle fem figurer.
- Vandballon: et cirkulært plask rammer alle aktive, ubeskyttede modstandere højst ét banefelt fra landingspunktet, inklusive kanten. Vægge stopper stadig skade. Aim-markering og varslet ring bruger samme radius som Core. Selve landingen giver et kort synligt plask i 0,3 s; det rammer én gang ved landing og er ikke en vedvarende fælde.
- Runden slutter ved **sidste indsamlede hjerte eller senest efter 120 sekunder**. Sidste hjerte tælles med før afgørelsen. Point/pokaler afgøres én gang. Næste runde har nye hjerter og tilfældige, adskilte starter som i build 16.
- Rundeafgørelse: portræt med faktisk navn og rundepoint direkte under hver figur ved guld/sølv/bronze. Delte placeringer viser alle de berørte portrætter. Ved tre eller flere delte guldvindere udvides rækken over hele bredden. Samlet stilling med guldpokaler står nedenunder.
- Snapshot V4 / profil competition-v4 gemmer rundens faktiske starttick og slutårsag. V1/V2/V3 migreres uden at ændre aktuelle positioner, hjerter eller point. Korte runders faktiske tid gemmes, så næste runde og AI-/angrebstimere fortsætter korrekt.

Native slutstatus dokumenteres i [testrapporten](TEST_REPORT.md). Ingen commit eller push.

## Historisk ændring — build 16

Brugerens seneste instruktion erstatter tidligere quizvalg om genopfyldning af hjerter:

- Ved hver rundestart fordeles alle fem tilfældigt på forskellige lovlige gangfelter, mindst seks felter fra hinanden og uden for midten. Alle får en farbar startretning. Kun knockout giver genstart fra midten.
- En runde har 240 almindelige hjerter. Indsamlede hjerter kommer ikke igen i samme runde. Et nyt fuldt sæt kommer ved næste rundestart. Den eksisterende timer på 120 sekunder gælder også ved en tømt bane.
- Et fast nordvendt minikort øverst til venstre viser de resterende hjerter og alle aktive spillere. Farver og numre matcher HUD'et; menneskets markør har hvid ramme. Spillere under KO vises igen, når de genstarter.
- Rundens guld, sølv og bronze bestemmes af rundepoint. Lige point deler placering, med spring over optagne pladser: 1, 1, 3. Kun guld giver en samlet pokal; øvrige medaljer er rundens placeringer.
- Under medaljerne vises OVERALL STANDINGS, sorteret efter pokaler og derefter samlede point. Tre synlige guldpokalpladser pr. deltager viser vejen til kampsejr.
- Alle fem får samme minikortinformation: resterende hjerter, aktive spillernes positioner og kortets genvejsstatus. AI bruger kortet til rutevalg; sigtning og reaktion på angreb bruger fortsat lokale observationer. Skjulte genstande, beskyttelse og angreb afsløres ikke af kortet.
- Snapshot V3 / profil competition-v3 gemmer en separat spawn-RNG. V1/V2-kampe migreres uden at flytte igangværende spillere, nulstille point eller genindsætte indsamlede hjerter. Den nye regel gælder fremover og ved næste rundestart.

Core-verifikation: 65 klassiske checks, 31 konkurrencechecks og 21 arenachecks består. Native tests består på Mac i tre formater og fysisk iPhone i begge landscapes. OS-livscyklus består; en frisk 300-sekunders Release-måling giver 60,00 fps og normal temperatur. Normal build 16 er installeret og startet. Ingen commit eller push.

## Historisk levering — build 15

N2–N4 er nu integreret i det normale spilflow. PLAY åbner separat figurvalg;
alle fem figurer kan være mennesket, og de øvrige fire er konkurrerende AI.
De fem genstande, varsling, ét-træfs KO, fem sekunders genstart, tre sekunders
beskyttelse, point/pokaler og delte resultater er implementeret. Det tættere
N1-kamera og skærmorienteret midterlinjestyring bevares.

65 klassiske checks, 24 N2-checks og 19 arena-checks består. Fem fulde kampe
med produktions-AI giver 26 runder/187.200 aktive ticks, 7.132 hjerter, 293
anvendte genstande og 48 KO. Ti yderligere seedede stressrunder giver 72.000
ticks og 120 checkpoints. Faktisk Unity JSON, V1-migrering og separate backups
består. Den fysiske iPhone består funktionstest og OS-livscyklus (9,52 sekunder
baggrund, identisk frosset kamp/AI-checkpoint og Playback-lydpolitik).

N5's friske Release-måling består: 900,003 sekunder, 59,964 fps, p95 18 ms,
værste frame 46,12 ms og normal temperatur i alle 181 samples. Fysisk native
kontrol, OS-livscyklus og normal installation/opstart består; gamle gemte filer
og indstillinger er byte-bevaret. Se [testrapporten](TEST_REPORT.md).
**Manuel gameplayaccept står åben**: kameravinkel/zoom, mulighed for at læse og
undvige angreb samt en håndført hel kamp. Fysisk iPad er fortsat fravalgt.

Den konkrete filopdeling, balance, lagring og fortsættelse på pc/Mac findes i
[overdragelsen for konkurrencen](COMPETITION_HANDOVER.md). Ældre statusafsnit
nedenfor er historik; de angiver ikke den aktuelle implementeringsblok.

## Aftalt spildesign — alle mod alle, 24. oktober 2026

Brugeren vurderer, at førstepersonsspillet ikke er sjovt: fjender opdages for
sent, og der er for lidt information til at planlægge en vej ud af en fælde.
Den tidligere godkendelse af styringsoplevelsen gælder fortsat som historisk
feedback; den er ikke en godkendelse af det samlede gameplay.

**Kamerabeslutning:** Vi går væk fra førsteperson. Kameraet skal vise et område
omkring spilleren, ikke hele banen. Et højt, skråt 3D-perspektiv er den foreslåede
udformning. 3D ønskes bevaret, hvis det giver interessant gameplay. Udsyn,
væghøjder, afstand og styring med et fast kamera skal konkretiseres og afprøves.

**Bane og styring:** Den nuværende referencebane bevares. Kameraets retning er
fast, og joysticket vælger retning efter gangene **på skærmen**. Retningsønsket
gemmes, indtil svinget kan udføres. Automatisk gang og midterlinje bevares.
Genvejene **åbner/lukker automatisk som i den nuværende version**.

**Fastlagte produktregler efter de 24 quizsvar:**

- Fem deltagere i en alle-mod-alle-konkurrence: én menneskestyret og fire
  computerstyrede. På sigt kan menneskelige modspillere tilføjes; online
  multiplayer er ikke en del af den aktuelle implementeringsopgave.
- Mennesket vælger **frit mellem alle fem figurer**; de fire øvrige styres af
  computeren. **Samme evner og regler gælder for alle**; særlige figurevner er
  ikke valgt. De eksisterende modeller og referencebilleder bruges fortsat.
- Alle konkurrerer om hjerter/point. AI skal også samle hjerter, bruge genstande
  og konkurrere indbyrdes, frem for at fire forfølgere kun jager mennesket.
- Hver runde varer **højst 2 minutter og slutter tidligere, når alle hjerter er taget** (build 17). Flest point i den enkelte runde giver
  rundesejren og en pokal til det samlede regnskab. Kampen slutter, når en
  spiller har vundet **3 pokaler**; antallet af runder er derfor variabelt.
- Optjente point føres videre mellem runder. Rundepoint skal derfor kunne
  adskilles fra samlede point. Flest pokaler afgør den samlede kampvinder;
  ved lige mange pokaler afgør samlede point. **Ved uafgjort runde deler de
  førende rundesejren og får en pokal hver.** Hvis flere dermed når tre pokaler
  samtidig, bruges samlede point blandt dem. **Ved fortsat fuld lighed deler
  de den samlede sejr**; der spilles ikke en ekstra afgørende runde.
- Spørgsmålstegnskasser skal løbende dukke op igen og give genstande, der kan
  bruges til børnevenlige angreb. Build 12's kasser kan kun bruges én gang pr.
  level; løbende genopfyldning er en ny regel, ikke eksisterende funktionalitet.
  Genstandspuljen er **banan, olie, fartboost, vandballon og lagkage**.
- Hver spiller bærer **én genstand**. **Den senest opsamlede erstatter den gamle**;
  fuldt inventar må derfor ikke forhindre opsamling af en ny genstand.
- **Bananer giver glid, og olie sænker farten; ingen af dem giver knockout.**
  **Vandballoner med et lille varslet plaskområde og lagkager, der flyver ligeud,**
  er valgt som nye angreb, der kan slå ud ved ét træf.
- Angreb følger **gangretningen med tydelig markering af skud-/kasteområdet**.
  De er **varslede, har kort rækkevidde og stoppes af vægge**. Spilleren bliver
  på midterlinjen og undviger ved at vende om eller vælge en anden rute.
- En spiller, som bliver slået ud, venter et stykke tid og genstarter fra midten
  af banen. Alle optjente point beholdes, både fra den aktuelle og tidligere
  runder. At blive slået ud koster kun tid: **5 sekunders ventetid** før genstart.
  Derefter gives **3 sekunders beskyttelse**, som ophører straks ved eget angreb.
  **Ét træf** fra et knockout-angreb slår ud. Spillere kan passere hinanden;
  almindelig kropskontakt giver ingen skade. **Hjerter kommer ikke igen under runden (revideret i build 16).** Næste runde starter med et nyt sæt. Figurer starter tilfældigt ved rundestart, kun KO-genstart sker i midten.
- Computerne får **samme minikortinformation som mennesket** om hjerter, positioner og genveje (revideret i build 16). Lokale kampobservationer og hukommelse bevares til sigtning og undvigelse.
- **De fire store krafthjerter fjernes, så alle hjerter er ens.** Den gamle
  krafttilstand, kædepoint for at fange forfølgere og særlige store hjerters
  bonus-/skjoldeffekt indgår ikke i den nye model. Referencebanens form bevares;
  kravet om præcis 240 små + 4 store som rundens mål er historisk.
- Spillet skal udvikle sin egen identitet gennem den nye konkurrence og
  præsentation. Brugeren har valgt at **beholde referencebanen og automatiske
  genveje**; egne baner og spilleraktiverede trykplader er ikke valgt.

**Quiz 1 — brugerens valg, 24. oktober 2026:**

1. Samlet vinder: **flest pokaler; samlede point afgør lighed**.
2. Slået ud: **behold alle point; mist kun tid under ventetiden før genstart**.
   Der tabes ikke hjerter eller point som følge af at blive slået ud.
3. Hjerter: **nye hjerter dukker op i skiftende områder** under runden.

**Quiz 2 — brugerens valg, 24. oktober 2026:**

4. Rundetid: **2 minutter**.
5. Kampens afslutning: **når en spiller har vundet 3 pokaler**, med variabelt
   antal runder. Det er ikke en kamp med et fast antal runder.
6. Ventetid før genstart fra midten: **5 sekunder**.

**Quiz 3 — brugerens valg, 24. oktober 2026:**

7. Antal træffere før knockout: **1**. Komisk reaktion og 5 sekunders ventetid
   før genstart; ingen helbredsmåler med flere træffere skal indføres ud fra
   det fravalgte forslag om 2 træffere.
8. Beskyttelse efter genstart: **3 sekunder**, som ophører straks ved eget angreb.
9. Kropskontakt: **spillerne kan passere hinanden uden skade**.

**Quiz 4 — brugerens valg, 24. oktober 2026:**

10. Bananer giver **glid**, olie giver **fartnedsættelse**, og **ingen af dem
    giver knockout**.
11. Begge nye knockout-angreb vælges: **vandballon med et lille varslet
    plaskområde og lagkage, der flyver ligeud**.
12. **Én genstand; den senest opsamlede erstatter den gamle**. Dette er
    brugerens præcisering og erstatter forslaget om at skulle bruge genstanden
    før næste kasse. Det gamle inventories spærring af nye kasser skal ændres.

**Quiz 5 — brugerens valg, 24. oktober 2026:**

13. Sigte: **i gangretningen med tydelig markering af skud- eller kasteområdet**.
14. Undvigelse: **varslede angreb med kort rækkevidde, stop ved vægge og
    vending/rutevalg langs gangens midterlinje**. Ingen ekstra undvigeknap eller
    fri sidelæns bevægelse er valgt.
15. Uafgjort runde: **delt rundesejr og en pokal til hver af de lige førende**.

**Quiz 6 — brugerens valg, 24. oktober 2026:**

16. Baner: **behold den nuværende referencebane**. Egne baner er ikke valgt,
    heller ikke som et aftalt senere trin i denne revision.
17. Joystick: **peg mod den ønskede gang på skærmen; retningen huskes, indtil
    svinget er muligt**. Automatisk gang og midterlinje bevares. Dette erstatter
    førstepersonsmodellens relative venstre/højre/vend-kommandoer.
18. Genveje: **automatisk åbning og lukning som i den nuværende version**.
    Spilleraktiverede trykplader er ikke valgt.

**Quiz 7 — brugerens valg, 24. oktober 2026:**

19. **Frit valg mellem alle fem figurer, med samme evner og regler for alle.**
20. **Store krafthjerter fjernes, så alle hjerter er ens.** De er ikke valgt
    som bonuspoint eller skjold; den gamle krafttilstand udgår.
21. **Fartboost bevares** i kasserne sammen med de fire andre genstande.

**Quiz 8 — brugerens valg, 24. oktober 2026:**

22. **Små hjertegrupper uden varsling; man opdager dem undervejs.** Ingen
    områdemeddelelse, global spawnlyd eller retningspil annoncerer nye grupper.
23. **Samme lokale udsyn som mennesket og hukommelse om det sete**. AI må kende
    det faste banelayout, men ikke usete modstanderes aktuelle positioner.
24. **Delt samlet sejr ved fuld lighed**, også efter sammenligning af samlede
    point blandt dem, der når tre pokaler samtidig.

Quizzen er afsluttet. Reglerne ovenfor er brugerbeslutninger. Startværdier og
udfyldende regler i prøveprofilen nedenfor er derimod forslag til afprøvning.
N1 blev implementeret i build 13/14. Konkurrencesimulationen fra N2 er nu
koblet til genstande, fair AI og skærmflow fra N3/N4. N5 verificeres i build 15.

## Revideret implementeringsplan — alle mod alle

### Mål og afgrænsning

Lever én lokal kampoplevelse med fem lige deltagere, taktisk udsyn og de aftalte
runder, genstande og genstarter. Første spilbare review skal især afklare, om
man kan se trusler, planlægge et par sving og undvige i tide. Grafisk finish
prioriteres efter, at den nye oplevelse fungerer i faktisk spil på iPhone.

Unity-version, URP, Input System, de fem karakterer, referencebilledet i menuen,
engelske spillertekster og iOS-lyd med Silent-knappen bevares. Ingen nye baner,
trykplader eller særlige figurevner indgår. Online multiplayer er en senere
udvidelse; ingen netværkstjeneste, konti eller matchmaking implementeres nu.

### Kampforløb og regnskab

1. PLAY åbner figurvalg på en separat skærm. Den eksisterende billedbaserede
   startmenu beholder sine tre handlinger uden ekstra karakterer eller tekster.
2. Valgt figur styres af mennesket; de fire øvrige er AI. Alle fem har samme
   bevægelses-, genstands-, kontakt- og genstartsregler.
3. En runde starter med eget nulstillet runderegnskab og 120 sekunders aktiv
   spilletid. Alle konkurrerer om den samme pulje hjerter og kasser.
4. At samle alle aktuelle hjerter afslutter ikke runden; nye små grupper kan
   dukke op indtil fristen. At blive slået ud afslutter heller ikke runden.
5. Ved fristen fryses rundens handlinger og point. Alle med flest rundepoint
   tildeles én pokal hver. Point fra runden overføres præcis én gang til den
   afsluttede del af kampregnskabet.
6. Hvis nogen nu har tre pokaler, findes kampvinderen blandt disse deltagere:
   flest samlede point vinder; fortsat lighed giver flere fælles vindere.
   Ellers vises runderesultat, og næste runde starter.

Modellen gemmer særskilt `roundScore`, `bankedScore` og `trophies` pr. deltager.
Samlede point under spil er `bankedScore + roundScore`. Rundesejr afgøres alene
af `roundScore`. Et afsluttet rundes id gemmes sammen med tildelingen, så save/
resume ikke kan tilføje rundens point eller pokaler to gange.

Simulationen bruger en entydig tidsfrist: kontakter før fristen kan give point,
kontakter ved/efter fristen gør ikke. Rundesejre tildeles samlet, før nogen
kampvinder udpeges. Test dette ved delt tredje pokal og ved rundeslut under KO.

### Teknisk opdeling og genbrug

De nuværende `GameSession`/`SessionSnapshot` modellerer én spiller og fire
forfølgere med særregler. De må ikke blot omdøbes til fem spillere.

| Område | Planlagt ændring |
| --- | --- |
| Core | Nye `CompetitionSession` og tilstandstyper med fem stabile deltager-id'er, fælles deltagerstruktur, kamp-/rundefaser og separate point/pokaler |
| Kommandoer | Menneske og AI afleverer samme retning-/genstandsvalg med deltager-id og simulationstick; én lokal simulation afgør udfaldet |
| Navigation | Genbrug `CorridorNavigation`, tunnel og geometri; gør central base-/døradgang ens for alle fem ved genstart. Gennemgå `MazeMap`'s nuværende aktørspecifikke adgang |
| Bane og ressourcer | Bevar kildens rækker, tunnel og eksisterende genveje. Konkurrenceprofilen filtrerer de fire store hjerter fra og holder nye hjerter/kasser i dynamiske tilstande |
| Genstande | Ejer-id, flyve-/varslingstid, rækkevidde, effektområde og udløb for alle fem genstandstyper. Gamle serialiseringsværdier må ikke omfortolkes |
| Runtime | Tilpas `GameApp`, `MazeView` og `GameHud` til fem deltagere, lokalt kamera, figurvalg, runderesultat og kampresultat |
| Input | Omsæt joystickretning til de projicerede gangretninger på skærmen. Gem et verdensretningsønske, så kamerafølgning og aspect ratio ikke ændrer en allerede valgt retning |
| Animation/lyd | Genbrug rigs og klip til løb, kast, glid og komisk knockout. Førstepersonshånden erstattes af synlig hel figur. Retningslyd beregnes i forhold til den nye kameravisning |
| Lagring | Nyt versioneret konkurrenceformat med banefingerprint og regelprofil, separat fra den gamle `session-v1.json`/snapshot v1–3 |
| Testværktøjer | Udvid Core-, serializer-, stress-, native- og performancetests til konkurrence. Gamle fixtures er historisk regressionsdækning, ikke test af den nye model |

Data og regler forbliver adskilt fra Unity. Seedede, adskilte tilfældighedsstrømme
og stabile id'er gør forløb reproducerbare og giver et grundlag for senere
multiplayer. Det er ikke en garanti for en færdig netværksarkitektur.

Behandl kontakt med et delt hjerte eller en kasse efter kontaktens tidspunkt.
Ved præcis samtidighed bruges en dokumenteret, reproducerbar afgørelse uden
permanent fordel til mennesket eller det laveste arrayindeks.

### Kamera, udsyn og fair AI

Kameraet følger menneskets figur med fast vinkel og viser flere nærliggende
gange. Hække/vægge sænkes eller skjules visuelt efter behov, uden at ændre
ganggrafen. Hele banen skal ikke blive synlig ved zoom ud. Kamerafølgning må
ikke rotere billedet ved sving eller give utilsigtede ture gennem tunnelens tomrum.

Afprøv et højt, skråt ortografisk kamera først. Det er et teknisk startforsøg,
ikke et separat brugerkrav. Afstand, vinkel og læsbarhed vurderes på iPhone i
begge landscape-retninger og på Mac i 16:9, bredformat og 4:3.

Brug et fælles beregnet lokalt udsyn som grundlag for kamera og AI-observationer.
AI får sit eget tilsvarende udsnit omkring sig samt observerede oplysninger
med tidspunkt. Den må huske og udlede, men ikke opdatere en uset modstanders
position fra den globale simulation. Som fair startprincip gælder samme
begrænsning for uopdagede ressourcer og usete ændringer af genveje.

AI-beslutninger modtager et observationsobjekt frem for direkte adgang til alle
aktørpositioner. To verdener med samme observation/hukommelse og AI-seed, men
forskellige usete modstandere, skal give samme beslutning. AI vælger mellem
indsamling, genstande, flugt og angreb på alle andre; de fire er ikke et hold
mod mennesket. Reaktionstid og målskift er balanceparametre.

### Foreslået prøveprofil — ikke brugerfastlagte værdier

Disse værdier og udfyldende regler skal ligge i en separat balanceprofil og
afprøves. De ændrer ikke quizbeslutningerne. Justér efter faktisk spiloplevelse;
de er ikke validerede eller endelige produktkrav.

| Parameter | Forslag til første prototype |
| --- | --- |
| Point | Ét almindeligt hjerte = 1 point. Ingen særskilte angrebs-/KO-point |
| Grundfart | 1,6 felter/sekund for alle fem; justér sammen med kamera og angrebsafstand |
| Første hjerter | De 240 små referencepositioner; de fire store hjerter fjernes. Nye grupper bruger gyldige, tomme gangfelter uden for genstartszonen |
| Nye hjertegrupper | Hvert 10. sekund op til to små grupper med tre hjerter hver; højst 240 aktive. Ingen annoncering. Manglende plads giver færre/udsat spawn, aldrig overlap med vægge eller eksisterende ressourcer |
| Kasser | Genbrug de fire kassepositioner; 10 sekunders genopfyldning efter opsamling, ens genstandssandsynlighed og ny feltindgang før næste opsamling |
| Banan | Kort kast fremad; glid ca. 0,8 sekund, mulighed for at bestille næste retning under reaktionen; højst 20 sekunders fældelevetid |
| Olie | 55 % fart i 2,5 sekunder efter kontakt; lægges på bevægelsessporet |
| Boost | 1,6 × grundfart i 4 sekunder; ingen ændring af kameraets vinkel eller udsnit |
| Lagkage | Ca. 0,35 sekunders forberedelse, 4 felters rækkevidde og 4 felter/sekund; stopper ved første træf eller væg |
| Vandballon | Ca. 0,35 sekunders forberedelse og 0,6 sekunds synligt kast; op til 3 felter frem, plaskradius ca. 0,75 felt. Ingen virkning gennem vægge |
| Egne genstande | Ejeren rammes ikke af egne angreb/fælder. Alle fire andre kan rammes; ingen holdregler |
| KO og inventar | En ubrugt genstand bevares ved KO; en allerede brugt genstand er forbrugt. Projektiler/fælder har fortsat deres normale udløb |
| Genstart | Centralt område med flere lovlige startfelter og lige adgang til eksisterende udgange. Fordel/rotér startretninger mellem runder |
| Genstartsbeskyttelse | 3 sekunder er aftalt. Forslag: blokér også glid/olie under beskyttelsen; brug af banan, olie, vandballon eller lagkage afslutter den, boost gør ikke |
| AI | Ens grundfart og genstandseffekter; første forsøg med ca. 0,3 sekunders reaktionstid og tydelig alder på hukommelsesdata |

Genvejenes automatiske cyklus genbruges. Reservationen skal omfatte alle fem
deltagere, begge ender af deres aktuelle gangkant og relevante kast/fælder.
Ingen lukning må klemme eller strande nogen. Nye ressourcer må ikke opstå i
en væg, en lukningszone eller selve genstartsområdet.

### Arbejdsblokke og færdigkriterier

Hver blok afsluttes med opdateret status og passende testbevis. Buildnumre
tildeles, når de faktiske builds laves; nedenstående er en rækkefølge, ikke
en påstand om nye færdige builds.

- [ ] **N1 — lokalt 3D-perspektiv og styring.** Vis den valgte figur i fuld
  størrelse i verden, fast kameraretning, læsbare nabogange og joystick efter
  skærmens gangretninger. Genbrug eksisterende modeller og bane. Native kontrol
  i tre skærmformater og fysisk iPhone; manuel vurdering af udsyn og sving.
  N1 er en perspektivprototype, ikke det færdige konkurrencespil.
  **Kode implementeret i build 13; tættere kamera i build 14.** Automatisk verifikation og begrænsninger
  står i [testrapporten](TEST_REPORT.md). Afkrydsning af hele blokken afventer
  brugerens manuelle vurdering af det nye udsyn og de skærmorienterede sving.
- [x] **N2 — fem deltagere og grundkonkurrence.** Fælles Core-tilstand, samme
  navigation for alle, dynamiske ens hjerter, genopfyldelige kasser,
  erstatningsinventar, 120-sekunders runder og regnskab til tre pokaler. Implementér
  delt runde-/kampsejr og et nyt versioneret snapshot. Deterministiske inputs
  er nok til regeltest her; de er ikke færdig AI. **Implementeret og verificeret
  24. oktober:** 24 konkurrencechecks, 65 klassiske regressionstests, faktisk
  Unity JSON/backup og fem fulde scriptede kampe. UI-/AI-integration følger N4.
- [x] **N3 — genstande og genstart.** Alle fem genstande, tydelige varsler,
  væg-/rækkeviddebegrænsninger, ejerskab, glid/olie/boost, ét træf til KO,
  5 sekunders ventetid og 3 sekunders beskyttelse. Udvid vægreservationer og
  test samtidige træf, genstart ved optaget midte og rundeslut under KO/kast.
- [x] **N4 — fire fair computerdeltagere og samlet flow.** Observationer og
  hukommelse, konkurrence om ressourcer, angreb/flugt mod alle andre og en
  afprøvet balanceprofil. Figurvalg efter PLAY, fem deltagere i HUD, timer,
  rundepoint, samlede point, pokaler og resultatskærme med flere vindere.
  Opdatér hjælp/tutorial/lyd/haptik og fjern den gamle lives-/kraft-/level-logik
  fra den nye brugeroplevelse. Alle tekster er engelske.
- [ ] **N5 — samlet test og overdragelse.** Automatisk verifikation, normal
  installation og overdragelse er færdige i build 15; afkrydsningen afventer
  den særskilte håndførte gameplayaccept.
  Fuld kamp med hver af de fem valgbare
  figurer i automatiske fixtures, stress af fem aktive deltagere, faktisk
  baggrund/save/resume og frisk Release-performance på iPhone. Native layout
  i begge landscape-retninger samt Mac 16:9, bredformat og 4:3. Manuel kamp-
  og gameplayvurdering på fysisk iPhone, normal signeret build uden testflag,
  opdateret dokumentation og nye kildehashes. Fysisk iPad er fortsat fravalgt
  denne omgang. Ingen Apple-upload indgår i disse blokke.

### Lagring og verifikation

Brug en ny versioneret matchfil og behold det gamle gemte spil og dets backup.
Et snapshot fra førstepersonsversionen må ikke blive tolket som fem deltageres
kamp. Indstillinger som lyd, musik, haptik og venstrehåndslayout bevares; det
gamle highscore-tal må ikke blandes med det nye kampregnskab.

Det nye snapshot omfatter mindst valgt figur, alle fem navigationstilstande,
runde-/kampfase, separate point/pokaler, KO-/beskyttelses-/genstandstimere,
projektiler/fælder, ressourcer/genopfyldning, vægfaser, seedstrømme, AI-hukommelse,
regelprofil og banefingerprint. Pause, baggrund og resume-nedtælling fryser
simulationen, også rundetiden. CONTINUE gendanner i pause. Valider atomisk
lagring, backup og præcis fortsættelse via Unitys faktiske serializer.

Udvid de eksisterende Core-/serializer-/stresschecks med de nye acceptkriterier
i [acceptmatricen](ACCEPTANCE_MATRIX.md). Kompilér mod den fastlåste Unity-version.
Native checks skal dække kamera, skærmretning, alle figurer, udskiftning af
inventar, kast/KO/genstart, fulde runder og delte sejre. Afprøv retning og
genstandsbrug samtidigt med rigtige fingre på den nye iPhone-version.

Den store test er en ny mindst 900-sekunders Release-kørsel med den nye
simulationsmodel, inklusive aktivitet mellem runder/kampe efter behov. Mål
framefordeling, hukommelse, temperatur, fejl og repræsentative hændelser for alle
fem. Rapportér faktisk spilletid særskilt fra resultat-/menutid. Build 9's
900-sekunders og build 10's korte målinger er historiske, ikke performanceaccept
af den nye konkurrence.

Gem nye rapporter/billeder i `docs/verification` med dato og faktisk buildnummer;
overskriv ikke tidligere beviser. Opdatér plan, acceptmatrix, testrapport og
udgivelsesstatus efter hver implementeret blok. Ved fejl eller maskinskifte
fortsættes fra sidste dokumenterede blok, ikke fra afkrydsede historiske M1–M6.

**Aktuel blokstatus:** N1–N4 er implementeret. N3/N4 har automatisk regel- og
integrationsdækning; N5's fysiske lange måling og normale installation består.
Manuel kamp- og gameplayaccept er fortsat en særskilt rest.

## Implementeret grundlag og tidligere beslutninger — til og med build 12

- Førstepersonsarkadespil til iPhone/iPad, begge landscape-retninger, offline.
- Alle spillerrettede tekster og App Store-/privatlivsudkast er på engelsk.
  Appen erklærer `en` som standardsprog og bruger engelsk talformat på alle
  enhedssprog. Dette erstatter tidligere danske UI-beslutninger.
- Teenagere og voksne er den primære målgruppe. Tonen er varm politisk satire
  med harmløs tegnefilmskomik.
- Zelenskyj er helten. Putin, Xi, Kim og Trump er forfølgerne med henholdsvis
  Chaser-, Ambusher-, Flanker- og Wanderer-adfærd.
- Alle fem er hele karakterer med ben, baseret på de fem PNG-referenceark.
  Spøgelsesunderkroppe erstattes af løbende karakterer.
- Verden er en mekanisk eventyrhave med afrundede materialer, grønt og messing.
- Startmenuen bruger brugerens `docs/images/Jagten gennem egoernes labyrint.png`
  som baggrund. Kun PLAY, CONTINUE SAVED GAME og HELP vises i en kolonne i det
  mørke område til højre. Ingen ekstra figurer, portrætter, titel, tagline,
  rekordtekst eller Settings-knap på startskærmen. Settings findes i pausemenuen.
  Originalbilledets proportioner og hele motiv bevares; knapper følger safe area.
- Automatisk gang på gangens midterlinje med relativt joystick: venstre/højre
  bestiller ét sving ved næste lovlige mulighed; op fortsætter ligeud og erstatter
  ventende sving; ned vender om. Ét udslag giver én kommando. Slip fortsætter,
  en væg stopper ved feltcentret. Brugeren valgte denne model efter build 9.
- Kameraet følger bevægelsesretningen vandret. Intet kig op/ned eller frit
  kameratræk. Genstandsknappen kan bruges samtidig med joysticket.
- Ingen hop eller sidelæns gang. En farlig forfølger spærrer stadig gangen.
- HUD viser `Lives:` med hjertesymboler. iOS-afspilning fortsætter med den fysiske
  Ring/Silent-knap på lydløs; medielydstyrke og spillets lydvalg gælder fortsat.
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
- Spilleren har kontinuerlig position på ganggrafen, en kardinal retning og ét gemt sving.
  Fjender navigerer på banens ganggraf med aktørspecifik adgang til basen.
- Samme topologi bestemmer bevægelse, vægpassager og fjendernes ruter.
  Kontinuerlige kontakter sikrer, at boost ikke springer hjerter eller fangst over.
- Snapshotformat 3 gemmer position, gangretning, automatisk gang og svingkø. Version 1/2 migreres til midterlinjen. Pause og
  baggrund forbruger ingen effekt- eller vægtid.
- Ingen backend, konto, annoncer, køb i appen eller netværksafhængighed i spillet.
- Touchstyringen bruger et spiltilpasset, rundt Unity-overlay med Input System.
  Valget, vurderingen af Apples alternativer og kontrolmål er gemt i
  [inputbeslutningen](INPUT_DECISION.md). Brugeren har eksplicit valgt den relative styring med automatisk gang.

## Milepæle og accept

Disse milepæle beskriver den tidligere førstepersonsmodel. De skal omsættes
til den nye konkurrence efter designquizzen; de eksisterende tests dokumenterer
build 12 og er ikke accept af den foreslåede alle-mod-alle-model.

- [x] **M0 — projekt og værktøjer.** Reproducerbart Unity-projekt, fastlåste
  versioner, banedata, testkommandoer og tidlig fysisk iPhone-build. En konkret
  værktøjsblokering dokumenteres, hvis enhedsbuild ikke er mulig.
- [ ] **M1 — bane og styring.** Rigtig 3D-geometri, automatisk gang, relativt joystick, kamera,
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
M1–M4 er implementeret og består de automatiske kontroller; fuld brugeraccept
fastholdes åben indtil det aftalte manuelle gameplayreview. Den nye
styringsoplevelse er særskilt brugeraccepteret 24. oktober på build 12;
en hel håndspillet runde og de øvrige manuelle kriterier er fortsat åbne.
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

Build 5 fortsætter med en lokal Blender-pilot, fordi Meshy-adgang blev afvist
af browserens sikkerhedskontrol. Putin har nu faktisk mesh, skelet og fem
animationer i Unity. Kilden og FBX følger repoet; Blender kræves kun for
redigering. Piloten opfylder ikke slutkvaliteten alene. Se
[produktion, kildefiler og begrænsninger](CHARACTER_PRODUCTION.md).

Build 6 forbedrer den lokale pilot med sammenhængende ansigtsovergange,
formet næseprofil/hår, bedre stoflag og albue-/knævægtning. En ny bagt
løbecyklus har støttefod mod gulvet, og skridttempoet følger figurens fart.
Kilder, generator, shader og måleresultater følger repoet.

## Testplan

- Importér og kontrollér 28 × 31 felter, præcis 240 + 4 hjerter og adgang til alle.
- Test automatisk midterlinjegang, relative sving, svingkø, vending, tunnel og sikre vægpassager.
- Test alle AI-roller, kraftudløb, returruter, pointkæde og én ekstra chance.
- Test fangst samtidig med kraft-/sidste hjerte samt Game Over og levelskifte.
- Test inventar, kast mod væg, olie, fælles glideimmunitet og boostkontakter.
- Test vægkombinationer og udsat lukning under en reserveret passage.
- Test pause/baggrund og versioneret lagring/genoptagelse.
- Test joystick/genstand samtidig, fast vandret kamera og UI på iPhone i begge retninger; fysisk iPad er fravalgt denne omgang.
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

## Samlet færdiggørelse efter build 6

Brugeren har bedt om resten af implementeringen og en stor samlet test.
Arbejdsblokken starter fra commit `2f8533f` (Build 6), rent checkout.

- **Build 7 — karakterhold:** Rigget Xi, Kim, Trump og Zelenskyj med egne
  proportioner, hår, kostumer og referenceark. Fælles reproducerbar eksport,
  forskellige bevægelser, faktisk 3D-menuavatar og importeret førstepersonshånd.
- **Build 8 — have og præsentation:** Beplantning, puds/træ/sten/messing,
  læsbare landmarks, baseudsmykning, flere egne lydsignaler, afstemt haptik,
  hjælpe-/Om-skærm og skalerbar grafikprofil.
- **Build 9 — samlet testbuild:** Native Mac/iPhone-kontrol med alle modeller,
  begge landscape-retninger, flere skærmformater, faktisk baggrund/genoptagelse
  når værktøjerne tillader det, og mindst 900 sekunders Release-måling på iPhone.
  Klargør ikon, screenshots, privatlivs-/assetoversigt og indsendelsesmateriale.

Alt uafhængigt implementeringsarbejde gennemføres før afslutningen. Visuel
slutaccept, fysisk iPad, ukendte kontakt-/rettighedsoplysninger og adgang til
TestFlight kan kræve eksterne oplysninger; de må ikke erklæres gennemført uden
bevis. Ingen betalt tjeneste bestilles. Offentlig udgivelse/Apple-upload er
ikke udført som del af lokal implementering og test.

## Historisk status — N2, konkurrencens regelsystem

`CompetitionSession` og `CompetitionSnapshot` er nye typer; det gamle
`GameSession` og dets fil er bevaret. Fem stabile id'er (`kim`, `putin`,
`trump`, `xi`, `zelenskyy`) bruger samme midterlinjenavigation, fart og adgang
til midte/døre. `humanId` kan være et hvilket som helst af dem. Startfelter
roterer mellem runder. Kropskontakt spærrer eller skader ikke.

Kommandoer har deltager-id, simulationstick og verdensretning. Kun kommandoer
til det aktuelle aktive tick accepteres. Navigationen kan aflevere delstræk,
så opsamling følger den faktiske rute ved sving og tunnel, ikke en diagonal
eller en linje gennem hele banen. Den klassiske navigationsadgang er fortsat
standard; kun konkurrencen åbner centralbasen for alle.

120 sekunder er præcis 7.200 aktive ticks. Pause, nedtælling og resultatskærme
forbruger ingen rundetid. Kontakter før fristen tæller; kontakt ved fristen
tæller ikke (numerisk lighed behandles med tolerance 1e-9 af ét tick).
Rundepoint og bankede point er separate; `TotalScore` tæller aldrig den netop
bankede runde dobbelt. Rundens id og settlementstatus bevares gennem lagring.
Alle lige rundevindere får én pokal. Samtidig tredje pokal afgøres af bankede
totalpoint blandt dem med tre; fuld lighed giver fælles kampsejr.

Startpuljen er 240 ens hjerter fra de små referencepositioner. Der er ingen
krafthjerter/krafttilstand i Core-konkurrencen. Hvert tiende aktive sekund
forsøges op til to sammenhængende grupper med tre hjerter, højst 240 aktive.
Nye grupper undgår optagne felter, deltagere, kasser, centralbasen og
genvejenes væg-/tilgangsfelter. Ingen spawnannoncering udsendes.
Kasserne genopfyldes efter ti sekunder, afrundet op til næste 60-Hz-tick.
En spiller skal forlade og genindtræde for ny opsamling; venten oven på
kassen giver ingen automatisk ny genstand. Nyt fund erstatter det gamle,
og puljen har banan, olie, boost, vandballon og lagkage. **Anvendelse/angreb
og KO er N3 og er ikke implementeret i N2.**

Samtidige delte opsamlinger afgøres først efter kontakttid. Ved numerisk
lighed bruges en separat seedet lodtrækning med kanoniske id'er. Mennesket
og det laveste arrayindeks får ingen prioritet. Ressourcer, genstande,
lighedsafgørelser og vægge har hver sin gemte tilfældighedsstrøm med blandede
startseeds. Automatiske genveje er genbrugt med reservation af begge ender
af hver af de fem deltageres aktuelle gangkant; kast/fælder tilføjes i N3.

`CompetitionSaveStore` gemmer version 1 med formatmarkør, banefingerprint,
regelprofil, alle fem navigationer/regnskaber/inventarer/kasselåse, ressourcer,
timere, vægge, seedstrømme og vinder-/settlementdata i **`competition-v1.json`**
med atomisk backup. Ukendte/ugyldige snapshots afvises før en live kamp ændres.
Indlæsning starter altid i pause. `session-v1.json`, backup, indstillinger og
gammel highscore læses/omskrives ikke af den nye store. N3/N4 skal udvide og
versionere formatet med angreb, KO og AI-hukommelse; gamle data bevares/migreres.

**Verifikation:** 24 konkurrencechecks og 65 klassiske tests består. Fem hele
kampe med scriptede inputs består: 41 runder, 295.200 aktive ticks, 4.920
simulerede sekunder og 3.817 samlede hjerter. Hvert muligt menneske-id indgår.
Unitys faktiske Newtonsoft JSON, atomisk backup, bevarelse af gamle filer og
præcis fortsættelse/engangstildeling består. Managed-kompilering har 0 fejl/
advarsler; den fastlåste Unity-editor validerer også den nye Core. Repository-
kontrollen består med 142 GUID'er og uændrede kildereferencer/modeller/bane.
[Testrapport](TEST_REPORT.md) og `docs/verification/2026-10-24-n2-core-*`.

Ingen ny iPhone-build er lavet i denne Core-blok. Det er ikke manuel gameplay-
accept, færdig konkurrence-AI eller en performancekørsel på enhed. Næste
implementering er **N3 — genstande, varsler, KO og genstart**, fulgt af N4's
AI/skærmflow og N5's fulde enhedsaccept. Der er ikke committet eller pushet.

## Historisk status — build 14, tættere N1-kamera

Brugeren bad efter build 13 om kameraet **markant tættere på karakteren**.
Den ortografiske halve billedhøjde er derfor reduceret fra 8,5 til 4,5 meter:
fuld billedhøjde fra 17 til 9 meter. Figuren bliver cirka 1,89 gange større
i hver billedretning, mens der vises et mindre lokalområde. Hældning 55°,
vandret vinkel 25°, automatisk midterlinjegang og skærmorienteret styring
bevares. Det er en ændring af zoom, ikke blot kameraets verdensafstand,
da sidstnævnte ikke forstørrer motivet i et ortografisk kamera.

Build 14 er fortsat N1-perspektivprototypen med de gamle spilleregler.
N2–N5 er ikke startet som del af kamerarettelsen. Afprøvningen dokumenteres
i [testrapporten](TEST_REPORT.md) og `docs/verification/2026-10-24-build14-*`.
Brugeren skal vurdere den nye størrelse og balancen mellem nærhed og udsyn;
feedbacken på build 13 er et ændringsønske, ikke accept af kameraet.

Managed-kompilering består med 0 fejl/advarsler. Native Mac-kontrol består i
1600×900, 2400×1080 og 1600×1200 med 43 PNG'er pr. format. Fysisk iPhone 13
består med 47 PNG'er på 2532×1170 inklusive gameplay i begge landscape-
retninger og safe area. [Aktuel iPhone-visning](verification/2026-10-24-build14-iphone13-view-LandscapeLeft.png).
Core, inputoversættelse, serializer og spilleregler er uændrede fra build 13's
65 beståede regeltests; den suite er ikke gentaget for zoomændringen.
Normal signeret **0.2.0 (14)** er auditeret, installeret og startet på telefonen
uden testflag; version og kørende proces er kontrolleret efter otte sekunder.
[Startbevis](verification/2026-10-24-build14-iphone13-normal-launch.json).
Aktuelle kilde-/asset-/dokumenthashes og testbeviser er gemt med build 14-prefix.
Ingen ny performance-/stresstest, fysisk iPad-test, Apple-upload eller commit/push.

## Historisk status — build 13, N1

Kameraet er højt, skråt og ortografisk med fast 55° hældning og 25° vandret
vinkel. Halv billedhøjde er 8,5 meter. Det følger spillerens interpolerede
position og tunnelens wrap uden at panorere gennem banen. Væggene er 1,15
meter høje med lav haveudsmykning; tunnelens tag er fjernet. Overlappende
vægtoppe og coplanar trækanter er rettet til det nye udsyn.

Zelenskyy vises i fuld størrelse med gul fodmarkering og YOU-label. Løb følger
faktisk tilbagelagt afstand; kast bruger figurens eksisterende Throw-klip.
Pause fryser positur og kast. Den gamle førstepersonshånd vises ikke.
Lydlytteren følger spilleren frem for kameraets høje position. Skyggeafstand
og skyggekort er justeret, så karakterer og hjerter også har skygger fra oven.

Joystickets udslag matches mod kameraets faktiske projektion af de fire
gangretninger. Et udslag gemmer én verdensretning; hold gentager ikke, slip
bevarer automatisk gang og ventende retning. Modsat retning vender om,
og vægge stopper ved midterlinjens feltcenter. Sving ændrer ikke kameravinklen.
Touch, tastatur og controller bruger den samme oversættelse. Hjælp/tutorial
er engelske og følger den nye styring; den uvirksomme FOV-indstilling er fjernet.

65 Core-tests består, heraf tre nye for projektion, inputlås og et gemt
skærmvalg før et sving. Managed-kompilering har 0 fejl/advarsler, og Unitys
faktiske Newtonsoft-serialisering består. Bane, PNG-referencer, modeller og
kildemanifester er uændrede; repositorykontrollen finder 139 unikke GUID'er.
Native kontroller, skærmbilleder og den signerede enhedsbuild dokumenteres i
[testrapporten](TEST_REPORT.md) og `docs/verification/2026-10-24-build13-*`.

Native kontrol består på Mac i 1600×900, 2400×1080 og 1600×1200, med 43 PNG'er
pr. format. Fysisk iPhone 13 består med 47 PNG'er inklusive gameplay i begge
landscape-retninger og safe area. Faktisk OS-baggrund/genoptagelse i 9,31
sekunder består med frosne timere, checkpoint og lydpolitik efter forgrund.
Normal signeret **0.2.0 (13)** er auditeret, installeret og startet på telefonen
uden testflag; proces 39848 er verificeret efter otte sekunder.
[Startbevis](verification/2026-10-24-build13-iphone13-normal-launch.json).
Ingen ny performance-/stresstest eller fysisk iPad-test er påstået. Der er
ikke uploadet til Apple eller udført commit/push.

Konkurrenceregler, figurvalg, nye genstande og fair lokal AI kommer senere.
Gamle liv/krafthjerter/levelregler er derfor stadig synlige. Brugerens tidligere
accept af relative sving i build 12 er ikke accept af denne nye styring.

## Historisk status — build 12

24. oktober er det eksisterende Release-projekt åbnet i Xcode 27.1 og genbygget.
Normal 0.2.0 (12) er signaturkontrolleret og geninstalleret på fysisk iPhone 13;
enhedens appliste bekræfter version/build. Efter oplåsning er appen startet i
forgrunden uden testflag; proces 39725 er verificeret kørende efter 8 sekunder.
Dette er ikke implementering af N1–N5 eller en ny gameplaytest.
Se [deploynotat](verification/2026-10-24-build12-iphone13-redeploy-mdtcgwa.json).

Build 12 er det eksisterende tekniske grundlag. Brugeren har efterfølgende
fravalgt førstepersonsretningen og fastlagt en alle-mod-alle-model gennem 24
quizsvar; se det aftalte spildesign og den reviderede plan øverst. Der er ikke
bygget en ny konkurrenceversion.

Brugeren har 24. oktober svaret **“Styringen fungerer godt”** på spørgsmålet om
automatisk gang og relative sving i build 12 på iPhone. Styringsoplevelsen er
nu manuelt accepteret. Det bekræfter ikke en hel runde, startmenuens design
eller slutkvaliteten af grafik/lyd. [Tilbagemelding](verification/2026-10-24-build12-manual-controls.json).

Ved genoptagelsen 24. oktober matcher alle 59 poster i build 12's kildeinventar
stadig deres hashes (49 er ikke genererede filer). Den lokale archives
eksekverbare fil matcher den tidligere audit. Repositorykontrollen består med
138 unikke Unity-GUID'er, uændrede referencer og modelhashes. Der er ikke lavet
en ny build, enhedsinstallation eller gameplay-/performancetest ved genoptagelsen.
[Kontrol af overdragelsen](verification/2026-10-24-build12-resume-check.json).

Startskærmen er ændret efter brugerens design: uændret kopi af baggrundsbilledet,
tre engelske menupunkter til højre, Help nederst og ingen ekstra tekst/figurer.
Continue Saved Game er nedtonet uden et gemt spil. Den skjulte 3D-menukamera-
rendering er deaktiveret; helten bruges stadig ved levelafslutning. Baggrunden
vises i originale proportioner, med en mørk flade som udvidelse på andre formater.

62 regeltests og managed-kompilering består (0 fejl/advarsler). Native Mac-
kontrol består ved 16:9, bredformat og 4:3; de sidste to gentaget med den
færdige overgang/knapstyling. Fysisk iPhone 13 består med 42 PNG'er inklusive
begge landscape-retninger, safe area og menu med/uden gemt spil. Menuen har
præcis tre tekster/knapper i rigtig rækkefølge, til højre og inden for safe area.
Help, Play, Continue, pauseindstillinger og øvrige gameplayfixtures består.

Normal signeret **0.2.0 (12)** er auditeret, installeret og startet på iPhone
uden testflag. Tidligere normale brugerdata er bevaret. Ingen Apple-upload
eller commit/push er udført. Source-/billedhashes og skærmbilleder følger
[testrapporten](TEST_REPORT.md). Originalbilledet er bevaret uændret.

## Historisk status — build 11

Alle menuer, HUD, tutorials, hændelses-/fejlbeskeder og 3D-skilte er oversat til
engelsk. `Lives:` bevarer hjertesymbolerne. Navnet staves Zelenskyy i spillet.
Tal og effekttider formateres uafhængigt af enhedens sprog, eksempelvis
`Best: 12,345` og `BOOST 1.5 s`. iOS erklærer kun `en` som appens sprog.
De engelske udgivelsesudkast ligger i [release](release/APP_STORE_EN.md).
Spilregler og gemte dataformater ændres ikke af sprogskiftet.

62 regeltests, Unity JSON og managed-kompilering består (0 fejl/advarsler).
Native Mac-kontrol består ved 1600×900 og 1600×1200; fysisk iPhone 13 består med
41 PNG'er på 2532×1170 inklusive begge landscape-retninger og safe area.
Testen kører bevidst med `da-DK` som tal-/UI-kultur og kontrollerer både engelsk
formatering og teksthøjde ved alle optagelser. Menu, hjælp, HUD og indstillinger
er visuelt kontrolleret. Regler og dataformater er uændrede.

Normal signeret **0.2.0 (11)** er auditeret, installeret og startet på iPhone
uden testflag. Archive indeholder kun `en` som understøttet app-sprog, og
signatur, ikon, tilladelsesnøgler, privatlivsmanifest og fravær af testharness
består. [Testrapport og billeder](TEST_REPORT.md) samt engelske
[udgivelsesudkast](release/APP_STORE_EN.md) er gemt i repoet.
Ingen upload til Apple, ekstern publicering eller commit/push er udført.
Tidligere performance-/stresstestbeviser nedenfor er historiske.

## Historisk status — build 10

Brugerens valgte styring, `Liv:` med tegnede hjerter og iOS Playback-lyd er
implementeret. **62/62** Core-tests og Unity JSON består. 540.000 ticks med
10 seeds består: 7.564 hjerter, 235 fangster, 145 genstande, 702 vægskift og
900 checkpoints. Stresstesten fandt en lukning før sidste feltcenter ved en
genvej; reservationen omfatter nu begge ender af spillerens aktuelle gangkant,
og en målrettet regressionstest beskytter denne adfærd.

Native Mac- og iPhone-kontrol består med sving via syntetisk touch, fast kamera,
livssymboler og de øvrige spilsystemer. iPhone: 41 PNG'er og begge landscape-
retninger. Mac 4:3: 39 PNG'er. Reel OS-baggrund i 9,36 sekunder består, inklusive
Playback-lydpolitikken efter forgrund. Kort fysisk Release-måling: 60,0021 s,
60,00 fps, p95 18,00 ms, værst 20,40 ms og ingen frames over 33,33 ms.

Den normale signerede **0.2.0 (10)** er installeret og startet på iPhone uden
testflag. Lokal archive og audit består. Kilder, beslutning og nye beviser er
gemt i repoet; der er ikke committet/pushet eller uploadet til Apple.
Brugeren har 21. oktober bekræftet både musik og effekter med fysisk
Ring/Silent-knap på lydløs som svar på build 10-spørgsmålet. Den afprøvede
build blev ikke særskilt identificeret ved lytningen; ingen ny build 12-lyttetest
påstås. Svingfornemmelsen blev senere accepteret på build 12 den 24. oktober.
Build 9's lange måling er
historisk og gælder kun build 9.

## Historisk status — samlet build 9

Version **0.2.0 (9)** samler arbejdsblokkene build 7, 8 og 9 i én reviewbuild.
Der er ikke lavet tre mellemkommits eller tre påståede enhedsgodkendelser.

| Område | Implementeret | Verificeret / afgrænsning |
| --- | --- | --- |
| Regler og bane | 244 hjerter, fire AI-roller, niveauer, chancer, kraft, genstande og sikre vægge | 56 C#-tests består, inklusive fysisk traversal i regelsimulationen og feedback for alle genstande under boost |
| Touch og UI | Begge håndlayouts, tre samtidige touchhandlinger, FOV, let grafik, hjælp/privatliv/support | Native Mac og fysisk iPhone består; 40 iPhone-PNG'er inkluderer begge faktiske landscape-retninger og safe area |
| Karakterer | Alle fem FBX/rigs, syv fælles klip, helten også Throw/Cheer, importeret arm og 3D-menuavatar | Alle fire forfølgere kontrolleret for skala/retning, deformation, fodkontakt, pause, glid, power, retur og profil/bagfra; kunstnerisk slutaccept mangler |
| Have og lyd | Stenplinter, trækanter, grønne toppe, blomster, ranker, fire landmarks, basepokaler, egne lydsignaler og native iOS-haptik | Native rendering og materialer består; håndført vurdering af stemning, lyd og haptik udestår |
| Lagring/livscyklus | Atomisk snapshot, backup, v1-migration og frosne effekter | Faktisk OS-baggrund/genoptagelse på iPhone i 9,24 sekunder består inkl. disk-checkpoint og resume-nedtælling |
| Lang måling | Særskilt Release-build med normal AI og isolerede testdata | 900,01 s på fysisk iPhone 13: 59,95 fps, p95 18,00 ms, værst 48,80 ms, ingen frames >50 ms; alle 181 temperaturprøver nominal |
| Udgivelsesmateriale | Originalt 1024px RGB-ikon, dansk metadataudkast, privatlivspolitik og assetkilder | Lokal signeret Release-archive og audit består, inklusive dansk locale. Ingen upload til Apple |

Efter den lange måling er den endelige Debug-kontrol gentaget og består igen
med alle 40 billeder og begge landscape-retninger. Den normale, signerede
Release-build 0.2.0 (9) fra den auditerede archive er derefter installeret og
startet uden testflag. Starten er bekræftet i
`Logs/iphone-build9-normal-launch.json`; processen var på det tidspunkt fortsat kørende.
Brugerens normale lager er bevaret, mens automatiske tests brugte separat lager.

Alle PNG-originaler er bevaret. Der er ikke købt tjenester eller credits.
De lokale modeller er forbedrede reviewmodeller med referencebaserede ansigter,
ikke slutgodkendt filmisk grafik. Se [produktion](CHARACTER_PRODUCTION.md).

## Overdragelse og næste konkrete skridt

**Aktiv prioritet:** Manuel gameplayvurdering af den installerede build 15.
Koden til N1–N4 og N5's automatiske test/overdragelse er færdige.
Se [aktuel overdragelse](COMPETITION_HANDOVER.md). Listen nedenfor er den
historiske build 12-reviewliste, ikke arbejdsrækkefølgen for den nye model.
Produktaccept af den gamle spilmodel er ikke en forudsætning for konkurrencen.

1. Resterende manuel accept på build 12: hel runde mod AI, den nye startmenu,
   læsbarhed og lyd-/haptikkvalitet. Automatisk gang og relative sving er
   brugeraccepteret 24. oktober; [styringsbevis](verification/2026-10-24-build12-manual-controls.json).
   Lyd med Ring/Silent-knappen på lydløs
   er brugerbekræftet; [lydbevis](verification/2026-10-21-manual-silent-switch-audio.json).
2. Kunstnerisk færdiggørelse og review af ansigter/hår, materialer, bevægelse
   og have mod M5/A28. De aktuelle modeller er fortsat reviewmodeller.
3. Fysisk iPad-test er fravalgt denne omgang. Mac 4:3 dokumenterer layoutet i
   det format; iPad-enhedsaccept står åben til senere.
4. Før TestFlight/App Store: afklar billed-/banerettigheder og ejeroplysninger,
   tilpas den valgte supportside til Maze of Egos, offentliggør korrekt
   privatlivspolitik og fastlæg endelige screenshots/aldersdeklarationer.
   Udkast og lokal archive-status ligger i [release](release/). Ingen Apple-upload
   er udført. Upload/distribution følger efter review.

De automatiske tests er afsluttet. [Testrapport](TEST_REPORT.md) og
`docs/verification` indeholder N2's Core-/serializer-/backupkontrol, fem fulde
scriptede kampe og 323 aktuelle kilde-/asset-/dokumenthashes med prefix
`2026-10-24-n2-core-`. Desuden bevares build 14's tættere kameravisning, N1-/funktionskontrol,
normal opstart, archive-audit og 315 kilde-/asset-/dokumenthashes. Build 13's
første N1-kontrol og OS-livscyklus er bevaret særskilt.
Derudover bevares build 12's menu-/funktionskontrol samt build 10's stresstest og korte
Release-måling og build 9's historiske 900-sekunders måling.

Support-URL valgt af brugeren: https://donkeytrump-api-p.azurewebsites.net/support.
HTTP 200 er kontrolleret; siden og dens privatlivslink handler aktuelt om Donkey
Trump 3D. Den eksterne hjemmeside er ikke ændret fra dette repository.

På ny pc/Mac: følg [opsætning](SETUP.md). FBX, `.blend`, generator, Unity-metadata,
plan og testbeviser skal med. Lokale Unity-/Blender-installationer og `Builds/`
er ignoreret og genskabes. Kildeændringerne er endnu ikke committet/pushet;
kopiering af arbejdsmappen bevarer dem, mens en ny Git-klon kun får commits.
Signeringskonto og credentials ligger ikke i projektkonfigurationen.

[Acceptkriterier og evidens](ACCEPTANCE_MATRIX.md).

[Historik til og med build 6](BUILD_HISTORY.md) er bevaret særskilt.
