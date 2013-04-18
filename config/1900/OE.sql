/*
OE:
	Reinigingsrechten en afvalstoffenheffing - actuele heffingsplichtige objecten
	
	Contact: h.raadsveld@gemeenteswf.nl

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
SELECT *
FROM PREVENT.ADDRESS@PREV ADDRESS
JOIN PREVENT.PLACE_STREET@PREV PLACE_STREET
ON ADDRESS.PLACE_STREET_ID = PLACE_STREET.ID_PLACE_STREET