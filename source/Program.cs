using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.XPath;

namespace RegistratieVergelijker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
#if !DEBUG
            try
            {
#endif
                // read the configurion
                FileInfo configurationfile;
                if (args.Count() > 0) configurationfile = new FileInfo(args[0]);
                else configurationfile = new FileInfo(Properties.Settings.Default.Configuration);

                if (!configurationfile.Exists)
                {
                    System.Console.Error.WriteLine("Configuration file: {0}  was not found.", configurationfile.FullName);
                    throw new FileNotFoundException("Configuration file was not found", configurationfile.FullName);
                }
                XPathNavigator configuration;
                {
                    System.Console.Out.WriteLine("configurationfile: " + configurationfile.FullName);
                    XPathDocument document = new XPathDocument(configurationfile.FullName);
                    configuration = document.CreateNavigator();
                }

                // basic configuration items
                string xpath = "/registratie-vergelijker/compare";
                XPathNavigator compareconfig = configuration.SelectSingleNode(xpath);
                if (compareconfig == null) throw new Exception("xpath: \"" + "/registratie-vergelijker/compare" + "\" returned no value in file:" + configurationfile.FullName);

                xpath = "/registratie-vergelijker/sources/source[@id='" + compareconfig.SelectSingleNode("@reference").Value + "']";
                XPathNavigator referenceconfig = configuration.SelectSingleNode(xpath);
                if (referenceconfig == null) throw new Exception("xpath: \"" + "/registratie-vergelijker/compare" + "\" returned no value in file:" + configurationfile.FullName);


                xpath = "/registratie-vergelijker/sources/source[@id='" + compareconfig.SelectSingleNode("@analysis").Value + "']";
                XPathNavigator analysisconfig = configuration.SelectSingleNode("/registratie-vergelijker/sources/source[@id='" + compareconfig.SelectSingleNode("@analysis").Value + "']");
                if (analysisconfig == null) throw new Exception("xpath: \"" + "/registratie-vergelijker/compare" + "\" returned no value in file:" + configurationfile.FullName);


                IReporter reporter = new ConsoleReporter();
                if (configuration.SelectSingleNode("/registratie-vergelijker/logging") != null)
                {
                    reporter = new DatabaseReporter(
                        configuration.SelectSingleNode("/registratie-vergelijker/logging/database-provider").Value,
                        configuration.SelectSingleNode("/registratie-vergelijker/logging/database-connection").Value
                        );
                }
                // sql
                String referencesql = referenceconfig.SelectSingleNode("database-query").Value;
                    if(referencesql == null || referencesql.Trim().Length == 0) {
                        // maybe in a file?
                        String filename = referenceconfig.SelectSingleNode("database-query/@queryfile").Value;
                        System.IO.FileInfo fi = new System.IO.FileInfo(filename);
                        System.Console.Out.WriteLine("\tquery from: " + fi.FullName);
                        referencesql = System.IO.File.ReadAllText(fi.FullName);
                }
                String analysesql = analysisconfig.SelectSingleNode("database-query").Value;
                    if(analysesql == null || referencesql.Trim().Length == 0) {
                        // maybe in a file?
                        String filename = analysisconfig.SelectSingleNode("database-query/@queryfile").Value;
                        System.IO.FileInfo fi = new System.IO.FileInfo(filename);
                        System.Console.Out.WriteLine("\tquery from: " + fi.FullName);
                        analysesql = System.IO.File.ReadAllText(fi.FullName);
                }

                // report our start
                int lengte = configurationfile.Name.Length - configurationfile.Extension.Length;
                String exportname = configurationfile.Name.Substring(0, lengte);
                reporter.Start(
                    exportname, 
                    compareconfig.SelectSingleNode("@reference").Value, 
                    compareconfig.SelectSingleNode("@analysis").Value,
                    System.IO.File.ReadAllText(configurationfile.FullName),
                    referencesql,
                    analysesql
                    );

                // create the data sources
                System.Console.WriteLine("[" + compareconfig.SelectSingleNode("@reference").Value + "] data will be loaded");
                RegistratieSource reference = new RegistratieSource(referenceconfig, referencesql);
                System.Console.WriteLine("[" + compareconfig.SelectSingleNode("@reference").Value + "] data loaded");
                System.Console.WriteLine("[" + compareconfig.SelectSingleNode("@analysis").Value + "] data will be loaded");
                RegistratieSource analysis = new RegistratieSource(analysisconfig, analysesql);
                System.Console.WriteLine("[" + compareconfig.SelectSingleNode("@analysis").Value + "] data loaded");

                // check the columns (better error messages!)
                #region matching
                foreach (XPathNavigator field in configuration.Select("//field"))
                {
                    XPathNavigator referencefield = field.SelectSingleNode("@reference-field");
                    if (referencefield != null && !reference.table.Columns.Contains(referencefield.Value))
                    {
                        System.Console.Error.WriteLine("reference-column:" + field.SelectSingleNode("@reference-field").Value + " not found in:");
                        foreach (var name in reference.table.Columns) System.Console.Error.WriteLine("\t" + name.ToString());

                        System.Console.WriteLine("");
                        System.Console.WriteLine("for csv files with an ';' seperator, create the file 'schema.ini' with the following contents:");
                        System.Console.WriteLine("\t[refenencefilename.csv]");
                        System.Console.WriteLine("\tFormat=Delimited(;)");

                        throw new InvalidDataException("reference-column:" + field.SelectSingleNode("@reference-field").Value + " not found ");
                    }
                    XPathNavigator analysisfield = field.SelectSingleNode("@analysis-field");
                    if (analysisfield != null && !analysis.table.Columns.Contains(analysisfield.Value))
                    {
                        System.Console.Error.WriteLine("analysis-column:" + field.SelectSingleNode("@analysis-field").Value + " not found in:");
                        foreach (var name in analysis.table.Columns) System.Console.Error.WriteLine("\t" + name.ToString());

                        System.Console.WriteLine("");
                        System.Console.WriteLine("for csv files with an ';' seperator, create the file 'schema.ini' with the following contents:");
                        System.Console.WriteLine("\t[analysisfilename.csv]");
                        System.Console.WriteLine("\tFormat=Delimited(;)");

                        throw new InvalidDataException("analysis-column:" + field.SelectSingleNode("@analysis-field").Value + " not found");
                    }
                }
                System.Console.WriteLine("[check] field references correct");
                #endregion

                // export into csv, so we can use i-spiegel
                #region export csv
                {
                    DirectoryInfo exportdirectory = new DirectoryInfo("data");
                    if (!exportdirectory.Exists) exportdirectory.Create();
                    exportdirectory = new DirectoryInfo("data\\" + exportname);
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
                        else {
                            a.Add(null);
                        }
                    }
                    System.Console.WriteLine("[export] exporting the data");
                    reference.Export(r, n, exportdirectory, compareconfig.SelectSingleNode("@reference").Value);
                    analysis.Export(a, n, exportdirectory, compareconfig.SelectSingleNode("@analysis").Value);
                }
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
                System.Console.WriteLine("[lookup] index succesfull");
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
                                reporter.EntryNoMatch(row, matcher, found, matchername, a, r);
                            }
                        }
                    }
                    if (fullmatch)
                    {
                        reporter.EntryMatch(row, matcher, found);
                    }
                }

                reporter.Stop(analysis.table.Rows.Count, reference.table.Rows.Count);
#if !DEBUG
            }
            catch (Exception ex)
            {
                Console.WriteLine("[error] Caught exception of type:" + ex.GetType().FullName);
                Console.WriteLine("\t" + ex.Message);                    
                Exception inner = ex.InnerException;
                while (inner != null)
                {
                    Console.WriteLine("\t" + inner.Message);
                    inner = inner.InnerException;
                }
                Console.WriteLine(ex.StackTrace);

                Console.ReadKey();
            }
#endif
        }
    }
}
