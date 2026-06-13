%% Vizuelna provera detekcije na kontrolnim frejmovima
% Svrha skripte je da proverimo sumnjive rezultate na dobijenom grafiku iz
% skripte "pokreni_obradu_celog_videa.m". To se moze proveriti i vizuelno
% puštanjem snimka ali radi matematičke korektnosti formiramo skriptu.
clear;
close all;
clc;

%% 1. Putanje projekta

folderSkripte = fileparts(mfilename('fullpath'));
folderProjekta = fileparts(folderSkripte);

folderVideo = fullfile(folderProjekta, 'Video');
folderRezultati = fullfile(folderProjekta, 'Rezultati');

putanjaRezultata = fullfile( ...
    folderRezultati, ...
    'rezultati_vertikalni_precnik.mat');

if ~isfile(putanjaRezultata)
    error([ ...
        'Fajl sa rezultatima nije pronađen. ' ...
        'Prvo pokrenite pokreni_obradu_celog_videa.m.']);
end

%% 2. Učitavanje rezultata obrade

ucitaniPodaci = load( ...
    putanjaRezultata, ...
    'rezultatiObrade');

rezultatiObrade = ucitaniPodaci.rezultatiObrade;

%% 3. Pronalaženje video-snimka

punaPutanja = rezultatiObrade.putanjaVidea;

if ~isfile(punaPutanja)

    [nazivFajla, putanjaFajla] = uigetfile( ...
        {'*.mp4;*.avi;*.mov', ...
         'Video fajlovi (*.mp4, *.avi, *.mov)'}, ...
        'Ponovo izaberite video-snimak', ...
        folderVideo);

    if isequal(nazivFajla, 0)
        error('Nije izabran video-snimak.');
    end

    punaPutanja = fullfile(putanjaFajla, nazivFajla);
end

videoObj = VideoReader(punaPutanja);

fps = rezultatiObrade.frameRate;

%% 4. Vremenski trenuci koje proveravamo

% Vremena su relativna u odnosu na početak analiziranog opsega.
vremenaZaProveru = [2, 5, 10, 30, 60, 90, 120, 150];

maksimalnoVreme = rezultatiObrade.vremeRelativno(end);

% Uklanjamo vremena koja izlaze izvan trajanja obrade.
vremenaZaProveru = vremenaZaProveru( ...
    vremenaZaProveru <= maksimalnoVreme);

if isempty(vremenaZaProveru)
    error('Nijedno zadato vreme nije unutar obrađenog opsega.');
end

%% 5. Priprema figure

brojKontrolnihFrejmova = numel(vremenaZaProveru);

brojKolonaGrafika = 2;
brojRedovaGrafika = ceil( ...
    brojKontrolnihFrejmova / brojKolonaGrafika);

figure( ...
    'Name', 'Vizuelna provera detekcije ivica', ...
    'NumberTitle', 'off');

tiledlayout( ...
    brojRedovaGrafika, ...
    brojKolonaGrafika, ...
    'TileSpacing', 'compact', ...
    'Padding', 'compact');

%% 6. Prikaz kontrolnih frejmova

for indeksKontrole = 1:brojKontrolnihFrejmova

    trazenoVreme = vremenaZaProveru(indeksKontrole);

    % Pronalazi rezultat čije je vreme najbliže zadatom vremenu.
    [~, indeksRezultata] = min(abs( ...
        rezultatiObrade.vremeRelativno - trazenoVreme));

    brojFrejma = ...
        rezultatiObrade.indeksiFrejmova(indeksRezultata);

    stvarnoVreme = ...
        rezultatiObrade.vremeRelativno(indeksRezultata);

    %% Učitavanje odgovarajućeg frejma iz videa

    vremeOdPocetkaVidea = (brojFrejma - 1) / fps;

    videoObj.CurrentTime = min( ...
        vremeOdPocetkaVidea, ...
        max(0, videoObj.Duration - 1 / fps));

    frejm = readFrame(videoObj);
    frejmGray = im2gray(frejm);

    roiSlika = imcrop( ...
        frejmGray, ...
        rezultatiObrade.roiPozicija);

    %% Učitavanje prethodno detektovanih ivica

    gornjaIvica = double( ...
        rezultatiObrade.gornjaIvica(indeksRezultata, :));

    donjaIvica = double( ...
        rezultatiObrade.donjaIvica(indeksRezultata, :));

    precnikMedijana = ...
        rezultatiObrade.precnikVertikalniMedijana( ...
            indeksRezultata);

    %% Usklađivanje širine slike i dužine nizova

    brojTacaka = min([ ...
        size(roiSlika, 2), ...
        numel(gornjaIvica), ...
        numel(donjaIvica)]);

    xKoordinate = 1:brojTacaka;

    gornjaIvica = gornjaIvica(1:brojTacaka);
    donjaIvica = donjaIvica(1:brojTacaka);

    %% Prikaz slike i kontura

    grafik = nexttile;

    imshow(roiSlika, [], 'Parent', grafik);
    hold(grafik, 'on');

    plot( ...
        grafik, ...
        xKoordinate, ...
        gornjaIvica, ...
        'r', ...
        'LineWidth', 1.5);

    plot( ...
        grafik, ...
        xKoordinate, ...
        donjaIvica, ...
        'c', ...
        'LineWidth', 1.5);

    hold(grafik, 'off');

    if rezultatiObrade.uspesnaDetekcija(indeksRezultata)
        statusDetekcije = 'uspešna';
    else
        statusDetekcije = 'neuspešna';
    end

    title( ...
        grafik, ...
        sprintf([ ...
            't = %.2f s | frejm %d\n' ...
            'medijana = %.2f px | detekcija: %s'], ...
            stvarnoVreme, ...
            brojFrejma, ...
            precnikMedijana, ...
            statusDetekcije));
end

sgtitle('Kontrolni frejmovi sa detektovanim ivicama');