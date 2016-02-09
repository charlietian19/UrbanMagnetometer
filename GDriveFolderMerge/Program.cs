using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.Configuration;
using Utils.GDrive;
using Google.Apis.Drive.v2.Data;

namespace GDriveFolderMerge
{
    class Program
    {        
        static void Main(string[] args)
        {
            var google = new GDrive("nuri-station.json");
            try
            {
                var root = google.PathToGoogleFile(Settings.RemoteDataFolder);
                Merge(google, root);
            }     
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }       
        }

        static void Merge(GDrive google, File folder)
        {            
            if (folder.MimeType != Settings.FoldersMimeType)
            {
                var msg = "Can't merge something that isn't a folder";
                throw new Exception(msg);
            }

            var uniqueFolders = new Dictionary<string, File>();
            var children = google.ChildList(folder);
            foreach (var child in children)
            {
                var file = google.GetFileInfo(child.Id);
                if (file.MimeType == Settings.FoldersMimeType)
                {
                    if (uniqueFolders.ContainsKey(file.Title))
                    {
                        MergeTrees(google, uniqueFolders[file.Title], file);
                    }
                    else
                    {
                        uniqueFolders.Add(file.Title, file);
                    }
                }
            }

            children = google.ChildList(folder);
            foreach (var kvp in uniqueFolders)
            {
                Merge(google, kvp.Value);
            }
        }

        /* Sets folder1 to be parent of every child in folder2.
        Deletes folder2. */
        static void MergeTrees(GDrive google, File folder1, File folder2)
        {
            if ((folder1.MimeType != Settings.FoldersMimeType) 
                || (folder2.MimeType != Settings.FoldersMimeType))
            {
                var msg = "One of the tree branches isn't a folder";
                throw new Exception(msg);
            }

            var children = google.ChildList(folder2);
            foreach (var child in children)
            {
                google.SetParent(child.Id, folder1.Id);
            }
            google.DeleteFile(folder2.Id);
        }
    }
}
