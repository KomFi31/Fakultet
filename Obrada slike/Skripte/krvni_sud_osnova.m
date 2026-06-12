%% Projekat: Analiza deformacije krvnog suda
clear;
close all;
clc;

%% Putanje projekta

% Folder u kojem se nalazi trenutna skripta
folderSkripte = fileparts(mfilename('fullpath'));

% Glavni folder projekta: "Obrada slike"
folderProjekta = fileparts(folderSkripte);

% Podfolderi projekta
folderVideo = fullfile(folderProjekta, 'Video');
folderRezultati = fullfile(folderProjekta, 'Rezultati');

% Automatsko pravljenje foldera za rezultate ako ne postoji
if ~isfolder(folderRezultati)
    mkdir(folderRezultati);
end
%% 1. Učitavanje video-snimka

[nazivFajla, putanjaFajla] = uigetfile( ...
    {'*.mp4;*.avi;*.mov', 'Video fajlovi (*.mp4, *.avi, *.mov)'}, ...
    'Izaberite video-snimak krvnog suda', ...
    folderVideo);

if isequal(nazivFajla, 0)
    error('Nije izabran video-snimak.');
end

punaPutanja = fullfile(putanjaFajla, nazivFajla);

videoObj = VideoReader(punaPutanja);

%% 2. Informacije o video-snimku

fps = videoObj.FrameRate;
trajanje = videoObj.Duration;
brojFrejmova = floor(trajanje * fps);

fprintf('Naziv video-snimka: %s\n', nazivFajla);
fprintf('Rezolucija: %d x %d piksela\n', ...
    videoObj.Width, videoObj.Height);
fprintf('Frame rate: %.2f fps\n', fps);
fprintf('Trajanje: %.2f s\n', trajanje);
fprintf('Približan broj frejmova: %d\n', brojFrejmova);

%% 3. Izbor početnog i krajnjeg frejma

pocetniFrejm = 1;
krajnjiFrejm = brojFrejmova;

% Za početak analiziramo ceo video.
% Kasnije će se ove vrednosti unositi kroz App Designer.

if pocetniFrejm < 1 || krajnjiFrejm > brojFrejmova
    error('Izabrani opseg frejmova nije ispravan.');
end

if pocetniFrejm >= krajnjiFrejm
    error('Početni frejm mora biti manji od krajnjeg frejma.');
end

srednjiFrejm = round((pocetniFrejm + krajnjiFrejm) / 2);

%% 4. Učitavanje početnog, srednjeg i krajnjeg frejma

videoObj.CurrentTime = (pocetniFrejm - 1) / fps;
frejmPocetak = readFrame(videoObj);

videoObj.CurrentTime = (srednjiFrejm - 1) / fps;
frejmSredina = readFrame(videoObj);

videoObj.CurrentTime = (krajnjiFrejm - 1) / fps;
frejmKraj = readFrame(videoObj);

%% 5. Pretvaranje u sivu sliku

frejmPocetakGray = im2gray(frejmPocetak);
frejmSredinaGray = im2gray(frejmSredina);
frejmKrajGray = im2gray(frejmKraj);

%% 6. Spajanje tri frejma u jednu referentnu sliku

% Koristi se srednja vrednost intenziteta tri frejma.
referentnaSlika = uint8( ...
    (double(frejmPocetakGray) + ...
     double(frejmSredinaGray) + ...
     double(frejmKrajGray)) / 3);

%% 7. Prikaz karakterističnih frejmova

figure('Name', 'Karakteristični frejmovi');

tiledlayout(2, 2);

nexttile;
imshow(frejmPocetakGray);
title(['Početni frejm: ', num2str(pocetniFrejm)]);

nexttile;
imshow(frejmSredinaGray);
title(['Srednji frejm: ', num2str(srednjiFrejm)]);

nexttile;
imshow(frejmKrajGray);
title(['Krajnji frejm: ', num2str(krajnjiFrejm)]);

nexttile;
imshow(referentnaSlika);
title('Referentna slika');

%% 8. Ručni izbor regiona analize

figure('Name', 'Izbor regiona krvnog suda');
imshow(referentnaSlika);
title('Označite region krvnog suda i dvaput kliknite unutar pravougaonika');

roiObjekat = drawrectangle;

% Čeka se da korisnik završi izbor.
wait(roiObjekat);

roiPozicija = round(roiObjekat.Position);

x = roiPozicija(1);
y = roiPozicija(2);
sirina = roiPozicija(3);
visina = roiPozicija(4);

fprintf('\nIzabrani ROI:\n');
fprintf('x = %d\n', x);
fprintf('y = %d\n', y);
fprintf('širina = %d\n', sirina);
fprintf('visina = %d\n', visina);

%% 9. Izdvajanje izabranog regiona

roiSlika = imcrop(referentnaSlika, roiPozicija);

figure('Name', 'Izabrani region');
imshow(roiSlika);
title('Region krvnog suda koji će se analizirati');

%% 10. Privremeno čuvanje osnovnih podataka

rezultati = struct();

rezultati.nazivVidea = nazivFajla;
rezultati.putanjaVidea = punaPutanja;

rezultati.frameRate = fps;
rezultati.trajanje = trajanje;
rezultati.brojFrejmova = brojFrejmova;

rezultati.pocetniFrejm = pocetniFrejm;
rezultati.srednjiFrejm = srednjiFrejm;
rezultati.krajnjiFrejm = krajnjiFrejm;

rezultati.roiPozicija = roiPozicija;

putanjaRezultata = fullfile( ...
    folderRezultati, ...
    'osnovni_podaci_krvni_sud.mat');

save(putanjaRezultata, 'rezultati');

fprintf('\nOsnovni podaci su sačuvani u:\n%s\n', ...
    putanjaRezultata);
