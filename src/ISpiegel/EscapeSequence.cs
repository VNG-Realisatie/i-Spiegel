using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpiegel
{
    public class EscapeSequence
    {
        List<string> vanEscapedArray = new List<string>();
        List<string> naarEscapedArray = new List<string>();
        

        public EscapeSequence()
        {

        }

        public EscapeSequence(string filename)
        {
            if (filename.Length > 0)
            {
                // StreamReader default => Encoding.UTF8
                using (var reader = new System.IO.StreamReader(filename))
                {

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        vanEscapedArray.Add(values[0]);
                        naarEscapedArray.Add(values[1]);
                    }
                }
            }
        }

        public String EscapeValue(object databasevalue)
        {            
            if (databasevalue == DBNull.Value) return null;
            var unescaped = Convert.ToString(databasevalue);

            if (vanEscapedArray.Count > 0)
            {
                // ugly, but hey it works :D
                var builder = new StringBuilder();
                int i = 0;
                while (i < unescaped.Length)
                {
                    var escaped = false;

                    // Kijk of we de huidige moeten escapen
                    for (int j = 0; j < vanEscapedArray.Count; j++)
                    {
                        var van = vanEscapedArray[j];
                        var naar = naarEscapedArray[j];
                        var huidige = unescaped.Substring(i);
                        if (huidige.StartsWith(van,StringComparison.Ordinal))
                        {
                            builder.Append(naar);
                            escaped = true;

                            // if(ISpiegel.Properties.Settings.Default.escape_logging) Console.WriteLine("\tEscaping character, match on: '" + van + "'-->'" + naar + "' on pos:" + i + " in string: '" + unescaped + "' current new string: '" + builder + "'");

                            i += van.Length;

                            // we hebben een scape gedaan, 
                            // we kunnen nu ook gewoon door naar het volgende karakter
                            break;
                        }
                    }

                    // niets gevonden, gewoon copieren
                    if (!escaped) {
                        builder.Append(unescaped[i]);
                        i++;
                    }
                }
                var result = builder.ToString();
                if (!result.Equals(unescaped))
                {
                    //System.Diagnostics.Debug.WriteLine("Escaped string, from: '" + unescaped + "' to '" + result + "'");
                    //if (ISpiegel.Properties.Settings.Default.escape_logging) Console.WriteLine("Escaped string, from: '" + unescaped + "' to '" + result + "'");
                }                
                return result;
            }
            else return unescaped;
        }
    }
}
