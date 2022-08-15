using System;
using System.Data;
using System.Data.Common;
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

        public DatabaseReporter(DbProviderFactory factory, DbConnection connection)
        {
            this.connection = connection;

            watch = System.Diagnostics.Stopwatch.StartNew();

            DbCommand command = factory.CreateCommand();
            command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "output WHERE 1 = 2";
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
            command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "outputline WHERE 1 = 2";
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

        public void Start(string vergelijkingnaam, string referentiename, string analysename, string configuration, string referencesql, string analysesql, string rapporttype)
        {
            // get our next id
            DbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT MAX(outputid) FROM " + Properties.Settings.Default.databaseprefix + "output ";
            object result = command.ExecuteScalar();
            long outputid = DBNull.Value == result ? 0 : Convert.ToUInt32(result);
            outputid++;

            ds = new DataSet();
            System.Diagnostics.Debug.WriteLine("ISpiegel.DatabaseReporter::Start > Starting query:" + kopadapter.SelectCommand.CommandText);
            kopadapter.Fill(ds, "output");
            System.Diagnostics.Debug.WriteLine("ISpiegel.DatabaseReporter::Start > Starting query:" + kopadapter.SelectCommand.CommandText);

            koprow = ds.Tables["output"].NewRow();

            koprow["outputid"] = outputid;
            koprow["tijdstip"] = DateTime.Now;
            koprow["vergelijkingnaam"] = vergelijkingnaam;
            koprow["configuratie"] = configuration;
            koprow["rapporttype"] = rapporttype;
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
            System.Diagnostics.Debug.WriteLine("ISpiegel.DatabaseReporter::Start > Starting query:" + kopadapter.InsertCommand.CommandText);
            kopadapter.Update(ds, "output");
            System.Diagnostics.Debug.WriteLine("ISpiegel.DatabaseReporter::Start > Finished query:" + kopadapter.InsertCommand.CommandText);

            regeladapter.SelectCommand.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "outputline WHERE outputid = " + outputid;
            System.Diagnostics.Debug.WriteLine("ISpiegel.DatabaseReporter::Start > Starting query:" + regeladapter.SelectCommand.CommandText);
            regeladapter.Fill(ds, "outputline");
            System.Diagnostics.Debug.WriteLine("ISpiegel.DatabaseReporter::Start > Finished query:" + kopadapter.SelectCommand.CommandText);

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
            Output.Info("\t" + ((int)((100.0 / analysecount) * match)) + "% gelijk #" + match + " afwijkend #" + nomatch + " mist #" + missing + "(adding #" + ds.Tables["outputline"].Rows.Count + " outputlines )");

            // save the regels
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            System.Diagnostics.Debug.WriteLine("ISpiegel.DatabaseReporter::Stop > Starting query:" + regeladapter.InsertCommand.CommandText);
            regeladapter.Update(ds, "outputline");
            System.Diagnostics.Debug.WriteLine("ISpiegel.DatabaseReporter::Stop > Starting query:" + regeladapter.InsertCommand.CommandText);
            stopwatch.Stop();
            Output.Info("\toutputlines weggeschreven in " + (int)stopwatch.Elapsed.TotalSeconds + " seconds)");

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

            System.Diagnostics.Debug.WriteLine("ISpiegel.DatabaseReporter::Stop > Starting query:" + kopadapter.SelectCommand.CommandText);
            kopadapter.Update(ds, "output");
            System.Diagnostics.Debug.WriteLine("ISpiegel.DatabaseReporter::Stop > Finished query:" + kopadapter.SelectCommand.CommandText);

            if (Properties.Settings.Default.influxdb_url != "")
            {
                var percentage = (100.0 / analysecount) * match;
                var point = InfluxDB.Client.Writes.PointData
                  .Measurement("ispiegel")
                  .Tag("referentieapplicatie", referentieapplicatie == null ? "GEEN" : referentieapplicatie)                  
                  .Tag("analyseapplicatie", analyseapplicatie == null ? "GEEN" : analyseapplicatie)
                  .Tag("referentiegemeentecode", referentiegemeentecode == null ? "GEEN" : referentiegemeentecode)
                  .Tag("analysegemeentecode", analysegemeentecode == null ? "GEEN" : analysegemeentecode)
                  .Field("referentieaantal", referencecount)
                  .Field("analyseaantal", analysecount)

                  .Field("gelijkaantal", match)
                  .Field("afwijkingaantal", nomatch)
                  .Field("nietgevondenaantal", missing)
                  .Field("percentage", percentage)
                  .Timestamp(DateTime.UtcNow, InfluxDB.Client.Api.Domain.WritePrecision.Ns);

                Output.Info("\t>>> posting to: " + Properties.Settings.Default.influxdb_url);
                Output.Info("\t>>>" + point.ToLineProtocol());

                try
                {
                    using (var client = InfluxDB.Client.InfluxDBClientFactory.Create(Properties.Settings.Default.influxdb_url, Properties.Settings.Default.influxdb_token))
                    {
                        using (var writeApi = client.GetWriteApi())
                        {
                            writeApi.WritePoint(point, Properties.Settings.Default.influxdb_bucket, Properties.Settings.Default.influxdb_org);
                        }
                    }
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
            var stream = new System.IO.MemoryStream();
            var writer = new System.Xml.XmlTextWriter(stream, Encoding.Unicode);
            writer.Formatting = System.Xml.Formatting.Indented;
            doc.WriteContentTo(writer);
            writer.Flush();
            stream.Flush();
            stream.Position = 0;
            var reader = new System.IO.StreamReader(stream);
            string result = reader.ReadToEnd();
            reader.Close();
            writer.Close();

            return result;
        }

        public void EntryMatch(Vergelijking vergelijking, string matcher, string sleutelcolumname, string checkcolumnname, string checkvalue, DataRegel found)
        {
            if (Properties.Settings.Default.output_everything)
            {
                DataRow row = ds.Tables["outputline"].NewRow();
                row["outputid"] = koprow["outputid"];
                row["regelnummer"] = ds.Tables["outputline"].Rows.Count + 1;
                row["status"] = "VALID";

                row["controle"] = matcher;
                row["sleutel"] = CreateRowXml(vergelijking.Naam, sleutelcolumname, Convert.ToString(found[sleutelcolumname]));
                row["analysewaarde"] = CreateRowXml(vergelijking.Naam, checkcolumnname, checkvalue);
                //row["referentiewaarde"] = CreateRowXml(vergelijking);
                row["referentiewaarde"] = DBNull.Value;
                //row["analyseregel"] = CreateRowXml(vergelijking.Naam, ToStringArray(found.Table.Columns), ToStringArray(found.ItemArray));
                row["analyseregel"] = CreateRowXml(vergelijking.Naam, found.FieldNames, found.FieldValues);
                row["referentieregel"] = DBNull.Value;

                row["analysesleutelwaarde"] = sleutelcolumname;
                row["referentiesleutelwaarde"] = DBNull.Value;
                row["analyseveldwaarde"] = string.Join(", ", found.FieldValues);
                row["referentieveldwaarde"] = DBNull.Value;

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


            row["controle"] = matcher;
            row["sleutel"] = CreateRowXml(vergelijking.Naam, sleutelcolumname, Convert.ToString(found[sleutelcolumname]));
            row["analysewaarde"] = CreateRowXml(vergelijking.Naam, checkcolumnname, checkvalue);
            row["referentiewaarde"] = DBNull.Value;
            row["analyseregel"] = CreateRowXml(vergelijking.Naam, found.FieldNames, found.FieldValues);
            row["referentieregel"] = DBNull.Value;

            row["analysesleutelwaarde"] = sleutelcolumname;
            row["referentiesleutelwaarde"] = DBNull.Value;
            row["analyseveldwaarde"] = string.Join(", ", found.FieldValues);
            row["referentieveldwaarde"] = DBNull.Value;

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
                row["analyseregel"] = CreateRowXml(vergelijking.Naam, searchrow.FieldNames, searchrow.FieldValues);
                row["referentieregel"] = CreateRowXml(vergelijking.Naam, found.FieldNames, found.FieldValues);

                row["analysesleutelwaarde"] = string.Join(", ", sleutel.fieldvalues);
                row["referentiesleutelwaarde"] = string.Join(", ", sleutel.fieldvalues);
                row["analyseveldwaarde"] = string.Join(", ", searchrow.FieldValues);
                row["referentieveldwaarde"] = string.Join(", ", found.FieldValues);

                ds.Tables["outputline"].Rows.Add(row);
            }
            match++;
        }

        public void EntryNoMatch(Vergelijking vergelijking, DataRegel searchrow, RegistratieItem searchitem, DataRegel found, string matcher, RegistratieItem analyse, RegistratieItem reference, RegistratieItem sleutel, bool countaserror)
        {
            if(countaserror || Properties.Settings.Default.output_nomatch_reportall) { 
                DataRow row = ds.Tables["outputline"].NewRow();
                row["outputid"] = koprow["outputid"];
                row["regelnummer"] = ds.Tables["outputline"].Rows.Count + 1;
                row["status"] = "NO MATCH";

                row["controle"] = matcher;
                row["sleutel"] = CreateRowXml(vergelijking.Naam, sleutel.fieldnames, sleutel.fieldvalues);
                row["analysewaarde"] = CreateRowXml(matcher, analyse.fieldnames, analyse.fieldvalues);
                row["referentiewaarde"] = CreateRowXml(matcher, reference.fieldnames, reference.fieldvalues);
                row["analyseregel"] = CreateRowXml(vergelijking.Analysis.DatabronNaam, searchrow.FieldNames, searchrow.FieldValues);
                row["referentieregel"] = CreateRowXml(vergelijking.Reference.DatabronNaam, found.FieldNames, found.FieldValues);

                row["analysesleutelwaarde"] = string.Join(", ", sleutel.fieldvalues);
                row["referentiesleutelwaarde"] = string.Join(", ", sleutel.fieldvalues);
                row["analyseveldwaarde"] = string.Join(", ", analyse.fieldvalues);
                row["referentieveldwaarde"] = string.Join(", ", reference.fieldvalues);

                ds.Tables["outputline"].Rows.Add(row);
            }
            if (countaserror)
            {
                nomatch++;
            }
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
            row["analyseregel"] = CreateRowXml(vergelijking.Analysis.DatabronNaam, searchrow.FieldNames, searchrow.FieldValues);
            row["referentieregel"] = DBNull.Value;


            row["analysesleutelwaarde"] = string.Join(", ", searchitem.fieldvalues);
            row["referentiesleutelwaarde"] = string.Join(", ", DBNull.Value);
            row["analyseveldwaarde"] = string.Join(", ", searchitem.fieldvalues);
            row["referentieveldwaarde"] = string.Join(", ", DBNull.Value);

            ds.Tables["outputline"].Rows.Add(row);
            missing++;
        }
    }
}
