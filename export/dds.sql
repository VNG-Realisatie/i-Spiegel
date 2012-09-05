-------------------------------------------
-- dds_huidige_adres
------------------------------------------- 
DROP MATERIALIZED VIEW dl_dds_adr_opslag;
CREATE MATERIALIZED VIEW dl_dds_adr_opslag
AS SELECT *
FROM dds_adr_opslag@pdds;

DROP MATERIALIZED VIEW dds_huidige_adres;
CREATE MATERIALIZED VIEW dds_huidige_adres
AS SELECT
  dds_adr_opslag.obrnam,
  dds_adr_opslag.huinum,
  dds_adr_opslag.huilet,
  dds_adr_opslag.huitvg,
  dds_adr_opslag.pkdnum,
  dds_adr_opslag.identna,
  dds_adr_opslag.identtgo,
  dds_adr_opslag.wplnam_bag
FROM dl_dds_adr_opslag dds_adr_opslag
WHERE dds_adr_opslag.indautadr = 'J'
AND dds_adr_opslag.datend       IS NULL;