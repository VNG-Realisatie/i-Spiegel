using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpiegel
{
    public class EscapeSequence
    {
        List<string> van = new List<string>();
        List<string> naar = new List<string>();
        

        public EscapeSequence()
        {

        }

        public EscapeSequence(string filename)
        {
            if (filename.Length > 0)
            {
                using (var reader = new System.IO.StreamReader(filename))
                {

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        van.Add(values[0]);
                        naar.Add(values[1]);
                    }
                }
            }
        }

        public String EscapeValue(object databasevalue)
        {            
            if (databasevalue == DBNull.Value) return null;
            var unescaped = Convert.ToString(databasevalue);

            if (van.Count > 0)
            {
                StringBuilder sb = new StringBuilder(unescaped);
                for(int i=0; i < van.Count; i++)
                {
                    sb.Replace(van[i], naar[i]);
                }
                return sb.ToString();
            }
            else return unescaped;
        }
    }
}
