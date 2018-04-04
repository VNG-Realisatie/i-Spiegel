using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace GegevensVergelijker
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
        private long invalid;

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
            builder.DataAdapter = regeladapter;
            regeladapter.InsertCommand = builder.GetInsertCommand();
            regeladapter.UpdateCommand = builder.GetUpdateCommand();
            regeladapter.DeleteCommand = builder.GetDeleteCommand();
        }

        public void Start(string exportname, string referentiename, string analysename, string configuration, string referencesql, string analysesql)
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
            koprow["vergelijkingnaam"] = exportname;
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
            koprow["ongeldigaantal"] = 0;            
            koprow["looptijd"] = new DateTime(watch.ElapsedTicks);

            ds.Tables["output"].Rows.Add(koprow);
            kopadapter.Update(ds, "output");

            regeladapter.SelectCommand.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "outputline WHERE outputid = " + outputid;
            regeladapter.Fill(ds, "outputline");

            match = 0;
            nomatch = 0;
            missing = 0;
            invalid = 0;
        }


        public void EntryInvalid(string vergelijking, string sleutelcolumname, string checkcolumnname, string checkvalue, DataRow found)
        {
            DataRow row = ds.Tables["outputline"].NewRow();
            row["outputid"] = koprow["outputid"];
            row["regelnummer"] = ds.Tables["outputline"].Rows.Count + 1;
            row["status"] = "INVALID";


//            row["melding"] = "check:" + checkname + " found:" + found[columnname] + " expected:" + checkvalue;
//            row["referentieregel"] = RegistratieSource.ToString(found);
//            //row["analyseregel"] = RegistratieSource.ToString(searchrow);
//            ds.Tables["outputline"].Rows.Add(row);


            row["controle"] = vergelijking;
            //row["sleutel"] = RegistratieSource.ToFieldXml(sleutelcolumname, Convert.ToString(found[sleutelcolumname]));
            //row["analysewaarde"] = RegistratieSource.ToFieldXml(checkcolumnname, checkvalue);
            row["sleutel"] = CreateRowXml(vergelijking, sleutelcolumname, Convert.ToString(found[sleutelcolumname]));
            row["analysewaarde"] = CreateRowXml(vergelijking, checkcolumnname, checkvalue);
            //row["referentiewaarde"] = DBNull.Value;
            row["referentiewaarde"] = CreateRowXml(vergelijking);
            //row["analyseregel"] = RegistratieSource.ToFieldXml(found);
            row["analyseregel"] = CreateRowXml(vergelijking, ToStringArray(found.Table.Columns), ToStringArray(found.ItemArray));
            row["referentieregel"] = CreateRowXml(vergelijking);
            ds.Tables["outputline"].Rows.Add(row);

            invalid++;
        }

        private string[] ToStringArray(DataColumnCollection columns)
        {
            var result = new List<string>();
            foreach(DataColumn cul in columns)
            {
                result.Add(cul.ColumnName);
            }
            return result.ToArray();
        }

        private string[] ToStringArray(object[] itemArray)
        {
            var result = new List<string>();
            foreach (object obj in itemArray)
            {
                result.Add(
                    Convert.ToString(obj)
                    );
            }
            return result.ToArray();
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
            var rootnode = doc.CreateElement(System.Xml.XmlConvert.EncodeName(vergelijking));
            var nil = doc.CreateAttribute("nil", "xsi", "http://w3.org/2001/XMLSchema-instance");
            if (fieldnames.Length > 0)
            {
                for(int i=0; i < fieldnames.Length; i++)
                {
                    var element = doc.CreateElement(System.Xml.XmlConvert.EncodeName(fieldnames[i]));
                    if(fieldvalues[i] == null)
                    {
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
                rootnode.Attributes.Append(nil);                    
            }
            doc.AppendChild(rootnode);

            var stream = new System.IO.MemoryStream();
            var  writer = new System.Xml.XmlTextWriter(stream, Encoding.Unicode);
            writer.Formatting = System.Xml.Formatting.Indented;
            doc.WriteContentTo(writer);
            writer.Flush();
            stream.Flush();
            stream.Position = 0;
            var reader = new System.IO.StreamReader(stream);
            return reader.ReadToEnd();
        }

        public void EntryNoMatch(string vergelijking, System.Data.DataRow searchrow, RegistratieItem searchitem, System.Data.DataRow found, string matcher, RegistratieItem analyse, RegistratieItem reference, RegistratieItem sleutel)
        {
            DataRow row = ds.Tables["outputline"].NewRow();
            row["outputid"] = koprow["outputid"];
            row["regelnummer"] = ds.Tables["outputline"].Rows.Count + 1;
            row["status"] = "NO MATCH";

            row["controle"] = matcher;
            row["sleutel"] = CreateRowXml(vergelijking, sleutel.fieldnames, sleutel.fieldvalues);
            row["analysewaarde"] = CreateRowXml(vergelijking, searchitem.fieldnames, searchitem.fieldvalues);
            row["referentiewaarde"] = CreateRowXml(vergelijking, reference.fieldnames, reference.fieldvalues);            
            row["analyseregel"] = CreateRowXml(vergelijking, ToStringArray(searchrow.Table.Columns), ToStringArray(searchrow.ItemArray));
            row["referentieregel"] = CreateRowXml(vergelijking, ToStringArray(found.Table.Columns), ToStringArray(found.ItemArray));
            ds.Tables["outputline"].Rows.Add(row);
            nomatch++;
        }

        public void EntryNotFound(string vergelijking, string matcher, System.Data.DataRow searchrow, RegistratieItem searchitem)
        {
            DataRow row = ds.Tables["outputline"].NewRow();
            row["outputid"] = koprow["outputid"];
            row["regelnummer"] = ds.Tables["outputline"].Rows.Count + 1;
            row["status"] = "NOT FOUND";


            row["controle"] = matcher;
            row["sleutel"] = CreateRowXml(vergelijking, searchitem.fieldnames, searchitem.fieldvalues);
            row["analysewaarde"] = CreateRowXml(vergelijking, searchitem.fieldnames, searchitem.fieldvalues);
            row["referentiewaarde"] = CreateRowXml(vergelijking);
            row["analyseregel"] = CreateRowXml(vergelijking, ToStringArray(searchrow.Table.Columns), ToStringArray(searchrow.ItemArray));
            row["referentieregel"] = CreateRowXml(vergelijking);
            ds.Tables["outputline"].Rows.Add(row);

            missing++;
        }
        public String ResultLine;
        private object HttpUtility;

        public void EntryMatch(string vergelijking, string matcher, string sleutelcolumname, string checkcolumnname, string checkvalue, DataRow found)
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
                row["sleutel"] = CreateRowXml(vergelijking, sleutelcolumname, Convert.ToString(found[sleutelcolumname]));
                row["analysewaarde"] = CreateRowXml(vergelijking, checkcolumnname, checkvalue);
                row["referentiewaarde"] = CreateRowXml(vergelijking);
                row["analyseregel"] = CreateRowXml(vergelijking, ToStringArray(found.Table.Columns), ToStringArray(found.ItemArray));
                row["referentieregel"] = CreateRowXml(vergelijking);
                ds.Tables["outputline"].Rows.Add(row);
            }
            match++;
        }
        
        public void EntryMatch(string vergelijking, System.Data.DataRow searchrow, System.Data.DataRow found, RegistratieItem sleutel)
        {
            DataRow row = ds.Tables["outputline"].NewRow();
            row["outputid"] = koprow["outputid"];
            row["regelnummer"] = ds.Tables["outputline"].Rows.Count + 1;
            row["status"] = "VALID";

            row["controle"] = DBNull.Value;
            row["sleutel"] = CreateRowXml(vergelijking, sleutel.fieldnames, sleutel.fieldvalues);
            row["analysewaarde"] = CreateRowXml(vergelijking);
            row["referentiewaarde"] = CreateRowXml(vergelijking);
            row["analyseregel"] = CreateRowXml(vergelijking, ToStringArray(searchrow.Table.Columns), ToStringArray(searchrow.ItemArray));
            row["referentieregel"] = CreateRowXml(vergelijking, ToStringArray(found.Table.Columns), ToStringArray(found.ItemArray));
            ds.Tables["outputline"].Rows.Add(row);

            match++;
        }

        public void Stop(string analyseapplicatie, string referentieapplicatie, string analysequery, string referentiequery, string analysegemeentecode, string referentiegemeentecode, long analysecount, long referencecount)
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
            koprow["ongeldigaantal"] = invalid;

            // the code that you want to measure comes here
            watch.Stop();
            koprow["looptijd"] = new DateTime(watch.ElapsedTicks);
            kopadapter.Update(ds, "output");
            // dimensions
            ResultLine = "referentieapplicatie=" + (referentieapplicatie == null ? "" : referentieapplicatie.Replace(" ", "\\ ").Replace(",", "\\,").Replace("=", "\\=")) + ",";
            ResultLine += "analyseapplicatie=" + (analyseapplicatie == null ? "" : analyseapplicatie.Replace(" ", "\\ ").Replace(",", "\\,").Replace("=", "\\=")) + ",";
            ResultLine += "referentiegemeentecode=" + (referentiegemeentecode == null ? "" : referentiegemeentecode.Replace(" ", "\\ ").Replace(",", "\\,").Replace("=", "\\=")) + ",";
            ResultLine += "analysegemeentecode=" + (analysegemeentecode == null ? "" : analysegemeentecode.Replace(" ", "\\ ").Replace(",", "\\,").Replace("=", "\\=")) + " ";
            // values
            ResultLine += "referentieaantal=" + referencecount + ",";
            ResultLine += "analyseaantal=" + analysecount + ",";
            ResultLine += "gelijkaantal=" + match + ",";
            ResultLine += "afwijkingaantal=" + nomatch + ",";
            ResultLine += "nietgevondenaantal=" + missing + ",";
            ResultLine += "ongeldigaantal=" + invalid + ",";
            ResultLine += "percentage=" + Math.Round((100.0 / analysecount) * match, 2);
        }
    }
}
