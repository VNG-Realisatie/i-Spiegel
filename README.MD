# I-spiegel 3.0

##  Gegevens vergelijken - waarom?. 
Binnen de gemeente hebben we te maken met veel persoons- en adresgegevens. Deze gegevens worden door de hele organisatie verspreid. Dit leidt tot vele processen en veel afnemende applicaties, die hier gebruik van maken. Het is van belang dat de gegevens binnen de ketens kloppen. Dus van begin tot eind. Om grip te krijgen op de kwaliteit kun je gebruik maken van de Gegevensvergelijker, oftewel de i-Spiegel 3.0. Hiermee vergelijk je de gegevens uit verschillende systemen met elkaar, dus de gegevens van de afnemende applicaties met de gegevens uit de bron. 

Afwijkingen kunnen ontstaan door allerlei oorzaken, bijvoorbeeld als gevolg van een update in de keten, onvoldoende beheer, verkeerde werkwijzen, stilstaande (technische) processen etc. De gegevensvergelijker kan leiden tot verbeteringen  in je gemeentelijke processen en werkinstructies. Ook stimuleert dit het contact tussen functioneel beheerders binnen een organisatie. 
Hiernaast geeft het ook inzicht in de trends van de ontwikkeling van de gegevenskwaliteit. De inspanningen om de kwaliteit te verbeteren zijn direct zichtbaar in de grafieken.

## Hoe werkt de iSpiegel 3.0?
Met behulp van de open source software (die gratis te downloaden is) maak je verbinding met de databases die je wilt vergelijken. Door de SQL statements naast elkaar te leggen, vergelijk je de verschillende velden met elkaar. De tool geeft vervolgens een percentage terug van de gegevens die overeen komen. Daarnaast wordt er ook een uitvallijst gemaakt, waarin de afwijkingen en verschillen te zien zijn.  Samen met de betreffende functioneel beheerder kunnen afspraken gemaakt worden over wat te doen met deze lijst. De percentages worden online gezet, op de website van de I-spiegel. Door de resultaten te publiceren, kan benchmarking plaatsvinden met andere gemeenten. Zo is in één blik duidelijk of er sprake is van een reëel percentage of dat het beter kan. 
Eenmaal ingericht kun je de query’s periodiek laten draaien. 

## Gebruikersgroep
Er is een gebruikersgroep, waarin ervaringen worden gedeeld, SQL query’s worden uitgewisseld en er wordt nagedacht over de verdere ontwikkeling van deze tool. 

## Hoe verhoudt de tool zich ivm. Architectuur en KING
De gegevensvergelijker krijgt zeker een positie in de architectuur. 
Het kwaliteitstool wordt ondergebracht binnen het thema gegevensmanagement op de gemmaonline website.
 
Het tactisch katern gegevensmanagement wordt ten aanzien van het onderdeel kwaliteit uitgebreid, en hierin wordt een verwijzing opgenomen naar de gegevensvergelijker (of beter gezegd de vernieuwde iSpiegel).

KING verwacht dat deze aanpassingen in januari 2018 gepubliceerd worden. 
Naast het tactisch katern zal ook een aparte pagina worden opgenomen in de gemmaonline waarin de functionaliteit van de gegevensvergelijker wordt beschreven. Naar deze pagina zal worden verwezen vanuit het tactisch katern gegevensmanagement. 

Voor het downloaden van de tool zal worden verwezen naar de iSpiegel website. KING ontvangt graag van ons een beschrijvende teksten ten aanzien van de functionaliteit van de gegevensvergelijker. 


## Applicatie
De applicatie is geschreven in C# en is een .NET console-applicatie, op deze manier kunnen we het elke dag geautomatiseerd laten draaien. De bron en vergelijkings queries, maar ook de resultaten van de vergelijking worden in een applicatie-specifieke-database opgeslagen.

Doordat er gebruik is gemaakt van de generieke database laag uit .NET maakt het niet uit welke database er wordt gebruikt. Bij de gemeente Súdwest-Fryslân wordt er voor de applicatie-specifieke-database Oracle gebruikt.
De vergelijkingen worden bij ons gedraaid over Oracle, MsSql, Mysql, maar ik heb ook getest met een Access database en een csv bestand.

Bij ons maakt de applicatie verbinding met de betreffende database en voert de query uit. We hebben hiervoor speciaal readonly-gebruikers gedefinieerd op de databases.
Eventueel zou je hier ook de bestaande i-spiegel exports (csv's) voor kunnen gebruiken om deze te vergelijken, maar dit moet dan wel ingeregeld worden.

Voor de sql maken we gebruik van bestaande rapportages om te kijken welke tabellen er gebruikt worden en hoe de structuur in elkaar zit.
De praktijk wijst uit dat je met deze informatie het beste zelf de queries kan schrijven. 
Het schrijven van deze querys laten we bewust door de functioneel beheerders van de betreffende vakapplicaties zelf doen.

## Werking
### Globaal
De tool wordt aangeroepen en haalt uit de database alle uit te voeren vergelijkingen.
Voor de verschillende vergelijken wordt er contact gelegd met de ingestelde database en worden de betreffende queries gedraait.
De output van de vergelijking wordt daarna opgeslagen in de database.

### Details
Na downloaden en uitpakken zijn er 5 directories aangemaakt, te weten:

#### bin : bevat de daadwerkelijke tool 
1. Gegevensvergelijker.exe : Deze tool start de SQL-queries op zoals deze in de database gedefinieerd staan.
2. Gegevensvergelijker.accdb
	- ISPIEGEL_DATABRON  : in veld QUERY van tabel ISPIEGEL_DATASOURCE (zie punt 5 van [Installatie] voor inrichting).
	- ISPIEGEL_VERGELIJKING : In tabel ISPIEGEL_VERGELIJING wordt in veld CONFIG vastgelegd welke 2 databronnen met elkaar vergeleken moeten worden worden en wat de matching criteria zijn per te vergelijken entry (zie punt 5 van [Installatie] voor inrichting).
	- ISPIEGEL_CONTROLE : Tabel ISPIEGEL_CHECK heeft als doel om controles te doen, de uitkomst van de regel wordt gecontroleerd tegen de referentie waarde.
	- ISPIEGEL_OUTPUT : Het resultaat van de vergelijkingen wordt weggeschreven in de tabellen ISPIEGEL_OUTPUT (globaal resultaat query) en
	- ISPIEGEL_OUTPUTLINE (detailresultaten query's).
3.  gegevensvergelijker.exe.config : De verbinding met de database staat in iSPIEGEL.exe.config (zie punt 2 onder [Installatie] voor inrichting).
4.  SAMPLE.ACCB bevat dummy-voorbeeld data die geraadpleegd wordt vanuit ISPIEGEL.ACCB om de werking van de datavergelijker te demonstreren. Deze bevat de volgende tabellen:
	- OC voorbeeld afnemersgegevens
	- R7.1 deel BAG-data verblijfsobjecten Bolsward (= voorbeeld bron-datat)
	- SIMPEL_ADRES voorbeeld aan object gekoppeld adresgegeven
	- SIMPEL_CHECK 
	- SIMPEL_PERSOON voorbeeld aan adres gekoppeld persoon	
Met de ISPIEGEL worden de volgende gegevens met elkaar vergeleken:
	- De data in OC wordt afgezet tegen bronbestand R7.1 definitie in 'C-Datakwaliteit BAG GBA')
	- De data in simpel_persoon wordt afgezet tegen bronbestand simpel_adres (definitie in 'SIMPEL_VERGELIJK')
5. bin\data  In de directory bin\data (niet te verwarren met de data-directory op het zelfde niveau als de bin-directory) worden tijdens draaien van de tool de resultaten van de query's weggeschreven. 

Per vergelijking wordt een directory met de naam van de vergelijking aangemaakt (in het gegeven voorbeeld de mappen
'C-Datakwaliteit BAG - GBA' en 'SIMPEL_VERGELIJK' zoals aangegeven in tabel ISPIEGEL_VERGELIJKING). 
Daarbinnen 2 .csv-bestanden (namen gelijk aan de query's) met daarin de resultaten van de bevragingen.
Bij een 2e (en volgende) keer draaien worden de resultaten toegevoegd aan de tabellen ISPIEGEL_OUTPUNT EN ISPIEGEL_OUTPUTLINE; de .csv's worden echter overschreven.
	
#### data : bevat de .sql's waarmee de benodigde tabellen aangemaakt kunnen worden indien wordt gekozen voor het opsalaan in een eigen database - zie [installatie]
bevat de .xlsx-en waarin voorbeelddata staat voor de inrichting van de database (dit geldt voor zowel de meegeleverde Access-database als een zelf 
aangemaakte) - zie [Inrichting]

#### doc
presentatie tool en filmpje over mogelijheden resultaten

#### resultaten
Bevat de resultaten van de verschillende gemeenten tot nu toe

#### src
broncode van de applicatie

## Installatie
    1.	De bestanden uit de directory: https://github.com/VNG-Realisatie/tree/master/bin moeten worden geplaatst in een aparte directory
    2.	Voor de koppeling naar de database waarin de data terecht komt, moet het configuratie bestand 'Ispiegel.exe.config' (een xml bestand in map 'bin')
    	worden aangepast.
    	Dit zijn de volgende onderdelen:
    	  a.	De database provider (leverancier en benaderingswijze database, te vinden onder de regel '<setting name="databaseprovider" serializeAs="String">'.
    			Pas de waarde tussen '<value>' en '</value>' aan.), bijvoorbeeld:
    			i.	System.Data.Odbc		(Microsoft Access)
    			ii.	System.Data.OleDb		(Microsoft Access)
    			iii.	System.Data.OracleClient	(Oracle)
    			iv.	Oracle.DataAccess.Client  	(Oracle)
    			v.	System.Data.SqlClient		(Microsoft SQL Server)
    			vi.	System.Data.SqlServerCe.3.5	(Microsoft SQL Server)
    			vii.	…
    	  b.	De connectiestring (= vindplaats en inloggegevens GGV-database waarin de definities staan en de resultaten worden opgeslagen), 
    			te vinden na '<add name="Ispiegel.Properties.Settings.databaseconnection" connectionString='
    			Zet achter 'Provider=' ....
    			Zet achter 'Data Source=' de vindplaats van de database.
    			zie https://www.connectionstrings.com/ voor connect-strings van specifieke databases.
    	  c.	De rest van de configuratie (inrichting e-mail voor notificatie na afloop draaien gegevensvergelijker)
    	  
    ------------ Optioneel ------------------------------------------------------------------------------------------------------------
    --- Indien gebruik wordt gemaakt van een andere database dan de standaard meegeleverde Access database ispiegel.accdb --
    
    Het is niet noodzakelijk om de access database (Gegevensvergelijker.accdb) te vervangen door een ander, maar vanuit de organisatie is dit meestal gewenst.
    3.	Creeer de database structuur door het uitvoeren van de https://github.com/VNG-Realisatie/tree/master/data/*.sql in de gebruikte database.
    ------------ Einde optioneel 

## Inrichting
1. Vul de database (tabellen GGV_DATASOURCE, GGV_COMPARE en GGV_CHECK) met de data uit de https://github.com/VNG-Realisatie/tree/master/data/*.xlsx bestanden voor die databronnen die je wilt vergelijken.
2. Localiseer de inrichting door aanpassen van dezelfde tabellen aan de lokale situatie:
	- 	Tabel: ISPIEGEL_CONTROLE	« vergelijking tussen 2 databronnen, gespecificeerd in een stukje xml
	- 	Tabel: ISPIEGEL_DATABRON	« databron van bv. bag-adressen, gba-adressen, gba-personen e.d.
	- 	Tabel: ISPIEGEL_VERGELIJKING	« conditionele test op een datasource	

Zie apart document doc/inrichting.xlsx voor een uitgebreidere beschrijving van de database en een toelichting op wat waar ingericht cq. aangepast moet worden.

## Gebruik
- Open een DOS-box via Start > Run > cmd
- Start op de commandprompt de GegevensVergelijker.exe op en kijk of alles succesvol verloopt. 
- Anders krijg je een email en een waarschuwing op de commandline wat er fout gaat.
- De resultaten zijn in de database te vinden in de volgende tabellen:
	- ISPIEGEL_OUTPUT              « totalen
	- ISPIEGEL_OUTPUTLINE          « details


bron van deze tekst is het bestand: readme.md
