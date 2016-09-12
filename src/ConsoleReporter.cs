using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hodor
{
    public class ConsoleReporter : IReporter 
    {
        private long match;
        private long nomatch;
        private long missing;

        public void Start(string exportname, string referencename, string analysename, string configuration, string referencesql, string analysesql)
        {
            Console.WriteLine("[start]\texportname:\t\t" + exportname);
            Console.WriteLine("[start]\treferencename:\t\t" + referencename);
            Console.WriteLine("[start]\tanalysename:\t\t" + analysename);

            match = 0;
            nomatch = 0;
            missing = 0;
        }

        public void EntryMatch(System.Data.DataRow searchrow, RegistratieItem searchitem, System.Data.DataRow foundrow)
        {
            // no need to print anything
            match++;
        }

        public void EntryNoMatch(System.Data.DataRow searchrow, RegistratieItem searchitem, System.Data.DataRow found, string matchername, RegistratieItem analyse, RegistratieItem reference)
        {
            Console.WriteLine("[NO MATCH]\t" + matchername + "\t" + searchitem);
            Console.WriteLine("\tfound:\t\t" + analyse);
            Console.WriteLine("\texpected:\t" + reference);

            nomatch++;
        }

        public void EntryNotFound(string primary, System.Data.DataRow searchrow, RegistratieItem searchitem)
        {
            Console.WriteLine("[NOT FOUND]\t" + primary);
            Console.WriteLine("\t\t" + searchitem);

            missing++;
        }

        public void Stop(long analysecount, long referencecount)
        {
            Console.WriteLine("[stop]\tanalysis.count:\t\t" + analysecount);
            Console.WriteLine("[stop]\treference.count:\t" + referencecount);
            Console.WriteLine("[stop]\tfull match\t\t" + match);
            Console.WriteLine("[stop]\thalve match\t\t" + nomatch);
            Console.WriteLine("[stop]\tno match\t\t" + missing);
            Console.WriteLine("[stop]");
            double percent = (100.0 / analysecount) * match;
            Console.WriteLine("[stop]\t==>>  " + percent.ToString("0.00") + "%  <<==");
            Console.WriteLine("[stop]");
            Console.WriteLine("press any key to continue......");
            Console.ReadKey();
        }
    }
}

