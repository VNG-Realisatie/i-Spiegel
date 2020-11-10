using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpiegel
{
    public class Vergelijking
    {
        public string Naam;
        public string VeldToewijzing;
        public string ReferentieDatabronNaam;
        public string AnalyseDatabronNaam;
        public string Rapporttype;

        public Vergelijking(string naam, string veldtoewijzing, string referentiedatabronnaam, string analysedatabronnaam, string rapporttype) {
            this.Naam = naam;
            this.VeldToewijzing = veldtoewijzing;
            this.ReferentieDatabronNaam = referentiedatabronnaam;
            this.AnalyseDatabronNaam = analysedatabronnaam;
            this.Rapporttype = rapporttype;
        }

        public Databron Reference {
            get;
            internal set;
        }
        public Databron Analysis {
            get;
            internal set;
        }
    }
}
