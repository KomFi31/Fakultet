function rezultatiObrade = obradi_video_krvnog_suda( ...
    punaPutanja, ...
    roiPozicija, ...
    pocetniFrejm, ...
    krajnjiFrejm, ...
    udeoMargine)
% OBRADI_VIDEO_KRVNOG_SUDA
% Obrađuje izabrani opseg video-snimka frejm po frejm.
%
% Za svaki frejm detektuje:
%   - gornju ivicu;
%   - donju ivicu;
%   - vertikalni prečnik po kolonama;
%   - prečnik duž normale na tangentu;
%   - reprezentativne prečnike kao medijane.

% Rezultat funkcije je struktura sa svim podacima obrade.

%% 1. Podrazumevana vrednost margine

if nargin < 5 || isempty(udeoMargine)
    udeoMargine = 0.22;
end

%% 2. Provera ulaznih podataka

if ~isfile(punaPutanja)
    error('Video-fajl nije pronađen:\n%s', punaPutanja);
end

if pocetniFrejm < 1
    error('Početni frejm mora biti najmanje 1.');
end

if krajnjiFrejm < pocetniFrejm
    error('Krajnji frejm mora biti veći ili jednak početnom.');
end

if numel(roiPozicija) ~= 4
    error('ROI pozicija mora imati oblik [x, y, širina, visina].');
end

%% 3. Otvaranje video-snimka

videoObj = VideoReader(punaPutanja);

fps = videoObj.FrameRate;
trajanje = videoObj.Duration;

priblizanBrojFrejmova = floor(trajanje * fps);

if krajnjiFrejm > priblizanBrojFrejmova
    warning( ...
        ['Krajnji frejm je veći od približnog broja frejmova. ' ...
         'Obrada će se zaustaviti na kraju videa.']);
end

brojPlaniranihFrejmova = ...
    krajnjiFrejm - pocetniFrejm + 1;

%% 4. Postavljanje na početak izabranog opsega

pocetnoVreme = (pocetniFrejm - 1) / fps;

videoObj.CurrentTime = min( ...
    pocetnoVreme, ...
    max(0, videoObj.Duration - 1 / fps));

%% 5. Priprema promenljivih

gornjaIvicaSviFrejmovi = [];
donjaIvicaSviFrejmovi = [];
precnikPoTackama = [];

% Rezultati prečnika izmerenog duž normale
precnikNormalaPoTackama = [];

precnikNormalaMedijana = nan( ...
    brojPlaniranihFrejmova, 1);

udeoUspesnihTacakaNormala = zeros( ...
    brojPlaniranihFrejmova, 1);

uspesnaDetekcijaNormala = false( ...
    brojPlaniranihFrejmova, 1);

porukeGresakaNormala = repmat( ...
    {''}, ...
    brojPlaniranihFrejmova, 1);

%Rezultati vertikalnog prečnika
precnikMedijana = nan( ...
    brojPlaniranihFrejmova, 1);

precnikSrednjaVrednost = nan( ...
    brojPlaniranihFrejmova, 1);

udeoUspesnihTacaka = zeros( ...
    brojPlaniranihFrejmova, 1);

uspesnaDetekcija = false( ...
    brojPlaniranihFrejmova, 1);

porukeGresaka = repmat( ...
    {''}, ...
    brojPlaniranihFrejmova, 1);

brojObradjenihFrejmova = 0;

%% 6. Prozor za praćenje napretka

prozorNapretka = waitbar( ...
    0, ...
    'Priprema obrade video-snimka...', ...
    'Name', 'Obrada krvnog suda');

%% 7. Obrada svakog frejma

for indeksFrejma = 1:brojPlaniranihFrejmova

    if ~hasFrame(videoObj)
        warning('Dostignut je kraj video-snimka.');
        break;
    end

    frejm = readFrame(videoObj);
    frejmGray = im2gray(frejm);

    roiSlika = imcrop( ...
        frejmGray, ...
        roiPozicija);

    if isempty(roiSlika)
        close(prozorNapretka);
        error('Izabrani ROI je prazan ili izlazi izvan slike.');
    end

    % Prilikom prvog frejma određujemo širinu ROI slike
    % i zauzimamo memoriju za sve rezultate.
    if indeksFrejma == 1

        brojKolonaROI = size(roiSlika, 2);

        gornjaIvicaSviFrejmovi = nan( ...
            brojPlaniranihFrejmova, ...
            brojKolonaROI, ...
            'single');

        donjaIvicaSviFrejmovi = nan( ...
            brojPlaniranihFrejmova, ...
            brojKolonaROI, ...
            'single');

        precnikPoTackama = nan( ...
            brojPlaniranihFrejmova, ...
            brojKolonaROI, ...
            'single');
        
        precnikNormalaPoTackama = nan( ...
            brojPlaniranihFrejmova, ...
            brojKolonaROI, ...
            'single');
    end

    try

        [gornjaIvica, donjaIvica, precnik, detalji] = ...
            detektuj_ivice_krvnog_suda( ...
                roiSlika, ...
                udeoMargine);

        gornjaIvicaSviFrejmovi(indeksFrejma, :) = ...
            single(gornjaIvica);

        donjaIvicaSviFrejmovi(indeksFrejma, :) = ...
            single(donjaIvica);

        precnikPoTackama(indeksFrejma, :) = ...
            single(precnik);

        brojIspravnihTacaka = nnz(isfinite(precnik));

        brojAnaliziranihTacaka = ...
            numel(detalji.koloneZaAnalizu);

        udeoUspesnihTacaka(indeksFrejma) = ...
            brojIspravnihTacaka / brojAnaliziranihTacaka;

        if brojIspravnihTacaka > 0

            precnikMedijana(indeksFrejma) = ...
                median(precnik, 'omitnan');

            precnikSrednjaVrednost(indeksFrejma) = ...
                mean(precnik, 'omitnan');
        end

        % Frejm smatramo uspešnim ako je pronađen prečnik
        % u najmanje 80% analiziranih kolona.
        uspesnaDetekcija(indeksFrejma) = ...
            udeoUspesnihTacaka(indeksFrejma) >= 0.80;

%% Računanje prečnika duž normale

    % Koristi se poseban try/catch kako eventualna greška
    % normalnog prečnika ne bi poništila vertikalne rezultate.
    try
    
        [precnikNormala, ~, ~, ~] = ...
            izracunaj_precnik_normalom( ...
                gornjaIvica, ...
                donjaIvica);
    
        precnikNormalaPoTackama(indeksFrejma, :) = ...
            single(precnikNormala);
    
        brojIspravnihTacakaNormala = ...
            nnz(isfinite(precnikNormala));
    
        udeoUspesnihTacakaNormala(indeksFrejma) = ...
            brojIspravnihTacakaNormala / ...
            brojAnaliziranihTacaka;
    
        if brojIspravnihTacakaNormala > 0
    
            precnikNormalaMedijana(indeksFrejma) = ...
                median(precnikNormala, 'omitnan');
        end
    
        uspesnaDetekcijaNormala(indeksFrejma) = ...
            udeoUspesnihTacakaNormala(indeksFrejma) >= 0.80;
    
    catch MENormala
    
        porukeGresakaNormala{indeksFrejma} = ...
            MENormala.message;
    
        warning( ...
            ['Neuspešno računanje normale u frejmu %d: %s'], ...
            pocetniFrejm + indeksFrejma - 1, ...
            MENormala.message);
    end

    catch ME

        porukeGresaka{indeksFrejma} = ME.message;

        warning( ...
            'Neuspešna detekcija u frejmu %d: %s', ...
            pocetniFrejm + indeksFrejma - 1, ...
            ME.message);
    end

    brojObradjenihFrejmova = indeksFrejma;

    %% Osvežavanje prikaza napretka

    if mod(indeksFrejma, 10) == 0 || ...
            indeksFrejma == brojPlaniranihFrejmova

        procenat = ...
            indeksFrejma / brojPlaniranihFrejmova;

        waitbar( ...
            procenat, ...
            prozorNapretka, ...
            sprintf( ...
                'Obrađen frejm %d od %d', ...
                indeksFrejma, ...
                brojPlaniranihFrejmova));

        drawnow limitrate;
    end
end

%% 8. Zatvaranje prozora napretka

if isgraphics(prozorNapretka)
    close(prozorNapretka);
end

if brojObradjenihFrejmova == 0
    error('Nijedan frejm nije uspešno učitan.');
end

%% 9. Uklanjanje neiskorišćenih redova

gornjaIvicaSviFrejmovi = ...
    gornjaIvicaSviFrejmovi( ...
        1:brojObradjenihFrejmova, :);

donjaIvicaSviFrejmovi = ...
    donjaIvicaSviFrejmovi( ...
        1:brojObradjenihFrejmova, :);

precnikPoTackama = ...
    precnikPoTackama( ...
        1:brojObradjenihFrejmova, :);

precnikMedijana = ...
    precnikMedijana(1:brojObradjenihFrejmova);

precnikSrednjaVrednost = ...
    precnikSrednjaVrednost(1:brojObradjenihFrejmova);

udeoUspesnihTacaka = ...
    udeoUspesnihTacaka(1:brojObradjenihFrejmova);

uspesnaDetekcija = ...
    uspesnaDetekcija(1:brojObradjenihFrejmova);

porukeGresaka = ...
    porukeGresaka(1:brojObradjenihFrejmova);

precnikNormalaPoTackama = ...
    precnikNormalaPoTackama( ...
        1:brojObradjenihFrejmova, :);

precnikNormalaMedijana = ...
    precnikNormalaMedijana( ...
        1:brojObradjenihFrejmova);

udeoUspesnihTacakaNormala = ...
    udeoUspesnihTacakaNormala( ...
        1:brojObradjenihFrejmova);

uspesnaDetekcijaNormala = ...
    uspesnaDetekcijaNormala( ...
        1:brojObradjenihFrejmova);

porukeGresakaNormala = ...
    porukeGresakaNormala( ...
        1:brojObradjenihFrejmova);

%% 10. Formiranje vremenske ose

indeksiFrejmova = ( ...
    pocetniFrejm: ...
    pocetniFrejm + brojObradjenihFrejmova - 1)';

% Vreme mereno od početka izabranog opsega
vremeRelativno = ...
    (0:brojObradjenihFrejmova - 1)' / fps;

% Vreme mereno od početka kompletnog video-snimka
vremeOdPocetkaVidea = ...
    (indeksiFrejmova - 1) / fps;

%% 11. Formiranje strukture rezultata

rezultatiObrade = struct();

rezultatiObrade.putanjaVidea = punaPutanja;
rezultatiObrade.frameRate = fps;

rezultatiObrade.pocetniFrejm = pocetniFrejm;
rezultatiObrade.krajnjiFrejm = ...
    indeksiFrejmova(end);

rezultatiObrade.brojObradjenihFrejmova = ...
    brojObradjenihFrejmova;

rezultatiObrade.roiPozicija = roiPozicija;
rezultatiObrade.udeoMargine = udeoMargine;

rezultatiObrade.indeksiFrejmova = ...
    indeksiFrejmova;

rezultatiObrade.vremeRelativno = ...
    vremeRelativno;

rezultatiObrade.vremeOdPocetkaVidea = ...
    vremeOdPocetkaVidea;

rezultatiObrade.gornjaIvica = ...
    gornjaIvicaSviFrejmovi;

rezultatiObrade.donjaIvica = ...
    donjaIvicaSviFrejmovi;

rezultatiObrade.precnikVertikalniPoTackama = ...
    precnikPoTackama;

rezultatiObrade.precnikVertikalniMedijana = ...
    precnikMedijana;

rezultatiObrade.precnikVertikalniSrednjaVrednost = ...
    precnikSrednjaVrednost;

rezultatiObrade.udeoUspesnihTacaka = ...
    udeoUspesnihTacaka;

rezultatiObrade.uspesnaDetekcija = ...
    uspesnaDetekcija;

rezultatiObrade.porukeGresaka = ...
    porukeGresaka;

rezultatiObrade.precnikNormalaPoTackama = ...
    precnikNormalaPoTackama;

rezultatiObrade.precnikNormalaMedijana = ...
    precnikNormalaMedijana;

rezultatiObrade.udeoUspesnihTacakaNormala = ...
    udeoUspesnihTacakaNormala;

rezultatiObrade.uspesnaDetekcijaNormala = ...
    uspesnaDetekcijaNormala;

rezultatiObrade.porukeGresakaNormala = ...
    porukeGresakaNormala;

end