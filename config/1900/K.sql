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