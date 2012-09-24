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

CREATE MATERIALIZED VIEW prevent_huidige_adres 
AS SELECT * 
FROM dl_prevent_address ADDRESS
JOIN dl_prevent_placestreet PLACE_STREET
ON ADDRESS.PLACE_STREET_ID = PLACE_STREET.ID_PLACE_STREET;
