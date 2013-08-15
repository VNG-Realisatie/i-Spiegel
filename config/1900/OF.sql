/*
OF:
	Vergunningen/handhaving - actuele adressen
	
	Contact: j.kemker@defriesemeren.nl

	Adresseerbaar object ID	
	Openbare ruimte ID	
	Nummeraanduiding ID	
	Straatnaam / Naam openbare ruimte*	
	Woonplaatsnaam*	
	Postcode*	
	Huisnummer*	
	Huisletter*	
	Huisnummertoevoeging*

*/
SELECT DISTINCT
	--IDENT AS VGVOB_IDENT,    
	VGVOB."STRAAT" AS openbareruimtenaam,
	VGVOB."HUISNR"  AS huisnummer, 
	VGVOB."HUISLT" AS huisletter,
	VGVOB."TOEV" AS huisnummertoevoeging,
	VGVOB."POSTCODE" AS postcode
FROM "VG"."VGVOB"@PGVG VGVOB