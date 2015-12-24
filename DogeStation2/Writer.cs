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
        private Uploader uploader;
        private string dataCacheFolder, stationName, dataUnits, samplingRate,
            channelNameX, channelNameY, channelNameZ, channelNameTime,
            dataFileNameFormat, timeFileNameFormat;

        private BinaryWriter x, y, z, t;
        private string xPath, yPath, zPath, tPath;
        private DateTime startDate;
        private bool isWriting = false;
        private long offset;

        /* Constructs the data writer given a Google Drive connection. */
        public Writer(Uploader uploader)
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
            stationName = settings["StationName"];
            samplingRate = settings["SamplingRate"];
            dataUnits = settings["DataUnits"];
            channelNameX = settings["ChannelNameX"];
            channelNameY = settings["ChannelNameY"];
            channelNameZ = settings["ChannelNameZ"];
            channelNameTime = settings["ChannelNameTime"];
            timeFileNameFormat = settings["TimeFileNameFormat"];
            dataFileNameFormat = settings["DataFileNameFormat"];
        }

        /* Returns the file name given the channel and the date. */
        private string GetFileName(string channel, DateTime time)
        {
            if (channel != channelNameTime)
            {
                return String.Format(dataFileNameFormat,
                    time.Year, time.Month, time.Day, time.Hour, channel, 
                    dataUnits, samplingRate);
            }
            else
            {
                return String.Format(dataFileNameFormat,
                    time.Year, time.Month, time.Day, time.Hour, channel);
            }
        }

        /* Stores the data from the sensor. */
        public void Write(double[] dataX, double[] dataY, double[] dataZ, 
            double systemSeconds, DateTime time)
        {
            if (!isWriting)
            {
                CreateFiles(time);
            }

            if (startDate.Hour != time.Hour)
            {
                CloseAndUploadAll();
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
            startDate = time;
            xPath = Path.Combine(dataCacheFolder, GetFileName(channelNameX, time));
            yPath = Path.Combine(dataCacheFolder, GetFileName(channelNameY, time));
            zPath = Path.Combine(dataCacheFolder, GetFileName(channelNameZ, time));
            tPath = Path.Combine(dataCacheFolder, GetFileName(channelNameTime, time));
            x = new BinaryWriter(File.Open(xPath, FileMode.Append, FileAccess.Write));
            y = new BinaryWriter(File.Open(yPath, FileMode.Append, FileAccess.Write));
            z = new BinaryWriter(File.Open(zPath, FileMode.Append, FileAccess.Write));
            t = new BinaryWriter(File.Open(tPath, FileMode.Append, FileAccess.Write));
            offset = 0;
            isWriting = true;
        }

        /* Closes the data files and sends them to the Google Drive. */
        private void CloseAndUploadAll()
        {
            CloseAndUpload(x, xPath);
            CloseAndUpload(y, yPath);
            CloseAndUpload(z, zPath);
            CloseAndUpload(t, tPath);
        }

        /* Closes and uploads a single file. */
        private void CloseAndUpload(BinaryWriter writer, string path)
        {
            writer.Close();
            uploader.UploadData(path, startDate);
        }
    }

}
