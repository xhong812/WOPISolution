using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Net;

namespace WOPIHost
{
    public class FileStorage : IFileStorage
    {
        public string localStoragePath = ConfigurationManager.AppSettings["FileLocalPath"];
        // TODO: add authentication flow from caller.
        //private string username = ConfigurationManager.AppSettings["FileServiceUserName"];
        //private string password = ConfigurationManager.AppSettings["FileServiceUserPassword"];

        /// <summary>
        /// Get file size.
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>File size</returns>
        public long GetFileSize(string name)
        {
            try
            {
                string fullPath = Path.Combine(localStoragePath, name);
                FileInfo fileInfo = new FileInfo(fullPath);
                long size = (int)fileInfo.Length;

                return size;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Return directory.
        /// </summary>
        /// <returns>DirectoryInfo</returns>
        public DirectoryInfo GetDirecotry()
        {
            return new DirectoryInfo(localStoragePath);
        }

        /// <summary>
        /// Get file last modified time.
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>File last modified time in DateTime UTC</returns>
        public DateTime? GetLastModifiedTime(string name)
        {
            try
            {
                string fullPath = Path.Combine(localStoragePath, name);
                FileInfo fileInfo = new FileInfo(fullPath);
                return fileInfo.LastWriteTimeUtc;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get file stream.
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>File stream</returns>
        public Stream GetFile(string name)
        {
            try
            {
                string fullPath = Path.Combine(localStoragePath, name);
                Stream stream = File.OpenRead(fullPath);
                return stream;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Upload file.
        /// </summary>
        /// <param name="name">File name</param>
        /// <param name="stream">File stream</param>
        /// <returns>Return 0 if success; Otherwise, return -1</returns>
        public int UploadFile(string name, Stream stream)
        {
            string fullPath = Path.Combine(localStoragePath, name);
            FileInfo putTargetFileInfo = new FileInfo(fullPath);

            try
            {

                // Either the file has a valid lock that matches the lock in the request, or the file is unlocked
                // and is zero bytes.  Either way, proceed with the PutFile.
                // TODO: Should be replaced with proper file save logic to a real storage system and ensures write atomicity
                using (var fileStream = File.Open(fullPath, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite))
                {
                    stream.CopyTo(fileStream);
                }

                stream.Close();
                stream.Dispose();

                return 0;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// Get file name list.
        /// </summary>
        /// <returns>File name list</returns>
        public List<string> GetFileNames()
        {
            string[] files = System.IO.Directory.GetFiles(localStoragePath);
            //List<string> result = files.ToList<string>();
            List<string> basenames = new List<string>();
            foreach (string path in files)
            {
                basenames.Add(Path.GetFileName(path));
            }

            return basenames;
        }

        /// <summary>
        /// Get the file version.
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>Return a string representing the file version.</returns>
        public string GetFileVersion(string name)
        {
            string fullPath = Path.Combine(localStoragePath, name);
            FileInfo fileInfo = new FileInfo(fullPath);
            return fileInfo.LastWriteTimeUtc.ToString("O" /* ISO 8601 DateTime format string */); // Using the file write time is an arbitrary choice.
        }

        /// <summary>
        /// Get the readonly status of the file.
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>Return true if readonly; Otherwise, return false</returns>
        public bool GetReadOnlyStatus(string name)
        {
            string fullPath = Path.Combine(localStoragePath, name);
            FileInfo fileInfo = new FileInfo(fullPath);
            return fileInfo.IsReadOnly;
        }

        /// <summary>
        /// Delete the file.
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>Return true if succeed.</returns>
        public void DeleteFile(string name)
        {
            string fullPath = Path.Combine(localStoragePath, name);
            FileInfo file = new FileInfo(fullPath);
            file.Delete();
        }

        /// <summary>
        /// Create a new file or overwrite an existing file.
        /// </summary>
        /// <param name="name">File name</param>
        /// <param name="stream">File stream</param>
        public void CreateOrOverwriteFile(string name, Stream stream)
        {
            string fullPath = Path.Combine(localStoragePath, name);
            FileInfo file = new FileInfo(fullPath);
            if (file.Exists)
            {
                using (var fileStream = File.Open(fullPath, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite))
                {
                    stream.CopyTo(fileStream);
                }

                stream.Close();
                stream.Dispose();
            }
            else
            {
                using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }

                stream.Close();
                stream.Dispose();
            }
        }

        /// <summary>
        /// Rename a file
        /// </summary>
        /// <param name="name">Old name</param>
        /// <param name="newName">New name</param>
        public bool RenameFile(string name, ref string newName)
        {
            string fullPath = Path.Combine(localStoragePath, name);
            FileInfo file = new FileInfo(fullPath);
            if (!file.Exists)
            {
                throw new Exception(string.Format("The file with name '{0}' does not exist.", name));
            }

            string newFullPath = Path.Combine(localStoragePath, newName + file.Extension);
            FileInfo fileNew = new FileInfo(newFullPath);
            if (fileNew.Exists)                
            {
                newName = newName + System.Guid.NewGuid();
                newFullPath = Path.Combine(localStoragePath, newName + file.Extension);
                //return false;
            }

            file.MoveTo(newFullPath);
            return true;
        }
    }
}
