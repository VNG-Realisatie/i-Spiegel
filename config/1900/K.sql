/*
!!!! exporteren met een ; als scheidingsteken (cvs/die eronder) !!!!

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
/*
SELECT
  WAATON.IDENT_WOZ AS wozobjectnummer,
  WAATON.ONDERDEEL AS nummerwozdeelobject,
  WAATAX.GEBRUIK_WOZ AS wozgebruikscode,
  WAATON.SOORT_OBJECT AS duwozcode,
  WAATON.BOUWJAAR AS bouwjaardeelobject,
  WAATON.OPPERVLAKTE AS gebruiksoppervlakte,
  WAATON.INHOUD AS brutoinhouddeelobject,
  LPAD(BAG_ACTUELE_ADRESSEN.BAG_VERBLIJFSOBJECT_ID, 16, '0') AS adresseerbaarobjectid,
  LPAD(BAGWOZ_KOPPEL.BAG_PANDID, 16, '0') AS pandid,
  BAG_ACTUELE_ADRESSEN.WOONPLAATSNAAM AS woonplaatsnaam,
  LPAD(BAG_ACTUELE_ADRESSEN.BAG_NUMMERAANDUIDING_ID, 16, '0') AS nummeraanduidingid,
  LPAD(BAG_ACTUELE_ADRESSEN.BAG_OPENBARERUIMTE_ID, 16, '0') AS openbareruimteid,
  BAG_ACTUELE_ADRESSEN.RUIMTENAAM AS naamopenbareruimte,
  BAG_ACTUELE_ADRESSEN.POSTCODE AS postcode,
  BAG_ACTUELE_ADRESSEN.HUISNR AS huisnummer,
  BAG_ACTUELE_ADRESSEN.HUISLT AS huisletter,
  BAG_ACTUELE_ADRESSEN.TOEVOEGING AS huisnummertoevoeging
FROM vg.WAATON@pgvg WAATON
JOIN vg.VGWOZ@pgvg VGWOZ
  ON VGWOZ.IDENT = WAATON.IDENT_WOZ
LEFT JOIN vg.WAATAX@pgvg WAATAX
  ON WAATAX.IDENT_WOZ = WAATON.IDENT_WOZ
  AND WAATAX.VOLGNR = WAATON.VOLGNR_TAX
LEFT JOIN BAGWOZ_KOPPEL
  ON BAGWOZ_KOPPEL.WOZ_OBJECTNUMMER = WAATON.IDENT_WOZ
  AND CAST(BAGWOZ_KOPPEL.WOZ_DEELOBJECTNUMMER AS DECIMAL) = CAST(WAATON.ONDERDEEL AS DECIMAL)
LEFT JOIN BAG_ACTUELE_ADRESSEN
  ON CAST(BAG_ACTUELE_ADRESSEN.BAG_VERBLIJFSOBJECT_ID AS DECIMAL) = CAST(BAGWOZ_KOPPEL.BAG_VBOID AS DECIMAL)
-- kan ook via WAATAX en peildatum
WHERE VGWOZ.DATUM_EINDE IS NULL
AND WAATON.VOLGNR_TAX = (  
  SELECT MAX(volgnummer.VOLGNR_TAX)
  FROM vg.WAATON@pgvg volgnummer
  WHERE volgnummer.IDENT_WOZ = WAATON.IDENT_WOZ
  AND volgnummer.ONDERDEEL = WAATON.ONDERDEEL
)
--AND WAATON.ident_woz = 190000000165
ORDER BY WAATON.IDENT_WOZ, WAATON.ONDERDEEL, WAATON.VOLGNR_TAX
*/
/*
SELECT
  waatgb.IDENT_WOZ,
  waatgb.VOLGNR_TAX,  
--  waatgb.VOORSCHRIFT_TON,
--  waatgb.ONDERDEEL_TON,
--  bagobjecten.ident_vob,
  bagobjecten.adresseerbaarobjectid,
  bagobjecten.pandid,
    --  NULL AS bagobjecten.woonplaatsnaam,
    --  NULL AS bagobjecten.nummeraanduidingid,
    --  NULL AS bagobjecten.openbareruimteid,
  bagobjecten.postcode,
  bagobjecten.huisnummer,
  bagobjecten.huisletter,
  bagobjecten.huisnummertoevoeging  
FROM waatgb@pgvg 
LEFT JOIN 
  (
    SELECT 
      verblijfsobject.ident AS ident_bag,
      verblijfsobject.ident_bag AS adresseerbaarobjectid,
      pand.ident_bag AS pandid,
    --  NULL AS woonplaatsnaam,
    --  NULL AS nummeraanduidingid,
    --  NULL AS openbareruimteid,
      verblijfsobject.postcode AS postcode,
      verblijfsobject.huisnr AS huisnummer,
      verblijfsobject.huislt AS huisletter,
      verblijfsobject.toev AS huisnummertoevoeging  
    FROM vgvob@pgvg verblijfsobject
    LEFT JOIN vgvob@pgvg pand
      ON pand.ident = verblijfsobject.ident_pnd
      AND pand.type_vbo = 10            -- bag_pand
    WHERE verblijfsobject.type_vbo = 1  -- bag_verblijfsobject
  UNION
    SELECT 
      pand.ident AS ident_bag,
      NULL AS adresseerbaarobjectid,
      pand.ident_bag AS pandid,
    --  NULL AS woonplaatsnaam,
    --  NULL AS nummeraanduidingid,
    --  NULL AS openbareruimteid,
      pand.postcode AS postcode,
      pand.huisnr AS huisnummer,
      pand.huislt AS huisletter,
      pand.toev AS huisnummertoevoeging  
    FROM vgvob@pgvg pand
    WHERE pand.type_vbo = 10            -- bag_pand
  ) bagobjecten
  ON bagobjecten.ident_bag = waatgb.ident_vob
*/ 
/*
SELECT
  WAATON.IDENT_WOZ AS wozobjectnummer,
  WAATON.ONDERDEEL AS nummerwozdeelobject,
  WAATAX.GEBRUIK_WOZ AS wozgebruikscode,
  WAATON.SOORT_OBJECT AS duwozcode,
  WAATON.BOUWJAAR AS bouwjaardeelobject,
  WAATON.OPPERVLAKTE AS gebruiksoppervlakte,
  WAATON.INHOUD AS brutoinhouddeelobject,
  waatgb.IDENT_WOZ,
  waatgb.VOLGNR_TAX,  
--  waatgb.VOORSCHRIFT_TON,
--  waatgb.ONDERDEEL_TON,
--  bagobjecten.ident_vob,
  bagobjecten.adresseerbaarobjectid,
  bagobjecten.pandid,
--  NULL AS bagobjecten.woonplaatsnaam,
--  NULL AS bagobjecten.nummeraanduidingid,
--  NULL AS bagobjecten.openbareruimteid,
  bagobjecten.postcode,
  bagobjecten.huisnummer,
  bagobjecten.huisletter,
  bagobjecten.huisnummertoevoeging  
FROM WAATON@pgvg WAATON
JOIN vg.VGWOZ@pgvg VGWOZ
  ON VGWOZ.IDENT = WAATON.IDENT_WOZ
LEFT JOIN vg.WAATAX@pgvg WAATAX
  ON WAATAX.IDENT_WOZ = WAATON.IDENT_WOZ
  AND WAATAX.VOLGNR = WAATON.VOLGNR_TAX
LEFT JOIN waatgb@pgvg waatgb
  ON waatgb.IDENT_WOZ = WAATON.IDENT_WOZ
  AND waatgb.VOLGNR_TAX = WAATON.VOLGNR_TAX  
LEFT JOIN 
(
  SELECT 
    verblijfsobject.ident AS ident_bag,
    verblijfsobject.ident_bag AS adresseerbaarobjectid,
    pand.ident_bag AS pandid,
  --  NULL AS woonplaatsnaam,
  --  NULL AS nummeraanduidingid,
  --  NULL AS openbareruimteid,
    verblijfsobject.postcode AS postcode,
    verblijfsobject.huisnr AS huisnummer,
    verblijfsobject.huislt AS huisletter,
    verblijfsobject.toev AS huisnummertoevoeging  
  FROM vgvob@pgvg verblijfsobject
  LEFT JOIN vgvob@pgvg pand
    ON pand.ident = verblijfsobject.ident_pnd
    AND pand.type_vbo = 10            -- bag_pand
  WHERE verblijfsobject.type_vbo = 1  -- bag_verblijfsobject
  UNION
  SELECT 
    pand.ident AS ident_bag,
    NULL AS adresseerbaarobjectid,
    pand.ident_bag AS pandid,
  --  NULL AS woonplaatsnaam,
  --  NULL AS nummeraanduidingid,
  --  NULL AS openbareruimteid,
    pand.postcode AS postcode,
    pand.huisnr AS huisnummer,
    pand.huislt AS huisletter,
    pand.toev AS huisnummertoevoeging  
  FROM vgvob@pgvg pand
  WHERE pand.type_vbo = 10            -- bag_pand
) bagobjecten
  ON bagobjecten.ident_bag = waatgb.ident_vob
-- kan ook via WAATAX en peildatum
WHERE VGWOZ.DATUM_EINDE IS NULL
AND WAATON.VOLGNR_TAX = (  
  SELECT MAX(volgnummer.VOLGNR_TAX)
  FROM WAATON@pgvg volgnummer
  WHERE volgnummer.IDENT_WOZ = WAATON.IDENT_WOZ
  AND volgnummer.ONDERDEEL = WAATON.ONDERDEEL
)
--AND WAATON.ident_woz = 190000000165
ORDER BY WAATON.IDENT_WOZ, WAATON.ONDERDEEL, WAATON.VOLGNR_TAX
*/
--------------------------------------------------------------------------------
SELECT
  WAATON.IDENT_WOZ AS wozobjectnummer,
  WAATON.ONDERDEEL AS nummerwozdeelobject,
  WAATAX.GEBRUIK_WOZ AS wozgebruikscode,
  WAATON.SOORT_OBJECT AS duwozcode,
  WAATON.BOUWJAAR AS bouwjaardeelobject,
  WAATON.OPPERVLAKTE AS gebruiksoppervlakte,
  WAATON.INHOUD AS brutoinhouddeelobject,
  --waatgb.IDENT_WOZ,
  --waatgb.VOLGNR_TAX,  
--  waatgb.VOORSCHRIFT_TON,
--  waatgb.ONDERDEEL_TON,
--  bagobjecten.ident_vob,
  bagobjecten.adresseerbaarobjectid,
  bagobjecten.pandid,
--  NULL AS bagobjecten.woonplaatsnaam,
--  NULL AS bagobjecten.nummeraanduidingid,
--  NULL AS bagobjecten.openbareruimteid,
  bagobjecten.postcode,
  bagobjecten.huisnummer,
  bagobjecten.huisletter,
  bagobjecten.huisnummertoevoeging  
FROM WAATON@pgvg WAATON
JOIN vg.VGWOZ@pgvg VGWOZ
  ON VGWOZ.IDENT = WAATON.IDENT_WOZ
LEFT JOIN vg.WAATAX@pgvg WAATAX
  ON WAATAX.IDENT_WOZ = WAATON.IDENT_WOZ
  AND WAATAX.VOLGNR = WAATON.VOLGNR_TAX
LEFT JOIN waatgb@pgvg waatgb
  ON waatgb.IDENT_WOZ = WAATON.IDENT_WOZ
  AND waatgb.VOLGNR_TAX = WAATON.VOLGNR_TAX  
  AND waatgb.ONDERDEEL_TON = WAATON.ONDERDEEL
LEFT JOIN 
(
  SELECT 
    verblijfsobject.ident AS ident_bag,
    verblijfsobject.ident_bag AS adresseerbaarobjectid,
    pand.ident_bag AS pandid,
  --  NULL AS woonplaatsnaam,
  --  NULL AS nummeraanduidingid,
  --  NULL AS openbareruimteid,
    verblijfsobject.postcode AS postcode,
    verblijfsobject.huisnr AS huisnummer,
    verblijfsobject.huislt AS huisletter,
    verblijfsobject.toev AS huisnummertoevoeging  
  FROM vgvob@pgvg verblijfsobject
  LEFT JOIN vgvob@pgvg pand
    ON pand.ident = verblijfsobject.ident_pnd
    AND pand.type_vbo = 10            -- bag_pand
  WHERE verblijfsobject.type_vbo = 1  -- bag_verblijfsobject
  AND verblijfsobject.DATUM_EINDE IS NULL
  UNION
  SELECT 
    pand.ident AS ident_bag,
    NULL AS adresseerbaarobjectid,
    pand.ident_bag AS pandid,
  --  NULL AS woonplaatsnaam,
  --  NULL AS nummeraanduidingid,
  --  NULL AS openbareruimteid,
    pand.postcode AS postcode,
    pand.huisnr AS huisnummer,
    pand.huislt AS huisletter,
    pand.toev AS huisnummertoevoeging  
  FROM vgvob@pgvg pand
  WHERE pand.type_vbo = 10            -- bag_pand
  AND pand.DATUM_EINDE IS NULL
) bagobjecten
  ON bagobjecten.ident_bag = waatgb.ident_vob
-- kan ook via WAATAX en peildatum
WHERE VGWOZ.DATUM_EINDE IS NULL
AND WAATON.VOLGNR_TAX = 
(  
  -- vind de laatste taxatie
  SELECT MAX(volgnummer.VOLGNR_TAX)
  FROM WAATON@pgvg volgnummer
  WHERE volgnummer.IDENT_WOZ = WAATON.IDENT_WOZ
)
--AND WAATON.ident_woz = 190000000165
ORDER BY WAATON.IDENT_WOZ, WAATON.ONDERDEEL, WAATON.VOLGNR_TAX
 