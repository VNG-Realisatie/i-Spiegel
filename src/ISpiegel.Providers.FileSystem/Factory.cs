using System.Data.Common;

namespace ISpiegel.Provider.FileSystem
{
    public class Factory : System.Data.Common.DbProviderFactory
    {
        /*
        public virtual bool CanCreateDataSourceEnumerator { get; }
        public virtual DbCommand CreateCommand();
        public virtual DbCommandBuilder CreateCommandBuilder();
        public virtual DbConnection CreateConnection();
        public virtual DbConnectionStringBuilder CreateConnectionStringBuilder();
        public virtual DbDataAdapter CreateDataAdapter();
        public virtual DbDataSourceEnumerator CreateDataSourceEnumerator();
        public virtual DbParameter CreateParameter();
        public virtual CodeAccessPermission CreatePermission(PermissionState state);
        */
        public override DbConnection CreateConnection()
        {
            return new Connection();
        }
        public override DbCommand CreateCommand()
        {
            return new Command();
        }
        public override DbDataAdapter CreateDataAdapter()
        {
            return new DataAdapter();
        }
    }
}
