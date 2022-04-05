using System;
using System.Data;
using System.Data.Common;

namespace ISpiegel.Provider.FileSystem
{
    public class Command : DbCommand
    {
        private string commandtekst;
        public override string CommandText
        {
            get
            {
                return commandtekst;
            }
            set
            {
                commandtekst = value;
            }
        }
        internal Connection connection;
        protected override DbConnection DbConnection
        {
            get
            {
                return connection;
            }
            set
            {
                connection = (Connection)value;
            }
        }
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return new DataReader(this);
        }

        public override int CommandTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override CommandType CommandType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool DesignTimeVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override UpdateRowSource UpdatedRowSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override DbParameterCollection DbParameterCollection => throw new NotImplementedException();

        protected override DbTransaction DbTransaction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        protected override DbParameter CreateDbParameter()
        {
            throw new NotImplementedException();
        }
    }
}
