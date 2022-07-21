using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;

namespace ISpiegel.Provider.OData
{
    public class DataReader : System.Data.Common.DbDataReader
    {
        private Command creator = null;

        private JsonDocument document;

        public DataReader(Command creator)
        {
            this.creator = creator;

            var request = System.Net.WebRequest.Create(creator.CommandText);

            string input = creator.connection.ConnectionString;
            foreach(var line in input.Split('\n'))
            {
                string[] header = line.Split(':');
                request.Headers.Add(header[0].Trim(), header[1].Trim());
            }
            var response = request.GetResponse();
            var reader = new System.IO.StreamReader(response.GetResponseStream());
            var body = reader.ReadToEnd();
            document = JsonDocument.Parse(body);
        }

        bool closed = false;
        public override bool IsClosed
        {
            get
            {
                return closed;
            }
        }

        Dictionary<int, string> fields;
        System.Text.Json.JsonElement.ArrayEnumerator rowenumerator;

        public override int FieldCount
        {
            get
            {
                fields = new Dictionary<int, string>();

                if (document.RootElement.GetArrayLength() == 0) return 0;
                rowenumerator = document.RootElement.EnumerateArray();
                JsonElement firstrow = rowenumerator.First();                
                foreach (JsonProperty field in firstrow.EnumerateObject())
                {
                    fields.Add(fields.Count, field.Name);
                }
                return firstrow.EnumerateObject().Count();
            }
        }

        public override string GetName(int ordinal)
        {
            return fields[ordinal];
        }

        public override Type GetFieldType(int ordinal)
        {
            // we maken gewoon overal een string van
            return typeof(string);
        }


        public override bool Read()
        {
            return rowenumerator.MoveNext();
        }

        public override int GetValues(object[] values)
        {
            JsonElement current = rowenumerator.Current;
            int i = 0;
            foreach (JsonProperty field in current.EnumerateObject())
            {
                JsonElement value = field.Value;
                if(value.ValueKind == JsonValueKind.Null)
                {
                    values[i++] = null;
                }
                else if (value.ValueKind == JsonValueKind.String)
                {
                    values[i++] = value.GetString();
                }
                else
                {
                    values[i++] = value.GetRawText();
                }
            }
            return i;
        }

        public override bool NextResult()
        {
            return rowenumerator.MoveNext();
        }

        public override void Close()
        {
            closed = true;
        }

        public override object this[int ordinal] => throw new NotImplementedException();

        public override object this[string name] => throw new NotImplementedException();

        public override int Depth => throw new NotImplementedException();


        public override bool HasRows => throw new NotImplementedException();

        public override int RecordsAffected => throw new NotImplementedException();


        public override bool GetBoolean(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override double GetDouble(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override float GetFloat(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetInt32(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetInt64(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public override string GetString(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override bool IsDBNull(int ordinal)
        {
            throw new NotImplementedException();
        }
    }
}
