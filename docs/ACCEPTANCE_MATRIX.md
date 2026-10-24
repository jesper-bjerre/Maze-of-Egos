# Acceptkriterier — konkurrenceversionen, build 17

## Build 17 — karakterkort, plaskradius, korte runder og resultatportrætter

| Krav | Teknisk dækning |
| --- | --- |
| Hele karakterkortet kan vælges | Native syntetiske tryk separat på foto og navn for alle fem figurer; Mac og fysisk iPhone i begge skærmretninger består |
| Vandballon rammer omkring landingen | Indenfor/inklusive kanten af én feltradius rammes alle ubeskyttede modstandere; udenfor rammes ingen; træf én gang ved landing, korrekt genindlæsning og kort synligt plask |
| Sidste hjerte afslutter runden | Sidste point tælles før afgørelse; tidlig slutårsag, præcis én pokaltildeling og videreførelse af faktiske ticks til næste runde består; native save/continue består på Mac |
| Foto, navn og point ved medaljerne | Portræt med navn og point nedenunder, alle delte placeringer, udvidet guldrække ved mindst tre guldvindere; samlet pokaltabel nedenunder |
| Ældre gemte kampe | V1/V2/V3→V4; faktisk Unity JSON og uændrede aktuelle positioner/point; korte runders starttick og slutårsag gemmes |

65 klassiske + 34 grundchecks + 23 arenachecks = 122 spilchecks består. Slutresultater for enheder dokumenteres i [testrapporten](TEST_REPORT.md). Manuel oplevelse er fortsat åben.

## Historisk build 16

| Krav | Verifikation |
| --- | --- |
| Tilfældig rundestart, kun central KO-genstart | 100 seeds, adskilte lovlige starter, alle med farbar retning; gemt RNG giver samme næste runde; eksisterende KO-regression består |
| Hjerter kommer ikke igen i runden | Fuld produktionsrunde med monotont faldende hjerteantal, tømt bane forbliver tom indtil 120 s; næste runde har 240 |
| Minikort viser hjerter og andre spillere | Nordvendt UI med resterende hjerter, fem nummer-/farvemarkører, hvid ramme om mennesket; Visuelt kontrolleret på Mac i tre formater og fysisk iPhone i begge landscapes |
| Guld/sølv/bronze for runden | Rank fra rundepoint, delte placeringer og spring over optagne pladser; visuel kontrol af særskilte medaljer og femdelte guldvindere |
| Samlet stilling under rundens medaljer, tydelige pokaler | Pokaler før samlede point, tre synlige guldpokalpladser og antal for hver spiller |
| Ældre gemte kampe bevares | V1/V2→V3, faktiske Unity JSON-data, ingen flytning af aktive spillere eller nulstilling af hjerter/point |

117 spilchecks består (65 + 31 + 21). Native slutresultater står i [testrapporten](TEST_REPORT.md). Manuel spiloplevelse er ikke accepteret af automatiske checks.

## Historisk teknisk dækning — build 15

N1–N4 er implementeret i konkurrenceflowet. N5's lange fysiske Release-måling
og normale installation består. Manuel gameplayaccept står åben. Den opdaterede [testrapport](TEST_REPORT.md)
er autoritativ for faktiske platformresultater.

| Kriterier | Ny dækning i build 15 | Resterende accept |
| --- | --- | --- |
| C01–C03 | Tæt lokalt kamera, fem valgbare importerede figurer, skærmretning og samtidig syntetisk joystick/genstandsinput i begge håndlayouts; fire aktive computerfigurer | Manuel vurdering af udsyn og fingerspil |
| C04–C07 | 24 grundchecks plus fulde kampe med produktions-AI: timer, point/pokaler, delte sejre, genopfyldning og erstatningsinventar | Balance vurderes ved håndført kamp |
| C08–C10 | Alle fem genstande fra hver figur; varsling/rækkevidde/vægge, KO, point-/inventarbevaring, 300 ticks ude, 180 ticks beskyttet og eget angreb/boost; samtidig gensidigt KO og optaget midte | Visuel og taktisk læsbarhed på telefon |
| C11 | Modellen modtager lokale observationer; kontrafaktiske usete positioner/ressourcer ændrer ikke beslutningen; hukommelse og reaktionstilstand gemmes; fem fulde kampe og ti ekstra stressrunder | Sjov og passende sværhedsgrad |
| C12 | Gangkantreservation fra N2 udvidet til flyvende angreb/fælder; lukning udskydes, mens et projektil skal gennem passagen | Ingen kendt teknisk rest |
| C13 | Faktisk Unity JSON/backup/migrering og fysisk OS-baggrund/forgrund med identisk frosset kamp/AI-checkpoint; normal lagring er adskilt fra classic | Ingen kendt teknisk rest |
| C14 | Original billedmenu, separat figurvalg, engelsk femspiller-HUD, hjælp, indstillinger, runde- og kampresultater med flere vindere | Endelig produkt-/grafikvurdering |
| C15 | 900,003 s Release på iPhone: 59,964 fps/p95 18 ms, normal temperatur; normal signeret build installeret/startet uden testflag; begge gameplay-landscapes og byte-bevarede gamle filer består | Håndført hel kamp og gameplayaccept; fysisk iPad er fravalgt |

De følgende N2-/N1-tabeller er den tidligere milepælsevidens. De erstattes af
ovenstående for den aktive model og bevares som historik.

## Historisk N2/N1-status før build 15

Quizzen med 24 produktvalg er afsluttet. Regler og arbejdsblokke N1–N5 står i
[implementeringsplanen](IMPLEMENTATION_PLAN.md). **N1 er implementeret i build
13 som perspektivprototype, med tættere zoom i build 14.** C01/C02 har teknisk dækning nedenfor;
manuel vurdering af kamera og styring afventer. **N2's Core-del er nu
implementeret og verificeret** som angivet nedenfor. C03–C15 har stadig
produkt-/integrationsdele til N3–N5; de er ikke fuldt accepteret.
De historiske A-kriterier længere nede kan ikke bruges som accept af ændrede
regler, kamera eller den nye skærmorienterede styring.

| N2-område | Aktuel teknisk evidens | Resterende integration/accept |
| --- | --- | --- |
| C03 | Fem lige Core-deltagere, alle mulige menneske-id'er, samme fart/baseadgang/input; alle fem kan vinde en hel kamp | Figurvalg, produktionens AI og runtimeflow i N4 |
| C04–C05 | 120 sekunder, præcis frist, separate/bankede point, delte runde-/kampsejre, tredje pokal og engangstildeling efter filindlæsning består | N3-test af rundeslut under KO/angreb; UI og fuld spilleroplevelse N4/N5 |
| C06–C07 | Ens dynamiske hjerter, små grupper uden varsling, cap/lovlige felter, seedede samtidige opsamlinger, genopfyldning, erstatningsinventar og ny kasseindgang består | Visning, AI-observationer og balance på enhed N4/N5 |
| C10/C12 | Ufarlig passage mellem aktører og automatisk vægreservation af alle fems aktuelle gangkanter består | KO/genstart/beskyttelse og reservation af projektiler/fælder N3 |
| C13 | Nyt versioneret snapshot, atomisk backup, gamle filer bevaret, frosne Core-timere og paused restore består med Unitys faktiske JSON | N3/N4-felter og tilslutning til appens OS-/CONTINUE-flow; fysisk samlet test N5 |

N2 har **24 konkurrencechecks og 65 klassiske regressionstests** samt fem
fulde scriptede kampe over 41 runder/295.200 ticks. Unity-editorvalidering
og managed-kompilering består. [Aktuelt testbevis](TEST_REPORT.md).
Scriptede inputs er den aftalte N2-testafgrænsning, ikke den færdige AI.
Installeret build 14 er fortsat N1; der er ikke lavet en N2-enhedsbuild.

N1 tilføjer Core-tests for projicerede retninger, én kommando pr. udslag og et
ventende verdensretningsvalg. Den native kontrol afprøver lokalt kamera, hel
spillerfigur, alle fire retninger uafhængigt af spillerens orientering,
vægstop, tunnel begge veje, sving efter slip, samtidig genstandsbrug og
pause-/kastanimation. Faktiske platformresultater og billeder findes i
[testrapporten](TEST_REPORT.md). De gamle forfølgere er bevaret i denne
prototype; testen er ikke accept af fem konkurrerende deltagere eller fair AI.

**Feedback efter build 13:** Brugeren ønsker kameraet markant tættere på
karakteren. Build 14 reducerer den ortografiske halve billedhøjde fra 8,5 til
4,5 meter (cirka 1,89 gange større figurer i hver billedretning). Mac-kontrollen
består i alle tre formater med 43 PNG'er pr. format. Fysisk iPhone 13 består
med 47 PNG'er inklusive gameplay i begge landscape-retninger. Det er ikke
manuel accept af den nye afstand; [testbevis](verification/2026-10-24-build14-native-checks.json).

| N1-kriterium | Teknisk evidens, build 13 | Resterende accept |
| --- | --- | --- |
| C01 | Native PASS på Mac 1600×900, 2400×1080 og 1600×1200 samt fysisk iPhone 13 2532×1170 i begge landscape-retninger; hel rigget spiller i lokalt udsnit, fast vinkel og lav geometri | Manuel vurdering af udsyn, læsbarhed og mulighed for at reagere |
| C02 | Nye Core-tests og native syntetisk touch: alle fire projicerede retninger fra modsat gangretning, svingkø efter slip, vending, vægstop, tunnel begge veje og samtidig genstandsbrug i begge håndlayouts | Ny håndført vurdering af skærmstyringen på iPhone; build 12-feedback gælder de gamle relative sving |

| ID | Det skal verificeres i den nye model |
| --- | --- |
| C01 | Lokalt, højt 3D-kamera med fast retning; figurer og nabogange kan ses, hele banen vises ikke; udsyn består i begge landscape-retninger og tre desktopformater |
| C02 | Automatisk midterlinjegang og gemt sving efter skærmens gangretninger; vending, vægstop, tunnel og samtidig genstandsbrug |
| C03 | Frit figurvalg mellem fem, fire AI og fælles regler/evner; ingen antagelse om at mennesket altid er Zelenskyy eller aktør 0 |
| C04 | 120-sekunders runder, adskilte runde-/samlede point og korrekt frist; ingen afslutning ved sidste hjerte eller KO |
| C05 | Alle lige rundevindere får en pokal; første tredje pokal afslutter kampen; samlede point og derefter delt kampsejr afgør samtidig tredje pokal; ingen dobbelttildeling ved resume |
| C06 | Ens hjerter uden store krafthjerter/krafttilstand; små nye grupper i skiftende områder uden varsling; lovlige felter, ingen duplikatopsamling eller fast fordel ved samtidighed |
| C07 | Kasser dukker op igen; én genstand pr. deltager, og seneste opsamling erstatter den gamle for menneske og AI |
| C08 | Banan giver glid, olie fartnedsættelse og boost fart; ens effekt for alle, ingen knockout fra banan/olie |
| C09 | Vandballon/lagkage varsles, følger gangretning, har begrænset rækkevidde og stopper ved vægge; ét gyldigt træf giver ét knockout |
| C10 | Point bevares ved KO, genstart fra midten efter 5 sekunder, 3 sekunders beskyttelse afsluttes ved eget angreb, kropskontakt er ufarlig og spærrer ikke |
| C11 | AI bruger lokal observation/hukommelse; samme observation og seed giver samme beslutning trods forskellige usete modstanderpositioner; alle fire konkurrerer også indbyrdes |
| C12 | Automatiske genveje bevarer sikkerhed for alle fem deltagere, begge ender af gangkanter og relevante kast/fælder |
| C13 | Versioneret konkurrencesnapshot, atomisk backup og frosne runde-/effekttimere ved pause/baggrund/resume; gamle saves/indstillinger bevares uden at blive blandet ind i kampregnskabet |
| C14 | Billedbaseret startmenu bevares; figurvalg sker separat efter PLAY; engelsk HUD/hjælp med timer, point/pokaler, KO og flere vindere; ingen gamle liv/krafthjertekrav i den nye oplevelse |
| C15 | Frisk native/enheds- og mindst 900-sekunders Release-måling af den nye model; manuel iPhone-vurdering af udsyn, reaktionstid, taktik og en hel kamp. Fysisk iPad er fortsat fravalgt denne omgang |

## Historisk testdækning — build 12

Status genbesøgt 24. oktober 2026. Build 12 er testet 21. oktober; kilderne
matcher fortsat det registrerede inventar ved genoptagelsen. Ingen ny
gameplaytest er udført. Alle spillerrettede tekster er engelske fra build 11. Automatisk evidens er adskilt fra menneskelig
produktaccept. A04 følger den aftalte joystickbeslutning i implementeringsplanen.

| ID | Konkret evidens | Resterende accept |
| --- | --- | --- |
| A01 | 28×31 og 240+4 i import-/repositorytest | Ingen teknisk rest |
| A02 | Grafadgang og kontinuerlig traversal af alle 244 i Core | Manuel banefornemmelse |
| A03 | Tunnelregler og native geometri | Manuel oplevelse af overgang |
| A04 | Automatisk midterlinjegang, relativ svingkø, vending, stop ved væg, slip fortsætter; syntetisk joystick/genstand i begge layouts; brugeren svarer “Styringen fungerer godt” om build 12 den 24. oktober | Styringsoplevelsen accepteret på iPhone; [manuelt bevis](verification/2026-10-24-build12-manual-controls.json) |
| A05 | Kraft, kæde, udløb og kontaktgrænser i Core | Visuel oplevelse |
| A06 | Retur giver ikke dobbeltpoint | Ingen teknisk rest |
| A07 | Én chance pr. fangst; score/hjerter bevaret | Ingen teknisk rest |
| A08 | Sidste hjerte/bonus/levelskifte i Core og native fixture | Hel håndspillet runde |
| A09 | Samtidige kontakter og sidste chance/hjerte i Core | Ingen teknisk rest |
| A10 | 10.000-grænsen inklusive bonus i Core | Ingen teknisk rest |
| A11 | Fuldt inventar bevarer spørgsmålstegn og tillader hjerte | Ingen teknisk rest |
| A12 | Banan, landing, immunitet, væg, tunnel og frosset kast; native arm/bue | Manuel kastfornemmelse |
| A13 | Boostfart, swept-kontakter og native genstandsbrug | Manuel fart/læsbarhed |
| A14 | Reservation af aktør/fælde/kast, forsinket lukning og native vægcyklus | Manuel læsbarhed |
| A15 | Alle otte stabile vægkombinationer i graf-test | Ingen teknisk rest |
| A16 | AI-returruter og ændret topologi; levende AI i lang test/stress | Manuel taktisk oplevelse |
| A17 | Snapshot v3 med svingkø, backup, v1/v2-migration, native mid-flight resume og reel OS-baggrund | Forskellige uventede OS-afbrydelser kan afprøves yderligere |
| A18 | Fast seed/input reproducerer 60 sekunders Core-forløb | Ingen teknisk rest |
| A19 | Samme faste ticks ved 30/60/120 renderfrekvens i Core | Ingen teknisk rest |
| A20 | Fysisk iPhone i begge landscape-retninger og safe area; Mac i tre formater | Fysisk iPad, fravalgt denne omgang |
| A21 | Progression over level 255 og sen power-varighed i Core | Ikke håndspillet til level 256 |
| A22 | Præcis 244 hjerteobjekter, pink/guld 3D-form og native billeder | Manuel småskærmslæsbarhed |
| A23 | Rolle-ID-test, fem uændrede referencer og fem importerede modeller | Endelig genkendelighed |
| A24 | Olie: 55 %, 2,5 sekunder, trail og fælles immunitet | Manuel effektoplevelse |
| A25 | Samme point/returregler; importeret Puff og miniature/boble | Manuel animationsaccept |
| A26 | Geometriske ikoner med engelsk tekst; native screenshots og teksthøjdekontrol under da-DK kultur | Manuel tilgængelighed/læsbarhed på iPhone/iPad |
| A27 | Sidste hjerte afslutter uden portal i Core/native | Ingen teknisk rest |
| A28 | Alle rigs/materialer i app; native billeder og lang performance | Åben: filmisk slutkvalitet, ansigter/hår, have og kunstnerisk review |

62 regel-/måle-/animationstests og den separate 540.000-ticks stresstest er
beskrevet i [testrapporten](TEST_REPORT.md). Teknisk bestået er ikke en
påstand om, at åbne manuelle eller kunstneriske kriterier er accepteret.

Build 12's startmenu er særskilt kontrolleret på Mac i flere formater og fysisk
iPhone: det uændrede baggrundsbillede, tre engelske handlinger til højre,
Help nederst, Continue med/uden save og ingen ekstra tekst eller figurer.
Brugerens vurdering af den nye menu er fortsat åben.
