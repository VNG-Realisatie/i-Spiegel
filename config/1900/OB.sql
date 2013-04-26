/*
OB:
	Gegevensmagazijn - actuele personen
	
	Contact: a.koole@gemeenteswf.nl

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
*/
SELECT 
  prso.admnum,
  prso.sofnum,
  prso.gesvvg,
  dds_tekensetconversie.teletex2unicode@PDDS(prso.gesnam_d) AS geslachtsnaam,
  prso.gebdat,
  dds_tekensetconversie.teletex2unicode@PDDS(dds_gemeente.gemeente) AS geboortegemeente,
  prso.gesand,
  --adro.sttnam_d,
  dds_tekensetconversie.teletex2unicode@PDDS(adro.obrnam_d) AS openbareruimtenaam,
  adro.huinum,
  adro.huilet,
  adro.huitvg,
  --adro.huiand,  
  adro.pkdnum,  
  --adro.wplnam,
  dds_tekensetconversie.teletex2unicode@PDDS(adro.wplnam_bag) AS woonplaats,
  adro.identobr,
  adro.identna,
  adro.identtgo,
  vboo.identvbo
FROM
  DDS_PRS_OPSLAG@PDDS prso,
  DDS_PRSADR_OPSLAG@PDDS padr,  
  DDS_ADR_OPSLAG@PDDS adro,
  DDS_PREDIKAAT@PDDS pred,
  DDS_VBO_OPSLAG@PDDS vboo,
  DDS_GEMEENTE@PDDS dds_gemeente
WHERE prso.PRSNR = padr.PRSNR
AND padr.ADRNR = adro.ADRNR
AND (prso.indgba = 'J' OR prso.indbasispersoon = 'J')
AND padr.SRTADR = 'I'
AND PADR.DATEND is null
AND PRED.PREDIKAAT(+) = PRSO.ADLPRE
AND adro.ADRNR = vboo.ADRNR
AND prso.gebgmn(+) = to_char(dds_gemeente.gemeentecode)