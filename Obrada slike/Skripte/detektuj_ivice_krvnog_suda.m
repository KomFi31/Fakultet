function [gornjaIvica, donjaIvica, precnik, detalji] = ...
    detektuj_ivice_krvnog_suda(roiSlika, udeoMargine)
% DETEKTUJ_IVICE_KRVNOG_SUDA
% Automatski pronalazi gornju i donju ivicu svetlog krvnog suda
% korišćenjem vertikalnog gradijenta i praćenja kontinuiteta.
%
% Ulazi:
%   roiSlika      - izdvojeni region krvnog suda
%   udeoMargine   - deo širine slike koji se izostavlja sa leve
%                   i desne strane, npr. 0.18 znači 18%
%
% Izlazi:
%   gornjaIvica   - pozicija gornje ivice za svaku kolonu
%   donjaIvica    - pozicija donje ivice za svaku kolonu
%   precnik       - vertikalno rastojanje između ivica
%   detalji       - pomoćni podaci za prikaz i proveru

if nargin < 2
    udeoMargine = 0.22; % Pri testiranju 0.22 se pokazalo malo preciznije
end

if udeoMargine < 0 || udeoMargine >= 0.45
    error('Udeo margine mora biti između 0 i 0.45.');
end

%% 1. Priprema slike

roiGray = im2gray(roiSlika);
roiDouble = im2double(roiGray);

% Uklanjanje sitnog impulsnog šuma i blago prostorno zaglađivanje
obradjenaSlika = medfilt2(roiDouble, [3 3], 'symmetric');
obradjenaSlika = imgaussfilt(obradjenaSlika, 1.2);

[brojRedova, brojKolona] = size(obradjenaSlika);

%% 2. Ograničavanje analiziranog dela po širini

brojKolonaMargine = round(udeoMargine * brojKolona);

pocetnaKolona = brojKolonaMargine + 1;
krajnjaKolona = brojKolona - brojKolonaMargine;

if pocetnaKolona >= krajnjaKolona
    error('Izabrana margina ostavlja premalo prostora za analizu.');
end

koloneZaAnalizu = pocetnaKolona:krajnjaKolona;

%% 3. Gruba procena vertikalne pozicije krvnog suda

% Srednji intenzitet svakog reda u centralnom delu ROI slike
profilRedova = mean( ...
    obradjenaSlika(:, koloneZaAnalizu), ...
    2);

profilRedova = movmean(profilRedova, 9);
profilNorm = mat2gray(profilRedova);

% Otsu prag odvaja svetli krvni sud od tamnije pozadine
pragProfila = graythresh(profilNorm);
maskaProfila = profilNorm > pragProfila;

% Traženje najdužeg neprekidnog svetlog regiona
promenaMaske = diff([false; maskaProfila; false]);

poceciRegiona = find(promenaMaske == 1);
krajeviRegiona = find(promenaMaske == -1) - 1;

if isempty(poceciRegiona)
    error('Nije pronađen svetli region krvnog suda.');
end

duzineRegiona = krajeviRegiona - poceciRegiona + 1;

[~, indeksNajduzeg] = max(duzineRegiona);

grubaGornjaIvica = poceciRegiona(indeksNajduzeg);
grubaDonjaIvica = krajeviRegiona(indeksNajduzeg);

%% 4. Vertikalni gradijent

% Pozitivan gradijent:
% tamna pozadina -> svetli krvni sud, odnosno gornja ivica.
%
% Negativan gradijent:
% svetli krvni sud -> tamna pozadina, odnosno donja ivica.

kernelY = [-1; 0; 1] / 2;

gradijentY = imfilter( ...
    obradjenaSlika, ...
    kernelY, ...
    'replicate', ...
    'corr');

gradijentY = imgaussfilt(gradijentY, 1);

%% 5. Inicijalna detekcija u srednjoj koloni

srednjaKolona = round( ...
    (pocetnaKolona + krajnjaKolona) / 2);

poluVisinaPocetnePretrage = 25;

opsegGornje = ...
    max(2, grubaGornjaIvica - poluVisinaPocetnePretrage): ...
    min(brojRedova - 1, ...
        grubaGornjaIvica + poluVisinaPocetnePretrage);

opsegDonje = ...
    max(2, grubaDonjaIvica - poluVisinaPocetnePretrage): ...
    min(brojRedova - 1, ...
        grubaDonjaIvica + poluVisinaPocetnePretrage);

[~, lokalniIndeks] = max( ...
    gradijentY(opsegGornje, srednjaKolona));

pocetnaGornja = opsegGornje(lokalniIndeks);

[~, lokalniIndeks] = min( ...
    gradijentY(opsegDonje, srednjaKolona));

pocetnaDonja = opsegDonje(lokalniIndeks);

%% 6. Praćenje ivica od sredine ka desnoj strani

gornjaIvica = nan(1, brojKolona);
donjaIvica = nan(1, brojKolona);

gornjaIvica(srednjaKolona) = pocetnaGornja;
donjaIvica(srednjaKolona) = pocetnaDonja;

maksimalniKorak = 7;

for kolona = srednjaKolona + 1:krajnjaKolona

    prethodnaGornja = round(gornjaIvica(kolona - 1));

    redoviGornje = ...
        max(2, prethodnaGornja - maksimalniKorak): ...
        min(brojRedova - 1, ...
            prethodnaGornja + maksimalniKorak);

    [~, indeks] = max(gradijentY(redoviGornje, kolona));

    gornjaIvica(kolona) = redoviGornje(indeks);

    prethodnaDonja = round(donjaIvica(kolona - 1));

    redoviDonje = ...
        max(2, prethodnaDonja - maksimalniKorak): ...
        min(brojRedova - 1, ...
            prethodnaDonja + maksimalniKorak);

    [~, indeks] = min(gradijentY(redoviDonje, kolona));

    donjaIvica(kolona) = redoviDonje(indeks);
end

%% 7. Praćenje ivica od sredine ka levoj strani

for kolona = srednjaKolona - 1:-1:pocetnaKolona

    prethodnaGornja = round(gornjaIvica(kolona + 1));

    redoviGornje = ...
        max(2, prethodnaGornja - maksimalniKorak): ...
        min(brojRedova - 1, ...
            prethodnaGornja + maksimalniKorak);

    [~, indeks] = max(gradijentY(redoviGornje, kolona));

    gornjaIvica(kolona) = redoviGornje(indeks);

    prethodnaDonja = round(donjaIvica(kolona + 1));

    redoviDonje = ...
        max(2, prethodnaDonja - maksimalniKorak): ...
        min(brojRedova - 1, ...
            prethodnaDonja + maksimalniKorak);

    [~, indeks] = min(gradijentY(redoviDonje, kolona));

    donjaIvica(kolona) = redoviDonje(indeks);
end

%% 8. Zaglađivanje pronađenih kontura

gornjaSegment = gornjaIvica(koloneZaAnalizu);
donjaSegment = donjaIvica(koloneZaAnalizu);

% Median filtar uklanja pojedinačne nagle skokove
gornjaSegment = movmedian(gornjaSegment, 11);
donjaSegment = movmedian(donjaSegment, 11);

% Srednja vrednost dodatno zaglađuje konture
gornjaSegment = movmean(gornjaSegment, 9);
donjaSegment = movmean(donjaSegment, 9);

gornjaIvica(koloneZaAnalizu) = gornjaSegment;
donjaIvica(koloneZaAnalizu) = donjaSegment;

%% 9. Direktni vertikalni prečnik

precnik = donjaIvica - gornjaIvica;

% Nelogične rezultate proglašavamo nevažećim
neispravneTacke = ...
    precnik <= 5 | ...
    donjaIvica <= gornjaIvica;

precnik(neispravneTacke) = NaN;

%% 10. Pomoćni izlazi

detalji.obradjenaSlika = obradjenaSlika;
detalji.gradijentY = gradijentY;
detalji.profilRedova = profilRedova;
detalji.pragProfila = pragProfila;

detalji.grubaGornjaIvica = grubaGornjaIvica;
detalji.grubaDonjaIvica = grubaDonjaIvica;

detalji.pocetnaKolona = pocetnaKolona;
detalji.krajnjaKolona = krajnjaKolona;
detalji.koloneZaAnalizu = koloneZaAnalizu;

end