/*
R7.1:
	BAG - actuele adresseerbare objecten

	Contact: t.vanderlaan@gemeenteswf.nl
	
	Adresseerbaar object ID*	
	Nummeraanduiding ID*	
	Openbare ruimte ID*	
	Naam openbare ruimte*	
	Woonplaatsnaam*	
	Postcode*	
	Huisnummer*	
	Huisletter*	
	Huisnummertoevoeging*
*/
-- verblijfsobjecten --
SELECT
  'verblijfsobject' AS ADRESTYPE,
  htmlunescape(bra_ruimte.ruimtenaam_d) AS RUIMTENAAM,
  bra_ruimte.ruimtenaam AS RUIMTENAAM_SIMPEL,
  htmlunescape(bra_ruimte.ruimtenaam_boco_d) AS RUIMTENAAM_BOCO,
  UPPER(bra_ruimte.ruimtenaam_boco) AS RUIMTENAAM_USIMPEL,
  bra_adres.huisnr        AS HUISNR,
  bra_adres.huislt        AS HUISLT,
  bra_adres.toevoeging    AS TOEVOEGING,
  bra_adres.postc_n || bra_adres.postc_a AS POSTCODE,
  htmlunescape(bra_woonplaats.woonplaatsnaam_d) AS WOONPLAATSNAAM,
  bra_woonplaats.woonplaatsnaam  AS WOONPLAATSNAAM_SIMPEL,
  UPPER(bra_woonplaats.woonplaatsnaam)  AS WOONPLAATSNAAM_USIMPEL,  
  bra_adres.adresid       AS BAG_NUMMERAANDUIDING_ID,
  bgr_verblijfsobject.verblijfsobjectid   AS BAG_VERBLIJFSOBJECT_ID
FROM 
  "BAGOWNER"."BRA_ADRES"@PBAG       bra_adres,
  "BAGOWNER"."BRA_RUIMTE"@PBAG      bra_ruimte,
  "BAGOWNER"."BRA_WOONPLAATS"@PBAG  bra_woonplaats,
  "BAGOWNER"."BGR_VERBLIJFSOBJECTADRES"@PBAG bgr_verblijfsobjectadres,
  "BAGOWNER"."BGR_VERBLIJFSOBJECT"@PBAG bgr_verblijfsobject
WHERE bra_adres.ddeinde               IS NULL
AND bra_adres.indactief               = 'J'
AND bra_ruimte.ddeinde                IS NULL
AND bra_ruimte.indactief              = 'J'
AND bra_woonplaats.ddeinde            IS NULL
AND bra_woonplaats.indactief          = 'J'
AND bra_adres.indauthentiek           = 'J'
AND bgr_verblijfsobject.ddeinde       IS NULL
AND bgr_verblijfsobject.indactief     = 'J'
AND bgr_verblijfsobject.indauthentiek = 'J' 
AND bra_adres.ruimtenr                = bra_ruimte.ruimtenr
AND bra_ruimte.woonplaatsnr           = bra_woonplaats.woonplaatsnr
AND bra_adres.adresnr                 = bgr_verblijfsobjectadres.adresnr
AND bgr_verblijfsobject.vobjnr        = bgr_verblijfsobjectadres.vobjnr
AND bgr_verblijfsobject.vobjvolgnr    = bgr_verblijfsobjectadres.vobjvolgnr
-- ligplaatsen --
UNION SELECT 
  'ligplaats',
  htmlunescape(bra_ruimte.ruimtenaam_d) AS RUIMTENAAM,
  bra_ruimte.ruimtenaam AS RUIMTENAAM_SIMPEL,
  htmlunescape(bra_ruimte.ruimtenaam_boco_d) AS RUIMTENAAM_BOCO,
  UPPER(bra_ruimte.ruimtenaam_boco) AS RUIMTENAAM_USIMPEL,
  bra_adres.huisnr,
  bra_adres.huislt,
  bra_adres.toevoeging,
  bra_adres.postc_n || bra_adres.postc_a, 
  htmlunescape(bra_woonplaats.woonplaatsnaam_d) AS WOONPLAATSNAAM,
  bra_woonplaats.woonplaatsnaam AS WOONPLAATSNAAM_SIMPEL,
  UPPER(bra_woonplaats.woonplaatsnaam)  AS WOONPLAATSNAAM_USIMPEL,
  bra_adres.adresid,
  bgr_ligplaats.ligplaatsid
FROM
  "BAGOWNER"."BRA_ADRES"@PBAG       bra_adres,
  "BAGOWNER"."BRA_RUIMTE"@PBAG      bra_ruimte,  
  "BAGOWNER"."BRA_WOONPLAATS"@PBAG  bra_woonplaats,
  "BAGOWNER"."BGR_LIGPLAATSADRES"@PBAG bgr_ligplaatsadres,
  "BAGOWNER"."BGR_LIGPLAATS"@PBAG bgr_ligplaats
WHERE bra_adres.ddeinde        IS NULL
AND bra_adres.indactief           = 'J'
AND bra_ruimte.ddeinde           IS NULL
AND bra_ruimte.indactief          = 'J'
AND bra_woonplaats.ddeinde       IS NULL
AND bra_woonplaats.indactief      = 'J'
AND bra_adres.indauthentiek       = 'J'
AND bgr_ligplaats.ddeinde        IS NULL
AND bgr_ligplaats.indactief       = 'J'
AND bgr_ligplaats.indauthentiek   = 'J'
AND bra_adres.ruimtenr            = bra_ruimte.ruimtenr
AND bra_ruimte.woonplaatsnr       = bra_woonplaats.woonplaatsnr
AND bra_adres.adresnr             = bgr_ligplaatsadres.adresnr
AND bgr_ligplaats.ligplaatsnr     = bgr_ligplaatsadres.ligplaatsnr
AND bgr_ligplaats.ligplaatsvolgnr = bgr_ligplaatsadres.ligplaatsvolgnr
-- ligplaatsen --
UNION SELECT 
  'standplaats',
  htmlunescape(bra_ruimte.ruimtenaam_d) AS RUIMTENAAM,
  bra_ruimte.ruimtenaam AS RUIMTENAAM_SIMPEL,
  htmlunescape(bra_ruimte.ruimtenaam_boco_d) AS RUIMTENAAM_BOCO,
  UPPER(bra_ruimte.ruimtenaam_boco) AS RUIMTENAAM_USIMPEL,  
  bra_adres.huisnr,
  bra_adres.huislt,
  bra_adres.toevoeging,
  bra_adres.postc_n || bra_adres.postc_a,
  htmlunescape(bra_woonplaats.woonplaatsnaam_d) AS WOONPLAATSNAAM,
  bra_woonplaats.woonplaatsnaam AS WOONPLAATSNAAM_SIMPEL,
  UPPER(bra_woonplaats.woonplaatsnaam)  AS WOONPLAATSNAAM_USIMPEL,
  bra_adres.adresid,
  bgr_standplaats.standplaatsid
FROM
  "BAGOWNER"."BRA_ADRES"@PBAG       bra_adres,
  "BAGOWNER"."BRA_RUIMTE"@PBAG      bra_ruimte,  
  "BAGOWNER"."BRA_WOONPLAATS"@PBAG  bra_woonplaats,
  "BAGOWNER"."BGR_STANDPLAATSADRES"@PBAG bgr_standplaatsadres,
  "BAGOWNER"."BGR_STANDPLAATS"@PBAG bgr_standplaats    
WHERE bra_adres.ddeinde            IS NULL
AND bra_adres.indactief               = 'J'
AND bra_ruimte.ddeinde               IS NULL
AND bra_ruimte.indactief              = 'J'
AND bra_woonplaats.ddeinde           IS NULL
AND bra_woonplaats.indactief          = 'J'
AND bra_adres.indauthentiek           = 'J'
AND bgr_standplaats.ddeinde          IS NULL
AND bgr_standplaats.indactief         = 'J'
AND bgr_standplaats.indauthentiek     = 'J'
AND bra_adres.ruimtenr                = bra_ruimte.ruimtenr
AND bra_ruimte.woonplaatsnr           = bra_woonplaats.woonplaatsnr
AND bra_adres.adresnr                 = bgr_standplaatsadres.adresnr
AND bgr_standplaats.standplaatsnr     = bgr_standplaatsadres.standplaatsnr
AND bgr_standplaats.standplaatsvolgnr = bgr_standplaatsadres.standplaatsvolgnr
