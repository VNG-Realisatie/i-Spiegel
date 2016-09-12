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

        public DatabaseReporter(DbProviderFactory factory, DbConnection connection) {
            this.connection = connection;

            DbCommand command = factory.CreateCommand();
            command.CommandText = "SELECT * FROM hodor_output";
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
            command.CommandText = "SELECT * FROM hodor_outputline";
            command.Connection = connection;

            regeladapter = factory.CreateDataAdapter();
            regeladapter.SelectCommand = command;
            builder = factory.CreateCommandBuilder();
            builder.DataAdapter = regeladapter;
            regeladapter.InsertCommand = builder.GetInsertCommand();
            regeladapter.UpdateCommand = builder.GetUpdateCommand();
            regeladapter.DeleteCommand = builder.GetDeleteCommand();
        }

        public void Start(string exportname, string referencename, string analysename, string configuration, string referencesql, string analysesql)
        {
            // get our next id
            DbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT MAX(outputid) FROM hodor_output ";
            object result = command.ExecuteScalar();
            long outputid = DBNull.Value == result ? 0 : Convert.ToUInt32(result);
            outputid++;

            ds = new DataSet();
            kopadapter.Fill(ds, "hodor_output");
            koprow = ds.Tables["hodor_output"].NewRow();
            koprow["outputid"] = outputid;
            koprow["tijdstip"] = DateTime.Now;
            koprow["vergelijking"] = exportname;
            koprow["configuratie"] = configuration;
            koprow["referencesql"] = referencesql;
            koprow["analysesql"] = analysesql;
            koprow["percentage"] = 0;
            koprow["referentieaantal"] = 0;
            koprow["analyseaantal"] = 0;
            koprow["gelijkaantal"] = 0;
            koprow["afwijkingaantal"] = 0;
            koprow["nietgevondenaantal"] = 0;
            koprow["looptijd"] = DateTime.MinValue;
            ds.Tables["hodor_output"].Rows.Add(koprow);
            kopadapter.Update(ds, "hodor_output");

            regeladapter.SelectCommand.CommandText = "SELECT * FROM hodor_outputline WHERE outputid = " + outputid;
            regeladapter.Fill(ds, "hodor_outputline");

            match = 0;
            nomatch = 0;
            missing = 0;
            invalid = 0;
        }

        public void EntryMatch(System.Data.DataRow searchrow)
        {
            /*
            DataRow row = ds.Tables["hodor_outputline"].NewRow();
            row["outputid"] = koprow["outputid"];
            row["regelnummer"] = ds.Tables["hodor_outputline"].Rows.Count + 1;
            row["status"] = "MATCH";
            row["melding"] = "match:" + searchitem;
            row["referentieregel"] = RegistratieSource.ToString(foundrow);
            row["analyseregel"] = RegistratieSource.ToString(searchrow);
            ds.Tables["hodor_outputline"].Rows.Add(row);
            */
            match++;
        }


        public void EntryInvalid(string checkname, string primary, string columnname, string checkvalue, DataRow found)
        {
            DataRow row = ds.Tables["hodor_outputline"].NewRow();
            row["outputid"] = koprow["outputid"];
            row["regelnummer"] = ds.Tables["hodor_outputline"].Rows.Count + 1;
            row["status"] = "INVALID";
            row["melding"] = "check:" + checkname + " found:" + found[columnname] + " expected:" + checkvalue;
            row["referentieregel"] = RegistratieSource.ToString(found);
            //row["analyseregel"] = RegistratieSource.ToString(searchrow);
            ds.Tables["hodor_outputline"].Rows.Add(row);

            invalid++;

        }

        public void EntryNoMatch(System.Data.DataRow searchrow, RegistratieItem searchitem, System.Data.DataRow found, string matchername, RegistratieItem analyse, RegistratieItem reference)
        {
            DataRow row = ds.Tables["hodor_outputline"].NewRow();
            row["outputid"] = koprow["outputid"];
            row["regelnummer"] = ds.Tables["hodor_outputline"].Rows.Count + 1;
            row["status"] = "NO MATCH";
            row["melding"] = "matcher:" + matchername + " found:" + analyse + " expected:" + reference;
            row["referentieregel"] = RegistratieSource.ToString(found);
            row["analyseregel"] = RegistratieSource.ToString(searchrow);
            ds.Tables["hodor_outputline"].Rows.Add(row);

            nomatch++;
        }

        public void EntryNotFound(string primary, System.Data.DataRow searchrow, RegistratieItem searchitem)
        {
            DataRow row = ds.Tables["hodor_outputline"].NewRow();
            row["outputid"] = koprow["outputid"];
            row["regelnummer"] = ds.Tables["hodor_outputline"].Rows.Count + 1;
            row["status"] = "NOT FOUND";
            row["melding"] = "matcher:" + primary + " search:" + searchitem;
            row["referentieregel"] = DBNull.Value;
            row["analyseregel"] = RegistratieSource.ToString(searchrow);
            ds.Tables["hodor_outputline"].Rows.Add(row);

            missing++;
        }

        public void Stop(long analysecount, long referencecount = 0)
        {
            // save the regels
            regeladapter.Update(ds, "hodor_outputline");

            // update the koprow
            koprow["percentage"] = (100.0 / analysecount) * match;
            koprow["referentieaantal"] = referencecount;
            koprow["analyseaantal"] = analysecount;
            koprow["gelijkaantal"] = match;
            koprow["afwijkingaantal"] = nomatch;
            koprow["nietgevondenaantal"] = missing;
            koprow["ongeldig"] = invalid;
            koprow["looptijd"] = DateTime.Now;
            kopadapter.Update(ds, "hodor_output");
        }
    }
}
