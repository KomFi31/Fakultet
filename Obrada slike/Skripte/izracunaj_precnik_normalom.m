function [precnikNormala, presekX, presekY, detalji] = ...
    izracunaj_precnik_normalom( ...
        gornjaIvica, ...
        donjaIvica, ...
        maksimalnaDuzina, ...
        korakPretrage)
% IZRACUNAJ_PRECNIK_NORMALOM
% Računa rastojanje od gornje do donje ivice krvnog suda
% duž normale na tangentu gornje ivice.
%
% Ulazi:
%   gornjaIvica       - pozicije gornje ivice po kolonama
%   donjaIvica        - pozicije donje ivice po kolonama
%   maksimalnaDuzina  - najveća dužina normale koja se pretražuje
%   korakPretrage     - korak kretanja duž normale u pikselima
%
% Izlazi:
%   precnikNormala    - prečnik duž normale za svaku kolonu
%   presekX           - x koordinata preseka sa donjom ivicom
%   presekY           - y koordinata preseka sa donjom ivicom
%   detalji           - dodatni podaci za prikaz i proveru

%% 1. Priprema ulaznih nizova

gornjaIvica = double(gornjaIvica(:)');
donjaIvica = double(donjaIvica(:)');

if numel(gornjaIvica) ~= numel(donjaIvica)
    error('Gornja i donja ivica moraju imati isti broj elemenata.');
end

brojKolona = numel(gornjaIvica);
xKoordinate = 1:brojKolona;

validneTacke = ...
    isfinite(gornjaIvica) & ...
    isfinite(donjaIvica) & ...
    donjaIvica > gornjaIvica;

if nnz(validneTacke) < 10
    error('Nema dovoljno validnih tačaka za računanje normale.');
end

%% 2. Podrazumevani parametri

if nargin < 3 || isempty(maksimalnaDuzina)

    maksimalniVertikalniPrecnik = max( ...
        donjaIvica(validneTacke) - ...
        gornjaIvica(validneTacke));

    % Ostavlja se rezerva jer normalni pravac može biti nagnut
    % Pa zbog toga ide 1.5 * 
    maksimalnaDuzina = ...
        1.5 * maksimalniVertikalniPrecnik; 
end

% Ako nije dat korak proveravamo na svakih 0.25 piksela
if nargin < 4 || isempty(korakPretrage)
    korakPretrage = 0.25;
end

if maksimalnaDuzina <= 0
    error('Maksimalna dužina mora biti pozitivna.');
end

if korakPretrage <= 0
    error('Korak pretrage mora biti pozitivan.');
end

%% 3. Izdvajanje validnog dela kontura

xValidno = xKoordinate(validneTacke);

gornjaValidna = gornjaIvica(validneTacke);
donjaValidna = donjaIvica(validneTacke);

%% 4. Dodatno zaglađivanje kontura

gornjaZagladjenaValidna = ...
    movmedian(gornjaValidna, 11);

gornjaZagladjenaValidna = ...
    movmean(gornjaZagladjenaValidna, 9);

donjaZagladjenaValidna = ...
    movmedian(donjaValidna, 11);

donjaZagladjenaValidna = ...
    movmean(donjaZagladjenaValidna, 9);

%% 5. Procena nagiba tangente

% Pošto su susedne kolone udaljene za jedan piksel,
% gradient daje približnu vrednost dy/dx.
nagibTangenteValidan = gradient( ...
    gornjaZagladjenaValidna);

%% 6. Priprema izlaznih nizova

precnikNormala = nan(1, brojKolona);
presekX = nan(1, brojKolona);
presekY = nan(1, brojKolona);

gornjaZagladjena = nan(1, brojKolona);
donjaZagladjena = nan(1, brojKolona);
nagibTangente = nan(1, brojKolona);

normalniVektorX = nan(1, brojKolona);
normalniVektorY = nan(1, brojKolona);

gornjaZagladjena(validneTacke) = ...
    gornjaZagladjenaValidna;

donjaZagladjena(validneTacke) = ...
    donjaZagladjenaValidna;

nagibTangente(validneTacke) = ...
    nagibTangenteValidan;

%% 7. Pretraga preseka normale sa donjom ivicom

rastojanjaZaPretragu = ...
    0:korakPretrage:maksimalnaDuzina;

for indeksTacke = 1:numel(xValidno)

    x0 = xValidno(indeksTacke);
    y0 = gornjaZagladjenaValidna(indeksTacke);

    nagib = nagibTangenteValidan(indeksTacke);

    % Tangentni vektor je [1, nagib].
    % Jedan normalni vektor je [-nagib, 1].
    %
    % Pozitivna y komponenta znači da se krećemo nadole,
    % odnosno od gornje ka donjoj ivici.
    normaVektora = sqrt(nagib^2 + 1);

    nx = -nagib / normaVektora;
    ny = 1 / normaVektora;

    normalniVektorX(x0) = nx;
    normalniVektorY(x0) = ny;

    %% Tačke duž normale

    xNormala = ...
        x0 + nx * rastojanjaZaPretragu;

    yNormala = ...
        y0 + ny * rastojanjaZaPretragu;

    %% Interpolacija donje ivice

    yDonjaInterpolirana = interp1( ...
        xValidno, ...
        donjaZagladjenaValidna, ...
        xNormala, ...
        'linear', ...
        NaN);

    % Pre preseka je normala iznad donje ivice:
    % yNormala - yDonjaInterpolirana < 0.
    %
    % Nakon preseka razlika postaje >= 0.
    razlika = ...
        yNormala - yDonjaInterpolirana;

    indeksPreseka = find( ...
        isfinite(razlika) & razlika >= 0, ...
        1, ...
        'first');

    if isempty(indeksPreseka) || indeksPreseka == 1
        continue;
    end

    %% Preciznija procena preseka između dve tačke

    prethodniIndeks = indeksPreseka - 1;

    s1 = rastojanjaZaPretragu(prethodniIndeks);
    s2 = rastojanjaZaPretragu(indeksPreseka);

    razlika1 = razlika(prethodniIndeks);
    razlika2 = razlika(indeksPreseka);

    if ~isfinite(razlika1) || ~isfinite(razlika2)
        continue;
    end

    if razlika2 ~= razlika1

        rastojanjePreseka = ...
            s1 - razlika1 * ...
            (s2 - s1) / ...
            (razlika2 - razlika1);

    else
        rastojanjePreseka = s2;
    end

    %% Čuvanje rezultata

    precnikNormala(x0) = rastojanjePreseka;

    presekX(x0) = ...
        x0 + nx * rastojanjePreseka;

    presekY(x0) = ...
        y0 + ny * rastojanjePreseka;
end

%% 8. Uklanjanje nelogičnih rezultata

vertikalniPrecnik = ...
    donjaZagladjena - gornjaZagladjena;

neispravneTacke = ...
    precnikNormala <= 5 | ...
    precnikNormala > 1.5 * vertikalniPrecnik;

precnikNormala(neispravneTacke) = NaN;
presekX(neispravneTacke) = NaN;
presekY(neispravneTacke) = NaN;

%% 9. Pomoćni podaci

detalji.gornjaZagladjena = ...
    gornjaZagladjena;

detalji.donjaZagladjena = ...
    donjaZagladjena;

detalji.nagibTangente = ...
    nagibTangente;

detalji.normalniVektorX = ...
    normalniVektorX;

detalji.normalniVektorY = ...
    normalniVektorY;

detalji.validneTacke = ...
    isfinite(precnikNormala);

detalji.maksimalnaDuzina = ...
    maksimalnaDuzina;

detalji.korakPretrage = ...
    korakPretrage;

end