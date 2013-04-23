/*
R5:
	GBA - personen met VOW-status
	
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
	Datum inschrijving laatst bekende woonadres*	
	Datum ingang status VOW*

*/
SELECT gba_vselarcp.ADMINISTRATIENUMMER,
  gba_vselarcp.BSN,
  gba_vselarcp.VOORVOEGSEL,
  gba_vselarcp.GESLACHTSNAAM,
  gba_vselarcp.GEBOORTEDATUM,
  gba_vselarcp.GEBOORTEPLAATSNAAM,
  gba_vselarcp.KODE_GESLACHT,
  gba_vselarcp.DATUM_VERTREK_UIT_NEDERLAND
FROM gba_vselarcp@PPIV
WHERE gba_vselarcp.KODE_LAND_WAARNAAR_VERTROKKEN = 0
