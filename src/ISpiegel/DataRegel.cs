using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpiegel
{
    public class DataRegel
    {
        private DataRow row;
        private Databron bron;


        private string[] ToStringArray(DataColumnCollection columns)
        {
            var result = new List<string>();
            foreach (DataColumn cul in columns)
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

        public string[] FieldNames {
            get
            {
                return ToStringArray(bron.Columns);
            }
        }
        public string[] FieldValues {
            get
            {                
                List<string> result = new List<string>();
                foreach(string fieldname in FieldNames)
                {
                    result.Add(this[fieldname]);
                }
                return result.ToArray();
            }
        }

        public DataRegel(Databron bron, DataRow row)
        {
            this.bron = bron;
            this.row = row;
        }

        public RegistratieItem GetFieldValues(string[] fieldnames)
        {
            string[] fieldvalues = new string[fieldnames.Count()];
            for (int i = 0; i < fieldnames.Count(); i++)
            {
                var value = row[fieldnames[i]];
                fieldvalues[i] = bron.Escaper.EscapeValue(value);
            }
            return new RegistratieItem(fieldnames, fieldvalues);
        }

        public String this[string fieldname]
        {
            get
            {
                if (!row.Table.Columns.Contains(fieldname)) throw new Exception("column with name:" + fieldname + " does not exist!");
                var value = row[fieldname];
                string result = bron.Escaper.EscapeValue(value);
                return result;
            }
        }
    }
}
