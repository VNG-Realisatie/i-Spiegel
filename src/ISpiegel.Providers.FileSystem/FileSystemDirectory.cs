using System;
namespace ISpiegel.Provider.FileSystem
{
    internal class FileSystemDirectory
    {
        private string p;

        public string Name { get; set; }
        public int FileCount { get; set; }
        public System.DateTime DateCreate { get; set; }
        public System.DateTime DateWrite { get; set; }
        public System.DateTime DateAccess { get; set; }
        public bool HasError = false;

        public FileSystemDirectory(System.IO.DirectoryInfo directory)
        {
            // alle filenames in lowercase!
            this.Name = directory.FullName.ToLower();
            try { 
                this.FileCount = directory.GetFiles().Length;
            }
            catch (System.IO.DirectoryNotFoundException dnfe)
            {
                // happens sometimes
                HasError = true;
                this.FileCount = 0;
            }
            catch (System.UnauthorizedAccessException uae)
            {
                // System.Exception {System.UnauthorizedAccessException}
                HasError = true;
                this.FileCount = 0;
                return;
            }
            //catch (Exception ex)
            //{
            //    HasError = true;
            //    this.FileCount = 0;
            //}
            this.DateCreate = directory.CreationTime;
            this.DateWrite = directory.LastWriteTime;
            this.DateAccess = directory.LastAccessTime;
        }

        public FileSystemDirectory(System.Data.OleDb.OleDbDataReader reader)
        {
            this.Name = Convert.ToString(reader["Name"]);
            this.FileCount = Convert.ToInt32(reader["FileCount"]);
            this.DateCreate = Convert.ToDateTime(reader["DateCreate"]);
            this.DateWrite = Convert.ToDateTime( reader["DateWrite"]);
            this.DateAccess = Convert.ToDateTime(reader["DateAccess"]);
        }

        internal FileSystemDirectory(string name)
        {
            // highly unwanted, but we'll make it working this wya...
            this.Name = name;
        }
    }
}
