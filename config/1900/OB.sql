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
  prso.gesnam,
  prso.gebdat,
  prso.gebgmn,
  prso.gesand,
  adro.sttnam_d,
  adro.obrnam_d,
  adro.huinum,
  adro.huilet,
  adro.huitvg,
  --adro.huiand,  
  adro.pkdnum,  
  adro.wplnam,
  adro.wplnam_bag,
  adro.identobr,
  adro.identna,
  adro.identtgo,
  vboo.identvbo
FROM
  DDS_PRS_OPSLAG@PDDS prso,
  DDS_PRSADR_OPSLAG@PDDS padr,  
  DDS_ADR_OPSLAG@PDDS adro,
  DDS_PREDIKAAT@PDDS pred,
  DDS_VBO_OPSLAG@PDDS vboo
WHERE prso.PRSNR = padr.PRSNR
AND padr.ADRNR = adro.ADRNR
AND (prso.indgba = 'J' OR prso.indbasispersoon = 'J')
AND padr.SRTADR = 'I'
AND PADR.DATEND is null
AND PRED.PREDIKAAT(+) = PRSO.ADLPRE
AND adro.ADRNR = vboo.ADRNR