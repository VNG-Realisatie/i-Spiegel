using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Xml.XPath;

namespace GegevensVergelijker
{        
    static class Program
    {
        private static string ReplaceVariables(string incoming) {
            string result = incoming.Replace("${WORKING_DIRECTORY}", System.IO.Directory.GetCurrentDirectory());
            return result;
        }


        private static DataTable GetData(DbProviderFactory configprovider, DbConnection configconnection, String datasourcename)
        {
            var command = configprovider.CreateCommand();
            // TODO: parameter
            command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "databron WHERE databronnaam LIKE '" + datasourcename + "'";
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
                //TODO: email versturen
                //      field xml
            Output.Info("***** START *****");
#if !DEBUG
            try
            {
#endif

            var provider = DbProviderFactories.GetFactory(Properties.Settings.Default.databaseprovider);
            var connection = provider.CreateConnection();
            connection.ConnectionString = ReplaceVariables(Properties.Settings.Default.databaseconnection);
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
                string comparename = Convert.ToString(comparerow["vergelijkingnaam"]);
                string config = Convert.ToString(comparerow["config"]);

                Output.Info("START: " + comparename);
#if !DEBUG
                try
                {
#endif
                    // what shall we do with the console reporter?
                    DatabaseReporter reporter = new DatabaseReporter(provider, connection);

                    XPathDocument document = new XPathDocument(new StringReader(config));
                    XPathNavigator compareconfig = document.CreateNavigator();
                    compareconfig = compareconfig.SelectSingleNode("/compare");

                    String referencedatasource = compareconfig.SelectSingleNode("@reference").Value;
                    String analysisdatasource = compareconfig.SelectSingleNode("@analysis").Value;

                    reporter.Start(
                        comparename,
                        referencedatasource,
                        analysisdatasource,
                        config,
                        null,
                        null
                        );

                    // create the data sources
                    Output.Info("\t[" + referencedatasource + "] data will be loaded");
                    DataTable referencetable = GetData(provider, connection, referencedatasource);
                    RegistratieSource reference = new RegistratieSource(referencetable);
                    Output.Info("\t[" + compareconfig.SelectSingleNode("@reference").Value + "] data loaded (#" + referencetable.Rows.Count + ")");
                    Output.Info("\t[" + compareconfig.SelectSingleNode("@analysis").Value + "] data will be loaded");
                    DataTable analysetable = GetData(provider, connection, analysisdatasource);
                    RegistratieSource analysis = new RegistratieSource(analysetable);
                    Output.Info("\t[" + compareconfig.SelectSingleNode("@analysis").Value + "] data loaded (#" + analysetable.Rows.Count + ")");

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
                            reporter.EntryNotFound(primary, row, matcher);
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
                                    reporter.EntryNoMatch(row, matcher, found, matchername, a, r, matcher);
                                }
                            }
                        }
                        if (fullmatch)
                        {
                            reporter.EntryMatch(row, found, matcher);
                        }
                    }
                    reporter.Stop(
                            reference.table.ExtendedProperties["applicatienaam"].ToString(),
                            analysis.table.ExtendedProperties["applicatienaam"].ToString(),
                            reference.table.ExtendedProperties["referentiequery"].ToString(),
                            analysis.table.ExtendedProperties["referentiequery"].ToString(),
                            reference.table.ExtendedProperties["gemeentecode"].ToString(),
                            analysis.table.ExtendedProperties["gemeentecode"].ToString(),
                            analysis.table.Rows.Count, 
                            reference.table.Rows.Count);

                if (Properties.Settings.Default.influxdb_url != "")
                {
                    var postdata = "ispiegel,";
                    postdata += "vergelijking=" + comparename.Replace(" ", "\\ ").Replace(",", "\\,").Replace("=", "\\=") + ",";
                    postdata += reporter.ResultLine;
                    System.Net.WebClient client = new System.Net.WebClient();
                    if (Properties.Settings.Default.influxdb_auth != "") {

                        // ik wil in één keer de boel versturen, stomme "client.Credentials = new NetworkCredential"!
                        string credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(Properties.Settings.Default.influxdb_auth));
                        client.Headers[System.Net.HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                    }
                    client.Headers[System.Net.HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    Output.Info("\t>>> posting to: " + Properties.Settings.Default.influxdb_url + " the following data:" + postdata);
                    var response = client.UploadString(Properties.Settings.Default.influxdb_url, postdata);
                }

                Output.Info("STOP: " + comparename);
#if !DEBUG
                }
                catch (Exception ex)
                {
                    Output.Warn("ERROR PROCESSING: " + comparename, ex);
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
                string checkname = Convert.ToString(checkrow["controlenaam"]);
                string datasourcename = Convert.ToString(checkrow["databronnaam"]);
                string primary = Convert.ToString(checkrow["sleutelkolom"]);
                string columnname = Convert.ToString(checkrow["controlekolom"]);
                string checkvalue = Convert.ToString(checkrow["controlewaarde"]);

                Output.Info("START: " + checkname);
#if !DEBUG
                try
                {
#endif
                    DatabaseReporter reporter = new DatabaseReporter(provider, connection);
                    reporter.Start(checkname, datasourcename, null, columnname + "='" + checkvalue + "'", datasourcename, null);
                    DataTable datatable = GetData(provider, connection, datasourcename);

                    foreach(DataRow datarow in datatable.Rows) {
                        if (Convert.ToString(datarow[columnname]).Equals(checkvalue)) reporter.EntryMatch(checkname, primary, columnname, checkvalue, datarow);
                        else reporter.EntryInvalid(checkname, primary, columnname, checkvalue, datarow);
                    }

                    reporter.Stop(
                        datatable.ExtendedProperties["applicatienaam"].ToString(), 
                        null,
                        datatable.ExtendedProperties["referentiequery"].ToString(),
                        null,
                        datatable.ExtendedProperties["gemeentecode"].ToString(), 
                        null, datatable.Rows.
                        Count, 
                        0);
                    Output.Info("STOP: " + checkname);
#if !DEBUG
                }
                catch (Exception ex)
                {
                    Output.Warn("ERROR PROCESSING: " + checkname, ex);
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
