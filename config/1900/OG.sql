/*
OG:
	OZB - actuele vestigingen van organisaties
	
	Contact: A.vanderSchaar@gemeenteswf.nl

	RSIN	
	KvK-nummer	
	Vestigingnummer	
	Handelsnaam vestiging**	
	(Statutaire) naam rechtspersoon**	
	Bezoekadres postcode**	
	Bezoekadres huisnummer**	
	Bezoekadres huisletter**	
	Bezoekadres huisnummertoevoeging**	
	Postadres straatnaam**	
	Postadres postcode**	
	Postadres huisnummer**	
	Postadres huisletter**	
	Postadres huisnummertoevoeging**

*/
SELECT
-- pvs_tvergunning.kgbr,
-- pvs_tvergunning.rvergunning,
-- pvs_tvergunning.nbestuurder,
------------------------------
sub_tnnpers.nnnp,
sub_tnnpers.nnnpstat,
------------------------------
adr_tadres.nstr,
adr_tadres.rhs,
adr_tadres.nhsrlt,
adr_tadres.khsrtv,
adr_tadres.kpstnum || adr_tadres.kpstalf AS postcode,
adr_tadres.nwpl,
adr_tadres.nwplbag,
adr_tadres.oidnnummeraand,
adr_tadres.identtgo
------------------------------
FROM pvs_tvergunning@PPAR
JOIN sub_tnnpers@PPAR
ON sub_tnnpers.rsbj = pvs_tvergunning.rsbj
-- AND sub_tnnpers.deinde IS NULL
JOIN sub_tsbjadr@PPAR
ON sub_tsbjadr.rsbj = pvs_tvergunning.rsbj
AND sub_tsbjadr.deinde IS NULL
JOIN adr_tadres@PPAR
ON adr_tadres.rsysadr = sub_tsbjadr.rsysadr
AND adr_tadres.deinde IS NULL
----------------------
