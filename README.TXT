~ Gegevensvergelijker ~

[Doel]
Naar voorbeeld van de I-Spiegel applicatie van KING heb ik een tool gemaakt om 2 registrraties te vergelijken.

[Werking]
De tool wordt aangeroepen en haalt uit de database alle uit te voeren vergelijkingen.
Voor de verschillende vergelijken wordt er contact gelegd met de ingestelde database en worden de betreffende queries gedraait.
De output van de vergelijking wordt daarna opgeslagen in de database.

[Applicatie]
De applicatie is geschreven in C# en is .NET console-applicatie, op deze manier kunnen we het elke dag geautomatiseerd laten draaien.
De bron en vergelijkings queries, maar ook de resultaten van de vergelijking worden in een applicatie-specifieke-database opgeslagen.
Doordat er gebruik is gemaakt van de generieke database laag uit .NET maakt het niet uit welke database er wordt gebruikt.
Bij de gemeente Súdwest-Fryslân wordt er voor de applicatie-specifieke-database Oracle gebruikt.
De vergelijkingen worden bij ons gedraaid over Oracle, MsSql, Mysql, maar ik heb ook getest met een Access database en een csv bestand.

Bij ons maakt de applicatie verbinding met de betreffende database en voert het query uit, we hebben hiervoor speciaal readonly-gebruikers gedefinieerd op de databases
Eventueel zou je hier ook de bestaande i-spiegel exports (csv's) voor kunnen gebruiken om deze te vergelijken, maar dit moet dan wel ingeregeld worden.

Voor de sql maken we gebruik van bestaande rapportages om te kijken welke tabellen er gebruikt worden en hoe de structuur in elkaar zit.
De praktijk wijst uit dat je met deze informatie het beste zelf de queries kan schrijven. 
Het schrijven van deze querys laten we bewust door de functioneelbeheerders van de betreffende vakapplicaties zelf schrijven.

[Installatie]
1.	De bestanden uit de directory: https://github.com/EduardWitteveen/GegevensVergelijker/tree/master/bin moeten worden geplaatst in een aparte directory
2.	In het bestand GegevensVergelijker.exe.config (een xml bestand) moet de configuratie bestand worden aangepast.
	Dit zijn de volgende onderdelen:
	  a.	De database provider: databaseprovider , bijvoorbeeld:
			i.		System.Data.Odbc
			ii.		System.Data.OleDb
			iii.	System.Data.OracleClient
			iv.		Oracle.DataAccess.Client
			v.		System.Data.SqlClient
			vi.		System.Data.SqlServerCe.3.5
			vii.	…
	  b.	De connectiestring: GegevensVergelijker.Properties.Settings.databaseconnection , zie https://www.connectionstrings.com/ 
	  c.	De rest van de configuratie
	  
	  Het is niet noodzakelijk om de access database (Gegevensvergelijker.accdb) te vervangen door een ander, maar vanuit de organisatie is dit meestal gewenst.
	  
3.	Creeer de database structuur door het uitvoeren van de https://github.com/EduardWitteveen/GegevensVergelijker/tree/master/data/*.sql in de gebruikte database.
4.	Vul de database met de data uit de https://github.com/EduardWitteveen/GegevensVergelijker/tree/master/data/*.xlsx bestanden.
5.	Pas waar nodig de volgende bestanden aan in database:
		a.	Tabel: GGV_DATASOURCE 	« databron van bvb bag-adressen, gba-adressen, gba-personen ed.
		b.	Tabel: GGV_COMPARE      « vergelijking tussen 2 databronnen, gespecificeerd in een stukje xml
		c.	Tabel: GGV_CHECK        « conditionele test op een datasource
6.	Start > Run > cmd 
7.	Start op de commandprompt de GegevensVergelijker.exe op en kijk of alles succesvol verloopt. 
8.	Anders krijg je een email en een waarschuwing op de commandline wat er fout gaat.
9.	De resultaten zijn te vinden op de volgende plekken:
		a.	GGV_OUTPUT              « totalen
		b.	GGV_OUTPUTLINE          « details