using System;
using System.Collections.Generic;
using Google.Apis.Drive.v2.Data;

namespace Utils.GDrive
{
    public interface IGDrivePathHelper
    {
        /* Creates a directory tree given absolute path. Doesn't support . and ..
        If any of the subpath can't be touched after creation, throws 
        System.IO.IOException() */
        File CreateDirectoryTree(string path);

        /* Returns Google file corresponding to the absolute path.
        If the file can't be found throws System.IO.FileNotFoundException(). */
        File PathToGoogleFile(string path);
    }

    public class GDrivePathHelper : IGDrivePathHelper
    {
        private IGDrive google;
        private Dictionary<String, File> dict;
        private char separator = System.IO.Path.DirectorySeparatorChar;
        private string[] separators = 
            {
                System.IO.Path.DirectorySeparatorChar.ToString()
            };
        private File root;

        public GDrivePathHelper(IGDrive google)
        {
            this.google = google;
            root = google.GetFileInfo(google.GetRootFolderId());
            dict = new Dictionary<string, File>();
            dict[""] = root;
            dict[separators[0]] = root;
        }

        /* Recursively creates a folder with the given absolute path
        and returns the corresponding Google File. */
        public File CreateDirectoryTree(string path)
        {
            return LookupPath(path, true);
        }

        /* Converts absolute path to Google File, if one exists.
        Otherwise throws FileNotFound exception. */
        public File PathToGoogleFile(string path)
        {
            return LookupPath(path, false);
        }

        /* Returns a child reference from the list that has the file name,
        or null if none exists. */
        private File GetFileReference(IList<ChildReference> children, 
            string name, string parent)
        {
            foreach (var child in children)
            {
                var file = google.GetFileInfo(child.Id);
                dict[System.IO.Path.Combine(parent, file.Title)] = file;
                if (file.Title == name)
                {
                    return file;
                }
            }
            return null;
        }

        /* Returns Google file ID for the directory contained in parent. */
        private File LookupPathHelper(IList<ChildReference> files, string dir, 
            File parent, string parentPath, bool createIfDoesNotExists)
        {
            File file = null;
            string currentPath = System.IO.Path.Combine(parentPath, dir);
            if (dict.ContainsKey(currentPath))
            {
                return dict[currentPath];
            }
            else
            {
                file = GetFileReference(files, dir, parentPath);
                if (file == null)
                {
                    if (createIfDoesNotExists)
                    {
                        google.NewFolder(dir, parent);
                        file = GetFileReference(google.ChildList(parent),
                            dir, parentPath);
                        if (file == null)
                        {
                            string msg = System.IO.Path.Combine(parentPath, dir);
                            throw new System.IO.IOException(msg);
                        }
                    }
                    else
                    {
                        throw new System.IO
                            .FileNotFoundException(currentPath);
                    }
                }
                dict[currentPath] = file;
                return file;
            }
        }

        /* Returns Google File given absolute path. */
        private File LookupPath(string path, bool createIfDoesNotExists)
        {
            path = path.Trim(separator);
            if (dict.ContainsKey(path))
            {
                return dict[path];
            }

            var dirs = path.Split(separators,
                StringSplitOptions.RemoveEmptyEntries);

            var parent = root;
            var files = google.ChildList(parent);
            string parentPath = "";
            
            foreach (var dir in dirs)
            {
                parent = LookupPathHelper(files, dir, parent, parentPath,
                    createIfDoesNotExists);
                parentPath = System.IO.Path.Combine(parentPath, dir);
                files = google.ChildList(parent);
            }
            return parent;
        }
    }
}
