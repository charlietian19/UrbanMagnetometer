using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: proper exception handling

namespace GDriveNURI
{
    class Writer
    {
        private UploadScheduler uploader;
        private string dataCacheFolder;
        private DatasetInfo info = null;
        private DatasetInfo postponedInfo = null;

        private BinaryWriter x, y, z, t;
        private bool isWriting = false;
        private long offset;

        /* Constructs the data writer given a Google Drive connection. */
        public Writer(UploadScheduler uploader)
        {
            this.uploader = uploader;
            ReadAppConfig();
            // TODO: scan the data cache folder for files that haven't been uploaded
            // and upload them as well
        }

        /* Initializes settings from the configuration file. */
        private void ReadAppConfig()
        {
            var settings = System.Configuration.ConfigurationManager.AppSettings;
            dataCacheFolder = settings["DataCacheFolder"];
        }
        
        /* Stores the data from the sensor. */
        public void Write(double[] dataX, double[] dataY, double[] dataZ, 
            double systemSeconds, DateTime time)
        {
            if (!isWriting)
            {
                CreateFiles(time);
            }

            if (info.StartDate.Hour != time.Hour)
            {
                CloseAndUploadAll(time);
                CreateFiles(time);
            }

            Append(dataX, dataY, dataZ, systemSeconds, time);
        }

        /* Appends the data into existing streams. */
        private void Append(double[] dataX, double[] dataY, double[] dataZ,
            double systemSeconds, DateTime time)
        {
            int length = dataX.Length;
            WriteArray(x, dataX);
            WriteArray(y, dataX);
            WriteArray(z, dataX);
            WriteTime(t, systemSeconds, time, offset, length);
            offset += length;
        }

        /* Writes an array of doubles into a binary file. */
        private static void WriteArray(BinaryWriter writer, double[] data)
        {
            foreach (double value in data)
            {
                writer.Write(value);
            }
        }

        /* Writes time data into a binary file. */
        private static void WriteTime(BinaryWriter writer, 
            double systemSeconds, DateTime time, long offset, int length)
        {
            writer.Write(offset);
            writer.Write(length);
            writer.Write(time.ToFileTimeUtc());
            writer.Write(systemSeconds);
        }

        /* Creates the data files in cache folder. */
        private void CreateFiles(DateTime time)
        {
            info = new DatasetInfo(time);
            x = new BinaryWriter(File.Open(info.FullPath(info.XFileName), 
                FileMode.Append, FileAccess.Write));
            y = new BinaryWriter(File.Open(info.FullPath(info.YFileName), 
                FileMode.Append, FileAccess.Write));
            z = new BinaryWriter(File.Open(info.FullPath(info.ZFileName), 
                FileMode.Append, FileAccess.Write));
            t = new BinaryWriter(File.Open(info.FullPath(info.TFileName), 
                FileMode.Append, FileAccess.Write));
            offset = 0;
            isWriting = true;
        }

        /* Closes the data files and sends them to the Google Drive.
        time indicates when the latest chunk of data was received. */
        private void CloseAndUploadAll(DateTime time)
        {
            x.Close();
            y.Close();
            z.Close();
            t.Close();

            /* Upload the latest postponed dataset, if any. */
            if ((postponedInfo != null) && !postponedInfo.SameFile(info))
            {
                uploader.UploadMagneticData(postponedInfo);
                postponedInfo = null;
            }

            if (!info.SameFile(time))
            {
                uploader.UploadMagneticData(info);
            }
            else
            {
                postponedInfo = info;
            }
        }
    }

}
