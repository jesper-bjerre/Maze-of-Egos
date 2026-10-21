# Maze of Egos — spildesign og implementeringsspecifikation til Codex

Version 1.1 · 21. oktober 2026 · Målplatform: iPhone og iPad · Sprog: dansk

> **Implementeringsbeslutninger:** Se [implementeringsplan og aktuel status](IMPLEMENTATION_PLAN.md).
> Den efterfølgende afklaring ændrer blandt andet styring til fri analog
> bevægelse, forfølgere til hele karakterer, målgruppen til teenagere/voksne og
> tilføjer korte komiske forskrækkelser samt mindst to sekunders krafttilstand.
> Ved modstrid gælder implementeringsplanens beslutninger. Grundspecifikationen
> nedenfor bevares som reference.

Opdatering 1.1: Politisk satire med Zelenskyj som helt, fire navngivne politiske karikaturer som forfølgere, hjerter som samleobjekter og tre harmløse bonusgenstande. **Maze of Egos** er et anbefalet arbejdsnavn, indtil et endeligt navn er valgt.

## 1. Opgaven til Codex

Design og implementér et singleplayer-arkadespil til iOS og iPadOS med Pac-Mans grundregler og originale banelayout, oplevet i førsteperson i en rigtig 3D-labyrint. Den gennemgående tone er humoristisk politisk satire, som samtidig fungerer som børne- og familiespil. Tilføj tre enkle, harmløse bonusgenstande og langsomt bevægelige vægge, som skaber midlertidige genveje. Målet er en spilbar, poleret app med henblik på udgivelse i App Store.

Spilleren er en animeret karikatur af **Volodymyr Zelenskyj**, spillets helt. Kameraet er hans øjne. Labyrintens høje, massive vægge skjuler tilstødende gange. Han samler hjerter automatisk, undgår fire spøgelseskarikaturer og vender jagten ved at samle et stort krafthjerte. Forfølgerne er **Donald Trump, Vladimir Putin, Xi Jinping og Kim Jong-un**. Når alle obligatoriske hjerter er samlet, afsluttes banen, point vises, og næste level begynder på samme bane med øget sværhedsgrad.

**Målet er at samle samtlige hjerter.** Der findes ikke en udgang, som skal findes eller nås. Labyrintens genveje er taktiske muligheder, ikke en flugt fra banen.

Den visuelle reference er den varme, humoristiske og filmiske 3D-animation i Pixar-filmen *Up*: bløde former, lækre materialer, udtryksfulde figurer og gennemarbejdet lys. **Dette er et centralt kvalitetskrav til både figurer og den faktiske spilscene.** En grafisk menu eller et flot reklamebillede opfylder ikke kravet alene. Skab et selvstændigt univers og egne assets med denne visuelle ambition.

Dette er en designspecifikation, ikke en færdig app. Tallene nedenfor er implementerbare startværdier. Hvor der står **designvalg**, er det vores valg til dette spil; det skal ikke præsenteres som en historisk Pac-Man-regel.

### Prioritering

1. Det skal være rart og præcist at styre på en touchskærm.
2. Prikker, jagt, kraftprikker, point og levels skal fungere fra ende til anden.
3. Førsteperson skal give spænding uden at blive desorienterende eller uretfærdigt.
4. Genstande og bevægelige vægge skal give få, letforståelige taktiske muligheder.
5. Filmisk grafik skal fungere med stabil billedhastighed på mobile enheder.

### Satirens tone og målgruppe

Zelenskyj er den positive hovedfigur, og de fire andre er komiske modstandere i spillets opdigtede univers. Det er en satirisk rollefordeling, ikke en faktuel klassifikation eller en undervisningsforklaring af landene og deres befolkninger.

Humoren skal kunne læses på to niveauer: Børn forstår den fjollede jagt og de klodsede reaktioner; voksne genkender karikaturerne og den politiske satire. Ingen politisk forhåndsviden er nødvendig for at spille.

- Komikken kommer fra store egoer, forfængelighed, pral og fjollede situationer i labyrinten.
- Brug bananskræller, oliepletter og fartboost. Der er ingen bomber, skydevåben, knive, eksplosioner eller andre våben.
- Alle figurer forbliver uskadt. Undgå blod, realistisk vold, dødsfremstillinger, krigsscener og skræmmende jumpscares.
- En figur, der glider, laver en kort tegnefilmsreaktion og kommer videre. Point belønner spilhandlinger, ikke fremstillinger af skade på personer.
- Når helten bliver fanget, vises **Fanget! Prøv igen**. Han mister en chance og starter igen; han bliver ikke dræbt.
- Når et krafthjerte vender jagten, kan helten sende en forfølger **til pause** med et harmløst tegnefilms-puf. Den klassiske spisefunktion bevares mekanisk, men vises uden bid eller indtagelse af et menneske.
- Satiren handler om de konkrete offentlige personer. Brug ikke befolkningsgrupper, etnicitet eller nationalitet som vittighedens mål.
- Eventuelle replikker er tydeligt fiktive tegnefilmsreplikker. Brug generiske komiske stemmer frem for autentiske eller efterlignede lydoptagelser af politikerne.

I menupunktet **Om spillet** beskrives oplevelsen kort som en fiktiv politisk satire uden tilknytning til de afbildede personer. Aldersklassifikation og eventuel Kids Category vurderes efter den faktiske build; de er ikke besluttet af dette dokument.

## 2. Afgrænsning af reference og offentlig udgivelse

Brugerens krav er den originale Pac-Man-bane. Bevar dette eksplicit i implementeringsplanen; byt ikke ubemærket banen ud med en tilfældig labyrint.

Apple har regler om efterligninger og tredjepartsrettigheder i App Review Guidelines 4.1 og 5.2. En ny 3D-præsentation er ingen garanti for godkendelse. Udgivelse af en version med original bane, figurer, navne eller andet beskyttet indhold kræver afklaring af de relevante rettigheder. Dette dokument fastslår ikke den juridiske beskyttelse af hver enkelt spilmekanik. [S1, S2]

Arkitekturen skal understøtte begge udgivelsesveje:

- **Original referencebane:** Layoutet i afsnit 6 bruges til den ønskede implementering og sammenligning. Registrér rettighedsstatus særskilt; medfølgende banedata er ingen licens.
- **Egen udgivelsesbane:** Hvis den originale bane ikke kan rettighedsafklares, kan en selvstændigt designet bane tilføjes som et nyt data-asset. Denne ændring skal være synlig som en produktbeslutning.

Det anbefalede arbejdsnavn er **Maze of Egos**. Det beskriver labyrinten og den politiske komik uden at love, at spilleren skal finde en udgang. Navnene nedenfor er kreative forslag og er ikke undersøgt for navne-, domæne- eller varemærkekonflikter.

| Navneforslag | Vurdering |
| --- | --- |
| **Maze of Egos** | Anbefaling: kort, let at huske og tydeligt satirisk |
| **Heart Chase** | Enkelt og børnevenligt; fremhæver hjerterne og jagten |
| **Hearts of the Maze** | Varmere og mere eventyrligt; mindre tydelig politisk satire |
| **Hearts vs. Egos** | Tydelig kontrast mellem heltens hjerter og forfølgernes egoer |
| **Escape the Maze** | Forståeligt, men kan skabe forventning om en udgang som sejrsmål |

Mulig tagline: **Collect hearts. Dodge big egos.** Dansk: **Saml hjerter. Undvig store egoer.**

Pac-Man, Mario Kart, Pixar, Up og Maze Runner bruges som interne referencer. Brug eget ikon, egne lyde, selvproducerede karikaturer og selvstændig markedsføring. Spillet indeholder ingen figurer eller historie fra filmene.

Tilføj til udgivelsesvurderingen Apples indholdsregler 1.1.1–1.1.2: Apple omtaler politisk satire, men en satirebetegnelse garanterer ikke accept af enhver udformning. Vurder konkret fremstillingen af de navngivne modstandere og den børnerettede præsentation sammen med de øvrige krav. [S1] Dette er en vurdering af den endelige udformning, ikke en opfordring til at fjerne de aftalte karikaturer under implementeringen.

Dette afsnit er en udgivelsesafhængighed. Det forhindrer ikke arbejdet med spillets kode og design.

## 3. Anbefalet teknisk løsning

Hvis der allerede findes et projekt, skal Codex først undersøge det og bevare en egnet eksisterende motor. Ved et nyt projekt er standardvalget:

| Område | Valg |
| --- | --- |
| Motor | Unity 6.3 LTS, kompatibel stabil patch låst i projektet |
| Programmering | C# |
| Grafik | Universal Render Pipeline, Metal på Apple-enheder |
| Input | Unity Input System med touch og controller via samme handlinger |
| UI | Ét samlet UI-system, som kan håndtere touch, safe areas og skalering |
| Spilregler | Separat C#-simulation uden afhængighed af scener og animationer |
| Baner og balance | Versionsstyrede data-assets; JSON til baner og importerbare tabeller til balance |
| Lagring | Lokal, versionsmærket savefil med atomisk skrivning |
| Distribution | Unity iOS-build til Xcode og derefter signering/TestFlight/App Store |
| Deployment target | iOS/iPadOS 17 eller nyere som produktvalg; bekræft kompatibilitet |
| Retning | Landscape med begge liggende retninger |
| Netværk | Fuld spiloplevelse offline |

Motorvalget er en anbefaling til dette projekt. Unitys dokumentation beskriver URP som en løsning til blandt andet mobilgrafik og angiver understøttelse af Metal. Match URP-versionen til den valgte editor. [S6, S7]

Brug den SDK- og Xcode-version, Apple kræver ved upload. Kontrolleret 21. oktober 2026: Apples krav fra 28. april 2026 er Xcode 26 eller nyere og SDK til iOS/iPadOS 26 eller nyere. Det er forskelligt fra appens minimumsversion for brugernes enheder. Kontrollér igen før indsendelse. [S3]

Start uden backend, konto, annoncer, køb i appen eller multiplayer. Game Center og cloud-save kan tilføjes senere. Kode må ikke være afhængig af, at disse tjenester er tilgængelige.

## 4. Spiloplevelsen

### En typisk runde

1. Spilleren vælger **Spil** og får en kort nedtælling.
2. Kameraet er i Zelenskyjs øjenhøjde; en række lysende hjerter viser gangen fremad.
3. Figuren bevæger sig automatisk fremad. Spilleren vælger sving og kan vende om.
4. De fire politiske spøgelseskarikaturer kan høres i nærheden og bliver synlige, når de kommer rundt om hjørner.
5. Et stort krafthjerte gør de relevante forfølgere midlertidigt harmløse, så de kan sendes tilbage til basen.
6. Et spørgsmålstegn giver en bananskræl, en olieplet eller et fartboost.
7. En væg markerer med lys og lyd, at en genvej åbner. Spilleren kan nå igennem, før den lukker.
8. Ved sidste hjerte stoppes runden, **Alle hjerter samlet!** vises, og næste level forberedes.
9. Ved kontakt med en aktiv forfølger bliver Zelenskyj fanget og mister én chance. Ved nul chancer vises resultat og mulighed for et nyt spil.

Det skal føles som et arkadespil med korte beslutninger. Undgå quests, dialogtræer, crafting og kompliceret inventar.

### Spiltilstande

Den almindelige **Spil**-knap starter den samlede oplevelse med begge tilføjelser aktiveret. Genstande og genveje findes fra level 1, men introduceres gradvist under den første runde.

Udviklingsværktøjer skal kunne slå `itemsEnabled`, `movingWallsEnabled` og `levelClearBonusEnabled` fra uafhængigt. En grundprofil uden tilføjelser bruges til at kontrollere basisreglerne. Der behøver ikke være en ekstra offentlig spiltilstand i første version.

## 5. Førsteperson, kamera og styring

### Standardstyring: automatisk fremdrift og relative sving

Dette er et **designvalg** til mobil. Spilleren styrer en person, som automatisk går langs gangens midterlinje. Fire logiske verdensretninger bestemmer bevægelsen; kameraet animeres blødt mellem retningerne.

| Input | Handling |
| --- | --- |
| Swipe mod venstre | Bestil et venstresving ved næste mulige kryds |
| Swipe mod højre | Bestil et højresving ved næste mulige kryds |
| Swipe ned | Vend 180 grader; tilladt midt på en gang |
| Swipe op | Fortsæt ligeud og annullér et ventende sving |
| Tryk på genstandsknap | Brug den gemte bonusgenstand |
| Tryk på pause | Frys simulationen og vis pausemenu |

- Gestus aflæses relativt til figurens logiske retning, når gestussen accepteres. Gem derefter en verdensretning, så et animerende kamera ikke ændrer bestillingen.
- Inputkøen rummer ét sving. Ny retningsinput erstatter det tidligere ønske.
- Ønsket udløber efter 1,0 sekund eller efter det første udførte sving. Det udløser aldrig et uventet sving langt senere.
- Udfør svinget ved det første lovlige kryds inden for tidsvinduet. Vis en lille pil som bekræftelse.
- Er ønsket blokeret, fortsætter figuren fremad, hvis det er muligt. Ellers står den stille ved væggen, indtil en lovlig retning vælges.
- Automatisk fremdrift genoptages efter et lovligt sving. Ingen straf for at holde stille ved en blindgyde.
- Begræns diagonal input; vælg gestussens primære akse efter en skaleret tærskel.
- En gesture, der starter på pause- eller genstandsknappen, må ikke også styre bevægelsen. Flere fingre må ikke annullere hinandens handlinger.

Tilføj valgfri store venstre/højre/vend-knapper til spillere, der foretrækker knapper. Samme handlinger skal kunne styres med en controller. Frit FPS-kamera og to virtuelle joysticks er ikke en del af første version.

### Kamera og målestok — startværdier

| Parameter | Standard |
| --- | --- |
| Én grid-enhed | 2,5 meter |
| Gangenes frie bredde | Cirka én grid-enhed |
| Væghøjde | 4,5 meter |
| Øjenhøjde | 1,55 meter |
| Spillerens kollisionsradius | 0,30 meter |
| Horisontal FOV | 80 grader; indstilling 70–95 |
| Animeret 90-graders drejning | Cirka 0,15 sekund |
| Animeret vending | Cirka 0,20 sekund |
| Head bob, motion blur, kamerarysten | Slået fra som standard |

Horisontal FOV er designets værdi; konvertér korrekt til motorens kameraindstilling for den aktuelle aspect ratio. Kamerainterpolation ændrer ikke simulationens hastighed og må ikke føre kameraet gennem vægge. Afprøv kameraets bue ved hjørner på en fysisk enhed.

Spilleren kan ikke hoppe, flyve, kravle eller se hen over væggene. Kraftprikker og boost ændrer ikke kamerahøjden. Der skal ikke være udsyn gennem vægge eller et permanent kort med alle fjender.

### Orientering og fair information

- Brug diskrete, genkendelige vægmaterialer og landemærker i forskellige dele af banen.
- Hjerter skal kunne ses på normal gåafstand uden at blænde.
- Brug rumlig lyd og en valgfri, grov retningsindikator ved en nærliggende fare. Den viser ikke en præcis fjende gennem væggen.
- Ved få resterende prikker kan en mild lyd hjælpe spilleren; ingen gennemlysning af vægge.
- Et pausekort kan vise besøgte gange, men ingen levende fjendepositioner. Det er en senere hjælpefunktion, ikke et MVP-krav.

## 6. Den originale referencebane som data

Referencebanen er et grid på **28 kolonner × 31 rækker** med **240 små prikker og fire kraftprikker**. Der er et centralt spøgelseshus og en forbindelsestunnel mellem højre og venstre side. Prikkernes samlede antal er 244. [S5]

I dette spil vises de samme positioner som **240 små hjerter og fire store krafthjerter**. Husets præsentation er forfølgernes komiske base. Dette ændrer hverken gangnettet, prikantallet eller sejrskriteriet.

Følgende er maskinlæsbare banedata, ikke en renderingsmetode. Kopiér til `classic_reference.json`, importér til et `MazeDefinition`, og generér rigtig 3D-geometri ud fra dataene. Kontrollér layoutet mod original referencegrafik under implementeringen; dimensions- og priktest beviser ikke alene historisk nøjagtighed.

```json
{
  "id": "classic_reference_v1",
  "schemaVersion": 1,
  "rightsStatus": "unverified",
  "width": 28,
  "height": 31,
  "rows": [
    "############################",
    "#............##............#",
    "#.####.#####.##.#####.####.#",
    "#o####.#####.##.#####.####o#",
    "#.####.#####.##.#####.####.#",
    "#..........................#",
    "#.####.##.########.##.####.#",
    "#.####.##.########.##.####.#",
    "#......##....##....##......#",
    "######.##### ## #####.######",
    "     #.##### ## #####.#     ",
    "     #.##          ##.#     ",
    "     #.## ###--### ##.#     ",
    "######.## #      # ##.######",
    "      .   #      #   .      ",
    "######.## #      # ##.######",
    "     #.## ######## ##.#     ",
    "     #.##          ##.#     ",
    "     #.## ######## ##.#     ",
    "######.## ######## ##.######",
    "#............##............#",
    "#.####.#####.##.#####.####.#",
    "#.####.#####.##.#####.####.#",
    "#o..##.......  .......##..o#",
    "###.##.##.########.##.##.###",
    "###.##.##.########.##.##.###",
    "#......##....##....##......#",
    "#.##########.##.##########.#",
    "#.##########.##.##########.#",
    "#..........................#",
    "############################"
  ],
  "expectedSmallDots": 240,
  "expectedPowerDots": 4,
  "playerStart": { "x": 13.5, "y": 23, "heading": "west" },
  "leaderStart": { "x": 13.5, "y": 11 },
  "ghostHomeCenter": { "x": 13.5, "y": 14 },
  "ghostDoorCells": [[13, 12], [14, 12]],
  "horizontalTunnelRow": 14,
  "itemCells": [[6, 5], [21, 5], [6, 20], [21, 20]],
  "shortcutCandidates": [
    { "id": "north", "wallCells": [[13, 1], [14, 1]], "endpoints": [[12, 1], [15, 1]] },
    { "id": "upper_middle", "wallCells": [[13, 8], [14, 8]], "endpoints": [[12, 8], [15, 8]] },
    { "id": "lower_middle", "wallCells": [[13, 20], [14, 20]], "endpoints": [[12, 20], [15, 20]] }
  ]
}
```

### Importregler

- Koordinater er nulbaserede. `(0,0)` er øverste venstre felt; `x` vokser mod højre og `y` nedad.
- `#` er væg, `.` er gang med lille prik, `o` er gang med kraftprik, `-` er spøgelsesdør. Et mellemrum har ingen prik.
- Bevar alle mellemrum i strengene. Hver række skal være præcis 28 tegn.
- Mellemrum inde i huset er kun lovlige for spøgelser. Husets indre er felterne `x=11..16, y=13..15`; døren er kun for spøgelser på vej ind/ud.
- Udvendige mellemrum uden for den forbundne spillerbane skal klassificeres som utilgængeligt tomrum. De må ikke blive ekstra spilbare rum ved meshgenerering.
- Find spillerens sammenhængende gangnet fra start uden adgang gennem spøgelsesdøren. Alle 244 prikker skal ligge i dette net.
- Halve startkoordinater ligger mellem to nabocentres positioner. Understøt start på en grafkant; afrund ikke uhensigtsmæssigt til en væg.
- Sidetunnelen forbinder enderne af række 14. Lav en kort skjult tunnelovergang, som bevarer retning og fart, uden udsyn til tomrummet uden for banen.
- Alle normale vægge forbliver lukkede i grundprofilen. De navngivne genveje er et separat lag oven på referencebanen.

Ved dokumentets oprettelse er dataenes dimensioner, antal prikker og adgang til alle 244 prikker fra start kontrolleret programmatisk. Codex skal gentage kontrollen i projektets importer og verificere selve layoutet visuelt.

## 7. Grundregler og point

### Samleobjekter

Prikker opsamles automatisk, når spillerens logiske bevægelse passerer deres opsamlingsområde. Brug bevægelsens gennemløb, så et hurtigt boost ikke kan springe en prik over.

**Præsentation:** Alle henvisninger til prikker og kraftprikker i de tekniske regler betyder henholdsvis små hjerter og store krafthjerter i det færdige spil. Bevar gerne interne IDs som `smallDot` og `powerDot`; UI, modeller og hjælpetekster bruger hjerter. Liv vises som små Zelenskyj-portrætter eller **Chancer × 3**, så de ikke forveksles med de 244 samlehjerter.

- Små hjerter: blanke, afrundede røde/rosa 3D-hjerter, cirka 0,25 meter brede, som svæver omkring én meter over gulvet.
- Krafthjerter: større gyldne 3D-hjerter med en tydelig aura og egen silhuet, cirka 0,55 meter brede.
- Et svagt lys og en rolig animation gør begge typer læsbare. Farven er ikke det eneste kendetegn.
- HUD viser **Hjerter tilbage: N**. Tælleren omfatter begge typer; spørgsmålstegn gør ikke.
- Der er ingen portal eller udgang, som kræves efter sidste hjerte. Levelskiftet sker automatisk.

| Hændelse | Point | Kommentar |
| --- | ---: | --- |
| Lille hjerte / lille prik | 10 | Obligatorisk for level-afslutning |
| Stort krafthjerte / kraftprik | 50 | Obligatorisk; udløser krafttilstand |
| Første/følgende spøgelse under samme kraftprik | 200 / 400 / 800 / 1.600 | Kæde nulstilles ved ny kraftprik [S5] |
| Spørgsmålstegn | 0 | Giver en genstand; tæller ikke som prik |
| Gennemført level | `1.000 × level` | **Designvalg**, ekstra belønning i hovedoplevelsen |

Den originale grundstruktur belønner prikker og spøgelser. Den særskilte levelbonus ovenfor er vores tilføjelse, ikke en original Pac-Man-regel. Grundprofilen har `levelClearBonusEnabled=false`.

### Liv og nye levels

- Start med tre liv i alt. Vis de resterende liv inklusive det aktive.
- Giv ét ekstra liv første gang den samlede score passerer 10.000. Det sker kun én gang pr. spil, også når grænsen passeres via en levelbonus. [S4]
- Når helten bliver fanget, bevares score, spiste prikker og allerede indsamlede spørgsmålstegn. Spilleren og spøgelserne starter igen fra deres startområder.
- Fjern gemt genstand, aktive effekter, udlagte bananskræller og oliepletter, når helten bliver fanget. Nulstil væggene til lukket grundtilstand og genstart deres tidsplan. Dette er designvalg for tilføjelserne.
- Når `remainingRequiredCollectibles == 0`, afsluttes level præcis én gang.
- Vis levelbonus og score under en kort overgang på cirka to sekunder. Ingen ekstra tryk er nødvendigt for at fortsætte.
- Næste level genskaber alle prikker og spørgsmålstegn, nulstiller aktører og effekter og indlæser den relevante balanceprofil. Resterende liv og samlet score bevares.
- Ved nul liv: Game Over, highscore, nået level, **Spil igen** og **Hovedmenu**.
- Der er ingen leveltimer og ingen sejr alene ved at vente.

### Valgfri klassisk bonusfrugt

Tilføj bonusfrugt efter MVP: to muligheder pr. level ved 70 og 170 opsamlede obligatoriske prikker, cirka ti sekunders levetid pr. mulighed. Brug egne visuelle bonusobjekter. Det er adskilt fra spørgsmålstegn og kræves ikke for at afslutte banen. Den klassiske reference bruger levelafhængige frugtpoint. [S5] Implementér værdierne som data og dokumentér dem, hvis funktionen aktiveres.

## 8. Kraftprikker og spøgelsernes tilstande

### Børnevenlig fremstilling af den klassiske spisefunktion

En kraftprik er visuelt et stort krafthjerte. Den giver Zelenskyj en varm, gylden hjerteeffekt. De berørte forfølgere får en komisk overrasket mine, en tydelig harmløs tilstand og et symbol over hovedet. De forsøger at flygte.

Ved kontakt kommer et kort puf med stjerner, hjerter eller sæbebobler. Karikaturen bliver til en lille, ufarlig boblefigur og skynder sig hjem til basen. Vis **Til pause! +200** og de følgende kædepoint. Brug samme livs- og pointlogik som den klassiske funktion, men vis ingen mund, bid, afrevne dele eller realistiske skader.

Når der nedenfor står **spises**, **spist** eller **genoplivning**, er det interne mekanikbegreber. Spillerens oplevelse er en kort komisk pause og tilbagevenden til jagten.

Når spilleren tager en kraftprik, skifter aktive spøgelser til en tydeligt genkendelig sårbar tilstand i den levelafhængige periode. De flygter og kan spises ved kontakt. Et spist spøgelse bliver til en ufarlig returfigur, går tilbage til huset og bliver senere farligt igen. Returnerende spøgelser kan ikke spises gentagne gange. [S4]

### Konkret adfærd

- Brug et centralt `PowerEffect` med en udløbstid i simulationstid.
- En ny kraftprik erstatter udløbstiden med `now + duration`; varigheder lægges ikke oven i hinanden. Spøgelseskæden starter igen ved 200.
- Aktive, almindelige spøgelser vender retning ved aktivering, hvor deres bevægelsestilstand tillader det. Returfigurer fortsætter hjem.
- Sårbare spøgelser får en fælles visuel tilstand og et eget animationssæt. Vis også et symbol, så farven ikke er eneste signal.
- Advar før udløb med ændret animation, UI og lyd. Ved en kort effekt skal advarslen være tilsvarende kort.
- Et spist spøgelse vælger en rute hjem gennem spøgelsesdøren. Efter en kort genoplivning forlader det huset igen. Som designvalg genindtræder det farligt og kan ikke give flere point fra den samme kraftprik.
- En senere kraftprik kan igen gøre det sårbart. Gem derfor `powerActivationId` pr. spist spøgelse.
- Husets dør og returfigurer følger samme passagebegrænsninger som banens aktuelle topologi tillader.

Adskil hovedadfærd, sårbarhed og eventuel bananstun i data. Undgå modstridende enkeltstående boolean-felter. Returtilstand er altid ufarlig; et sårbart og bedøvet spøgelse kan stadig spises. Stun forlænger ikke kraftprikkens varighed.

## 9. Fire fjender med forskellig adfærd

Brug selvproducerede, genkendelige karikaturer af de fire aftalte personer, med forskellige silhuetter, ansigter og lydsignaturer. De udfylder de klassiske roller direkte jagt, baghold, flankering og skiftende afstandsadfærd. [S5]

Følgende er den konkrete implementering i dette spil:

| Figurreference | Intern rolle | Jagtmål | Komisk personlighed |
| --- | --- | --- | --- |
| **Vladimir Putin — Rusland** | Chaser | Spillerens aktuelle gridposition | Overdrevent alvorlig og stiv; mister kort sin værdighed på en bananskræl |
| **Xi Jinping — Kina** | Ambusher | Fire felter foran spillerens bevægelsesretning | Sirlig og metodisk; retter sin jakke efter et fjollet glid |
| **Kim Jong-un — Nordkorea** | Flanker | Spejl Chasers position omkring punktet to felter foran spilleren | Teatralsk selvsikker; ser forbavset ud, når en genvej lukker |
| **Donald Trump — USA** | Wanderer | Spillerens position ved afstand over otte felter; ellers eget hjørnemål | Pralende og selvoptaget; stort hår og slips følger de komiske bevægelser |

Rollefordelingen og personlighederne er kreative valg til spillet. De er ikke faktuelle beskrivelser af personernes handlinger eller lande. Lås mappingen i karakterdata, så modeller, lyd og AI-rolle ikke byttes ved et tilfældigt arrayindeks.

```json
{
  "hero": { "id": "zelenskyy", "displayName": "Volodymyr Zelenskyj" },
  "pursuers": [
    { "id": "putin", "displayName": "Vladimir Putin", "role": "Chaser" },
    { "id": "xi", "displayName": "Xi Jinping", "role": "Ambusher" },
    { "id": "kim", "displayName": "Kim Jong-un", "role": "Flanker" },
    { "id": "trump", "displayName": "Donald Trump", "role": "Wanderer" }
  ]
}
```

Spøgelsesformen er en stiliseret, flydende tegnefilmskrop med et genkendeligt karikaturhoved, ansigt og enkle kostumedetaljer. Det skal ligne en hel, venlig tegnefilmsfigur med en spøgelsesagtig underkrop. Undgå løse menneskehoveder og horrorudtryk. De fire skal kunne skelnes på kropsform, hår, tøj og bevægelse; ikke blot på farve.

Det er en reproducerbar fortolkning af rollerne. Hardwarefejl og historiske koordinatbugs er ikke krav.

### Navigationsregler

- Træf valg ved grafknuder/kryds. Lovlige bevægelser bestemmes af aktuelle passager og aktørens adgangsregler.
- I jagt vælges blandt lovlige næste retninger efter afstand til rollens målpunkt. Definér en fast tie-break-rækkefølge, så samme situation er reproducerbar.
- Almindelige spøgelser vender ikke vilkårligt 180 grader ved et kryds. Tillad vending ved tvungen tilstandsovergang eller ved en blindgyde.
- I sårbar tilstand vælges lovlige retninger med en seedet tilfældighedsgenerator. Spilleren kan lære adfærden, men fjenderne må ikke fryse.
- Returnerende figurer bruger en rute hjem på den aktuelle graf. Ingen flyvning gennem vægge.
- Spøgelserne kan dele en gang og passere hinanden. Undgå fysisk kø og skub mellem fjender.
- AI må kende spillerens logiske position, selv uden direkte udsyn, som i den klassiske jagtstruktur. Giv ikke spilleren tilsvarende præcise oplysninger på HUD.

Skift mellem jagt og en kort spredningsfase. Start med en datastyret rytme på 7 sekunders spredning og 20 sekunders jagt, gentaget to gange, derefter vedvarende jagt. Dette er en **forenklet designprofil**, som kan erstattes af mere originaltro tabeller senere.

I første prototype står Chaser uden for huset. De øvrige slipper ud efter henholdsvis 2, 5 og 8 sekunders aktiv spilletid. Det er **designvalg**, ikke en påstand om originalens udslipstællere. På længere sigt kan udslip også afhænge af opsamlede prikker.

Med karakterfordelingen betyder det, at Putin starter uden for basen. Xi, Kim og Trump kommer derefter ud i denne rækkefølge. Timingen er gameplay-data, ikke et udsagn om personernes politiske roller.

## 10. Spørgsmålstegn og de tre bonusgenstande

Spørgsmålstegn er små, flydende samleobjekter med eget udseende. De fire positioner i banedataene ligger på eksisterende prikfelter. Den normale prik skal stadig opsamles og tælle; et spørgsmålstegn erstatter aldrig en obligatorisk prik.

### Fælles regler — designvalg

- Ét inventarfelt med højst én gemt genstand.
- Brug stabile genstands-IDs: `bananaPeel`, `oilSlick` og `speedBoost`. Ikon, animation og effekt vælges ud fra ID'et.
- Startfordeling: 35 % bananskræl, 35 % olieplet og 30 % fartboost. Fordelingen er data og kan justeres efter afprøvning.
- Resultatet vises straks. En kort visuel animation må ikke forsinke brugen af en genstand, når en fjende nærmer sig.
- Et spørgsmålstegn kan indsamles én gang pr. level. Det respawner ikke i samme level.
- Er inventarfeltet fyldt, forbliver spørgsmålstegnet tilgængeligt, og spilleren får en diskret indikation. Den normale prik opsamles alligevel.
- Spilleren aktiverer selv genstanden med én tydelig knap.
- Ingen køb, betaling eller ekstra point for at få en bestemt genstand.

### A. Bananskræl

| Parameter | Startværdi |
| --- | --- |
| Brug | Ét tryk kaster en skræl fremad langs den aktuelle gang |
| Maksimal kasteafstand | To grid-enheder |
| Ved væg under kast | Stop på sidste lovlige landingsfelt før væggen |
| Ved kryds | Fortsæt ligeud; ingen automatisk søgning efter fjender |
| Levetid efter landing | 20 sekunder eller indtil første gyldige træf |
| Effekt | Spøgelset glider komisk og står stille i 1,2 sekund |
| Efterfølgende immunitet mod ny bananstun | Tre sekunder |

Bananskrællen er en fælde og spærrer ikke gangen. Den påvirker ikke spilleren, returfigurer eller figurer inde i huset. Et ramt spøgelse mister ikke sin rolle og får ikke automatisk sårbar status. Brug swept-kollision også for hurtige spøgelser. Har kastet ingen lovlig landingsposition, forbruges genstanden ikke.

En alternativ **læg bag dig**-handling kan komme senere. Første version har én entydig kastemetode og én knap.

### B. Olieplet

| Parameter | Startværdi |
| --- | --- |
| Brug | Ét tryk lægger en lille tegneserieolieplet bag Zelenskyj |
| Placering | Seneste lovlige gangpunkt cirka en halv grid-enhed bag spilleren |
| Levetid | 15 sekunder eller indtil første gyldige træf |
| Effekt | Første berørte forfølger glider videre med 55 % af sin ellers aktuelle fart i 2,5 sekunder |
| Formål | Skabe afstand til en forfølger uden at standse den helt |
| Udseende | Lille, skinnende 3D-plet med tydelig kant og en kort komisk glideanimation |

Oliepletten antændes aldrig og giver ingen skade. Den er hverken en bombe eller et projektil. Den spærrer ikke gangen og rammer ikke spilleren, returfigurer eller figurer inde i huset. Er der ingen lovlig placering, forbruges genstanden ikke.

Bananskræl og olie deler et kort `slipImmunity`-vindue på tre sekunder efter et gyldigt træf. Nye glid udløses ikke i immunitetsvinduet, og eksisterende glidetid forlænges ikke. En olie-ramt forfølger kan stadig sendes til pause under en aktiv krafttilstand. Krafttilstandens timer fortsætter normalt.

Bananskræl giver et kort stop; olie giver en længere, mildere forsinkelse. Begge får deres eget letgenkendelige ikon. Grundoplevelsen har netop disse tre simple genstande inklusive boost; tilføj ikke et større våben- eller genstandskatalog.

### C. Fartboost

| Parameter | Startværdi |
| --- | --- |
| Brug | Ét tryk |
| Varighed | Fire sekunder |
| Hastighed | Mindst 1,6 × spillerens normale levelhastighed |
| Krav | Boostet spiller skal være hurtigere end alle aktive jagende spøgelser |
| Invulnerabilitet | Ingen |
| Gennem vægge | Ikke tilladt |

Beregn for eksempel `boostSpeed = max(normalPlayerSpeed * 1.6, fastestThreatSpeed * 1.15)`. Medtag alle aktive hastighedsmodifikatorer hos jagende spøgelser. Ufarlige returfigurer er ikke med i sammenligningen.

Kraftprik og boost kan virke samtidig. En kraftprik gør fjender spiselige; boost gør spilleren hurtig. Boostet må ikke multiplicere hastigheden gentagne gange. En ny aktivering under et aktivt boost genstarter kun varigheden. Vis en diskret effekt i skærmens kant og en timer, uden at ændre FOV eller ryste kameraet.

## 11. Bevægelige vægge og midlertidige genveje

Stemningen henter inspiration fra en levende filmlabyrint. I dette spil er bevægelsen lokal, langsom og varslet. Hele banen skal ikke bygges om midt i et løb.

### Grundprincip

De oprindelige gange bevares. Nogle udvalgte vægsektioner kan trækkes ned i gulvet og forbinde to allerede eksisterende gange. Når sektionen lukker igen, forsvinder genvejen. Den almindelige bane er fortsat farbar og gennemførlig.

De tre kandidater i banedataene er indledende forslag, som skal kontrolleres i 3D. De åbner gennem to vægfelter. Opret midlertidige gangknuder i disse felter, og forbind de angivne endepunkter. Ingen prikker eller spørgsmålstegn placeres i vægsektionerne.

### Standardtidsplan — designvalg

- Første begivenhed tidligst 15 sekunder efter aktiv start på et level.
- Derefter ét forsøg på en ny åbning hvert 12.–18. sekund, styret af banens seed.
- Højst én genvej er aktiv i den første prototype. Højst to efter afprøvning.
- Varsling før åbning: 1,5 sekund med lysmarkering og mekanisk lyd.
- Åbningsanimation: 1,25 sekund. Passagen bliver farbar, når den er helt åben.
- Åben periode: Fem sekunder. Giv en tydelig advarsel det sidste sekund.
- Lukkeanimation: 1,25 sekund, når en sikker lukning kan begynde.
- Samme sektion har mindst 15 sekunders pause efter en afsluttet cyklus.

### Kollisions- og sikkerhedsregler

1. Spilleren og spøgelserne har samme ret til at bruge en åben genvej.
2. En bevægende væg må aldrig skære gennem en aktør, klemme nogen eller skubbe dem ind i en væg.
3. Reserver passagen, mens en aktør krydser den. Lukning udsættes, indtil alle aktører og reservationer er ude af hele vægsektionen og dens sikkerhedsmargin.
4. Bliv i varslet, åben tilstand ved udsat lukning. Brug ingen timeout, som alligevel knuser eller teleporterer en aktør.
5. Når sikker lukning starter, skal adgangsgraf, indgangskontrol og kollisionsgeometri skifte koordineret. En ny aktør må ikke kunne påbegynde passage under lukningen.
6. En gennemført krydsning må aldrig afbrydes af en grafændring.
7. Ændringer må ikke fjerne oprindelige gangforbindelser, ændre husets dør eller blokere sidetunnelen.
8. Fjender genberegner relevante ruter, når `topologyVersion` ændres. Ingen fuld NavMesh-genbagning pr. frame.
9. Resterende obligatoriske prikker skal kunne nås i enhver stabil vægtilstand.

Et muligt flugtøjeblik opstår, når spilleren når over, og væggen lukker, før forfølgeren når ind. En forfølger, der allerede er i passagen, får også lov at komme ud. Spillet giver ingen usynlig særbehandling til spilleren.

## 12. Levels og sværhedsgrad

Samme grundbane gentages. Der kommer flere point og højere pres, mens spillerens kendskab til banen bliver en fordel. Originalens progression sker i trin og har undtagelser; implementér ikke en ubegrænset lineær acceleration og kald den original.

Brug følgende relative hastigheder som klassisk reference. Alle værdier skaleres til den valgte 3D-målestok. [S5]

| Level | Spiller | Almindeligt spøgelse | Sårbart spøgelse |
| --- | ---: | ---: | ---: |
| 1 | 0,80 | 0,75 | 0,50 |
| 2–4 | 0,90 | 0,85 | 0,55 |
| 5–20 | 1,00 | 0,95 | 0,60 |
| 21+ | 0,90 | 0,95 | Ikke relevant uden sårbarhed |

**3D-designvalg:** `baseSpeed = 2.0 grid-enheder/sekund`. Level 1 giver dermed normalt spilleren 1,6 og et jagende spøgelse 1,5 grid-enheder/sekund. Afprøv især tiden til at genkende og vælge sving. Skalér alle basishastigheder samlet, hvis det kræves på touch.

Sårbarhedsvarighed for level 1–21 som reference, i sekunder: [S5]

```json
[6, 5, 4, 3, 2, 5, 2, 2, 1, 5, 2, 1, 1, 3, 1, 1, 0, 1, 0, 0, 0]
```

Level 21 og senere bruger sidste profil. Ved nul sekunder giver kraftprikken stadig sine prikpoint og den konfigurerede vending, men ingen spiselig periode. Fortæl ikke spilleren, at fjender er spiselige, når de ikke er det.

For første prototype bruges jævn bevægelse under prikopsamling. En senere, valgfri klassisk bevægelsesprofil kan indføre originalinspirerede mikropauser. Dette påvirker forholdet mellem effektiv spillerhastighed og fjendehastighed og skal afprøves særskilt.

Tunnelhastighed, udslipsregler og eventuel acceleration hos Chaser ved få resterende prikker skal være konfigurationsfelter. En enkel startprofil giver spøgelser halv normal fart i tunnelen og ingen ekstra slutfaseacceleration. Sådanne forenklinger skal fremgå af implementeringens dokumentation.

Spillet skal fortsætte ud over level 255 uden tæller-overflow, ødelagt bane eller genskabelse af historiske hardwarefejl.

## 13. Visuel retning, lyd og animation

### Udtryk

**Pixar 3D med *Up* som visuel reference er et styrende designkrav.** Omsæt det til konkrete egenskaber: varme farver, bløde volumenformer, stærke ansigtsudtryk, omhyggelig materialebehandling og filmisk lys. Det skal kunne ses i gameplay fra første person, også når alle fire forfølgere er aktive.

Lav et eventyrligt, indbydende univers med komisk spænding. Labyrinten kan ligne en mekanisk eventyrhave med pudsede vægge, malet træ, afrundede sten, messingdetaljer og få humoristiske prestigeobjekter ved forfølgernes base. Materialer og farver hjælper spilleren med at lære områderne at kende. Der er ingen krigsruiner eller virkelig frontlinje som baggrund.

- Bred, blød belysning og varme højlys; køligere skygger giver dybde.
- Afrundede kanter og tydelig volumen i vægge og figurer. Brug bevels og velvalgte normaler i den endelige geometri.
- Store, udtryksfulde hoveder, let overdrevne proportioner og letlæselige ansigter. Figurerne ligner den samme animationsfilms verden.
- Hud, hår og stof har forskellige, afstemte materialer med bløde refleksioner. Ansigter må ikke ligne flade fotos klistret på kugler.
- Små røde/rosa 3D-hjerter på de 240 normale samlepositioner og store gyldne krafthjerter på de fire kraftpositioner.
- Et roligt, malerisk farvehierarki adskiller gulv, vægge, hjerter og figurer, også på en lille iPhone-skærm.
- Spørgsmålstegn har eget design, læsbar kontrast og en kort, behagelig lyd.
- Et flot stillbillede erstatter ikke 3D-assets og animationer i den kørende scene. Brug rigtige modeller, som kan ses fra alle relevante vinkler.

### Zelenskyj skal være tydelig i et førstepersonsspil

Helten designes som en genkendelig, positiv Zelenskyj-karikatur med kort mørkt hår, kort skæg, olivengrøn T-shirt og et varmt, beslutsomt udtryk. Brug en enkel, animeret 3D-avatar i hovedmenu og introduktion. Et lille portræt i HUD kan markere spillerens identitet uden at fylde udsynet.

I gameplay kan hans hænder og de grønne ærmer kort ses, når han bruger en genstand. Undgå et stort svævende førstepersons-våbenfelt. Kameraet forbliver i hans øjne; gameplay skifter ikke til tredjeperson for at vise figuren. En kort fejring mellem levels kan vise avataren uden at ændre selve spilperspektivet.

### Krav til karakterassets

| Figur | Genkendelige visuelle træk | Animationsretning |
| --- | --- | --- |
| Zelenskyj | Kort mørkt hår, skæg, grøn T-shirt | Opfindsom, venlig og energisk |
| Trump | Markant lyst/orange hår, mørk jakke, langt rødt slips | Store armbevægelser; hår og slips får komisk efterbevægelse |
| Putin | Kort hår, genkendelig hovedform, stramt mørkt jakkesæt | Stiv, overdrevent kontrolleret bevægelse, som brydes af glid |
| Xi | Genkendelig frisure, ansigtsform og mørk jakke | Sirlig bevægelse og små forsøg på at rette tøjet efter et uheld |
| Kim | Genkendelig frisure, ansigtsform og enkel mørk jakke | Overdreven selvsikkerhed og store overraskede reaktioner |

Karikaturen skal bygges på den enkelte persons træk og kostume. Samme varme stil gælder alle fem; modstanderne skal være komiske, ikke dæmoniske eller skræmmende.

Før afslutning af M5 skal der være modeller, materialer, rig og animationer til de relevante tilstande, en oversigt over assetkilder/licenser og en fungerende mobiltilpasset eksport. Primitive pladsholdere er tilladt i M1–M4, men opfylder ikke den visuelle definition af færdig.

### Visuel godkendelse før udgivelseskandidaten

Kontrollér fire konkrete scener: en normal gang med hjerter, et møde med alle fire karikaturer, krafttilstanden og en åbnet genvej. Vurder dem både i enhedsscreenshots og under bevægelse. De fem figurer skal være genkendelige og hænge stilmæssigt sammen; hjørner, lys og materialer skal opleves som en gennemarbejdet animationsverden.

Codex skal rapportere manglende assets eller en uopfyldt visuel kvalitet som udestående arbejde. Filmisk kvalitet må ikke erklæres opnået alene på baggrund af en beskrivende prompt eller en grå prototype.

### Animationer

Forfølgere: rolig bevægelse, jagt, komisk kraftreaktion, glid på banan, glid på olie, harmløst puf og tilbagevenden fra basen. Bananglidet skal være morsomt og hurtigt læseligt. Fangstsekvensen skal være kort og kunne dæmpes med indstillinger for reduceret bevægelse. Ingen figur ligger skadet tilbage.

### Lyd

Brug egne eller licenserede lyde. Hjerteopsamling har små variationer og en begrænset samlet lydstyrke. Forfølgere har hver sin komiske lydsignatur. Krafthjerte, boost og vægge har tydeligt forskellige start- og slutsignaler. Brug let, legende musik, hvis musik er aktiveret. Der er ingen skud, eksplosioner, krigslyde eller optagelser, som giver indtryk af autentiske udtalelser fra personerne.

Rumlig lyd er en fordel med høretelefoner, men alle vigtige hændelser skal også kunne forstås uden lyd. Haptik er valgfri og bruges kort ved kraftprik, genstand, advarsel og tab af liv.

## 14. Skærme, HUD og tilgængelighed

| Skærm | Indhold |
| --- | --- |
| Hovedmenu | Arbejdsnavn, Zelenskyj-avatar, Spil, rekord, indstillinger, kort hjælp, Om spillet |
| Gameplay | Score, level, chancer, hjerter tilbage, genstandsknap, effekt-timer, pause |
| Pause | Fortsæt, genstart spil, indstillinger, hovedmenu |
| Levelskifte | Alle hjerter samlet!, levelbonus, samlet score |
| Game Over | Score, rekord, nået level, spil igen, hovedmenu |

Visuelt HUD-layout: score/level øverst til venstre, chancer og hjerter øverst til højre, genstand nederst ved højre tommelfinger, pause i et sikkert øvre hjørne. Tilpas til iPad, safe areas og kameraudskæringer.

Første introduktion viser højst tre korte instruktioner under spillet: **Swipe for at dreje**, **Tag et stort hjerte for at vende jagten**, **Tryk her for at bruge din genstand**. Hovedmålet **Saml alle hjerter** vises inden start. Stop eller sænk tempoet i selve introduktionen efter behov; registrér det som træning, hvis score sammenlignes.

Indstillinger omfatter lyd, musik, haptik, swipes/knapper, venstre-/højrehåndslayout, FOV og reduceret bevægelse. Brug tekst/symboler sammen med farve. Menuer skal have forståelige tilgængelighedslabels. Aktivt gameplay kræver en særskilt praktisk vurdering af tilgængelighed; lov ikke fuld VoiceOver-understøttelse uden afprøvning.

## 15. Arkitektur og simulation

Hold logik og præsentation adskilt. En væganimation eller et partikelsystem må ikke være kilden til sandheden om en regel.

| Ansvar | Foreslået komponent |
| --- | --- |
| Samlet spilsession og tilstandsskift | `GameSession` |
| Banedata og importvalidering | `MazeDefinition`, `MazeValidator` |
| Aktuelle gangforbindelser og aktøradgang | `MazeGraph`, `TopologyService` |
| Bevægelse, svingkø og tunneler | `GridMovementSystem` |
| Touch/controller til logiske handlinger | `InputRouter` |
| Kamera og bevægelseskomfort | `FirstPersonCameraRig` |
| Fjendernes mål, tilstande og ruter | `GhostDirector`, `GhostAgent` |
| Prikker og obligatoriske tællere | `CollectibleSystem` |
| Kraftprikker og effektprioritet | `PowerEffectSystem` |
| Inventar, banan, olie og boost | `ItemSystem` |
| Karakter-ID, udseende og kobling til AI-rolle | `CharacterDefinition`, `CharacterRoster` |
| Teknisk prik-ID til hjertepræsentation | `CollectiblePresentation` |
| Genveje, reservationer og varsling | `MovingWallSystem` |
| Point og ekstra liv | `ScoreSystem` |
| Levelparametre og seed | `LevelDirector` |
| HUD, lyd, animation og effekter | Præsentationskomponenter via domænehændelser |
| Lagring og migration | `SaveService` |

### Tekniske regler

- Fast simulationstrin, eksempelvis 60 ticks/sekund, med interpoleret rendering. Regler må ikke afhænge af renderingsframerate.
- Alle timere bruger den samme pauserbare simulationstid. Indlæsning, baggrund og pause forbruger ingen boost- eller krafttid.
- Brug kontinuerlig position på en grafkant samt logisk retning. Tilecentre er navigationsknuder; en figur teleporterer ikke fra felt til felt visuelt.
- Processér hele den tilbagelagte strækning, hvis en aktør passerer flere felter i ét tick.
- Find spiller/fjende-kontakt med swept checks eller tilsvarende, inklusive når de bytter plads på en kant. Boost må ikke gøre spilleren immun ved tunneling.
- Brug et seed pr. spilsession og afledte seeds pr. level/system. Gem tilfældighedsgeneratorernes tilstand, hvis en igangværende runde genoptages.
- Brug en versionsmærket topologigraf. Kun relevante ruter skal invalideres ved en ændring.
- Pool samleobjekter, fælder og effekter. Undgå gentagne allokeringer under stabilt gameplay.
- Skift banedata og regler uden at ændre kode i kamera, UI eller meshgenerator.
- Et selvstændigt spøgelseshus-net forbindes til gangnettet med aktørspecifikke dørkanter.

### Hændelsesrækkefølge og samtidige hændelser

Løs bevægelse og kontakter efter tidspunkter langs et simulationstrins bevægelsesforløb. Ved nøjagtig samme kontaktøjeblik har prikopsamling forrang over fjendekontakt, så en kraftprik kan redde spilleren.

Efter alle hændelser på samme tidspunkt: Kontakt med en aktiv forfølger giver fangst og udsætter level-afslutning. Er sidste prik samtidig taget, forbliver tælleren nul. Hvis der stadig er liv, afsluttes banen efter fangstsekvensen uden en ny spilbar tom bane; ellers bliver resultatet Game Over uden levelbonus. Dette er et eksplicit **designvalg** for et sjældent grænsetilfælde.

En kraftprik med udløbstid lig kontaktøjeblikket er udløbet. At spise et spøgelse kan registreres højst én gang, også hvis flere kollisionschecks rammer samme figur. Levelbonus og ekstra liv gives gennem idempotente hændelser.

### Lagring og livscyklus

Brug ordene hjerter, krafthjerter, chancer, fanget og til pause i den spillerrettede tekst. Interne referencebegreber må ikke lække ud som tekster om at dræbe eller spise personer.

Gem indstillinger og rekord straks ved relevante ændringer. Ved en igangværende runde gemmes et versionsmærket snapshot ved pause og når appen går i baggrunden: score, liv, level, prikbitset, spørgsmålstegn, aktørpositioner/retninger, effekter, genstand, vægfaser/reservationer, RNG-tilstand og regelprofil.

Ved tilbagevenden: indlæs i pauset tilstand og lad spilleren fortsætte via en kort nedtælling. Validér snapshot mod banens version. Hvis et snapshot er beskadiget eller ikke kan migreres, bevar rekord/indstillinger og tilbyd et nyt spil. Ændr ikke baggrundstid til aktiv spilletid.

## 16. Ydeevne og test på enheder

**Mål**, som skal dokumenteres ved måling:

- 60 fps på iPhone 13 som referenceenhed; mål CPU- og GPU-frame time omkring eller under 16,7 ms under normal belastning.
- Stabil spilfølelse på en repræsentativ iPad og den ældste valgte understøttede enhed. Brug en særskilt 30-fps-grafikprofil, hvis nødvendigt; simulationens regler bevares.
- Ingen længere hak ved genstandsopsamling, vægåbning, fangst eller levelskifte.
- Kontrolleret batteri- og varmeudvikling ved mindst 15 minutters sammenhængende spil.

Start med enkle materialer, bagt lys på statisk geometri og få dynamiske skygger. Bevægelige vægge skal have belysning, som fungerer i både åben og lukket tilstand. Bagte skygger fra en lukket væg må ikke blive stående, når den åbner.

Render ikke hundredvis af unødvendige realtidslys for prikkerne. Brug emissive materialer, instancing og begrænset bloom. Begræns transparens og overdraw. Reducér først skygger, posteffekter og render scale, hvis GPU-budgettet overskrides.

Tag performanceoptagelser med profiler på fysiske enheder. Simulator og desktop er ikke tilstrækkelig dokumentation for mobilperformance.

## 17. Implementeringsplan i milepæle

### M0 — projekt og beslutninger

Undersøg repository og lokale instruktioner. Fastlæg eksisterende/ny motor, lås versioner og opret en kort plan. Registrér referencebanens rettighedsstatus og afhængigheder for udgivelse. Byg en tom app på en fysisk iPhone tidligt.

**Færdig når:** Projektet kan åbnes reproducerbart, og en minimal build kan køre på en enhed, eller en konkret lokal værktøjsblokering er dokumenteret.

### M1 — spilbar 3D-bane og styring

Importér banedata, generér 3D-vægge/gulv, tilføj kamera, swipekontrol, svingbuffer og sidetunnel. Begynd med simple geometrier. Opret debugvisning af graf og aktørpositioner.

**Færdig når:** Spilleren kan besøge alle prikfelter uden at gå gennem vægge, miste orienteringen på grund af inputfejl eller se over væggene.

### M2 — komplet grundspil

Implementér 244 samlehjerter, krafttilstand, de fire navngivne forfølgere med hver sin AI-rolle, base, score, chancer, fangstsekvens, level-afslutning og progression. Tilføj HUD, pause og genstart. Modellerne kan stadig være tydeligt mærkede pladsholdere på dette trin.

**Færdig når:** En hel runde og et levelskifte kan gennemføres, og Game Over fungerer uden udviklerindgreb.

### M3 — spørgsmålstegn

Implementér det ene inventarfelt, alle tre genstande, deres interaktioner og HUD. Brug de fire faste itempositioner fra banedataene.

**Færdig når:** Banan giver et kort stop, olie giver en midlertidig fartreduktion, boost overhaler alle aktive forfølgere, og ingen af delene ændrer antallet af obligatoriske hjerter.

### M4 — bevægelige vægge

Implementér én genvej først, derefter de øvrige kandidater. Tilføj varsling, passage-reservation, sikker lukning og opdateret AI-navigation.

**Færdig når:** Spilleren kan flygte gennem en tidsbegrænset genvej; aktører kan hverken klemmes eller strande, og en hel bane fortsat kan afsluttes.

### M5 — visuel og lydmæssig kvalitet

Udskift grå geometrier med de fem selvproducerede karikaturer, gennemarbejdede materialer og animationer i den aftalte Pixar/Up-inspirerede stil. Tilføj den komiske fremstilling af fangst og krafttilstand, lyd, haptik, hjælp og indstillinger. Afprøv førstepersonskomfort og balance med børn og voksne i målgruppen.

**Færdig når:** Det ønskede varme 3D-udtryk fungerer i faktisk gameplay med opfyldt performancebudget, også under jagt og vægskift.

### M6 — udgivelseskandidat

Færdiggør save/resume, enhedstest, App Store-materiale, rettighedsafklaring, privatlivsoplysninger og TestFlight-afprøvning. Kontrollér gældende Apple-krav og tredjeparts-SDK'ers bidrag til privatlivsdeklarationer.

**Færdig når:** En gennemtestet build kan indsendes med korrekt dokumentation. App Store-godkendelse afgøres af Apple og kan ikke loves i implementeringen.

## 18. Acceptkriterier og testscenarier

Skriv automatiske tests for regler, som kan ødelægge en runde. Undgå tests, der blot gentager trivielle getters eller konkrete UI-hierarkier.

| ID | Kriterium | Verifikation |
| --- | --- | --- |
| A01 | Banen har 28 × 31 felter, 240 små og fire store prikker | Importertest |
| A02 | Alle obligatoriske prikker kan nås fra start uden adgang til huset | Graf-/flood-fill-test |
| A03 | Tunnelen forbinder siderne med korrekt retning og fart | Integrationstest og enhed |
| A04 | Tidligt swipe udføres ved første lovlige kryds; gammelt input udløber | Simulationstest og touchtest |
| A05 | Kraftprik giver point, sårbarhed og kæde; udløb gør fjenden farlig | Regeltest med simuleret tid |
| A06 | Returnerende eller allerede spist fjende giver ikke gentagne point | Regeltest |
| A07 | Liv går tabt én gang pr. fangstsekvens; prikker og score bevares | Regeltest |
| A08 | Sidste prik giver præcis ét levelskifte og én eventuel bonus | Regeltest |
| A09 | Samtidig kraftprik/kontakt og sidste prik/fangst følger afsnit 15 | Grænsetilfældetest |
| A10 | Ekstra liv gives én gang ved overskridelse af 10.000 | Regeltest, også ved bonus |
| A11 | Fuldt inventar spiser ikke spørgsmålstegnet og blokerer ikke normal prik | Regeltest |
| A12 | Bananskræl giver én stun, overholder immunitet og spærrer ingen gang | Regeltest og visuel test |
| A13 | Boost er hurtigere end alle aktive trusler og springer ingen kontakter over | Hastigheds- og swept-test |
| A14 | Væg lukker ikke på aktører; graf og passageadgang er enige | Integrationstest med reservationer |
| A15 | Alle stabile vægkombinationer bevarer adgang til resterende prikker | Graf-test over kombinationer |
| A16 | En fjende kan følge en genvej og reagere på, at den lukkes | Integrationstest |
| A17 | Pause og baggrund fryser alle timere; save/resume bevarer runden | Livscyklustest på enhed |
| A18 | Samme seed og input giver samme spiludfald inden for samme build | Reproducerbar simulationstest |
| A19 | Regler giver samme udfald ved rendering med 30/60/120 fps | Simulationstest |
| A20 | Kamera, UI og touch fungerer på både iPhone og iPad i begge landscape-retninger | Manuel enhedstest |
| A21 | Level 256 og videre kan indlæses og gennemføres | Progressionstest |
| A22 | De 240 normale og fire store samleobjekter vises som de korrekte hjerter | Præsentationstest |
| A23 | Zelenskyj er helten; Putin/Xi/Kim/Trump er koblet til de aftalte AI-roller | Datavalidering og visuel kontrol |
| A24 | Olie giver 55 % fart i 2,5 sekunder, forbruges ved træf og overholder fælles glideimmunitet | Regeltest |
| A25 | Krafttilstandens puf giver samme point/returregler uden skadesfremstilling | Regeltest og manuel animationstest |
| A26 | Hjerter, HUD-chancer og genstande kan skelnes på form og tekst | Manuel test på iPhone og iPad |
| A27 | Sidste hjerte afslutter banen uden krav om udgang eller portal | Regeltest |
| A28 | Menu, modeller og gameplay opfylder afsnit 13; ingen endelige pladsholdere | Visuel gennemgang og performanceoptagelse |

Test også manuelt: vend om foran en fjende; brug boost gennem et kryds; kast ved en væg; tag to kraftprikker hurtigt; bliv fanget under en åben genvej; gå i baggrunden midt i en lukkevarsling; mist sidste liv samtidig med sidste prik.

## 19. App Store-leverancer

Forbered eget appnavn og ikon, screenshots fra reelt gameplay på iPhone og iPad, en præcis beskrivelse, supportadresse og privatlivspolitik. Beskriv eventuel databrug ud fra den faktiske build og dens SDK'er. Et offlinekoncept betyder ikke automatisk, at alle indbyggede SDK'er er uden databrug.

Udfyld aldersklassifikation ud fra den endelige udformning af tegneseriespænding, jagt og politisk satire. Den visuelle stil alene afgør ikke målgruppe eller aldersklassifikation. Kids Category kræver en særskilt beslutning og gennemgang af de gældende krav; den er ikke automatisk valgt. Beskriv spillets fiktive satire og uskadelige gameplay tydeligt i reviewnoter. [S1]

Dokumentér licenser til modeller, musik, lyde, fonte og tredjepartskode. Dokumentér afklaringen af den originale bane og andre relevante rettigheder før offentlig distribution. Kontrollér Apples regler om efterligninger igen. [S1, S2]

## 20. Startinstruktion, som kan gives direkte til Codex

> Læs hele denne specifikation og repositoryets instruktioner. Undersøg først, hvad der allerede findes. Ved et nyt projekt bruges Unity 6.3 LTS med URP og C#, medmindre det eksisterende projekt giver en konkret grund til et andet valg.
>
> Opret en kort implementeringsplan med milepælene M0–M6, identificér værktøjs- og udgivelsesafhængigheder, og begynd derefter med M0 og M1. Brug dokumentets banedata og valider dem automatisk. Opret et kørbart projekt med rigtige scener og nødvendige konfigurationer; lever ikke kun løse scripts.
>
> Den kreative retning i version 1.1 er bindende: Zelenskyj er den positive hovedfigur i førsteperson; Trump, Putin, Xi og Kim er de fire genkendelige, komiske spøgelseskarikaturer. Vis de 240 små og fire store prikker som hjerter. Spilleren vinder et level ved at samle alle hjerter; en udgang er ikke et mål. Maze of Egos er et foreslået arbejdsnavn, ikke et endeligt navnevalg.
>
> Brug varm, filmisk 3D-animation med Pixar-filmen Up som visuel reference. Dette gælder modeller, materialer, lys og animationer i den kørende app. Der er ingen bomber eller våben. De tre genstande er bananskræl, olieplet og fartboost. Fangst og den klassiske spisefunktion vises som harmløs tegnefilmskomik. Et flot statisk billede eller generiske primitive modeller opfylder ikke den endelige grafiske specifikation.
>
> Prioritér præcis touchstyring, gangnavigation og et behageligt førstepersonskamera. Implementér derefter en hel grundrunde, før bonusgenstande og bevægelige vægge tilføjes. Den færdige hovedoplevelse skal have begge tilføjelser aktiveret.
>
> Hold reglerne adskilt fra rendering, animation og input. Gør balance og baner datastyrede. Genskab ikke historiske hardwarefejl. Dokumentér alle afvigelser fra specifikationen og alle bevidste forenklinger af originalreglerne.
>
> Brug egne eller korrekt licenserede assets. Bevar kravet om original referencebane synligt, men betragt ikke dette dokument som rettighedstilladelse til en offentlig udgivelse.
>
> Arbejd i små, sammenhængende ændringer. Kør relevante tests og dokumentér, hvad der faktisk er kørt på enhed. Hvis et værktøj mangler, så færdiggør det uafhængige arbejde og angiv den præcise blokering; påstå ikke, at en build eller test er gennemført. Stop ikke efter en plan, når implementering er mulig.

## 21. Kilder og brug af referencer

Kontrolleret 21. oktober 2026. Kilder understøtter referenceoplysninger og platformskrav. Styring, karikaturroller, hjertepræsentation, målestok, genstandstal, vægtider, sikkerhedsregler, arkitektur og milepæle er projektets designvalg.

- **[S1] Apple — App Review Guidelines, især 4.1 og 5.2:** [Åbn kilde](https://developer.apple.com/app-store/review/guidelines/)
- **[S2] Apple — Preventing copycat and impersonation rejections:** [Åbn kilde](https://developer.apple.com/help/app-review/guideline-reference/4-1-prevent-copycat-rejection/)
- **[S3] Apple — Upcoming Requirements:** [Åbn kilde](https://developer.apple.com/news/upcoming-requirements/)
- **[S4] Basic Fun — licenseret PAC-MAN-produktmanual:** [Åbn kilde](https://www.basicfun.com/wp-content/uploads/2023/03/9530_ArcadeClassics_Pacman4C_Instructions_CustomerService.pdf) — kilde til den overordnede spilcyklus, kraftprikker og ekstra liv; ikke dokumentation for alle detaljer i arkadeudgaven.
- **[S5] Jamey Pittman — The Pac-Man Dossier:** [Åbn kilde](https://pacman.holenet.info/) — uafhængig analyse af originalspillet, herunder prikantal, point, fjender og progression; ikke en officiel Namco-specifikation. De viste referenceværdier er et udgangspunkt, ikke en påstand om frame-præcis emulation.
- **[S6] Unity — URP requirements and compatibility:** [Åbn kilde](https://docs.unity.com/en-us/engine/6000.3/manual/render-pipelines/universal-render-pipeline/requirements)
- **[S7] Unity — Mobile game optimization in Unity 6:** [Åbn kilde](https://unity.com/resources/mobile-game-optimization-in-unity-6)
