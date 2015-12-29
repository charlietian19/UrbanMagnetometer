using System;
using System.IO;
using System.Collections.Generic;
using Google.Apis.Drive.v2.Data;

namespace GDriveNURI
{
    public class GDrivePathHelper
    {
        private GDrive google;
        private Dictionary<String, String> dict;
        private char separator = Path.PathSeparator;
        private string[] separators = { Path.PathSeparator.ToString() };
        private string root;

        public GDrivePathHelper(GDrive google)
        {
            this.google = google;
            root = google.GetRootFolderId();
            dict = new Dictionary<string, string>();
        }

        /* Recursively creates a folder with the given absolute path. */
        public void CreateFolder(string path)
        {
            LookupPath(path, true);
        }

        /* Returns a child reference from the list that has the file name,
        or null if none exists. */
        private Google.Apis.Drive.v2.Data.File getFileReference(
            IList<ChildReference> children, string name, string parent)
        {
            foreach (var child in children)
            {
                var file = google.GetFileInfo(child.Id);
                dict.Add(Path.Combine(parent, file.Title), file.Id);
                if (file.Title == name)
                {
                    return file;
                }
            }
            return null;
        }

        /* Returns Google file ID given absolute path. */
        private string LookupPath(string path, bool createIfDoesNotExists)
        {
            // TODO: refactor this
            path = path.Trim(separator);
            var dirs = path.Split(separators,
                StringSplitOptions.RemoveEmptyEntries);

            var parent = root;
            var files = google.ChildList(parent);
            string parentPath = "";
            Google.Apis.Drive.v2.Data.File file = null;
            foreach (var dir in dirs)
            {
                string currentPath = Path.Combine(parentPath, dir);
                
                if (dict.ContainsKey(currentPath))
                {
                    parent = dict[currentPath];
                }
                else
                {
                    file = getFileReference(files, dir, parentPath);
                    if (file == null)
                    {
                        if (createIfDoesNotExists)
                        {
                            google.NewFolder(currentPath, parent);
                        }
                        else
                        {
                            throw new FileNotFoundException(currentPath);
                        }
                    }
                    parent = file.Id;
                }
                parentPath = Path.Combine(parentPath, dir);
                files = google.ChildList(parent);
            }
            return file.Id;
        }

        /* Converts absolute path to Google file ID with caching. */
        public string FileToId(string path)
        {
            path = path.Trim(separator);
            if (dict.ContainsKey(path))
            {
                return dict[path];
            }
            return LookupPath(path, false);
        }

        /* Converts absolute path to Google folder ID. */
        public string FolderToId(string path)
        {
            return FileToId(path);
        }

        /* Returns true if the file exists and false otherwise. */
        public bool FileExists(string path)
        {
            return false;
        }

        /* Returns true if the folder exists and false otherwise. */
        public bool FolderExists(string path)
        {
            return false;
        }

    }
}
