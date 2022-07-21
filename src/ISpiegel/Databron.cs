using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace ISpiegel
{
    public class Databron
    {
        public static Databron GetData(DbProviderFactory configprovider, DbConnection configconnection, String datasourcename)
        {
            var command = configprovider.CreateCommand();
            command.CommandText = "SELECT * FROM " + Properties.Settings.Default.databaseprefix + "databron WHERE databronnaam LIKE '" + datasourcename + "'";
            command.Connection = configconnection;
            var adapter = configprovider.CreateDataAdapter();
            adapter.SelectCommand = command;
            var table = new DataTable();

            System.Diagnostics.Debug.WriteLine("ISpiegel.Databron::GetData > Starting query:" + adapter.SelectCommand.CommandText);
            adapter.Fill(table);
            System.Diagnostics.Debug.WriteLine("ISpiegel.Databron::GetData > Finished query:" + adapter.SelectCommand.CommandText);

            if (table.Rows.Count != 1) throw new ISpiegelException("Kon de datasource met naam: '" + datasourcename + "' niet vinden!");


            String datasource_provider = Convert.ToString(table.Rows[0]["provider"]);
            String datasource_connectionstring = Convert.ToString(table.Rows[0]["connectionstring"]);
            System.Diagnostics.Debug.WriteLine("ISpiegel.Databron::GetData > Connectionstring:" + datasource_connectionstring);
            String datasource_query = Convert.ToString(table.Rows[0]["query"]);
            String applicatie = Convert.ToString(table.Rows[0]["applicatienaam"]);
            String gemeentecode = Convert.ToString(table.Rows[0]["gemeentecode"]);
            String escapesequence = "";
            // make backwards compatible
            if (table.Columns.Contains("escapesequence"))
            {
                escapesequence = Convert.ToString(table.Rows[0]["escapesequence"]);
            }
            else
            {
                Output.Warn("!!! Column 'escapesequence' not defined in databron, using NO escapers anywhere !!!");
            }
            EscapeSequence escaper = new EscapeSequence(escapesequence);

            var connection = new ISpiegel.Provider.FileSystem.Connection();
            DbProviderFactory datasource_factory = null;
            if (datasource_provider.ToLower().Equals("ispiegel.providers.filesystem"))
            {
                datasource_factory = new ISpiegel.Provider.FileSystem.Factory();
            }
            else if (datasource_provider.ToLower().Equals("ispiegel.providers.odata"))
            {
                datasource_factory = new ISpiegel.Provider.OData.Factory();
            }
            else
            {
                datasource_factory = DbProviderFactories.GetFactory(datasource_provider);
            }
            var datasource_connection = datasource_factory.CreateConnection();
            datasource_connection.ConnectionString = datasource_connectionstring.Replace("${WORKING_DIRECTORY}", System.IO.Directory.GetCurrentDirectory());
            System.Diagnostics.Debug.WriteLine("ISpiegel.Databron::GetData > Connectionstring:" + datasource_connection.ConnectionString);
            datasource_connection.Open();
            var datasource_command = datasource_factory.CreateCommand();
            datasource_command.CommandText = datasource_query;
            datasource_command.Connection = datasource_connection;
            var datasource_adapter = datasource_factory.CreateDataAdapter();
            datasource_adapter.SelectCommand = datasource_command;
            var datasource_table = new DataTable();
            System.Diagnostics.Debug.WriteLine("ISpiegel.Databron::GetData > Starting query:" + datasource_adapter.SelectCommand.CommandText);
            datasource_adapter.Fill(datasource_table);
            System.Diagnostics.Debug.WriteLine("ISpiegel.Databron::GetData > Finished query:" + datasource_adapter.SelectCommand.CommandText);

            datasource_connection.Close();
            return new Databron(datasource_table, datasourcename, applicatie, datasource_query, gemeentecode, escaper);
        }

        private DataTable data;
        private string databronnaam;
        private string applicatienaam;
        private string referentiequery;
        private string gemeentecode;
        private EscapeSequence escaper;
        public EscapeSequence Escaper
        {
            get
            {
                return escaper;
            }
        }

        internal Databron(DataTable data, string databronnaam, string applicatienaam, string referentiequery, string gemeentecode, EscapeSequence escaper)
        {
            this.data = data;
            this.databronnaam = databronnaam;
            this.applicatienaam = applicatienaam;
            this.referentiequery = referentiequery;
            this.gemeentecode = gemeentecode;
            this.escaper = escaper;
        }

        public int Count
        {
            get
            {
                return data.Rows.Count;
            }
        }

        public DataColumnCollection Columns
        {
            get
            {
                return data.Columns;
            }
        }

        public List<DataRegel> Regels
        {
            get
            {
                // for now we will wrap everything
                List<DataRegel> regels = new List<DataRegel>();
                foreach (DataRow row in data.Rows)
                {
                    regels.Add(new DataRegel(this, row));
                }
                return regels;
            }
        }

        public String DatabronNaam
        {
            get
            {
                return databronnaam;
            }
        }
        public String ApplicatieNaam
        {
            get
            {
                return applicatienaam;
            }
        }
        public String ReferentieQuery
        {
            get
            {
                return referentiequery;
            }
        }

        public String GemeenteCode
        {
            get
            {
                return gemeentecode;
            }
        }

        public SortedDictionary<RegistratieItem, DataRegel> GetSortedList(string[] fieldnames)
        {
            SortedDictionary<RegistratieItem, DataRegel> result = new SortedDictionary<RegistratieItem, DataRegel>();
            foreach (System.Data.DataRow row in data.Rows)
            {
                //RegistratieItem item = GetFieldValues(row, fieldnames);
                DataRegel regel = new DataRegel(this, row);
                RegistratieItem item = regel.GetFieldValues(fieldnames);
                if (!result.ContainsKey(item)) result.Add(item, regel);
                else Output.Warn("\tdouble entry in reference for: " + item);
            }
            return result;
        }

        public void Export(List<string> fields, List<string> names, System.IO.DirectoryInfo exportdirectory, string filename)
        {
            if (fields.Count != names.Count) throw new ISpiegelException("count mismatch!");
            filename = System.IO.Path.Combine(exportdirectory.FullName, filename + ".csv");
            System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, false);


            // fieldnames
            foreach (String fieldname in names)
            {
                if (fields[names.IndexOf(fieldname)] != null)
                {
                    writer.Write("\"" + fieldname + "\";");
                }
            }
            writer.Write(writer.NewLine);

            // rows
            foreach (DataRow row in data.Rows)
            {
                foreach (String fieldname in fields)
                {
                    if (fieldname != null)
                    {
                        writer.Write("\"" + row[fieldname].ToString().Replace("\"", "\"\"") + "\";");
                    }
                }
                writer.Write(writer.NewLine);
            }
            writer.Close();
            Output.Info("\t[export] successfull written to:" + filename);
        }
    }
}
