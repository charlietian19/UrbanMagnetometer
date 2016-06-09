using System;
using System.Text;
using System.Threading;
using Utils.Fixtures;

namespace Utils.GPS.SerialGPS
{
    public class SerialGps : ITimeSource
    {
        /* Called when a rising PPS edge is received. */
        public event Action<long> PpsReceived;

        /* Called when new timing data is received. */
        public event Action<GpsData> TimestampReceived;

        /* Stores last recorded state of the PPS pin*/
        bool CD = false;

        /* Guards the ticks field. */
        Mutex mutex = new Mutex();

        /* When the last PPS pulse was received. */
        long ticks = 0;

        /* Receive the character stream, split it in lines 
        and give each string to the NMEA processing function */
        private StringBuilder buffer = new StringBuilder();

        /* Serial port with the GPS receiver */
        private ISerial serial;

        /* System performance clock */
        private IStopwatch stopwatch;

        /* The interval between the PPS pulse and the NMEA GPSRMC message
        has to be smaller than this for the GPS data to be valid. In other
        words, the PPS pulse and the time message have to arrive within the
        same second, or the PPS is likely missing. */
        private long threshold;

        /* Time source with a GPS receiver connected to a serial port */
        public SerialGps(string portName)
        {
            serial = new SerialPortWrapper(portName);
            stopwatch = new StopwatchWrapper();
            ConfigureSerialGps();
        }

        /* Time source with a GPS receiver connected to a serial port 
        and custom baud rate */
        public SerialGps(string portName, int baudRate) 
        {
            serial = new SerialPortWrapper(portName, baudRate);
            stopwatch = new StopwatchWrapper();
            ConfigureSerialGps();
        }

        /* Time source using a custom serial port object and a stopwatch */
        public SerialGps(ISerial serial, IStopwatch stopwatch)
        {
            this.serial = serial;
            this.stopwatch = stopwatch;
            ConfigureSerialGps();
        }

        /* Configures the serial port, subscribes to events */
        private void ConfigureSerialGps()
        {
            threshold = stopwatch.Frequency;
            serial.PpsChanged += Serial_PpsChanged;
            serial.DataReceived += Serial_DataReceived;
            serial.PortOpened += Serial_PortOpened;
        }

        /* Opens the serial port */
        public void Open()
        {
            serial.Open();
        }

        /* Closes the serial port */
        public void Close()
        {
            serial.Close();
        }

        /* Called when the port has finished opening */
        private void Serial_PortOpened()
        {
            CD = serial.GetPpsLevel();
        }

        /* Called when a byte is received through the serial port*/
        private void Serial_DataReceived(byte data)
        {
            char letter = Convert.ToChar(data);
            if (letter == '\r')
            {
                return;
            }

            if (letter == '\n')
            {
                ParseNMEA(buffer.ToString());
                buffer.Clear();
                return;
            }

            buffer.Append(letter);
        }

        /* Called when a non-data pin has changed level */
        private void Serial_PpsChanged(bool state)
        {
            CD = state;
            if (CD)
            {
                OnPpsPulse();
            }
        }

        /* Executes when a PPS pulse is received */
        private void OnPpsPulse()
        {
            var ticks = stopwatch.GetTimestamp();
            mutex.WaitOne();
            this.ticks = ticks;
            mutex.ReleaseMutex();

            if (PpsReceived != null)
            {
                PpsReceived(ticks);
            }
        }

        /* Tries to parse GPRMC message, calls OnNMEA when it is received.
        It might parse other message types in the future. */
        private void ParseNMEA(string msg)
        {
            GpsData data;
            if (Helpers.IsGpsrmc(msg))
            {
                data = Helpers.ParseGpsrmc(msg);
            }
            else
            {
                return;
            }

            mutex.WaitOne();
            data = FixGpsTime(data, ticks);
            mutex.ReleaseMutex();

            if (TimestampReceived != null)
            {
                TimestampReceived(data);
            }
        }

        /* Checks this NMEA message corresponds to the last stored PPS pulse.
        If it does, update the ticks field to the PPS pulse arrival. If not,
        mark the GPS data as invalid. */
        private GpsData FixGpsTime(GpsData data, long ticks)
        {
            if (data.ticks - ticks > threshold)
            {
                data.valid = false;
            }
            else
            {
                data.ticks = ticks;
            }

            return data;
        }
    }
}
