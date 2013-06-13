/*
OA:
	WOZ - actuele eigenaren WOZ-objecten (woningen)

	Contact: attie.zijlstra@gemeenteswf.nl

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
select 
  DISTINCT
  PBSPSN.A_NUMMER, 
  PBSPSN."SOFI_NR", 
  PBSPSN.VVOEG_VVG,
  PBSPSN.NAAM_VERKORT,
  TO_CHAR(PBSPSN.GEB_DATUM, 'YYYYMMDD') AS geboortedatum,  
  'niet bekend' AS Geboorteplaats,
  PBSPSN."GESLACHT",
  PBSPBT."POSTKODE",
  PBSPBT."STRAAT",
  PBSPBT."HUISNR",
  PBSPBT."HUISLT",
  PBSPBT."HUISTV"
from VG.VGBEL@PGVG 
LEFT OUTER JOIN VG.F5SUB1@PGVG 
   on VGBEL."STAMNR_SUB"=F5SUB1."STAMNUMMER"
LEFT OUTER JOIN VG.PBSPSN@PGVG 
  on F5SUB1."MICRO_VLG"=PBSPSN."REG_NR"
LEFT OUTER JOIN VG.PBSPBT@PGVG 
  on PBSPSN."IDENT_PBT"=PBSPBT."IDENT"
WHERE VGBEL."DATUM_EINDE" is null 
AND VGBEL."CODE_BEL"='E'
AND PBSPSN.SOORT_PERSOON = '1'
AND NOT A_NUMMER IS NULL
AND PBSPBT.GEMKD_GEM = 1900
-- AND PBSPSN.A_NUMMER = 7306070678 