/*
OD:
	WOZ - actuele WOZ-objecten met een adres
	
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
-- met containers
SELECT 
  vgwoz.ident AS wozobject,
  pbspbt.ident_tgo Adresseerbaar_object_ID, 
  pbspbt.ident_na nummeraanduiding_ID, 
  pbspbt.ident_obr Openbareruimte_ID,
  vgwoz.straat ruimtenaam,
  pbspbt.woonplaats woonplaatsnaam,
  vgwoz.postcode, 
  vgwoz.huisnr, 
  vgwoz.huislt, 
  vgwoz.toev
FROM 
    vg.vgwoz@PGVG, 
    vg.pbspbt@PGVG, 
    vg.waatax@PGVG 
WHERE  vgwoz.datum_einde IS NULL
AND waatax.peildatum_tyd = '01-01-12'
AND waatax.gebruik_woz not in ('80')
AND waatax.ident_woz=vgwoz.ident
AND vgwoz.ident_pbt = pbspbt.ident
ORDER BY pbspbt.ident_tgo
/*
-- zonder containers
SELECT 
  vgwoz.ident AS wozobject,
  pbspbt.ident_tgo Adresseerbaar_object_ID, 
  pbspbt.ident_na nummeraanduiding_ID, 
  pbspbt.ident_obr Openbareruimte_ID,
  vgwoz.straat ruimtenaam,
  pbspbt.woonplaats woonplaatsnaam,
  vgwoz.postcode, 
  vgwoz.huisnr, 
  vgwoz.huislt, 
  vgwoz.toev
FROM 
  vg.vgwoz@PGVG, 
  vg.pbspbt@PGVG
WHERE vgwoz.datum_einde IS NULL
AND vgwoz.ident_pbt = pbspbt.ident
order by pbspbt.ident_tgo
*/
