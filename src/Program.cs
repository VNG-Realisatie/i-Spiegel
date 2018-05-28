using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Xml.XPath;

namespace ISpiegel
{        
    static class Program
    {
        private static string ReplaceVariables(string incoming) {
            string result = incoming.Replace("${WORKING_DIRECTORY}", System.IO.Directory.GetCurrentDirectory());
            return result;
        }


        private static DataTable GetData(DbProviderFactory configprovider, DbConnection configconnection, String datasourcename)
        {

            // const string sql = "SELECT * FROM SOME_TABLE WHERE Name = @name";
            // cmd.CommandText = sql;
            // cmd.Parameters.AddWithValue("@name", name);


            var command = configprovider.CreateCommand();
            // TODO: parameter
            command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "databron WHERE databronnaam LIKE '" +  datasourcename + "'";
            command.Connection = configconnection;
            var adapter = configprovider.CreateDataAdapter();
            adapter.SelectCommand = command;
            var table = new DataTable();
            adapter.Fill(table);
            if (table.Rows.Count != 1) throw new Exception("Kon de datasource met naam:" + datasourcename + " niet vinden!");

            String datasource_provider = Convert.ToString( table.Rows[0]["provider"]);
            String datasource_connectionstring = Convert.ToString(table.Rows[0]["connectionstring"]);
            String datasource_query = Convert.ToString(table.Rows[0]["query"]);
            String applicatie = Convert.ToString(table.Rows[0]["applicatienaam"]);
            String gemeentecode = Convert.ToString(table.Rows[0]["gemeentecode"]);

            var datasource_factory = DbProviderFactories.GetFactory(datasource_provider);
            var datasource_connection = datasource_factory.CreateConnection();
            datasource_connection.ConnectionString = ReplaceVariables(datasource_connectionstring);
            datasource_connection.Open();
            var datasource_command = datasource_factory.CreateCommand();
            datasource_command.CommandText = datasource_query;
            datasource_command.Connection = datasource_connection;
            var datasource_adapter = datasource_factory.CreateDataAdapter();
            datasource_adapter.SelectCommand = datasource_command;
            var datasource_table = new DataTable();
            datasource_adapter.Fill(datasource_table);
            datasource_connection.Close();

            datasource_table.ExtendedProperties.Add("databronnaam", datasourcename);
            datasource_table.ExtendedProperties.Add("applicatienaam", applicatie);
            datasource_table.ExtendedProperties.Add("referentiequery", datasource_query);
            datasource_table.ExtendedProperties.Add("gemeentecode", gemeentecode);

            return datasource_table;
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // . as decimal-seperator, etc      
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            //TODO: 
            //      field xml
            Output.Info("***** START *****");
#if !DEBUG
            try
            {
#endif

            var provider = DbProviderFactories.GetFactory(Properties.Settings.Default.databaseprovider);
            var connection = provider.CreateConnection();
            connection.ConnectionString = ReplaceVariables(Properties.Settings.Default.databaseconnection);
            // If error.message == The 'Microsoft.ACE.OLEDB.12.0' provider is not registered on the local machine. ==> are we debugging in 32-bits (x86) mode?
            connection.Open();

            #region COMPARE
            var command = provider.CreateCommand();
            command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "vergelijking WHERE actief = -1";
            command.Connection = connection;
            var adapter = provider.CreateDataAdapter();
            adapter.SelectCommand = command;
            var table = new DataTable();
            adapter.Fill(table);
            foreach (DataRow comparerow in table.Rows)
            {
                string vergelijkingnaam = Convert.ToString(comparerow["vergelijkingnaam"]);
                string veldtoewijzing = Convert.ToString(comparerow["veldtoewijzing"]);
                string referentiedatabronnaam = Convert.ToString(comparerow["referentiedatabronnaam"]);
                string analysedatabronnaam = Convert.ToString(comparerow["analysedatabronnaam"]);

                Output.Info("START: " + vergelijkingnaam);
#if !DEBUG
                try
                {
#endif
                    // what shall we do with the console reporter?
                    DatabaseReporter reporter = new DatabaseReporter(provider, connection);

                    XPathDocument document = new XPathDocument(new StringReader(veldtoewijzing));
                    XPathNavigator compareconfig = document.CreateNavigator();
                    compareconfig = compareconfig.SelectSingleNode("/compare");

                    reporter.Start(
                        vergelijkingnaam,
                        referentiedatabronnaam,
                        analysedatabronnaam,
                        veldtoewijzing,
                        null,
                        null
                        );

                    // create the data sources
                    Output.Info("\t[" + referentiedatabronnaam + "] data will be loaded");
                    DataTable referencetable = GetData(provider, connection, referentiedatabronnaam);
                    RegistratieSource reference = new RegistratieSource(referencetable);
                    Output.Info("\t[" + referentiedatabronnaam + "] data loaded (#" + referencetable.Rows.Count + ")");

                    Output.Info("\t[" + analysedatabronnaam + "] data will be loaded");
                    DataTable analysetable = GetData(provider, connection, analysedatabronnaam);
                    RegistratieSource analysis = new RegistratieSource(analysetable);
                    Output.Info("\t[" + analysedatabronnaam + "] data loaded (#" + analysetable.Rows.Count + ")");

                    // check the columns (better error messages!)
                    #region matching
                    foreach (XPathNavigator field in compareconfig.Select("//field"))
                    {
                        XPathNavigator referencefield = field.SelectSingleNode("@reference-field");
                        if (referencefield != null && !reference.table.Columns.Contains(referencefield.Value))
                        {
                            Output.Warn("reference-column:" + field.SelectSingleNode("@reference-field").Value + " not found in:");
                            foreach (var name in reference.table.Columns) Output.Warn("\t" + name.ToString());
                            throw new InvalidDataException("reference-column:" + field.SelectSingleNode("@reference-field").Value + " not found ");
                        }
                        XPathNavigator analysisfield = field.SelectSingleNode("@analysis-field");
                        if (analysisfield != null && !analysis.table.Columns.Contains(analysisfield.Value))
                        {
                            Output.Warn("analysis-column:" + field.SelectSingleNode("@analysis-field").Value + " not found in:");
                            foreach (var name in analysis.table.Columns) Output.Warn("\t" + name.ToString());
                            throw new InvalidDataException("analysis-column:" + field.SelectSingleNode("@analysis-field").Value + " not found");
                        }
                    }
                    Output.Info("\t[check] field references correct");
                #endregion

                // export into csv, so we can use i-spiegel
                #region export csv
                /*
                {
                    DirectoryInfo exportdirectory = new DirectoryInfo("data");
                    if (!exportdirectory.Exists) exportdirectory.Create();
                    exportdirectory = new DirectoryInfo("data\\" + comparename);
                    if (!exportdirectory.Exists) exportdirectory.Create();

                    List<string> n = new List<string>();
                    List<string> r = new List<string>();
                    List<string> a = new List<string>();
                    foreach (XPathNavigator field in compareconfig.Select("//field"))
                    {
                        n.Add(field.SelectSingleNode("@name").Value);
                        if (field.SelectSingleNode("@reference-field") != null)
                        {
                            r.Add(field.SelectSingleNode("@reference-field").Value);
                        }
                        else
                        {
                            r.Add(null);
                        }
                        if (field.SelectSingleNode("@analysis-field") != null)
                        {
                            a.Add(field.SelectSingleNode("@analysis-field").Value);
                        }
                        else
                        {
                            a.Add(null);
                        }
                    }
                    Output.Info("\tSTART: exporting the data");
                    try
                    {
                        reference.Export(r, n, exportdirectory, compareconfig.SelectSingleNode("@reference").Value);
                        analysis.Export(a, n, exportdirectory, compareconfig.SelectSingleNode("@analysis").Value);

                        Output.Info("\tSTOP: exporting the data");
                    }
                    catch(Exception ex) {
                        Output.Warn("\tERROR: exporting the data", ex);
                    }
                }
                */
                #endregion

                // matches
                #region build matchers
                Dictionary<string, List<string>[]> matchers = new Dictionary<string, List<string>[]>();
                    foreach (XPathNavigator match in compareconfig.Select("match"))
                    {
                        String name = match.SelectSingleNode("@id").Value;
                        List<string> r = new List<string>();
                        List<string> a = new List<string>();
                        foreach (XPathNavigator field in match.Select("field"))
                        {
                            r.Add(field.SelectSingleNode("@reference-field").Value);
                            a.Add(field.SelectSingleNode("@analysis-field").Value);
                        }
                        List<string>[] ra = new List<string>[2];
                        ra[0] = r;
                        ra[1] = a;
                        matchers.Add(name, ra);
                    }
                    #endregion

                    // create compare array
                    #region build lookup
                    String primary = compareconfig.SelectSingleNode("@primary").Value;
                    SortedDictionary<RegistratieItem, DataRow> lookup = reference.GetSortedList(matchers[primary][0].ToArray());
                    Output.Info("\t[lookup] index succesfull");
                    #endregion

                    // now start the loop
                    foreach (System.Data.DataRow row in analysis.table.Rows)
                    {
                        // primary match
                        string[] analysisrows = matchers[primary][1].ToArray();
                        RegistratieItem matcher = analysis.GetFieldValues(row, analysisrows);

                        if (!lookup.ContainsKey(matcher))
                        {
                            reporter.EntryNotFound(vergelijkingnaam, primary, row, matcher);
                            continue;
                        }
                        System.Data.DataRow found = lookup[matcher];


                        bool fullmatch = true;
                        foreach (string matchername in matchers.Keys)
                        {
                            if (matchername != primary)
                            {
                                string[] analysisfields = matchers[matchername][1].ToArray();
                                string[] referencefields = matchers[matchername][0].ToArray();

                                RegistratieItem a = analysis.GetFieldValues(row, analysisfields);
                                RegistratieItem r = reference.GetFieldValues(found, referencefields);

                                if (!a.Equals(r))
                                {
                                    fullmatch = false;
                                    reporter.EntryNoMatch(vergelijkingnaam, row, matcher, found, matchername, a, r, matcher);
                                }
                            }
                        }
                        if (fullmatch)
                        {
                            reporter.EntryMatch(vergelijkingnaam, row, found, matcher);
                        }
                    }
                    reporter.Stop(
                            vergelijkingnaam,
                            reference.table.ExtendedProperties["applicatienaam"].ToString(),
                            analysis.table.ExtendedProperties["applicatienaam"].ToString(),
                            reference.table.ExtendedProperties["referentiequery"].ToString(),
                            analysis.table.ExtendedProperties["referentiequery"].ToString(),
                            reference.table.ExtendedProperties["gemeentecode"].ToString(),
                            analysis.table.ExtendedProperties["gemeentecode"].ToString(),
                            analysis.table.Rows.Count, 
                            reference.table.Rows.Count);


                Output.Info("STOP: " + vergelijkingnaam);
#if !DEBUG
                }
                catch (Exception ex)
                {
                    Output.Warn("ERROR PROCESSING: " + vergelijkingnaam, ex);
                }
#endif
            }
            #endregion COMPARE

            #region CHECK

            command = provider.CreateCommand();
            // "small detail", in access boolean: true = false and visaversa
            command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "controle WHERE actief = -1";
            command.Connection = connection;
            adapter = provider.CreateDataAdapter();
            adapter.SelectCommand = command;
            table = new DataTable();
            adapter.Fill(table);
            foreach (DataRow checkrow in table.Rows)
            {
                string vergelijkingnaam = Convert.ToString(checkrow["controlenaam"]);
                string datasourcename = Convert.ToString(checkrow["databronnaam"]);
                string primary = Convert.ToString(checkrow["sleutelkolom"]);
                string columnname = Convert.ToString(checkrow["controlekolom"]);
                string checkvalue = Convert.ToString(checkrow["controlewaarde"]);

                Output.Info("START: " + vergelijkingnaam);
#if !DEBUG
                try
                {
#endif
                    DatabaseReporter reporter = new DatabaseReporter(provider, connection);
                    reporter.Start(vergelijkingnaam, null, datasourcename, columnname + "='" + checkvalue + "'", null, datasourcename);
                    DataTable datatable = GetData(provider, connection, datasourcename);

                    foreach(DataRow datarow in datatable.Rows) {
                        if (Convert.ToString(datarow[columnname]).Equals(checkvalue))
                        {
                            reporter.EntryMatch(vergelijkingnaam, vergelijkingnaam, primary, columnname, checkvalue, datarow);
                        }
                        else {
                            reporter.EntryInvalid(vergelijkingnaam, vergelijkingnaam, primary, columnname, checkvalue, datarow);
                        }
                    }

                    reporter.Stop(
                        vergelijkingnaam,
                        datatable.ExtendedProperties["applicatienaam"].ToString(), 
                        null,
                        datatable.ExtendedProperties["referentiequery"].ToString(),
                        null,
                        datatable.ExtendedProperties["gemeentecode"].ToString(), 
                        null, datatable.Rows.
                        Count, 
                        0);
                    Output.Info("STOP: " + vergelijkingnaam);
#if !DEBUG
                }
                catch (Exception ex)
                {
                    Output.Warn("ERROR PROCESSING: " + vergelijkingnaam, ex);
                }
#endif
            }
            #endregion CHECK            

            connection.Close();
            Output.Info("***** STOP *****");
#if !DEBUG
            }
            catch (Exception ex)
            {
                Output.Error("***** ERROR *****", ex);
            }
#endif
            if (Properties.Settings.Default.email_smtp.Length > 0)
            {
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                message.To.Add(Properties.Settings.Default.email_receiver);
                message.Subject = Properties.Settings.Default.email_subject;
                message.From = new System.Net.Mail.MailAddress(Properties.Settings.Default.email_from);
                message.Body = Output.ToString();
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(Properties.Settings.Default.email_smtp);
                smtp.UseDefaultCredentials = true;
                smtp.Send(message);
            }
            else
            {
                Console.WriteLine("\n\n=== no emailserver configured, waiting for user-input ===");
                Console.WriteLine("Press Any Key to Continue");
                Console.ReadKey();
            }
        }
    }
}
