using System;
using System.Collections.Generic;
using Utils.Configuration;
using Utils.GDrive;
using Google.Apis.Drive.v2.Data;

namespace GDriveFolderMerge
{
    public class Program
    {   
        public static string foldersMimeType;
        public static void Main(string[] args)
        {
            var google = new GDrive("nuri-station.json");
            try
            {
                foldersMimeType = Settings.FoldersMimeType;
                CleanUpGDrive(google, Settings.RemoteDataFolder);
            }     
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }       
        }

        public static void CleanUpGDrive(IGDrive google, string rootPath)
        {
            var root = google.PathToGoogleFile(rootPath);
            Merge(google, root);
        }

        /* 
            Merges all the folders with repeating names in the given folder,
            calls itself recursively on every remaining child folder
        */
        public static void Merge(IGDrive google, File folder)
        {            
            if (folder.MimeType != foldersMimeType)
            {
                var msg = "Can't merge something that isn't a folder";
                throw new Exception(msg);
            }

            Console.WriteLine("Merging files inside " + folder.Title 
                + " (Id = " + folder.Id + ")");

            /* Set of all folders with non-repeating names in this folder */
            var uniqueFolders = new Dictionary<string, File>();
            var children = google.ChildList(folder);
            foreach (var child in children)
            {
                var file = google.GetFileInfo(child.Id);
                if (file.MimeType == foldersMimeType)
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
        public static void MergeTrees(IGDrive google, File folder1, File folder2)
        {
            if ((folder1.MimeType != foldersMimeType) 
                || (folder2.MimeType != foldersMimeType))
            {
                var msg = "One of the tree branches isn't a folder";
                throw new Exception(msg);
            }

            Console.WriteLine("Merging contents of two" + folder1.Title
                + "folders" + " (" + folder2.Id + "->" + folder1.Id + ")");

            var children = google.ChildList(folder2);
            foreach (var child in children)
            {
                google.SetParent(child.Id, folder1.Id);
            }

            Console.WriteLine("Deleting " + folder2.Id);
            google.DeleteFile(folder2.Id);
        }
    }
}
