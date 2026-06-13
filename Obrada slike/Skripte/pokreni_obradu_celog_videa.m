%% Obrada celog video-snimka krvnog suda
clear;
close all;
clc;

%% 1. Putanje projekta

folderSkripte = fileparts(mfilename('fullpath'));
folderProjekta = fileparts(folderSkripte);

folderVideo = fullfile(folderProjekta, 'Video');
folderRezultati = fullfile(folderProjekta, 'Rezultati');

if ~isfolder(folderRezultati)
    mkdir(folderRezultati);
end

%% 2. Učitavanje prethodno izabranog ROI regiona

putanjaOsnovnihPodataka = fullfile( ...
    folderRezultati, ...
    'osnovni_podaci_krvni_sud.mat');

if ~isfile(putanjaOsnovnihPodataka)
    error([ ...
        'Osnovni podaci nisu pronađeni. ' ...
        'Prvo pokrenite krvni_sud_osnova.m.']);
end

ucitaniPodaci = load( ...
    putanjaOsnovnihPodataka, ...
    'rezultati');

osnovniPodaci = ucitaniPodaci.rezultati;

%% 3. Provera putanje do video-snimka

punaPutanja = osnovniPodaci.putanjaVidea;

if ~isfile(punaPutanja)

    [nazivFajla, putanjaFajla] = uigetfile( ...
        {'*.mp4;*.avi;*.mov', ...
         'Video fajlovi (*.mp4, *.avi, *.mov)'}, ...
        'Ponovo izaberite video-snimak', ...
        folderVideo);

    if isequal(nazivFajla, 0)
        error('Nije izabran video-snimak.');
    end

    punaPutanja = fullfile( ...
        putanjaFajla, ...
        nazivFajla);
end

%% 4. Pokretanje obrade

udeoMargine = 0.22;

rezultatiObrade = obradi_video_krvnog_suda( ...
    punaPutanja, ...
    osnovniPodaci.roiPozicija, ...
    osnovniPodaci.pocetniFrejm, ...
    osnovniPodaci.krajnjiFrejm, ...
    udeoMargine);

%% 5. Čuvanje rezultata

putanjaRezultata = fullfile( ...
    folderRezultati, ...
    'rezultati_vertikalni_precnik.mat');

% Format -v7.3 je pogodniji za veće matrice.
save( ...
    putanjaRezultata, ...
    'rezultatiObrade', ...
    '-v7.3');

fprintf('\nRezultati obrade sačuvani su u:\n%s\n', ...
    putanjaRezultata);

%% 6. Statistika uspešnosti

brojUspesnih = nnz( ...
    rezultatiObrade.uspesnaDetekcija);

ukupanBroj = ...
    rezultatiObrade.brojObradjenihFrejmova;

procenatUspesnih = ...
    100 * brojUspesnih / ukupanBroj;

fprintf('\nUspešna detekcija: %d od %d frejmova\n', ...
    brojUspesnih, ...
    ukupanBroj);

fprintf('Procenat uspešnosti: %.2f %%\n', ...
    procenatUspesnih);

%% 7. Prikaz vertikalnog prečnika u vremenu

figure('Name', 'Prečnik krvnog suda u vremenu');

plot( ...
    rezultatiObrade.vremeRelativno, ...
    rezultatiObrade.precnikVertikalniMedijana, ...
    'LineWidth', 1.2);

grid on;

xlabel('Vreme [s]');
ylabel('Medijana vertikalnog prečnika [px]');

title('Promena prečnika krvnog suda u vremenu');