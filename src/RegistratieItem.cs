using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ISpiegel
{
    public class RegistratieItem : IComparable
    {
        public string[] fieldnames;
        public string[] fieldvalues;
        

        public RegistratieItem(string[] fieldnames, string[] fieldvalues)
        {
            this.fieldnames = fieldnames;
            this.fieldvalues = fieldvalues;
        }
        private int CompareTo(object obj) {
            // misschien nog eens de fieldnames vergelijken, maar dat kan altijd nog

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
}
