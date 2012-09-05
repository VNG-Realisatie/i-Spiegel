-------------------------------------------
-- bag_huidige_woonplaats
-------------------------------------------
DROP  MATERIALIZED VIEW dl_bag_woonplaats;
CREATE MATERIALIZED VIEW dl_bag_woonplaats
AS SELECT *
FROM bagowner.bra_woonplaats@pbag;

DROP MATERIALIZED VIEW dl_bag_woonplaatsgeoinformatie;
CREATE MATERIALIZED VIEW dl_bag_woonplaatsgeoinformatie
AS SELECT *
FROM bagowner.bra_woonplaatsgeoinformatie@pbag;

DROP MATERIALIZED VIEW bag_huidige_woonplaatsen;
CREATE MATERIALIZED VIEW bag_huidige_woonplaatsen
AS SELECT
  bra_woonplaats.ddingang,
  bra_woonplaats.woonplaatsnaam,
  bra_woonplaatsgeoinformatie.*
FROM 
  dl_bag_woonplaatsgeoinformatie bra_woonplaatsgeoinformatie, 
  dl_bag_woonplaats bra_woonplaats
WHERE bra_woonplaatsgeoinformatie.woonplaatsnr = bra_woonplaats.woonplaatsnr
  AND bra_woonplaats.wplvolgnr = bra_woonplaatsgeoinformatie.wplvolgnr
  AND bra_woonplaats.ddeinde IS NULL
  AND bra_woonplaats.indactief = 'J';
  
DELETE FROM USER_SDO_GEOM_METADATA WHERE TABLE_NAME = 'bag_huidige_woonplaatsen';
INSERT INTO USER_SDO_GEOM_METADATA (
  TABLE_NAME, COLUMN_NAME, DIMINFO, SRID
)
VALUES (
  'bag_huidige_woonplaatsen', 'LIGGING',
  MDSYS.SDO_DIM_ARRAY (
    MDSYS.SDO_DIM_ELEMENT('X', -7000.000000000, 300000.000000000, 5.0E-5),
    MDSYS.SDO_DIM_ELEMENT('Y', 289000.000000000, 629000.000000000, 5.0E-5)
  ),
  NULL
);
DROP INDEX bag_huidige_woonplaatsen_gi;
CREATE INDEX bag_huidige_woonplaatsen_gi
ON bag_huidige_woonplaatsen(LIGGING)
INDEXTYPE IS MDSYS.SPATIAL_INDEX
PARAMETERS (' SDO_INDX_DIMS=2 LAYER_GTYPE="POLYGON"');
-------------------------------------------
-- bag_huidige_vbo
-------------------------------------------
DROP MATERIALIZED VIEW dl_bag_ruimte;
CREATE MATERIALIZED VIEW dl_bag_ruimte
AS SELECT *
FROM bagowner.bra_ruimte@pbag;

DROP MATERIALIZED VIEW dl_bag_verblijfsobjectadres;
CREATE MATERIALIZED VIEW dl_bag_verblijfsobjectadres
AS SELECT *
FROM bagowner.bgr_verblijfsobjectadres@pbag;

DROP MATERIALIZED VIEW dl_bag_verblijfsobject;
CREATE MATERIALIZED VIEW dl_bag_verblijfsobject
AS SELECT *
FROM bagowner.bgr_verblijfsobject@pbag;

DROP MATERIALIZED VIEW dl_bag_adres;
CREATE MATERIALIZED VIEW dl_bag_adres
AS SELECT *
FROM bagowner.bra_adres@pbag;

DROP MATERIALIZED VIEW dl_bag_adresgeoinformatie;
CREATE MATERIALIZED VIEW dl_bag_adresgeoinformatie
AS SELECT *
FROM bagowner.bra_adresgeoinformatie@pbag;

-- DBMS_XMLGEN.CONVERT('field &amp;', 1) TAGS -- "1" means "UNESCAPE"
DROP MATERIALIZED VIEW bag_huidige_vbo;
CREATE MATERIALIZED VIEW bag_huidige_vbo 
 AS SELECT 
    bra_ruimte.ruimtenaam,
    bra_ruimte.ruimtenaam_boco,
    bra_adres.huisnr,
    bra_adres.huislt,
    bra_adres.toevoeging,
    bra_adres.postc_n,
    bra_adres.postc_a,
    bra_adres.adresid,
    bgr_verblijfsobject.verblijfsobjectid,
    bra_woonplaats.woonplaatsnaam,
    bra_adresgeoinformatie.ligging
  FROM 
    dl_bag_adres bra_adres,
    dl_bag_ruimte bra_ruimte,
    dl_bag_woonplaats bra_woonplaats,
    dl_bag_verblijfsobjectadres bgr_verblijfsobjectadres,
    dl_bag_verblijfsobject bgr_verblijfsobject,
    dl_bag_adresgeoinformatie bra_adresgeoinformatie
  WHERE bra_adres.ddeinde               IS NULL
  AND bra_adres.indactief               = 'J'
  AND bra_ruimte.ddeinde                IS NULL
  AND bra_ruimte.indactief              = 'J'
  AND bra_woonplaats.ddeinde            IS NULL
  AND bra_woonplaats.indactief          = 'J'
  AND bra_adres.indauthentiek           = 'J'
  AND bgr_verblijfsobject.ddeinde       IS NULL
  AND bgr_verblijfsobject.indactief     = 'J'
  AND bgr_verblijfsobject.indauthentiek = 'J' 
  AND bra_adres.ruimtenr                = bra_ruimte.ruimtenr
  AND bra_ruimte.woonplaatsnr           = bra_woonplaats.woonplaatsnr
  AND bra_adres.adresnr                 = bgr_verblijfsobjectadres.adresnr
  AND bgr_verblijfsobject.vobjnr        = bgr_verblijfsobjectadres.vobjnr
  AND bgr_verblijfsobject.vobjvolgnr    = bgr_verblijfsobjectadres.vobjvolgnr
  AND bra_adresgeoinformatie.adresnr    (+)= bra_adres.adresnr  -- right outer join, so we also have "nevenadressen"
  AND bra_adresgeoinformatie.adrvolgnr  (+)= bra_adres.adrvolgnr;
-------------------------------------------
-- bag_huidige_ligplaats
-------------------------------------------
DROP MATERIALIZED VIEW dl_bag_ligplaatsadres;
CREATE MATERIALIZED VIEW dl_bag_ligplaatsadres
AS SELECT *
FROM bagowner.bgr_ligplaatsadres@pbag;

DROP MATERIALIZED VIEW dl_bag_ligplaats;
CREATE MATERIALIZED VIEW dl_bag_ligplaats
AS SELECT *
FROM bagowner.bgr_ligplaats@pbag;

DROP MATERIALIZED VIEW dl_bag_ligplaatsgeoinformatie;
CREATE MATERIALIZED VIEW dl_bag_ligplaatsgeoinformatie
AS SELECT *
FROM bagowner.bgr_ligplaatsgeoinformatie@pbag;

DROP MATERIALIZED VIEW bag_huidige_ligplaats;
CREATE MATERIALIZED VIEW bag_huidige_ligplaats
AS SELECT 
    bra_ruimte.ruimtenaam,
    bra_ruimte.ruimtenaam_boco,
    bra_adres.huisnr,
    bra_adres.huislt,
    bra_adres.toevoeging,
    bra_adres.postc_n,
    bra_adres.postc_a,   
    bra_adres.adresid,
    bgr_ligplaats.ligplaatsid,
    bra_woonplaats.woonplaatsnaam,
    SDO_GEOM.SDO_POINTONSURFACE(bgr_ligplaatsgeoinformatie.ligging, 
      MDSYS.SDO_DIM_ARRAY (
        MDSYS.SDO_DIM_ELEMENT('X', -7000.000000000, 300000.000000000, 5.0E-5),
        MDSYS.SDO_DIM_ELEMENT('Y', 289000.000000000, 629000.000000000, 5.0E-5)
      )) AS ligging
  FROM
    dl_bag_adres bra_adres,
    dl_bag_ruimte bra_ruimte,
    dl_bag_woonplaats bra_woonplaats,
    dl_bag_ligplaatsadres bgr_ligplaatsadres,
    dl_bag_ligplaats bgr_ligplaats,
    dl_bag_ligplaatsgeoinformatie bgr_ligplaatsgeoinformatie
  WHERE bra_adres.ddeinde        IS NULL
  AND bra_adres.indactief           = 'J'
  AND bra_ruimte.ddeinde           IS NULL
  AND bra_ruimte.indactief          = 'J'
  AND bra_woonplaats.ddeinde       IS NULL
  AND bra_woonplaats.indactief      = 'J'
  AND bra_adres.indauthentiek       = 'J'
  AND bgr_ligplaats.ddeinde        IS NULL
  AND bgr_ligplaats.indactief       = 'J'
  AND bgr_ligplaats.indauthentiek   = 'J'
  AND bra_adres.ruimtenr            = bra_ruimte.ruimtenr
  AND bra_ruimte.woonplaatsnr       = bra_woonplaats.woonplaatsnr
  AND bra_adres.adresnr             = bgr_ligplaatsadres.adresnr
  AND bgr_ligplaats.ligplaatsnr     = bgr_ligplaatsadres.ligplaatsnr
  AND bgr_ligplaats.ligplaatsvolgnr = bgr_ligplaatsadres.ligplaatsvolgnr
  AND bgr_ligplaatsgeoinformatie.ligplaatsnr = bgr_ligplaats.ligplaatsnr
  AND bgr_ligplaatsgeoinformatie.ligplaatsvolgnr = bgr_ligplaats.ligplaatsvolgnr


-------------------------------------------
-- bag_huidige_standplaats
------------------------------------------- 
DROP MATERIALIZED VIEW dl_bag_standplaatsadres;
CREATE MATERIALIZED VIEW dl_bag_standplaatsadres
AS SELECT *
FROM bagowner.bgr_standplaatsadres@pbag;

DROP MATERIALIZED VIEW dl_bag_standplaats;
CREATE MATERIALIZED VIEW dl_bag_standplaats
AS SELECT *
FROM bagowner.bgr_standplaats@pbag;
  
DROP MATERIALIZED VIEW dl_bag_standplaatsgeo;
CREATE MATERIALIZED VIEW dl_bag_standplaatsgeo
AS SELECT *
FROM bagowner.bgr_standplaatsgeoinformatie@pbag;  
  
DROP MATERIALIZED VIEW bag_huidige_standplaats;
CREATE MATERIALIZED VIEW bag_huidige_standplaats
AS SELECT 
    bra_ruimte.ruimtenaam,
    bra_ruimte.ruimtenaam_boco,
    bra_adres.huisnr,
    bra_adres.huislt,
    bra_adres.toevoeging,
    bra_adres.postc_n,
    bra_adres.postc_a,
    bra_adres.adresid,
    bgr_standplaats.standplaatsid,
    bra_woonplaats.woonplaatsnaam,
    SDO_GEOM.SDO_POINTONSURFACE(bgr_standplaatsgeoinformatie.ligging, 
      MDSYS.SDO_DIM_ARRAY (
        MDSYS.SDO_DIM_ELEMENT('X', -7000.000000000, 300000.000000000, 5.0E-5),
        MDSYS.SDO_DIM_ELEMENT('Y', 289000.000000000, 629000.000000000, 5.0E-5)
      ))AS ligging
FROM 
    dl_bag_adres bra_adres,
    dl_bag_ruimte bra_ruimte,
    dl_bag_woonplaats bra_woonplaats,
    dl_bag_standplaatsadres bgr_standplaatsadres,
    dl_bag_standplaats bgr_standplaats,
    dl_bag_standplaatsgeo bgr_standplaatsgeoinformatie
WHERE bra_adres.ddeinde            IS NULL
AND bra_adres.indactief               = 'J'
AND bra_ruimte.ddeinde               IS NULL
AND bra_ruimte.indactief              = 'J'
AND bra_woonplaats.ddeinde           IS NULL
AND bra_woonplaats.indactief          = 'J'
AND bra_adres.indauthentiek           = 'J'
AND bgr_standplaats.ddeinde          IS NULL
AND bgr_standplaats.indactief         = 'J'
AND bgr_standplaats.indauthentiek     = 'J'
AND bra_adres.ruimtenr                = bra_ruimte.ruimtenr
AND bra_ruimte.woonplaatsnr           = bra_woonplaats.woonplaatsnr
AND bra_adres.adresnr                 = bgr_standplaatsadres.adresnr
AND bgr_standplaats.standplaatsnr     = bgr_standplaatsadres.standplaatsnr
AND bgr_standplaats.standplaatsvolgnr = bgr_standplaatsadres.standplaatsvolgnr 
AND bgr_standplaatsgeoinformatie.standplaatsnr = bgr_standplaats.standplaatsnr
AND bgr_standplaatsgeoinformatie.standplaatsvolgnr = bgr_standplaats.standplaatsvolgnr;
-------------------------------------------
-- bag_huidige_adres
------------------------------------------- 
CREATE OR REPLACE TYPE T_Geometry AS OBJECT ( 
   geometry  mdsys.sdo_geometry,
   tolerance number,
   order member function orderBy(p_compare_geom in T_Geometry)
   return number
);  
  
CREATE OR REPLACE TYPE BODY T_Geometry 
AS
   Order Member Function orderBy(p_compare_geom in T_Geometry)
   Return number
   Is
      v_geom         sdo_geometry;
      v_compare_geom sdo_geometry;
   Begin
      if (SELF.geometry is null) then
         return -1;
      elsif (p_compare_geom is null) Then
         return 1;
      end if;
      v_geom         := sdo_geom.sdo_centroid(SELF.geometry,SELF.tolerance);
      v_compare_geom := sdo_geom.sdo_centroid(p_compare_geom.geometry,SELF.tolerance);
      IF ( v_geom.sdo_point.x < v_compare_geom.sdo_point.x ) THEN
         RETURN -1;  -- any negative number will do
      ELSIF ( v_geom.sdo_point.x > v_compare_geom.sdo_point.x ) THEN 
         RETURN 1;   -- any positive number will do
      ELSIF ( v_geom.sdo_point.y < v_compare_geom.sdo_point.y ) THEN
         RETURN -1;  -- any negative number will do
      ELSIF ( v_geom.sdo_point.y > v_compare_geom.sdo_point.y ) THEN 
         RETURN 1;   -- any positive number will do
      ELSE 
         RETURN 0;
      END IF;
   END;
END;

DROP MATERIALIZED VIEW bag_huidige_adres;
CREATE MATERIALIZED VIEW bag_huidige_adres
AS 
  SELECT 
    'verblijfsobject' AS adrestype, 
    bag_huidige_vbo.ruimtenaam AS ruimtenaam,
    bag_huidige_vbo.ruimtenaam_boco AS ruimtenaam_boco,    
    bag_huidige_vbo.huisnr AS huisnr,
    bag_huidige_vbo.huislt AS huislt,
    bag_huidige_vbo.toevoeging AS toevoeging,
    bag_huidige_vbo.postc_n AS postc_n,
    bag_huidige_vbo.postc_a AS postc_a,
    bag_huidige_vbo.adresid AS adresid,  
    bag_huidige_vbo.verblijfsobjectid AS objectid,
    bag_huidige_vbo.woonplaatsnaam AS woonplaatsnaam
    --,bag_huidige_vbo.ligging
  FROM bag_huidige_vbo
--  ORDER BY t_geometry(bag_huidige_vbo.ligging, 5.0E-5)
UNION
  SELECT
    'ligplaats', 
    bag_huidige_ligplaats.ruimtenaam,
    bag_huidige_ligplaats.ruimtenaam_boco,
    bag_huidige_ligplaats.huisnr,
    bag_huidige_ligplaats.huislt,
    bag_huidige_ligplaats.toevoeging,
    bag_huidige_ligplaats.postc_n,
    bag_huidige_ligplaats.postc_a,
    bag_huidige_ligplaats.adresid,  
    bag_huidige_ligplaats.ligplaatsid,
    bag_huidige_ligplaats.woonplaatsnaam
  FROM bag_huidige_ligplaats
--ORDER BY t_geometry(bag_huidige_ligplaats.ligging, 5.0E-5)
UNION
  SELECT
    'standplaats', 
    -- bag_huidige_standplaats.*
    bag_huidige_standplaats.ruimtenaam,
    bag_huidige_standplaats.ruimtenaam_boco,
    bag_huidige_standplaats.huisnr,
    bag_huidige_standplaats.huislt,
    bag_huidige_standplaats.toevoeging,
    bag_huidige_standplaats.postc_n,
    bag_huidige_standplaats.postc_a,
    bag_huidige_standplaats.adresid,  
    bag_huidige_standplaats.standplaatsid,
    bag_huidige_standplaats.woonplaatsnaam    
  FROM bag_huidige_standplaats
--  ORDER BY t_geometry(bag_huidige_standplaats.ligging, 5.0E-5)