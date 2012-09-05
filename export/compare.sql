------------------------------------------
-- vergelijk_adres
-------------------------------------------
CREATE OR REPLACE  VIEW COMPARE_BAG_HUIDIGE_ADRES
AS
  SELECT postc_n || postc_a || '#'
    || huisnr || '#'
    || huislt || '#'
    || toevoeging || '#'
    || ruimtenaam || '#'
    || adresid || '#'
    || objectid AS vergelijksleutel
  FROM bag_huidige_adres;

CREATE OR REPLACE VIEW COMPARE_DDS_HUIDIGE_ADRES
AS SELECT
	pkdnum || '#' 
	|| huinum ||  '#' 
	|| huilet ||  '#' 
	|| huitvg ||  '#' 
	|| obrnam ||  '#' 
	|| identna ||  '#' 
	|| identtgo  AS vergelijksleutel
FROM dds_huidige_adres;

CREATE OR REPLACE OR REPLACE VIEW COMPARE_GBA_HUIDIGE_ADRES
AS SELECT
	gba_tgbaadr.kpst_num || gba_tgbaadr.kpst_alf || '#' 
	|| gba_tgbaadr.rhs ||  '#' 
	|| gba_tgbaadr.nhsr_lt ||  '#' 
	|| gba_tgbaadr.khsr_tv ||  '#' 
	|| gba_tgbaadr.nopr ||  '#' 
	|| gba_tgbaadr.nid_nad ||  '#' 
	|| gba_tgbaadr.nid_vbo
FROM gba_huidige_adres;

CREATE OR REPLACE VIEW COMPARE_HUIDIGE_ADRES AS  
SELECT
  COMPARE_BAG_HUIDIGE_ADRES.vergelijksleutel AS bagkey,
  COMPARE_DDS_HUIDIGE_ADRES.vergelijksleutel AS ddskey,
  COMPARE_GBA_HUIDIGE_ADRES.vergelijksleutel AS gbakey
FROM COMPARE_BAG_HUIDIGE_ADRES 
FULL OUTER JOIN COMPARE_DDS_HUIDIGE_ADRES
ON COMPARE_BAG_HUIDIGE_ADRES.vergelijksleutel = COMPARE_DDS_HUIDIGE_ADRES.vergelijksleutel
FULL OUTER JOIN COMPARE_GBA_HUIDIGE_ADRES
ON COMPARE_DDS_HUIDIGE_ADRES.vergelijksleutel = COMPARE_GBA_HUIDIGE_ADRES.vergelijksleutel

CREATE OR REPLACE VIEW COMPARE_HUIDIGE_ADRES_FOUTEN AS  
SELECT *
FROM COMPARE_HUIDIGE_ADRES
WHERE bagkey IS NULL
OR ddskey IS NULL
OR gbakey IS NULL