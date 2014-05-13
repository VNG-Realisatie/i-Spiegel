using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace RegistratieVergelijker
{
    public class DatabaseReporter : IReporter 
    {
        private DbConnection connection = null;
        private DataSet ds = null;
        private DbDataAdapter kopadapter = null;
        private DataRow koprow = null;
        private DbDataAdapter regeladapter = null;

        private long match;
        private long nomatch;
        private long missing;

        public DatabaseReporter(String provider, String connectionstring)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            connection = factory.CreateConnection();
            connection.ConnectionString = connectionstring;

            DbCommand command = factory.CreateCommand();
            command.CommandText = "SELECT * FROM registratievergelijking";
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
            command.CommandText = "SELECT * FROM registratievergelijkingregel";
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
            command.CommandText = "SELECT MAX(registratievergelijkingid) FROM registratievergelijking ";
            command.Connection.Open();
            object result = command.ExecuteScalar();
            long registratievergelijkingid = DBNull.Value == result ? 0 : Convert.ToUInt32(result);
            registratievergelijkingid++;
            command.Connection.Close();

            ds = new DataSet();
            kopadapter.Fill(ds, "registratievergelijking");
            koprow = ds.Tables["registratievergelijking"].NewRow();
            koprow["registratievergelijkingid"] = registratievergelijkingid;
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
            ds.Tables["registratievergelijking"].Rows.Add(koprow);
            kopadapter.Update(ds, "registratievergelijking");

            regeladapter.SelectCommand.CommandText = "SELECT * FROM registratievergelijkingregel WHERE registratievergelijkingid = " + registratievergelijkingid;
            regeladapter.Fill(ds, "registratievergelijkingregel");

            match = 0;
            nomatch = 0;
            missing = 0;
        }

        public void EntryMatch(System.Data.DataRow searchrow, RegistratieItem searchitem, System.Data.DataRow foundrow)
        {
            /*
            DataRow row = ds.Tables["registatievergelijkingregel"].NewRow();
            row["registratievergelijkingid"] = koprow["registratievergelijkingid"];
            row["regelnummer"] = ds.Tables["registatievergelijkingregel"].Rows.Count + 1;
            row["status"] = "MATCH";
            row["melding"] = "match:" + searchitem;
            row["referentieregel"] = RegistratieSource.ToString(foundrow);
            row["analyseregel"] = RegistratieSource.ToString(searchrow);
            ds.Tables["registatievergelijkingregel"].Rows.Add(row);
            */
            match++;
        }

        public void EntryNoMatch(System.Data.DataRow searchrow, RegistratieItem searchitem, System.Data.DataRow found, string matchername, RegistratieItem analyse, RegistratieItem reference)
        {
            DataRow row = ds.Tables["registatievergelijkingregel"].NewRow();
            row["registratievergelijkingid"] = koprow["registratievergelijkingid"];
            row["regelnummer"] = ds.Tables["registatievergelijkingregel"].Rows.Count + 1;
            row["status"] = "NO MATCH";
            row["melding"] = "matcher:" + matchername + " found:" + analyse + " expected:" + reference;
            row["referentieregel"] = RegistratieSource.ToString(found);
            row["analyseregel"] = RegistratieSource.ToString(searchrow);
            ds.Tables["registatievergelijkingregel"].Rows.Add(row);

            nomatch++;
        }

        public void EntryNotFound(string primary, System.Data.DataRow searchrow, RegistratieItem searchitem)
        {
            DataRow row = ds.Tables["registatievergelijkingregel"].NewRow();
            row["registratievergelijkingid"] = koprow["registratievergelijkingid"];
            row["regelnummer"] = ds.Tables["registatievergelijkingregel"].Rows.Count + 1;
            row["status"] = "NOT FOUND";
            row["melding"] = "matcher:" + primary + " search:" + searchitem;
            row["referentieregel"] = DBNull.Value;
            row["analyseregel"] = RegistratieSource.ToString(searchrow);
            ds.Tables["registatievergelijkingregel"].Rows.Add(row);

            missing++;
        }

        public void Stop(long analysecount, long referencecount)
        {
            // save the regels
            regeladapter.Update(ds, "registatievergelijkingregel");

            // update the koprow
            koprow["percentage"] = (100.0 / analysecount) * match;
            koprow["referentieaantal"] = referencecount;
            koprow["analyseaantal"] = analysecount;
            koprow["gelijkaantal"] = match;
            koprow["afwijkingaantal"] = nomatch;
            koprow["nietgevondenaantal"] = missing;
            koprow["looptijd"] = DateTime.Now;
            kopadapter.Update(ds, "registratievergelijking");
        }
    }
}
