/*
R6:
	GBA - overleden personen (< 5 jaar)

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
	Datum overlijden*
*/
SELECT gba_vselarcp.DATUM_OVERLIJDEN,
  gba_vselarcp.ADMINISTRATIENUMMER,
  gba_vselarcp.BSN,
  gba_vselarcp.VOORVOEGSEL,
  gba_vselarcp.GESLACHTSNAAM,
  gba_vselarcp.GEBOORTEDATUM,
  gba_vselarcp.GEBOORTEPLAATSNAAM,
  gba_vselarcp.KODE_GESLACHT,
  gba_vselarcp.POSTKODE_NUMERIEK,
  gba_vselarcp.POSTKODE_ALFANUMERIEK,
  gba_vselarcp.STRAATNAAM,
  gba_vselarcp.STRAATNAAM_ZD,
  gba_vselarcp.STRAATNAAM_TLX,
  gba_vselarcp.HUISNUMMER,
  gba_vselarcp.HUISLETTER,
  gba_vselarcp.TOEVOEGING_HUISNUMMER,
  gba_vselarcp.AANDUIDING_HUISNUMMER,
  gba_vselarcp.GEMEENTEKODE_INSCHRIJVING
FROM gba_vselarcp@PPIV
WHERE NOT gba_vselarcp.DATUM_OVERLIJDEN IS NULL
