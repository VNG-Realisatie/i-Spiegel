REM =========== Verwijder de oude data bestanden ===========
cd %~dp0
rmdir /s /q data
mkdir data

rmdir /s /q  i-spiegel\Uploadbestanden
rmdir /s /q  i-spiegel\Detailresultaten

REM =========== I-Spiegel vergelijkingen ===========
bin\RegistratieVergelijker.exe "config\1900\1.1-Buitengemeentelijke verhuizingen bijstand.xrv"
bin\RegistratieVergelijker.exe "config\1900\2-Verschil in burgelijke staat.xrv"
bin\RegistratieVergelijker.exe "config\1900\3-Geen kwijtschelding aangevraagd.xrv"
bin\RegistratieVergelijker.exe "config\1900\4-Uitkeringen op briefadres.xrv"
bin\RegistratieVergelijker.exe "config\1900\5.1-VOW - Bijstand.xrv"
bin\RegistratieVergelijker.exe "config\1900\6-Aanslagen naar overleden grondbezitters.xrv"
bin\RegistratieVergelijker.exe "config\1900\7.1-Oninbare vorderingen door niet bestaande adressen.xrv"
bin\RegistratieVergelijker.exe "config\1900\8.1-Inningen OZB BAG - OZB.xrv"
bin\RegistratieVergelijker.exe "config\1900\A-Datakwaliteit GBA - WOZ.xrv"
bin\RegistratieVergelijker.exe "config\1900\B-Datakwaliteit Prefill Gegevensmagazijn - GBA.xrv"
bin\RegistratieVergelijker.exe "config\1900\C-Datakwaliteit BAG - GBA.xrv"
bin\RegistratieVergelijker.exe "config\1900\D-Datakwaliteit BAG - WOZ.xrv"
bin\RegistratieVergelijker.exe "config\1900\E-Datakwaliteit reinigingsrechten en afstoffenheffing - BAG.xrv"
bin\RegistratieVergelijker.exe "config\1900\F-Datakwaliteit vergunningen- en handhavingssysteem - BAG.xrv"
bin\RegistratieVergelijker.exe "config\1900\G-Datakwaliteit NHR - OZB.xrv"

REM =========== NIET I-Spiegel vergelijkingen ===========
bin\RegistratieVergelijker.exe "config\1900\X-BAG_KVK.xrv"
bin\RegistratieVergelijker.exe "config\1900\X-BAG_PBS.xrv"
bin\RegistratieVergelijker.exe "config\1900\X-GBA_DDS.xrv"


REM =========== De batchdingen klaarzetten ===========
i-spiegel\i-SpiegelBatch.exe  "config\1900\1.1-Buitengemeentelijke verhuizingen bijstand.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\2-Verschil in burgelijke staat.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\4-Uitkeringen op briefadres.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\A-Datakwaliteit GBA - WOZ.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\C-Datakwaliteit BAG - GBA.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\D-Datakwaliteit BAG - WOZ.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\E-Datakwaliteit reinigingsrechten en afstoffenheffing - BAG.i-spiegel"
i-spiegel\i-SpiegelBatch.exe  "config\1900\F-Datakwaliteit vergunningen- en handhavingssysteem - BAG.i-spiegel"