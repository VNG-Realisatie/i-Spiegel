REM === BAG GBA VERGELIJKING ===
bin\RegistratieVergelijker.exe config\BAG_GBA.xrv > log\BAG_GBA.LOG
REM === EN I-SPIEGEL BESTAND KLAARZETTEN
start i-spiegel\i-SpiegelBatch.exe config\BAG_GBA.i-spiegel

REM === BAG PBS VERGELIJKING ===
bin\RegistratieVergelijker.exe config\BAG_PBS.xrv > log\BAG_PBS.LOG

REM === BAG WOZ VERGELIJKING ===
bin\RegistratieVergelijker.exe config\BAG_WOZ.xrv > log\BAG_WOZ.LOG
REM === EN I-SPIEGEL BESTAND KLAARZETTEN
start i-spiegel\i-SpiegelBatch.exe config\BAG_WOZ.i-spiegel

REM === BAG VERGUNNING VERGELIJKING ===
bin\RegistratieVergelijker.exe config\BAG_VERGUNNING.xrv > log\BAG_VERGUNNING.LOG
REM === EN I-SPIEGEL BESTAND KLAARZETTEN
start i-spiegel\i-SpiegelBatch.exe config\BAG_VERGUNNING.i-spiegel

REM === BAG PREVENT VERGELIJKING ===
bin\RegistratieVergelijker.exe config\BAG_PREVENT.xrv > log\BAG_PREVENT.LOG
REM === EN I-SPIEGEL BESTAND KLAARZETTEN
start i-spiegel\i-SpiegelBatch.exe config\BAG_PREVENT.i-spiegel

REM === BAG KVK VERGELIJKING ===
bin\RegistratieVergelijker.exe config\BAG_KVK_ZONDERACCENT.xrv > log\BAG_KVK_ZONDERACCENT.LOG

REM === BAG KEY2PARKEREN VERGELIJKING ===
bin\RegistratieVergelijker.exe config\GBA_K2P.xrv > log\GBA_K2P.LOG
bin\RegistratieVergelijker.exe config\DDS_NNP_K2P.xrv > log\DDS_NNP_K2P.LOG
bin\RegistratieVergelijker.exe config\DDS_NP_K2P.xrv > log\DDS_NP_K2P.LOG