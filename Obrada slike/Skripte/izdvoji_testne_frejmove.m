%% 11. Čuvanje karakterističnih frejmova za pregled

folderPrimeri = fullfile(folderProjekta, 'Primeri');

if ~isfolder(folderPrimeri)
    mkdir(folderPrimeri);
end

% Izdvajanje istog ROI regiona iz sva tri frejma
roiPocetak = imcrop(frejmPocetakGray, roiPozicija);
roiSredina = imcrop(frejmSredinaGray, roiPozicija);
roiKraj = imcrop(frejmKrajGray, roiPozicija);

% Čuvanje celih frejmova
imwrite(frejmPocetakGray, ...
    fullfile(folderPrimeri, 'frejm_pocetak.png'));

imwrite(frejmSredinaGray, ...
    fullfile(folderPrimeri, 'frejm_sredina.png'));

imwrite(frejmKrajGray, ...
    fullfile(folderPrimeri, 'frejm_kraj.png'));

% Čuvanje izdvojenog regiona krvnog suda
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