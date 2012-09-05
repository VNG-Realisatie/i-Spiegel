-------------------------------------------
-- gba_huidige_adres
------------------------------------------- 
CREATE MATERIALIZED VIEW dl_gba_tgbaadr
AS 
SELECT *
FROM gba_tgbaadr@ppiv;

CREATE MATERIALIZED VIEW gba_huidige_adres 
AS SELECT 
  gba_tgbaadr.kpst_num,
  gba_tgbaadr.kpst_alf, 
  gba_tgbaadr.rhs,
  gba_tgbaadr.nhsr_lt,
  gba_tgbaadr.khsr_tv,
  gba_tgbaadr.nopr,
  gba_tgbaadr.nid_nad,
  gba_tgbaadr.nid_vbo
FROM dl_gba_tgbaadr gba_tgbaadr
WHERE gba_tgbaadr.ibag   = 'J'
AND gba_tgbaadr.dadr_end IS NULL;