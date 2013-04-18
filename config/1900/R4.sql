/*
R4:
	GBA - personen met een briefadres

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
	Datum inschrijving GBA op briefadres*
*/
SELECT 
  GBA_VSELAKTP.administratienummer AS anummer,
  GBA_VSELAKTP.bsn,
  GBA_VSELAKTP.voornaam,
  GBA_VSELAKTP.voorvoegsel,
  GBA_VSELAKTP.geslachtsnaam,
  GBA_VSELAKTP.geboortedatum,
  GBA_VSELAKTP.geboorteplaatsnaam AS geboorteplaats,
  GBA_VSELAKTP.kode_geslacht AS geslacht,
  GBA_VSELAKTP."POSTKODE_NUMERIEK" || GBA_VSELAKTP."POSTKODE_ALFANUMERIEK" AS postcode, 
  -- GBA_VSELAKTP.straatnaam,
  GBA_VSELADRS."NAAM_OPENBARE_RUIMTE" AS openbareruimtenaam,   
  GBA_VSELAKTP.huisnummer,
  GBA_VSELAKTP.huisletter,      
  GBA_VSELAKTP.toevoeging_huisnummer AS huisnummertoevoeging,
  GBA_VSELADRS."GEMEENTEDEEL" AS WOONPLAATS,
  GBA_VSELADRS."IDENT_VERBLIJFPLAATS", 
  GBA_VSELADRS."IDENT_NUMMERAANDUIDING",
  GBA_VSELAKTP."KODE_FUNKTIE_ADRES",
  GBA_VSELAKTP."DATUM_ADRESHOUDING"
FROM
  "PRODPIV"."GBA_VSELAKTP"@PPIV,
  "PRODPIV"."GBA_VSELADRS"@PPIV
WHERE GBA_VSELAKTP."SYSTEEMNUMMER_ADRES" = GBA_VSELADRS."SYSTEEMNUMMER_ADRES"
AND GBA_VSELAKTP."KODE_FUNKTIE_ADRES" = 'B'