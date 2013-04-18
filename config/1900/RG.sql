/*
RG:
	NHR - actuele vestigingen binnen de gemeentegrenzen

	Contact: a.koole@gemeenteswf.nl

	KvK-nummer	
	RSIN	
	Vestigingnummer	
	Handelsnaam vestiging	
	(Statutaire) naam rechtspersoon	
	Rechtsvorm	
	Aanduiding H/N vestiging	
	SBI-code	
	Gemeentecode	
	Datum aanvang vestiging	
	Datum einde vestiging	
	-- BEZOEKADRES				
	Postcode bezoekadres
	Straatnaam bezoekadres	
	Huisnummer bezoekadres**	
	Huisletter bezoekadres	
	Huisnummertoevoeging bezoekadres*	
	-- POSTADRES
	Postcode postadres	
	Straatnaam postadres	
	Huisnummer postadres**	
	Huisletter postadres	
	Huisnummertoevoeging postadres*	
*/
SELECT
  dossiernummer AS kvknummer,
  'rsin onbekend' AS RSIN,
  'vestigingsnummer onbekend' AS vestigingnummer, 
  handelsnaam,
  aanspreektitel || ' ' || voorletters || ' '  || achternaam AS naamrechtspersoon,
  rechtsvorm,
  hoofdzaak AS vestigingstype,
  'sbicode onbekend' AS sbicode,
  1900 AS gemeentecode,
  datum_vestiging_huidig_adres AS datum_aanvang_vestiging, 
  NULL AS  datum_einde_vestiging, 
----  
  Straatnaam AS straatnaam_bezoek,  
  CASE WHEN toevoeging IS NULL THEN
    Huisnummer
  ELSE
    Huisnummer || toevoeging
  END AS huisnummer_bezoek,
  postkode AS postcode_bezoek,
  woonplaats AS woonplaats_bezoek,
----  
  Straatnaam_cor AS straatnaam_post,  
  CASE WHEN toevoeging_cor IS NULL THEN
    Huisnummer_cor
  ELSE
    Huisnummer_cor || toevoeging_cor
  END AS huisnummer_post,
  postkode_cor AS postcode_post,
  woonplaats_cor AS woonplaats_post
FROM KVK.kvk_nw
WHERE actief = 'JA'
-- AND dossiernummer = '01109553'
AND SUBSTR(postkode, 1, 4) IN 
(
  SELECT DISTINCT SUBSTR(postcode, 1, 4)
  FROM bag_actuele_adressen
)