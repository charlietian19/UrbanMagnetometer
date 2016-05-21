using System;
using System.Text;
using PInvokeSerialPort;
using System.Diagnostics;
using System.Threading;

namespace Utils.GPS
{
    // Serial port that has a GPS connected to it. Raises events when
    // a PPS pulses are received, and NMEA messages are parsed

    public class SerialGps : SerialPort
    {
        public delegate void PpsEventDelegate(long ticks);
        public delegate void NmeaEventDelegate(GpsData data);

        /* Called when a PPS pulse arrives. */
        public event PpsEventDelegate OnPps;

        /* Called when a GPSRMC message arrives. */
        public event NmeaEventDelegate OnTimeUpdate;

        /* Stores last recorded state of the PPS pin*/
        bool CD = false;

        /* Guards the ticks field. */
        Mutex mutex = new Mutex();

        /* When the last PPS pulse was received. */
        long ticks = 0;

        /* The interval between the PPS pulse and the NMEA GPSRMC message
        has to be smaller than this for the GPS data to be valid. In other
        words, the PPS pulse and the time message have to arrive within the
        same second, or the PPS is likely missing. */
        readonly long threshold = Stopwatch.Frequency;

        public SerialGps(string portName) : base(portName)
        {
            InitializeDefaults();
        }

        public SerialGps(string portName, int baudRate) 
            : base(portName, baudRate)
        {
            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            AutoReopen = true;
            DataReceived += SerialGPS_DataReceived;
        }

        /* Receive the character stream, split it in lines 
        and give each string to the NMEA processing function */
        private StringBuilder buffer = new StringBuilder();

        /* Called when a new byte is received from the serial port. */
        private void SerialGPS_DataReceived(byte obj)
        {
            if (obj == '\r')
            {
                return;
            }

            if (obj == '\n')
            {
                ParseNMEA(buffer.ToString());
                buffer.Clear();
                return;
            }
            
            buffer.Append(Convert.ToChar(obj));
        }


        /* When the modem status changes, check if CD changed and raise
        PPSEvent if a rising edge is detected */
        protected override void OnStatusChange(ModemStatus mask, ModemStatus state)
        {
            if (mask.Rlsd)
            {
                CD = state.Rlsd;
                if (CD)
                {
                    OnPpsPulse();
                }
            }
            base.OnStatusChange(mask, state);
        }

        /* Check the initial CD status (the pin where PPS pulses are) */
        protected override bool AfterOpen()
        {
            CD = GetModemStatus().Rlsd;
            return base.AfterOpen();
        }

        /* Executes when a PPS pulse is received */
        private void OnPpsPulse()
        {
            var ticks = Stopwatch.GetTimestamp();
            mutex.WaitOne();
            this.ticks = ticks;
            mutex.ReleaseMutex();

            if (OnPps != null)
            {
                OnPps(ticks);
            }
        }
        
        /* Tries to parse GPRMC message, calls OnNMEA when it is received.
        It might parse other message types in the future. */
        private void ParseNMEA(string msg)
        {
            GpsData data;
            try
            {
                data = Helpers.ParseGpsrmc(msg);                
            }
            catch (ArgumentException)
            {
                return;
            }

            data = FixGpsTime(data, ticks);            
            if (OnTimeUpdate != null)
            {
                OnTimeUpdate(data);
            }
        }

        /* Checks this NMEA message corresponds to the last stored PPS pulse.
        If it does, update the ticks field to the PPS pulse arrival. If not,
        mark the GPS data as invalid. */
        private GpsData FixGpsTime(GpsData data, long ticks)
        {
            mutex.WaitOne();
            if (data.ticks - ticks > threshold)
            {
                data.valid = false;
            }
            else
            {
                data.ticks = ticks;
            }
            mutex.ReleaseMutex();

            return data;
        }
    }
}
