# Analiza deformacije krvnog suda

MATLAB projekat za automatsku analizu promene prečnika krvnog suda na video-snimku.

Aplikacija omogućava izbor dela video-snimka i regiona od interesa, detekciju gornje i donje ivice krvnog suda, računanje prečnika kroz vreme i grafički prikaz rezultata.

## Korišćene tehnologije

* MATLAB R2023a
* App Designer
* Image Processing Toolbox
* VideoReader
* MATLAB grafičke i numeričke funkcije

## Funkcionalnosti

Aplikacija omogućava:

* učitavanje video-snimka;
* reprodukciju i pauziranje videa;
* pregled pojedinačnih frejmova pomoću slidera;
* izbor početnog i krajnjeg frejma;
* izbor pravougaonog regiona od interesa — ROI;
* automatsku detekciju gornje i donje ivice krvnog suda;
* računanje vertikalnog prečnika;
* računanje prečnika duž normale na lokalnu tangentu;
* prikaz promene prečnika kroz vreme;
* čuvanje rezultata u `.mat` fajl;
* ponovno učitavanje prethodno sačuvanih rezultata.

## Pokretanje aplikacije

1. Otvoriti MATLAB R2023a.
2. Postaviti folder projekta kao trenutni MATLAB folder.
3. Otvoriti fajl:

```text
Aplikacija/AnalizaKrvnogSuda.mlapp
```

4. Pokrenuti aplikaciju klikom na dugme **Run** u App Designer-u.
5. U aplikaciji učitati video-snimak.
6. Izabrati početni i krajnji frejm.
7. Izabrati ROI koji obuhvata analizirani deo krvnog suda.
8. Pokrenuti obradu.
9. Po potrebi sačuvati dobijene rezultate.

## Osnovni tok algoritma

Obrada se sastoji iz sledećih koraka:

1. Učitavanje izabranog frejma video-snimka.
2. Izdvajanje prethodno definisanog ROI regiona.
3. Pretvaranje slike u sivu sliku.
4. Filtriranje median i Gaussian filtrom.
5. Procena položaja krvnog suda na osnovu intenziteta piksela.
6. Računanje vertikalnog gradijenta slike.
7. Detekcija gornje i donje ivice krvnog suda.
8. Praćenje ivica od centralne kolone ka levoj i desnoj strani.
9. Zaglađivanje detektovanih kontura.
10. Računanje vertikalnog prečnika.
11. Računanje prečnika duž normale.
12. Čuvanje i grafički prikaz rezultata.

## Detekcija ivica

Krvni sud je svetliji od pozadine.

Gornja ivica predstavlja prelaz sa tamne pozadine na svetli krvni sud, dok donja ivica predstavlja prelaz sa svetlog krvnog suda na tamnu pozadinu.

Za detekciju se koristi vertikalni gradijent slike:

```matlab
kernel = [-1; 0; 1] / 2;
```

Za gornju ivicu traži se lokalni maksimum gradijenta, a za donju ivicu lokalni minimum.

Detekcija počinje u centralnom delu ROI regiona, nakon čega se ivice prate ka levoj i desnoj strani. Položaj naredne tačke ograničen je položajem prethodno pronađene tačke, čime se smanjuje mogućnost prelaska na druge objekte i smetnje u slici.

## Računanje prečnika

### Vertikalni prečnik

Vertikalni prečnik računa se kao razlika položaja donje i gornje ivice u istoj koloni:

```text
D(x) = y_donja(x) - y_gornja(x)
```

Kao reprezentativna vrednost jednog frejma koristi se medijana validnih prečnika.

### Prečnik duž normale

Za svaku tačku gornje ivice procenjuje se lokalna tangenta.

Na osnovu nagiba tangente formira se normalni pravac koji se prati do preseka sa donjom ivicom.

Ova metoda predstavlja geometrijski pravilniju procenu širine kod zakrivljenih delova krvnog suda.

Kod skoro horizontalnog krvnog suda vertikalni i normalni prečnik imaju veoma slične vrednosti.

## Struktura projekta

```text
Obrada slike
│
├── Aplikacija
│   └── AnalizaKrvnogSuda.mlapp
│
├── Skripte
│   ├── krvni_sud_osnova.m
│   ├── izdvoji_testne_frejmove.m
│   ├── detektuj_ivice_krvnog_suda.m
│   ├── test_detekcija_ivica.m
│   ├── obradi_video_krvnog_suda.m
│   ├── pokreni_obradu_celog_videa.m
│   ├── proveri_kontrolne_frejmove.m
│   ├── izracunaj_precnik_normalom.m
│   └── test_precnika_normalom.m
│
├── Video
├── Rezultati
├── Primeri
├── README.md
└── PROJEKAT_STATUS.md
```

## Rezultati

Algoritam je testiran na video-snimku od 4554 frejma.

Za sve frejmove dobijene su numerički validne vrednosti vertikalnog i normalnog prečnika.

Vizuelnom proverom kontrolnih frejmova potvrđeno je da detektovane konture uglavnom prate stvarne ivice krvnog suda.

Kod stabilnog dela video-snimka vertikalni prečnik je najčešće približno jednak ili malo veći od prečnika izračunatog duž normale.

## Ograničenja

Rezultati mogu biti manje stabilni kada su prisutni:

* pomeranje ili drmanje kamere;
* zamućenje slike;
* slab kontrast između krvnog suda i pozadine;
* objekti i mehurići u blizini ivica;
* značajno pomeranje krvnog suda izvan izabranog ROI regiona.

Zbog toga aplikacija omogućava izbor stabilnog vremenskog intervala i odgovarajućeg ROI regiona.

## Čuvanje rezultata

Rezultati se čuvaju u MATLAB `.mat` fajlu.

Sačuvana struktura sadrži:

* početni i krajnji frejm;
* vremensku osu;
* položaje gornje i donje ivice;
* vertikalne prečnike;
* prečnike izračunate duž normale;
* medijane prečnika po frejmu;
* podatke o ROI regionu;
* informacije o obrađenom video-snimku.

## Napomena o GitHub repozitorijumu

Video-snimci, generisani `.mat` fajlovi i izdvojeni PNG frejmovi nisu postavljeni na GitHub zbog veličine i načina organizacije projekta.

Folderi `Video`, `Rezultati` i `Primeri` zadržani su u strukturi projekta pomoću `.gitkeep` fajlova.
