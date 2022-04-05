using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace ISpiegel.Provider.FileSystem
{
    public class DataReader : System.Data.Common.DbDataReader
    {

        /*        
        id
        directory
        name
        extension
        size
        creationtime
        wrritetime
        accesstime
        owner
        */


        /*        
                    ID  // fullname in lowercase            
                    this.Directory = directory;
                    // alle filenames in lowercase!
                    this.Name = file.Name.ToLower();
                    this.Extension = file.Extension.ToLower();
                    this.Size = file.Length;

                    this.DateCreate = file.CreationTime;
                    this.DateWrite = file.LastWriteTime;
                    this.DateAccess = file.LastAccessTime;

                    System.Security.Principal.SecurityIdentifier sid = null;
                    try
                    {
                        System.Security.AccessControl.FileSecurity fileSecurity = file.GetAccessControl();
                sid = fileSecurity.GetOwner(typeof(System.Security.Principal.SecurityIdentifier)) as System.Security.Principal.SecurityIdentifier;
                        System.Security.Principal.NTAccount ntAccount = sid.Translate(typeof(System.Security.Principal.NTAccount)) as System.Security.Principal.NTAccount;
                        this.Owner = ntAccount.Value;

        */
        private Command creator = null;
        private List<Alphaleonis.Win32.Filesystem.FileInfo> foundfiles = null;
        private List<Alphaleonis.Win32.Filesystem.DirectoryInfo> founddirectories = null;

        public DataReader(Command creator)
        {
            this.creator = creator;



            foundfiles = new List<Alphaleonis.Win32.Filesystem.FileInfo>();
            founddirectories = new List<Alphaleonis.Win32.Filesystem.DirectoryInfo>();

            // onze dir root
            founddirectories.Add(new Alphaleonis.Win32.Filesystem.DirectoryInfo(creator.CommandText));
        }

        bool closed = false;
        public override bool IsClosed
        {
            get
            {
                return closed;
            }
        }

        public override int FieldCount
        {
            get
            {
                return 9;
            }
        }

        public override string GetName(int ordinal)
        {
            switch (ordinal)
            {
                case 0:
                    return "id";
                case 1:
                    return "directory";
                case 2:
                    return "name";
                case 3:
                    return "extension";
                case 4:
                    return "size";
                case 5:
                    return "creationtime";
                case 6:
                    return "writetime";
                case 7:
                    return "accesstime";
                case 8:
                    return "owner";
                default:
                    throw new IndexOutOfRangeException("field not found:" + ordinal);

            }
        }

        public override Type GetFieldType(int ordinal)
        {
            switch (ordinal)
            {
                case 0:
                    //return "id";
                    return typeof(string);
                case 1:
                    //return "directory";
                    return typeof(string);
                case 2:
                    //return "name";
                    return typeof(string);
                case 3:
                    //return "extension";
                    return typeof(string);
                case 4:
                    //return "size";
                    return typeof(long);
                case 5:
                    //return "creationtime";
                    return typeof(DateTime);
                case 6:
                    //return "wrritetime";
                    return typeof(DateTime);
                case 7:
                    // return "accesstime";
                    return typeof(DateTime);
                case 8:
                    // return "owner";
                    return typeof(string);
                default:
                    throw new IndexOutOfRangeException("field not found:" + ordinal);

            }
        }


        public override bool Read()
        {
            // no more files, fetch some more
            while (foundfiles.Count == 0 && founddirectories.Count != 0)
            {
                var currentdir = founddirectories[0];
                founddirectories.RemoveAt(0);
                try
                {
                    foundfiles.AddRange(currentdir.GetFiles());
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("exception for getFiles() for:" + currentdir.FullName + " exception:" + ex); }
                try
                {
                    founddirectories.AddRange(currentdir.GetDirectories());
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("exception for getDirectories() for:" + currentdir.FullName + " exception:" + ex); }
            }
            return foundfiles.Count > 0;
        }

        public override int GetValues(object[] values)
        {
            var currentfile = foundfiles[0];
            // gaat dit goed met de volgende regel? 
            foundfiles.RemoveAt(0);

            System.Diagnostics.Debug.WriteLine("working on:" + currentfile.FullName);
            //0 - id
            values[0] = currentfile.FullName.ToLower();
            //1 - directory
            values[1] = currentfile.Directory.FullName.ToLower();
            //2 - name
            values[2] = currentfile.Name.ToLower();
            //3 - extension
            values[3] = currentfile.Extension.ToLower();
            //4 - size
            values[4] = currentfile.Length;
            //5 - creationtime
            values[5] = currentfile.CreationTime;
            //6 - wrritetime
            values[6] = currentfile.LastWriteTime;
            //7 - accesstime
            values[7] = currentfile.LastAccessTime;
            //8 - owner
            string user = null;
            try
            {
                user = System.IO.File.GetAccessControl(currentfile.FullName).GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();
            }
            catch (Exception ex) { }
            values[8] = user;

            return 9;
        }

        public override bool NextResult()
        {
            throw new NotImplementedException();
            /*
            var currentfile = foundfiles[0];
            foundfiles.RemoveAt(0);
            return foundfiles.Count > 0;
            */
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
