# Fakultet

U ovom repozitorijumu nalaze se projekti koje sam radio tokom studija.  
Fokus je na razumevanju principa rada, a ne samo implementaciji.
Dodatni projekti koji će biti obrađeni do kraja studija:
  - Diplomski rad (CodeSys)
  - Primena DSP u upravljanju (Python)
  - Analiza slike u biomedicini (MatLab)
  - Projektovanje SCADA sistema (C#)
---

## Projekti

### 1. SpaceInvadersRL

- Grupni projekat
- Izrada u Python
  ## Implementacija:
    - Q-learning
    - SARSA algoritam
    - Kreiranje okruženja za igru
    - Dokumentovanje učenja kroz grafike i trendove
 
  ## Metodologija
    Implementirani su reinforcement learning algoritmi:
    - Q-learning (off-policy)
    - SARSA (on-policy)

    Agent uči optimalnu strategiju kroz interakciju sa okruženjem koristeći reward sistem.

  ## Reward sistem
    - +X poena za uništenog protivnika  
    - -X za gubitak života  
    - mali negativan reward za neaktivnost (da se podstakne kretanje)

  ## Izazovi
  - balansiranje exploration/exploitation
  - spor proces učenja
  - definisanje optimalnog reward sistema

  ## Rešenja
  - korišćenje ε-greedy strategije  
  - podešavanje parametara kroz eksperimente  

  ## Moj doprinos
    - Implementacija Q-learning i SARSA algoritama  
    - Dizajn reward sistema  
    - Vizualizacija procesa učenja kroz animaciju  
    - Analiza performansi algoritama  
  
  ## Svrha projekta:
    - Projektovanje funkcionalnog agenta pomoću učenja sa podsticajem koji će uspešno da igra igricu formata Space Invaders. Proverava se koja od korišćenih metoda daje bolje rezultate. Ispostavlja se, kao što je i očekivano, da Q učenje brže konvergira dok SARSA daje stabilnije rezultate s manjom varijansom.
📎 Napomena: Dokumentacija je starija jer su naknadno tražene izmene projekta.

### 2. Centrifugalni filter sa frekventnim regulatorom

- Invididualni projekat
- AutoCAD
  ## Implementacija:
    - Izrada crteža u AutoCAD
    - Korišćenje layer-a za organizaciju elemenata (instalacije, oznake, dimenzije)
    - Precizno kotiranje i skaliranje crteža
    - Korišćenje standardnih simbola i oznaka (P&I)
    - Optimizacija preglednosti crteža (lineweight, raspored elemenata)
  
  ## Doprinos:
    - Samostalna izrada crteža
    - Organizacija strukture projekta
    - Provera tačnosti u odnosu na indurstrijske standarde
  
  ## Svrha projekta:
    - Izrada tehničkog crteža instalacije u skladu sa zadatim projektantskim zahtevima i standardima. Cilj je bio precizno modelovanje sistema i jasno prikazivanje svih relevantnih elemenata za izvođenje
    - Upoznavanje s okruženjem AutoCAD
    - Veza na EPlan projekat


### 3. Ožičenje postrojenja centrifugalnog filtra sa frekventnim regulatorom

- Invidivualni projekat
- EPLAN
## Implementacija:
  - Izrada šema ožičenja u EPLAN Electric P8
  - Korišćenje standardizovanih simbola i komponenti
  - Definisanje veza između elemenata (kablovi, terminali, uređaji)
  - Organizacija projekta kroz strukturu stranica i oznaka
  - Automatsko numerisanje i označavanje komponenti
  - Generisanje liste materijala (BOM) i prikaza konekcija
  
  ## Doprinos:
    - Samostalna izrada
    - Logičko povezivanje svih komponenti sistema
    - Optimizacija preglednosti
    - Provera ispravnosti i konzistentnosti
    - integracija sa PLC sistemom
  
  ## Svrha projekta:
    - Izrada kompletne električne dokumentacije za ožičenje sistema, sa ciljem jasnog i standardizovanog prikaza električnih veza radi lakše implementacije i održavanja

📎 Napomena: U prilogu je samo odštampan PDF elektroinstalacija i plana s obzirom da je projekat rađen na virutelnoj mašini u prostorijama fakulteta, gde je i branjen, i da originalan fajl nemam sačuvan već samo izveštaj iz EPLAN.

### 4. Digitalna obrada signala sa mrežnom smetnjom

## Implementacija

Projekat je realizovan u Pythonu kroz Jupyter Notebook.  
Korišćen je sopstveni mono govorni snimak u WAV formatu, frekvencije odabiranja 16 kHz.

U početnom delu projekta izvršeno je:

- učitavanje audio signala,
- normalizacija signala,
- prikaz osnovnih informacija o snimku,
- prikaz govornog signala u vremenskom domenu.

U nastavku projekta biće dodata mrežna smetnja frekvencije 50 Hz, izvršena FFT analiza i projektovani digitalni filteri za njeno potiskivanje.

## Doprinos

Projekat prikazuje osnovne korake digitalne obrade audio signala u Pythonu.  
Poseban akcenat je na analizi realnog govornog signala i razumevanju uticaja mrežne smetnje na signal.

Kroz projekat se obrađuju sledeći koncepti:

- rad sa WAV audio fajlovima,
- vremenska analiza signala,
- frekvencijska analiza pomoću FFT-a,
- modelovanje sinusne smetnje,
- projektovanje i poređenje digitalnih filtera.

## Svrha projekta

Svrha projekta je da se prikaže kako se realan govorni signal može analizirati i obraditi pomoću metoda digitalne obrade signala.

Projekat služi kao praktičan primer primene teorijskih znanja iz obrade signala, posebno u kontekstu uklanjanja mrežne smetnje od 50 Hz iz audio signala.
