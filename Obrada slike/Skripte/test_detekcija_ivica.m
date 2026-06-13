%% Test detekcije ivica na tri karakteristična frejma
clear;
close all;
clc;

%% Putanje

folderSkripte = fileparts(mfilename('fullpath'));
folderProjekta = fileparts(folderSkripte);
folderPrimeri = fullfile(folderProjekta, 'Primeri');

naziviSlika = {
    'roi_pocetak.png'
    'roi_sredina.png'
    'roi_kraj.png'
};

naslovi = {
    'Početni frejm'
    'Srednji frejm'
    'Krajnji frejm'
};

%% Detekcija i prikaz ivica

figure('Name', 'Detekcija ivica krvnog suda');

tiledlayout(3, 1, ...
    'TileSpacing', 'compact', ...
    'Padding', 'compact');

rezultatiTesta = struct([]);

for indeksSlike = 1:numel(naziviSlika)

    putanjaSlike = fullfile( ...
        folderPrimeri, ...
        naziviSlika{indeksSlike});

    if ~isfile(putanjaSlike)
        error('Nije pronađen fajl: %s', putanjaSlike);
    end

    roiSlika = imread(putanjaSlike);

    [gornjaIvica, donjaIvica, precnik, detalji] = ...
        detektuj_ivice_krvnog_suda(roiSlika, 0.22);

    rezultatiTesta(indeksSlike).gornjaIvica = gornjaIvica;
    rezultatiTesta(indeksSlike).donjaIvica = donjaIvica;
    rezultatiTesta(indeksSlike).precnik = precnik;
    rezultatiTesta(indeksSlike).detalji = detalji;

    sledeciGrafik = nexttile;

    imshow(roiSlika, 'Parent', sledeciGrafik);
    hold(sledeciGrafik, 'on');

    plot( ...
        sledeciGrafik, ...
        gornjaIvica, ...
        'r', ...
        'LineWidth', 1.5);

    plot( ...
        sledeciGrafik, ...
        donjaIvica, ...
        'c', ...
        'LineWidth', 1.5);

    hold(sledeciGrafik, 'off');

    srednjiPrecnik = median(precnik, 'omitnan');

    title(sledeciGrafik, ...
        sprintf('%s — medijana prečnika: %.2f px', ...
        naslovi{indeksSlike}, ...
        srednjiPrecnik));
end

%% Prikaz prečnika po kolonama

figure('Name', 'Prečnik krvnog suda po kolonama');

tiledlayout(3, 1, ...
    'TileSpacing', 'compact', ...
    'Padding', 'compact');

for indeksSlike = 1:numel(naziviSlika)

    sledeciGrafik = nexttile;

    plot( ...
        sledeciGrafik, ...
        rezultatiTesta(indeksSlike).precnik, ...
        'LineWidth', 1.2);

    grid(sledeciGrafik, 'on');

    xlabel(sledeciGrafik, 'Kolona ROI slike');
    ylabel(sledeciGrafik, 'Prečnik [px]');
    title(sledeciGrafik, naslovi{indeksSlike});
end