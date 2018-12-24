using System;
namespace ISpiegel.Provider.FileSystem
{
    internal class FileSystemFile
    {
        private System.Data.OleDb.OleDbDataReader reader;

        public string Name { get; set; }
        public FileSystemDirectory Directory { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public String Owner { get; set; }
        public System.DateTime DateCreate { get; set; }
        public System.DateTime DateWrite { get; set; }
        public System.DateTime DateAccess { get; set; }

        public FileSystemFile(FileSystemDirectory directory, System.IO.FileInfo file)
        {
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

            }
            catch (System.Security.Principal.IdentityNotMappedException ex)
            {

                if(sid != null) this.Owner = sid.ToString();
                else this.Owner = "unknown";
            }
            catch(Exception ex)
            {
                this.Owner = "error:" + ex.ToString();
            }
        }

        public FileSystemFile(System.Data.OleDb.OleDbDataReader reader)
        {
            this.Name = Convert.ToString(reader["Name"]);
            this.Directory = new FileSystemDirectory(Convert.ToString(reader["DirectoryName"]));
            this.Extension = Convert.ToString(reader["Extension"]);
            this.Size = Convert.ToInt32(reader["Size"]);
            this.DateCreate = Convert.ToDateTime(reader["DateCreate"]);
            this.DateWrite = Convert.ToDateTime(reader["DateWrite"]);
            this.DateAccess = Convert.ToDateTime(reader["DateAccess"]);
            this.DateAccess = Convert.ToDateTime(reader["Owner"]);
        }
    }
}
