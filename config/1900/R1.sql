/*
R1:
	GBA - personen verhuisd (< 5 jaar, >2 mnd)

	Contact: A.Kraak@gemeenteswf.nl

	A-nummer*	
	BSN*	
	Voorvoegsel*	
	Geslachtsnaam*	
	Geboortedatum*	
	Geboorteplaats*	
	Geslacht*	
	Postcode*	
	Straatnaam	
	Huisnummer*	
	Huisletter*	
	Huisnummertoevoeging*	
	Datum inschrijving GBA andere gemeente*
*/
SELECT
  administratienummer AS anummer,    
  -- SOFI_NUMMER
  bsn,  
  voornaam,
  voorvoegsel,
  geslachtsnaam,
  geboortedatum,
  geboorteplaatsnaam AS geboorteplaats,
  kode_geslacht AS geslacht,
  postkode_numeriek || postkode_alfanumeriek AS postcode,  
  straatnaam,
  huisnummer,
  huisletter,	
  toevoeging_huisnummer AS huisnummertoevoeging,
  datum_uitschrijving_gemeente AS datum_uitschrijving
FROM GBA_VSELARCP@PPIV
-- overleden WHERE NOT DATUM_OVERLIJDEN IS NULL
-- verhuist WHERE NOT GEMEENTEKODE_WAARNR_VERTROKKEN IS NULL
-- WHERE DATUM_OVERLIJDEN IS NULL
-- >2maand
WHERE DATUM_UITSCHRIJVING_GEMEENTE < TO_CHAR(ADD_MONTHS(sysdate, -2), 'YYYYMMDD')
-- <5jaar
AND DATUM_UITSCHRIJVING_GEMEENTE > TO_CHAR(ADD_MONTHS(sysdate, -12 * 5), 'YYYYMMDD')