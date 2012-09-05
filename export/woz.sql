-------------------------------------------
-- woz_huidige_adres
------------------------------------------- 
DROP MATERIALIZED VIEW dl_woz_adressen;
CREATE MATERIALIZED VIEW dl_woz_adressen
AS SELECT *
FROM VG.PBSPBT@pgvg;

DROP MATERIALIZED VIEW woz_huidige_adres;
CREATE MATERIALIZED VIEW woz_huidige_adres 
AS SELECT *
FROM dl_woz_adressen
WHERE gemkd_gem = 1900
AND DATUM_EINDE IS NULL