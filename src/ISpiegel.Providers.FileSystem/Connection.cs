using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpiegel.Provider.FileSystem
{
    public class Connection : DbConnection
    {
        private string connectionstring;
        private System.IO.DirectoryInfo path;

        public override string ConnectionString {
            get {
                return connectionstring;
            }
            set {
                connectionstring = value;
            }
        }
        private ConnectionState state = ConnectionState.Closed;

        public override ConnectionState State
        {
            get
            {
                return state;
            }
        }

        public override void Open()
        {
            state = ConnectionState.Open;
    }

        public override void Close()
        {
            state = ConnectionState.Closed;
        }

        public override string Database => throw new NotImplementedException();

        public override string DataSource => throw new NotImplementedException();

        public override string ServerVersion => throw new NotImplementedException();


        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        protected override DbCommand CreateDbCommand()
        {
            throw new NotImplementedException();
        }
    }
}
