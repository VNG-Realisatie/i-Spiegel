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
            Output.Info("*****************\n***** START *****\n*****************");
            //Output.Info("Using: " + Properties.Settings.Default.output_format + " as output format");

            string filter = null;
            if (args.Count() > 0)
            {
                filter = args[0];
                Output.Info("Filter op vergelijking: '" + filter + "'");
            }

#if !DEBUG
            try
            {
#endif

                var provider = DbProviderFactories.GetFactory(Properties.Settings.Default.databaseprovider);
                var connection = provider.CreateConnection();
                connection.ConnectionString = Properties.Settings.Default.databaseconnection.Replace("${WORKING_DIRECTORY}", System.IO.Directory.GetCurrentDirectory());
                Output.Info("ISpiegel connection:" + connection.ConnectionString);
                // If error.message == The 'Microsoft.ACE.OLEDB.12.0' provider is not registered on the local machine. ==> are we debugging in 32-bits (x86) mode?
                connection.Open();

                #region COMPARE
                var command = provider.CreateCommand();
                if(filter == null) { 
                    command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "vergelijking WHERE actief = -1 ORDER BY vergelijkingnaam";
                }
                else
                {
                    command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "vergelijking WHERE vergelijkingnaam LIKE '" + filter + "'";
                }
                command.Connection = connection;
                var adapter = provider.CreateDataAdapter();
                adapter.SelectCommand = command;
                var table = new DataTable();
                adapter.Fill(table);
                foreach (DataRow comparerow in table.Rows)
                {
                    var vergelijking = new Vergelijking(Convert.ToString(comparerow["vergelijkingnaam"]), Convert.ToString(comparerow["veldtoewijzing"]), Convert.ToString(comparerow["referentiedatabronnaam"]), Convert.ToString(comparerow["analysedatabronnaam"]), Convert.ToString(comparerow["rapporttype"]));
                    Output.Info("START: " + vergelijking.Naam);
#if !DEBUG
                    try
                    {
#endif
                        // what shall we do with the console reporter?
                        DatabaseReporter reporter = new DatabaseReporter(provider, connection);

                        XPathDocument document = new XPathDocument(new StringReader(vergelijking.VeldToewijzing));
                        XPathNavigator compareconfig = document.CreateNavigator();
                        compareconfig = compareconfig.SelectSingleNode("/compare");

                        reporter.Start(
                            vergelijking.Naam,
                            vergelijking.ReferentieDatabronNaam,
                            vergelijking.AnalyseDatabronNaam,
                            vergelijking.VeldToewijzing,
                            null,
                            null,
                            vergelijking.Rapporttype
                            );

                        // create the data sources
                        Output.Info("\t[" + vergelijking.ReferentieDatabronNaam + "] data will be loaded");
                        var reference = Databron.GetData(provider, connection, vergelijking.ReferentieDatabronNaam);
                        vergelijking.Reference = reference;
                        //RegistratieSource reference = new RegistratieSource(referencetable);
                        Output.Info("\t[" + vergelijking.ReferentieDatabronNaam + "] data loaded (#" + reference.Count + ")");

                        Output.Info("\t[" + vergelijking.AnalyseDatabronNaam + "] data will be loaded");
                        var analysis = Databron.GetData(provider, connection, vergelijking.AnalyseDatabronNaam);
                        vergelijking.Analysis = analysis;
                        //RegistratieSource analysis = new RegistratieSource(analysetable);
                        Output.Info("\t[" + vergelijking.AnalyseDatabronNaam + "] data loaded (#" + analysis.Count + ")");

                        // check the columns (better error messages!)
                        #region matching
                        foreach (XPathNavigator field in compareconfig.Select("//field"))
                        {
                            XPathNavigator referencefield = field.SelectSingleNode("@reference-field");
                            if (referencefield != null && !reference.Columns.Contains(referencefield.Value))
                            {
                                Output.Warn("reference-column:" + field.SelectSingleNode("@reference-field").Value + " not found in:");
                                foreach (var name in reference.Columns) Output.Warn("\t" + name.ToString());
                                throw new InvalidDataException("reference-column:" + field.SelectSingleNode("@reference-field").Value + " not found ");
                            }
                            XPathNavigator analysisfield = field.SelectSingleNode("@analysis-field");
                            if (analysisfield != null && !analysis.Columns.Contains(analysisfield.Value))
                            {
                                Output.Warn("analysis-column:" + field.SelectSingleNode("@analysis-field").Value + " not found in:");
                                foreach (var name in analysis.Columns) Output.Warn("\t" + name.ToString());
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
                            if (matchers.ContainsKey(name)) throw new Exception("match with id:" + name + " does already exist!");
                            matchers.Add(name, ra);
                        }
                        #endregion

                        // create compare array
                        #region build lookup
                        String primary = compareconfig.SelectSingleNode("@primary").Value;

                        SortedDictionary<RegistratieItem, DataRegel> lookup = reference.GetSortedList(matchers[primary][0].ToArray());
                        Output.Info("\t[lookup] index succesfull");
                        #endregion

                        // now start the loop
                        foreach (DataRegel regel in analysis.Regels)
                        {
                            // primary match
                            string[] analysisrows = matchers[primary][1].ToArray();
                            //RegistratieItem matcher =  analysis.GetFieldValues(row, analysisrows);
                            RegistratieItem matcher = regel.GetFieldValues(analysisrows);

                            if (!lookup.ContainsKey(matcher))
                            {
                                //reporter.EntryNotFound(vergelijking, primary, row, matcher);
                                reporter.EntryNotFound(vergelijking, primary, regel, matcher);
                                continue;
                            }
                            // System.Data.DataRow found = lookup[matcher];
                            DataRegel found = lookup[matcher];


                            bool fullmatch = true;
                            bool firsterror = true;
                            foreach (string matchername in matchers.Keys)
                            {
                                if (matchername != primary)
                                {
                                    string[] analysisfields = matchers[matchername][1].ToArray();
                                    string[] referencefields = matchers[matchername][0].ToArray();

                                    // RegistratieItem a = analysis.GetFieldValues(row, analysisfields);
                                    // RegistratieItem r = reference.GetFieldValues(found, referencefields);
                                    RegistratieItem a = regel.GetFieldValues(analysisfields);
                                    RegistratieItem r = found.GetFieldValues(referencefields);


                                    if (!a.Equals(r))
                                    {
                                        fullmatch = false;
                                        reporter.EntryNoMatch(vergelijking, regel, matcher, found, matchername, a, r, matcher, firsterror);
                                        firsterror = false;
                                    }
                                }
                            }
                            if (fullmatch)
                            {
                                reporter.EntryMatch(vergelijking, regel, found, matcher);
                            }
                        }
                        reporter.Stop(
                                vergelijking.Naam,
                                reference.ApplicatieNaam,
                                analysis.ApplicatieNaam,
                                reference.ReferentieQuery,
                                analysis.ReferentieQuery,
                                reference.GemeenteCode,
                                analysis.GemeenteCode,
                                analysis.Count, 
                                reference.Count);


                        Output.Info("STOP: " + vergelijking.Naam);
#if !DEBUG
                    }
                    catch (Exception ex)
                    {
                        Output.Warn("ERROR PROCESSING: " + vergelijking.Naam, ex);
                    }
#endif
                }
                #endregion COMPARE

                #region CHECK

                command = provider.CreateCommand();
                // "small detail", in access boolean: true = false and visaversa
                if(filter == null) { 
                    command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "controle WHERE actief = -1 ORDER BY controlenaam";
                }
                else
                {
                    command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "controle WHERE controlenaam LIKE '" + filter +  "'";
                }
                command.Connection = connection;
                adapter = provider.CreateDataAdapter();
                adapter.SelectCommand = command;
                table = new DataTable();
                adapter.Fill(table);
                
                foreach (DataRow checkrow in table.Rows)
                {
                    string controlenaam = Convert.ToString(checkrow["controlenaam"]);
                    string datasourcename = Convert.ToString(checkrow["databronnaam"]);
                    string primary = Convert.ToString(checkrow["sleutelkolom"]);
                    string columnname = Convert.ToString(checkrow["controlekolom"]);
                    string checkvalue = Convert.ToString(checkrow["controlewaarde"]);
                    string rapportype = Convert.ToString(checkrow["rapportype"]);
                    var vergelijking = new Vergelijking(controlenaam, primary, datasourcename, columnname, rapportype);
                    Output.Info("START: " + controlenaam);
#if !DEBUG
                    try
                    {
#endif
                        DatabaseReporter reporter = new DatabaseReporter(provider, connection);
                        reporter.Start(controlenaam, null, datasourcename, columnname + "='" + checkvalue + "'", null, datasourcename, rapportype);
                    var controle = Databron.GetData(provider, connection, datasourcename); 

                    foreach(DataRegel datarow in controle.Regels) {
                        var found = datarow[columnname];
                        if (Convert.ToString(found).Equals(checkvalue))
                        {
                            reporter.EntryMatch(vergelijking, controlenaam, primary, columnname, checkvalue, datarow);
                        }
                        else {
                                reporter.EntryInvalid(vergelijking, controlenaam, primary, columnname, checkvalue, datarow);
                            }
                        }

                        reporter.Stop(
                            controlenaam,
                        controle.ApplicatieNaam, 
                            null,
                            controle.ReferentieQuery,
                            null,
                        controle.GemeenteCode, 
                        null, 
                        controle.Regels.Count, 
                            0);
                        Output.Info("STOP: " + controlenaam);
#if !DEBUG
                    }
                    catch (Exception ex)
                    {
                        Output.Warn("ERROR PROCESSING: " + controlenaam, ex);
                    }
#endif
                }
                #endregion CHECK            

                connection.Close();
                Output.Info("*****************\n***** STOP ******\n*****************");
#if !DEBUG
            }
            catch (Exception ex)
            {
                Output.Error("*****************\n***** ERROR ******\n*****************", ex);
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
