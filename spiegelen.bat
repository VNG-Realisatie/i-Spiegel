REM =========== Verwijder de oude data bestanden ===========
cd %~dp0
rmdir /s /q i-spiegel\Detailresultaten
rmdir /s /q i-spiegel\Sleutelbestanden
rmdir /s /q i-spiegel\Systeembestanden
rmdir /s /q i-spiegel\Uploadbestanden


REM =========== De batchdingen klaarzetten ===========
i-spiegel\i-SpiegelBatch.exe  "config\1900\1.1-Buitengemeentelijke verhuizingen bijstand.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\2-Verschil in burgelijke staat.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\4-Uitkeringen op briefadres.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\A-Datakwaliteit GBA - WOZ.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\C-Datakwaliteit BAG - GBA.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\D-Datakwaliteit BAG - WOZ.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\E-Datakwaliteit reinigingsrechten en afstoffenheffing - BAG.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\F-Datakwaliteit vergunningen- en handhavingssysteem - BAG.i-spiegel"