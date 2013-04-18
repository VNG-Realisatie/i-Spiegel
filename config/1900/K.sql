/*
K:
	WOZ-deelobjecten
	
	Contact: t.vanderlaan@gemeenteswf.nl

	WOZ objectnummer**	
	Nummer WOZ deelobject**	
	WOZ gebruikscode**	
	DUWOZ-code**	
	Bouwjaar (deel)object*	
	Gebruiksoppervlakte deelobject* (m2)	
	Bruto inhoud deelobject* (m3)	
	Adresseerbaar object ID*	
	Pand ID*	
	Woonplaatsnaam	
	Nummeraanduiding ID	
	Openbare ruimte ID	
	Naam openbare ruimte	
	Postcode*	
	Huisnummer*	
	Huisletter*	
	Huisnummertoevoeging*
*/
SELECT
  WAATON.IDENT_WOZ AS WOZ_OBJECTNUMMER,
  WAATON.ONDERDEEL AS WOZ_DEELOBJECTNUMMER,
  WAATAX.GEBRUIK_WOZ AS WOZ_GEBRUIKSCODE,
  WAATON.SOORT_OBJECT AS DUWOZ_CODE,
  WAATON.BOUWJAAR AS BOUWJAAR_DEELOBJECT,
  WAATON.OPPERVLAKTE AS GEBRUIKSOPPERVLAKTE_DEELOBJECT,
  WAATON.INHOUD AS BRUTO_INHOUD_DEELOBJECT,
  BAGWOZ_KOPPEL.BAG_VBOID AS VBO_ID,
  BAGWOZ_KOPPEL.BAG_PANDID AS PAND_ID,
  BAG_ACTUELE_ADRESSEN.WOONPLAATSNAAM,
  BAG_ACTUELE_ADRESSEN.BAG_NUMMERAANDUIDING_ID,
  NULL AS BAG_OPENBARERUIMTE_ID,
  BAG_ACTUELE_ADRESSEN.RUIMTENAAM AS OPENBARERUIMTENAAM,
  BAG_ACTUELE_ADRESSEN.POSTCODE,
  BAG_ACTUELE_ADRESSEN.HUISNR,
  BAG_ACTUELE_ADRESSEN.HUISLT,
  BAG_ACTUELE_ADRESSEN.TOEVOEGING
FROM vg.WAATON@pgvg WAATON
LEFT JOIN vg.WAATAX@pgvg WAATAX
  ON WAATAX.IDENT_WOZ = WAATON.IDENT_WOZ
  AND WAATAX.VOLGNR = WAATON.VOLGNR_TAX
LEFT JOIN BAGWOZ_KOPPEL
  ON BAGWOZ_KOPPEL.WOZ_OBJECTNUMMER = WAATON.IDENT_WOZ
  AND CAST(BAGWOZ_KOPPEL.WOZ_DEELOBJECTNUMMER AS DECIMAL) = CAST(WAATON.ONDERDEEL AS DECIMAL)
LEFT JOIN BAG_ACTUELE_ADRESSEN
  ON BAG_ACTUELE_ADRESSEN.BAG_VERBLIJFSOBJECT_ID = BAGWOZ_KOPPEL.BAG_VBOID  
-- kan ook via WAATAX en peildatum
WHERE WAATON.VOLGNR_TAX = (  
  SELECT MAX(volgnummer.VOLGNR_TAX)
  FROM vg.WAATON@pgvg volgnummer
  WHERE volgnummer.IDENT_WOZ = WAATON.IDENT_WOZ
  AND volgnummer.ONDERDEEL = WAATON.ONDERDEEL
)
ORDER BY WAATON.IDENT_WOZ, WAATON.ONDERDEEL, WAATON.VOLGNR_TAX
===============================================================
/*
SELECT
            WOZ_OBJECTNUMMER,
            WOZ_DEELOBJECTNUMMER,
            WOZ_DEELOBJECTCODE,
            BAG_PANDID,
            BAG_VBOID,
            bgr_pand.*
            bgr_verblijfsobject.*
FROM BAGWOZ_KOPPEL
( 
        JOIN "BAGOWNER"."BGR_PAND"@PBAG bgr_pand
        ON CAST(bgr_pand.pandid AS DECIMAL)= CAST(BAGWOZ_KOPPEL.BAG_PANDID AS DECIMAL)
        WHERE BAGWOZ_KOPPEL.BAG_PANDID IS NULL
        AND bgr_pand.ddeinde       IS NULL
        AND bgr_pand.indactief     = 'J'
        AND bgr_pand.indauthentiek = 'J'
        AND (
          bgr_pand.pandstatuscode = '04' -- ingebruik (niet ingemeten)
          OR bgr_pand.pandstatuscode = '05' -- ingebruik
        )
)
(
        JOIN "BAGOWNER"."BGR_VERBLIJFSOBJECT"@PBAG bgr_verblijfsobject
        ON CAST(bgr_verblijfsobject.verblijfsobjectid AS DECIMAL)= CAST(BAGWOZ_koppel.BAG_VERBLIJFSOBJECTID AS DECIMAL)
        --AND bgr_verblijfsobject.verblijfsobjectid IS NULL
        WHERE BAGWOZ_koppel.BAG_VERBLIJFSOBJECTID IS NULL
        AND bgr_verblijfsobject.ddeinde       IS NULL
        AND bgr_verblijfsobject.indactief     = 'J'
        AND bgr_verblijfsobject.indauthentiek = 'J'
        AND (
          bgr_verblijfsobject.vobjstatuscode = '03' --in gebruik  
          OR bgr_verblijfsobject.vobjstatuscode = '02' --in gebruik (niet ingemeten)
        )
)
-----------------------------------------------------------------------------------------
SELECT 
  * 
FROM 
  "BAGOWNER"."BGR_VERBLIJFSOBJECT"@PBAG bgr_vbo,
  "BAGOWNER"."BGR_PAND"@PBAG bgr_pand
 WHERE      
  bgr_vbo.ddeinde       IS NULL AND 
  bgr_vbo.indactief     = 'J' AND 
  bgr_vbo.indauthentiek = 'J' AND
  (
          bgr_vbo.vobjstatuscode = '03' --in gebruik  
          OR bgr_vbo.vobjstatuscode = '02' --in gebruik (niet ingemeten)
  )AND 
  bgr_pand.ddeinde       IS NULL AND 
  bgr_pand.indactief     = 'J' AND 
  bgr_pand.indauthentiek = 'J' AND
  (
        bgr_pand.pandstatuscode = '04' -- ingebruik (niet ingemeten)
        OR bgr_pand.pandstatuscode = '05' -- ingebruik
  )
---------------------------------------------------------------------------------------
Select * From vg.WAATON@pgvg;  
--------------------------------------------------------------------------------
SELECT
  woz.ident_woz,
  woz.onderdeel,
  woz.t_soort_object,
  woz.soort_object,
  woz.bouwjaar,
  woz.inhoud,
  woz.oppervlakte,
  bag_vbo.verblijfsobjectid,
  bag_pand.pandid 
FROM 
  BAGWOZ_KOPPEL,
  vg.WAATON@pgvg woz,
  BAGOWNER.BGR_PAND@PBAG bag_pand,
  BAGOWNER.BGR_vbo@PBAG bag_vbo,
*/