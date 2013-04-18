/*
O1.1:
	Uitkeringsregistratie - actuele gebruikers Algemene Bijstand
	
	Contact: b.kamstra@gemeenteswf.nl

	A-nummer
	BSN	
	Voorvoegsel*
	Geslachtsnaam*
	Geboortedatum
	Geboorteplaats
	Geslacht*
	Postcode*
	Straatnaam
	Huisnummer*
	Huisletter*
	Huisnummertoevoeging*
	Datum inschrijving adres*
	Datum aanvang uitkering*
	Maandelijks bedrag uitkering*
*/
SELECT 
  szclient.a_nummer,
  szclient.sofi_nummer AS bsn,
  szclient.voorvoegsels, 
  szclient.naam AS geslachtsnaam, 
  TO_CHAR(szclient.dd_geboorte, 'YYYYMMDD') AS geboortedatum,
  szclient.geboorteplaats, 
  szclient.ind_geslacht AS geslacht, 
  REPLACE(szclient.postkode, ' ', '') AS postcode,
  szclient.straat, 
  szclient.huisnummer, 
  szclient.huisletter, 
  szclient.huisnr_toev, 
  TO_CHAR(szclient.dd_begin_adr, 'YYYYMMDD') AS datum_inschrijving_adres, 
  TO_CHAR(szdos.dd_oorspr, 'YYYYMMDD') AS datum_aanvang_uitkering,
  1000.00 AS maandelijks_bedrag_uitkering
FROM
  prod.szclient@PGWS
JOIN prod.szdos@PGWS
  ON szdos.clientnr = szclient.clientnr
WHERE szdos.kode_regeling = 0
AND szdos.dd_st_per_alg <= sysdate 
AND 
(
  szdos.dd_end_per_alg IS NULL 
OR 
  szdos.dd_end_per_alg >= sysdate
)
AND szdos.gemeentekode = 1900