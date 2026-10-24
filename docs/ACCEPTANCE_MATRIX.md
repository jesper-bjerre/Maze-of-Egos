# Acceptkriterier — alle-mod-alle-plan og build 12-historik

## Planlagt konkurrence — 24. oktober 2026

Quizzen med 24 produktvalg er afsluttet. Regler og arbejdsblokke N1–N5 står i
[implementeringsplanen](IMPLEMENTATION_PLAN.md). **Alle C-kriterier nedenfor
er endnu ikke implementeret/verificeret.** Der er ikke lavet en ny build.
De historiske A-kriterier længere nede kan ikke bruges som accept af ændrede
regler, kamera eller den nye skærmorienterede styring.

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
