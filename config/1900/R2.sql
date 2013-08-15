/*
R2:
	GBA - actuele personen

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
	Datum huwelijkssluiting/geregistreerd partnerschap* 
	Datum ontbinding huwelijkssluiting/geregistreerd partnerschap*
*/
/*
SELECT 
  GBA_VSELAKTP.administratienummer AS anummer,
  GBA_VSELAKTP.bsn,
  GBA_VSELAKTP.voornaam,
  GBA_VSELAKTP.voorvoegsel,
  GBA_VSELAKTP.geslachtsnaam,
  GBA_VSELAKTP.geboortedatum,
  GBA_VSELAKTP.geboorteplaatsnaam AS geboorteplaats,
  GBA_VSELAKTP.kode_geslacht AS geslacht,
  GBA_VSELAKTP.POSTKODE_NUMERIEK || GBA_VSELAKTP.POSTKODE_ALFANUMERIEK AS postcode, 
  -- GBA_VSELAKTP.straatnaam,
  GBA_VSELADRS."NAAM_OPENBARE_RUIMTE" AS openbareruimtenaam,   
  GBA_VSELAKTP.huisnummer,
  GBA_VSELAKTP.huisletter,      
  GBA_VSELAKTP.toevoeging_huisnummer AS huisnummertoevoeging,
  GBA_VSELADRS.GEMEENTEDEEL AS WOONPLAATS,
  GBA_VSELADRS.IDENT_VERBLIJFPLAATS, 
  GBA_VSELADRS.IDENT_NUMMERAANDUIDING,
  GBA_VSELHUW.DATUM_HUWELIJK,
  GBA_VSELHUW.DATUM_HUWELIJKSONTBINDING
FROM
  PRODPIV.GBA_VSELAKTP@PPIV
JOIN PRODPIV.GBA_VSELADRS@PPIV
  ON GBA_VSELAKTP.SYSTEEMNUMMER_ADRES = GBA_VSELADRS.SYSTEEMNUMMER_ADRES
JOIN PRODPIV.GBA_VSELHUW@PPIV
  ON GBA_VSELAKTP.SYSTEEMNUMMER_PERSOON = GBA_VSELHUW.SYSTEEMNUMMER_PERSOON
--WHERE GBA_VSELAKTP.administratienummer = 9245906875
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
  GBA_VSELAKTP.POSTKODE_NUMERIEK || GBA_VSELAKTP.POSTKODE_ALFANUMERIEK AS postcode, 
  -- GBA_VSELAKTP.straatnaam,
  GBA_VSELADRS."NAAM_OPENBARE_RUIMTE" AS openbareruimtenaam,   
  GBA_VSELAKTP.huisnummer,
  GBA_VSELAKTP.huisletter,      
  GBA_VSELAKTP.toevoeging_huisnummer AS huisnummertoevoeging,
  GBA_VSELADRS.GEMEENTEDEEL AS WOONPLAATS,
  GBA_VSELADRS.IDENT_VERBLIJFPLAATS, 
  GBA_VSELADRS.IDENT_NUMMERAANDUIDING,
  GBA_VSELHUW.DATUM_HUWELIJK,
  GBA_VSELHUW.DATUM_HUWELIJKSONTBINDING
FROM
  PRODPIV.GBA_VSELAKTP
JOIN PRODPIV.GBA_VSELADRS
  ON GBA_VSELAKTP.SYSTEEMNUMMER_ADRES = GBA_VSELADRS.SYSTEEMNUMMER_ADRES
JOIN PRODPIV.GBA_VSELHUW
  ON GBA_VSELAKTP.SYSTEEMNUMMER_PERSOON = GBA_VSELHUW.SYSTEEMNUMMER_PERSOON
--WHERE GBA_VSELAKTP.administratienummer = 9245906875