function izdvoji_testne_frejmove( ...
    folderProjekta, ...
    frejmPocetakGray, ...
    frejmSredinaGray, ...
    frejmKrajGray, ...
    referentnaSlika, ...
    roiPozicija)
% IZDVOJI_TESTNE_FREJMOVE
% Čuva karakteristične frejmove i njihove ROI oblasti radi
% provere i podešavanja algoritma za detekciju ivica.

folderPrimeri = fullfile(folderProjekta, 'Primeri');

if ~isfolder(folderPrimeri)
    mkdir(folderPrimeri);
end

%% Izdvajanje istog ROI regiona iz sva tri frejma

roiPocetak = imcrop(frejmPocetakGray, roiPozicija);
roiSredina = imcrop(frejmSredinaGray, roiPozicija);
roiKraj = imcrop(frejmKrajGray, roiPozicija);

%% Čuvanje celih frejmova

imwrite(frejmPocetakGray, ...
    fullfile(folderPrimeri, 'frejm_pocetak.png'));

imwrite(frejmSredinaGray, ...
    fullfile(folderPrimeri, 'frejm_sredina.png'));

imwrite(frejmKrajGray, ...
    fullfile(folderPrimeri, 'frejm_kraj.png'));

%% Čuvanje ROI oblasti

imwrite(roiPocetak, ...
    fullfile(folderPrimeri, 'roi_pocetak.png'));

imwrite(roiSredina, ...
    fullfile(folderPrimeri, 'roi_sredina.png'));

imwrite(roiKraj, ...
    fullfile(folderPrimeri, 'roi_kraj.png'));

imwrite(referentnaSlika, ...
    fullfile(folderPrimeri, 'referentna_slika.png'));

fprintf('\nKarakteristični frejmovi su sačuvani u:\n%s\n', ...
    folderPrimeri);

end