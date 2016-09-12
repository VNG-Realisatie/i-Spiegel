using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hodor
{
    interface IReporter
    {
        void Start(string exportname, string referencename, string analysename, string configuration, string referencesql, string analysesql);

        void EntryMatch(System.Data.DataRow searchrow, RegistratieItem searchitem, System.Data.DataRow foundrow);
        void EntryNoMatch(System.Data.DataRow searchrow, RegistratieItem searchitem, System.Data.DataRow found, string matchername, RegistratieItem analyse, RegistratieItem reference);
        void EntryNotFound(string primary, System.Data.DataRow searchrow, RegistratieItem searchitem);

        void Stop(long analysecount, long referencecount);
    }
}
