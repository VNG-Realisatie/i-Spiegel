using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace RegistratieVergelijker
{
    public class RegistratieItem : IComparable
    {
        public string[] fieldvalues;

        public RegistratieItem(string[] fieldvalues)
        {
            this.fieldvalues = fieldvalues;
        }
        private int CompareTo(object obj) {
            if (!(obj is RegistratieItem)) throw new ArgumentException("Object is not a RegistratieItem.");
            RegistratieItem r2 = (RegistratieItem) obj;

            if(fieldvalues.Count() != r2.fieldvalues.Count()) throw new ArgumentException("Object is not a RegistratieItem with same field count");

            for (int i = 0; i < fieldvalues.Count(); i++ )
            {
                int result = String.Compare(fieldvalues[i], r2.fieldvalues[i]);
                if (result != 0) return result;
            }
            return 0;
        }
        int IComparable.CompareTo(object obj)
        {
            return CompareTo(obj);
        }
        public override bool Equals(object obj)
        {
            return 0 == CompareTo(obj);
        }
        public override string ToString()
        {
            string result = null;
            foreach (string field in fieldvalues)
            {
                string v = field == null ? " (null) ": "'" + field + "'";
                if (result == null) result = v;
                else result = result + "," + v;
            }
            return "{" + result + "}";
            
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int result = 0;
                foreach (string field in fieldvalues)
                {
                    result = result ^ (field != null ? field.GetHashCode() : 0);
                }
                return result;
            }
        }

    }

    public class RegistratieSource
    {

        public DataTable table = new DataTable();

        public RegistratieSource(System.Xml.XPath.XPathNavigator config)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(config.SelectSingleNode("database-provider").Value);
            
            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = config.SelectSingleNode("database-connection").Value;

            String sql = config.SelectSingleNode("database-query").Value;
            if(sql == null || sql.Trim().Length == 0) {
                // maybe in a file?
                String filename = config.SelectSingleNode("database-query/@queryfile").Value;
                System.IO.FileInfo fi = new System.IO.FileInfo(filename);
                System.Console.Out.WriteLine("\tquery from: " + fi.FullName);
                sql = System.IO.File.ReadAllText(fi.FullName);
            }

            DbCommand selectCommand = factory.CreateCommand();
            selectCommand.Connection = connection;
            selectCommand.CommandText = sql;

            DbDataAdapter adapter = factory.CreateDataAdapter();
            adapter.SelectCommand = selectCommand;
            adapter.Fill(table);
        }

        public RegistratieItem GetFieldValues(DataRow row, string[] fieldnames)
        {
            string[] fieldvalues = new string[fieldnames.Count()];
            for (int i = 0; i < fieldnames.Count(); i++)            
            {
                var fieldvalue = row[fieldnames[i]];
                if (fieldvalue == DBNull.Value) fieldvalues[i] = null;
                else fieldvalues[i] = Convert.ToString(fieldvalue);
            }
            return new RegistratieItem(fieldvalues);
        }

        public SortedDictionary<RegistratieItem, DataRow> GetSortedList(string[] fieldnames)
        {
            SortedDictionary<RegistratieItem, DataRow> result = new SortedDictionary<RegistratieItem, DataRow>();
            foreach (System.Data.DataRow row in table.Rows)
            {
                RegistratieItem item = GetFieldValues(row, fieldnames);
                if (!result.ContainsKey(item)) result.Add(item, row);
                else System.Console.Error.WriteLine("\tdouble entry in reference for: " + item);
            }
            return result;
        }

        public static string ToString(DataRow row)
        {
            string result = null;
            foreach (object field in row.ItemArray)
            {
                string v = (field == null || field == DBNull.Value) ? " (null) " : "'" + field + "'";
                if (result == null) result = v;
                else result = result + "," + v;
            }
            return "{" + result + "}";

        }

        public void Export(List<string> fields, List<string> names, System.IO.DirectoryInfo exportdirectory, string filename)
        {
            if(fields.Count != names.Count) throw new Exception("count mismatch!");
            filename = System.IO.Path.Combine(exportdirectory.FullName, filename + ".csv");
            System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, false);


            // fieldnames
            foreach(String fieldname in names) {
                if (fields[names.IndexOf(fieldname)] != null)
                {
                    writer.Write("\"" + fieldname + "\";");
                }
            }
            writer.Write(writer.NewLine);

            // rows
            foreach (DataRow row in table.Rows)
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
            System.Console.WriteLine("[export] successfull written to:" + filename);
        }
    }
}
