using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ISpiegel
{
    public class DatabaseReporter
    {
        private DbConnection connection = null;
        private DataSet ds = null;
        private DbDataAdapter kopadapter = null;
        private DataRow koprow = null;
        private DbDataAdapter regeladapter = null;

        private long match;
        private long nomatch;
        private long missing;
        //private long invalid;

        private System.Diagnostics.Stopwatch watch;

        public DatabaseReporter(DbProviderFactory factory, DbConnection connection) {
            this.connection = connection;

            watch = System.Diagnostics.Stopwatch.StartNew();

            DbCommand command = factory.CreateCommand();
            command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "output";
            command.Connection = connection;

            kopadapter = factory.CreateDataAdapter();
            kopadapter.SelectCommand = command;
            DbCommandBuilder builder = factory.CreateCommandBuilder();
            builder.ConflictOption = ConflictOption.OverwriteChanges;
            builder.DataAdapter = kopadapter;
            // Wanneer : Additional information: De Microsoft.Jet.OLEDB.4.0-provider is niet geregistreerd op de lokale computer.
            //  ==> draaien als x86 
            kopadapter.InsertCommand = builder.GetInsertCommand();
            kopadapter.UpdateCommand = builder.GetUpdateCommand();
            kopadapter.DeleteCommand = builder.GetDeleteCommand();

            command = factory.CreateCommand();
            command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "outputline";
            command.Connection = connection;

            regeladapter = factory.CreateDataAdapter();
            regeladapter.SelectCommand = command;
            builder = factory.CreateCommandBuilder();
            builder.ConflictOption = ConflictOption.OverwriteChanges;
            builder.DataAdapter = regeladapter;
            regeladapter.InsertCommand = builder.GetInsertCommand();
            regeladapter.UpdateCommand = builder.GetUpdateCommand();
            regeladapter.DeleteCommand = builder.GetDeleteCommand();
        }

        public void Start(string vergelijkingnaam, string referentiename, string analysename, string configuration, string referencesql, string analysesql)
        {
            // get our next id
            DbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT MAX(outputid) FROM " + Properties.Settings.Default.databaseprefix + "output ";
            object result = command.ExecuteScalar();
            long outputid = DBNull.Value == result ? 0 : Convert.ToUInt32(result);
            outputid++;

            ds = new DataSet();
            kopadapter.Fill(ds, "output");
            koprow = ds.Tables["output"].NewRow();

            koprow["outputid"] = outputid;
            koprow["tijdstip"] = DateTime.Now;
            koprow["vergelijkingnaam"] = vergelijkingnaam;
            koprow["configuratie"] = configuration;
            koprow["referentienaam"] = referentiename;
            koprow["analysenaam"] = analysename;
            koprow["referentiequery"] = referencesql;
            koprow["analysequery"] = analysesql;
            koprow["referentieapplicatie"] = "";
            koprow["analyseapplicatie"] = "";
            koprow["referentiegemeentecode"] = "";
            koprow["analysegemeentecode"] = "";
            koprow["percentage"] = 0;
            koprow["referentieaantal"] = 0;
            koprow["analyseaantal"] = 0;          
            koprow["gelijkaantal"] = 0;
            koprow["afwijkingaantal"] = 0;
            koprow["nietgevondenaantal"] = 0;
            //koprow["ongeldigaantal"] = 0;            
            koprow["looptijd"] = watch.Elapsed.TotalSeconds;

            ds.Tables["output"].Rows.Add(koprow);
            kopadapter.Update(ds, "output");

            regeladapter.SelectCommand.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "outputline WHERE outputid = " + outputid;
            regeladapter.Fill(ds, "outputline");

            match = 0;
            nomatch = 0;
            missing = 0;
            //invalid = 0;
        }

        public class ShootAndForgetWebClient : System.Net.WebClient
        {
            protected override System.Net.WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                request.Timeout = 1000;//In Milli seconds
                return request;
            }
        }

        public void Stop(string vergelijkingnaam, string analyseapplicatie, string referentieapplicatie, string analysequery, string referentiequery, string analysegemeentecode, string referentiegemeentecode, long analysecount, long referencecount)
        {
            // save the regels
            regeladapter.Update(ds, "outputline");

            // update the koprow
            koprow["referentieapplicatie"] = referentieapplicatie;
            koprow["analyseapplicatie"] = analyseapplicatie;
            koprow["referentiequery"] = referentiequery;
            koprow["analysequery"] = analysequery;
            koprow["referentiegemeentecode"] = referentiegemeentecode;
            koprow["analysegemeentecode"] = analysegemeentecode;
            koprow["percentage"] = (100.0 / analysecount) * match;
            koprow["referentieaantal"] = referencecount;
            koprow["analyseaantal"] = analysecount;
            koprow["gelijkaantal"] = match;
            koprow["afwijkingaantal"] = nomatch;
            koprow["nietgevondenaantal"] = missing;
            //koprow["ongeldigaantal"] = invalid;

            // the code that you want to measure comes here
            watch.Stop();
            koprow["looptijd"] = watch.Elapsed.TotalSeconds;
            //ds.Tables["output"].Rows[0]. koprow
            //koprow.SetModified();
            kopadapter.Update(ds, "output");

            if (Properties.Settings.Default.influxdb_url != "")
            {

                // dimensions
                String ResultLine = "referentieapplicatie=" + (referentieapplicatie == null ? "" : referentieapplicatie.Replace(" ", "\\ ").Replace(",", "\\,").Replace("=", "\\=")) + ",";
                ResultLine += "analyseapplicatie=" + (analyseapplicatie == null ? "" : analyseapplicatie.Replace(" ", "\\ ").Replace(",", "\\,").Replace("=", "\\=")) + ",";
                ResultLine += "referentiegemeentecode=" + (referentiegemeentecode == null ? "" : referentiegemeentecode.Replace(" ", "\\ ").Replace(",", "\\,").Replace("=", "\\=")) + ",";
                ResultLine += "analysegemeentecode=" + (analysegemeentecode == null ? "" : analysegemeentecode.Replace(" ", "\\ ").Replace(",", "\\,").Replace("=", "\\=")) + " ";
                // values
                ResultLine += "referentieaantal=" + referencecount + ",";
                ResultLine += "analyseaantal=" + analysecount + ",";
                ResultLine += "gelijkaantal=" + match + ",";
                ResultLine += "afwijkingaantal=" + nomatch + ",";
                ResultLine += "nietgevondenaantal=" + missing + ",";
                //ResultLine += "ongeldigaantal=" + invalid + ",";
                ResultLine += "percentage=" + Math.Round((100.0 / analysecount) * match, 2);

                var postdata = "ispiegel,";
                postdata += "vergelijking=" + vergelijkingnaam.Replace(" ", "\\ ").Replace(",", "\\,").Replace("=", "\\=") + ",";
                postdata += ResultLine;
                var  client = new ShootAndForgetWebClient();
                if (Properties.Settings.Default.influxdb_auth != "")
                {

                    // ik wil in één keer de boel versturen, stomme "client.Credentials = new NetworkCredential"!
                    string credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(Properties.Settings.Default.influxdb_auth));
                    client.Headers[System.Net.HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                }
                client.Headers[System.Net.HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                Output.Info("\t>>> posting to: " + Properties.Settings.Default.influxdb_url + " the following data:" + postdata);
                try
                {
                    var response = client.UploadString(Properties.Settings.Default.influxdb_url, postdata);
                }
                catch (System.Net.WebException ex)
                {
                    Output.Warn("Sending the data to: " + Properties.Settings.Default.influxdb_url, ex);
                }
            }

        }

        private string CreateRowXml(string vergelijking)
        {
            return CreateRowXml(vergelijking, new string[] { }, new string[] { });
        }
        private string CreateRowXml(string vergelijking, string fieldname, string fieldvalue)
        {
            return CreateRowXml(vergelijking, new string[] { fieldname }, new string[] { fieldvalue });
        }
        private string CreateRowXml(string vergelijking, string[] fieldnames, string[] fieldvalues)
        {
            var doc = new System.Xml.XmlDocument();
            if (Properties.Settings.Default.output_format == "html")
            {
                var table = doc.CreateElement("table");
                var tablename = doc.CreateAttribute("name");
                tablename.Value = vergelijking.ToLower();
                table.Attributes.Append(tablename);
                doc.AppendChild(table);

                for (int i = 0; i < fieldnames.Length; i++)
                {
                    var tr = doc.CreateElement("tr");
                    table.AppendChild(tr);

                    var tdname = doc.CreateElement("td");
                    tdname.InnerText = fieldnames[i].ToLower();
                    tr.AppendChild(tdname);

                    var tdvalue = doc.CreateElement("td");
                    if (fieldvalues[i] == null || Convert.ToString(fieldvalues[i]).Length == 0)
                    {
                        // we moeten er iets in hebben staan om de uitlijning altijd goed te houden
                        tdvalue.InnerXml = "&nbsp;";
                    }
                    else
                    {
                        tdvalue.InnerText = fieldvalues[i];
                    }
                    tr.AppendChild(tdvalue);
                }
            }
            else {                 
                var rootnode = doc.CreateElement(System.Xml.XmlConvert.EncodeName(vergelijking).ToLower());
                if (fieldnames.Length > 0)
                {
                    for(int i=0; i < fieldnames.Length; i++)
                    {
                        var element = doc.CreateElement(System.Xml.XmlConvert.EncodeName(fieldnames[i]).ToLower());
                        if(fieldvalues[i] == null || Convert.ToString(fieldvalues[i]).Length == 0)
                        {
                            var nil = doc.CreateAttribute("nil", "xsi", "http://w3.org/2001/XMLSchema-instance");
                            nil.Value = Convert.ToString(true);
                            element.Attributes.Append(nil);
                        }
                        else
                        {
                            element.InnerText = fieldvalues[i];
                        }
                        rootnode.AppendChild(element);
                    }
                }
                else
                {
                    var nil = doc.CreateAttribute("nil", "xsi", "http://w3.org/2001/XMLSchema-instance");
                    nil.Value = Convert.ToString(true);
                    rootnode.Attributes.Append(nil);                    
                }
                doc.AppendChild(rootnode);
            }
            var stream = new System.IO.MemoryStream();
            var writer = new System.Xml.XmlTextWriter(stream, Encoding.Unicode);
            writer.Formatting = System.Xml.Formatting.Indented;
            doc.WriteContentTo(writer);
            writer.Flush();
            stream.Flush();
            stream.Position = 0;
            var reader = new System.IO.StreamReader(stream);
            return reader.ReadToEnd();
        }

        public void EntryMatch(Vergelijking vergelijking, string matcher, string sleutelcolumname, string checkcolumnname, string checkvalue, DataRegel found)
        {
            if (Properties.Settings.Default.output_everything) { 
                DataRow row = ds.Tables["outputline"].NewRow();
                row["outputid"] = koprow["outputid"];
                row["regelnummer"] = ds.Tables["outputline"].Rows.Count + 1;
                row["status"] = "VALID";
                //            row["melding"] = "check:" + checkname + " found:" + found[columnname] + " expected:" + checkvalue;
                //            row["referentieregel"] = RegistratieSource.ToString(found);
                //            //row["analyseregel"] = RegistratieSource.ToString(searchrow);
                //            ds.Tables["outputline"].Rows.Add(row);

                row["controle"] = matcher;
                row["sleutel"] = CreateRowXml(vergelijking.Naam, sleutelcolumname, Convert.ToString(found[sleutelcolumname]));
                row["analysewaarde"] = CreateRowXml(vergelijking.Naam, checkcolumnname, checkvalue);
                //row["referentiewaarde"] = CreateRowXml(vergelijking);
                row["referentiewaarde"] = DBNull.Value;
                //row["analyseregel"] = CreateRowXml(vergelijking.Naam, ToStringArray(found.Table.Columns), ToStringArray(found.ItemArray));
                row["analyseregel"] = CreateRowXml(vergelijking.Naam, found.FieldNames, found.FieldValues);
                row["referentieregel"] = DBNull.Value;
                ds.Tables["outputline"].Rows.Add(row);
            }
            match++;
        }

        public void EntryInvalid(Vergelijking vergelijking, string matcher, string sleutelcolumname, string checkcolumnname, string checkvalue, DataRegel found)
        {
            DataRow row = ds.Tables["outputline"].NewRow();
            row["outputid"] = koprow["outputid"];
            row["regelnummer"] = ds.Tables["outputline"].Rows.Count + 1;
            row["status"] = "INVALID";


            //            row["melding"] = "check:" + checkname + " found:" + found[columnname] + " expected:" + checkvalue;
            //            row["referentieregel"] = RegistratieSource.ToString(found);
            //            //row["analyseregel"] = RegistratieSource.ToString(searchrow);
            //            ds.Tables["outputline"].Rows.Add(row);


            row["controle"] = matcher;
            //row["sleutel"] = RegistratieSource.ToFieldXml(sleutelcolumname, Convert.ToString(found[sleutelcolumname]));
            //row["analysewaarde"] = RegistratieSource.ToFieldXml(checkcolumnname, checkvalue);
            row["sleutel"] = CreateRowXml(vergelijking.Naam, sleutelcolumname, Convert.ToString(found[sleutelcolumname]));
            row["analysewaarde"] = CreateRowXml(vergelijking.Naam, checkcolumnname, checkvalue);
            row["referentiewaarde"] = DBNull.Value;
            //row["referentiewaarde"] = CreateRowXml(vergelijking);
            //row["analyseregel"] = RegistratieSource.ToFieldXml(found);
            //row["analyseregel"] = CreateRowXml(vergelijking.Naam, ToStringArray(found.Table.Columns), ToStringArray(found.ItemArray));
            row["analyseregel"] = CreateRowXml(vergelijking.Naam, found.FieldNames, found.FieldValues);
            //row["referentieregel"] = CreateRowXml(vergelijking);
            row["referentieregel"] = DBNull.Value;
            ds.Tables["outputline"].Rows.Add(row);

            nomatch++;
        }


        public void EntryMatch(Vergelijking vergelijking, DataRegel searchrow, DataRegel found, RegistratieItem sleutel)
        {
            if (Properties.Settings.Default.output_everything)
            {
                DataRow row = ds.Tables["outputline"].NewRow();
                row["outputid"] = koprow["outputid"];
                row["regelnummer"] = ds.Tables["outputline"].Rows.Count + 1;
                row["status"] = "VALID";

                row["controle"] = DBNull.Value;
                row["sleutel"] = CreateRowXml(vergelijking.Naam, sleutel.fieldnames, sleutel.fieldvalues);
                row["analysewaarde"] = CreateRowXml(vergelijking.Naam);
                row["referentiewaarde"] = CreateRowXml(vergelijking.Naam);
                //row["analyseregel"] = CreateRowXml(vergelijking.Naam, ToStringArray(searchrow.Table.Columns), ToStringArray(searchrow.ItemArray));
                //row["referentieregel"] = CreateRowXml(vergelijking.Naam, ToStringArray(found.Table.Columns), ToStringArray(found.ItemArray));
                row["analyseregel"] = CreateRowXml(vergelijking.Naam, searchrow.FieldNames, searchrow.FieldValues);
                row["referentieregel"] = CreateRowXml(vergelijking.Naam, found.FieldNames, found.FieldValues);
                ds.Tables["outputline"].Rows.Add(row);
            }
            match++;
        }

        public void EntryNoMatch(Vergelijking vergelijking, DataRegel searchrow, RegistratieItem searchitem, DataRegel found, string matcher, RegistratieItem analyse, RegistratieItem reference, RegistratieItem sleutel)
        {
            DataRow row = ds.Tables["outputline"].NewRow();
            row["outputid"] = koprow["outputid"];
            row["regelnummer"] = ds.Tables["outputline"].Rows.Count + 1;
            row["status"] = "NO MATCH";

            row["controle"] = matcher;
            row["sleutel"] = CreateRowXml(vergelijking.Naam, sleutel.fieldnames, sleutel.fieldvalues);
            row["analysewaarde"] = CreateRowXml(matcher, analyse.fieldnames, analyse.fieldvalues);
            row["referentiewaarde"] = CreateRowXml(matcher, reference.fieldnames, reference.fieldvalues);
            //row["analyseregel"] = CreateRowXml(vergelijking.Analysis.DatabronNaam, ToStringArray(searchrow.Table.Columns), ToStringArray(searchrow.ItemArray));
            //row["referentieregel"] = CreateRowXml(vergelijking.Reference.DatabronNaam, ToStringArray(found.Table.Columns), ToStringArray(found.ItemArray));
            row["analyseregel"] = CreateRowXml(vergelijking.Analysis.DatabronNaam, searchrow.FieldNames, searchrow.FieldValues);
            row["referentieregel"] = CreateRowXml(vergelijking.Reference.DatabronNaam, found.FieldNames, found.FieldValues);
            ds.Tables["outputline"].Rows.Add(row);
            nomatch++;
        }

        public void EntryNotFound(Vergelijking vergelijking, string matcher, DataRegel searchrow, RegistratieItem searchitem)
        {
            DataRow row = ds.Tables["outputline"].NewRow();
            row["outputid"] = koprow["outputid"];
            row["regelnummer"] = ds.Tables["outputline"].Rows.Count + 1;
            row["status"] = "NOT FOUND";


            row["controle"] = matcher;
            row["sleutel"] = CreateRowXml(vergelijking.Naam, searchitem.fieldnames, searchitem.fieldvalues);
            row["analysewaarde"] = CreateRowXml(matcher, searchitem.fieldnames, searchitem.fieldvalues);
            row["referentiewaarde"] = DBNull.Value;
            //row["analyseregel"] = CreateRowXml(vergelijking.Analysis.DatabronNaam, ToStringArray(searchrow.Table.Columns), ToStringArray(searchrow.ItemArray));
            row["analyseregel"] = CreateRowXml(vergelijking.Analysis.DatabronNaam, searchrow.FieldNames, searchrow.FieldValues);
            row["referentieregel"] = DBNull.Value;
            ds.Tables["outputline"].Rows.Add(row);

            missing++;
        }
    }
}
