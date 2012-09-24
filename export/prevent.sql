-------------------------------------------
-- Prevent database
-------------------------------------------
DROP  MATERIALIZED VIEW dl_prevent_address;
CREATE MATERIALIZED VIEW dl_prevent_address
AS SELECT *
FROM PREVENT.ADDRESS@prev;

DROP MATERIALIZED VIEW dl_prevent_placestreet;
CREATE MATERIALIZED VIEW dl_prevent_placestreet
AS SELECT *
FROM PREVENT.PLACE_STREET@prev;