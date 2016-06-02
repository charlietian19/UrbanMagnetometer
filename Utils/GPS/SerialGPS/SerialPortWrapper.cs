using System;
using PInvokeSerialPort;

/* Wraps PInvokeSerialPort mostly to facilitate testing */

namespace Utils.GPS.SerialGPS
{
    class SerialPortWrapper : SerialPort, ISerial
    {
        public event Action<bool> PpsChanged;
        public event Action PortOpened;

        new public void Open()
        {
            base.Open();
        }

        public SerialPortWrapper(string portName)
            : base(portName) { }

        public SerialPortWrapper(string portName, int baudRate) 
            : base(portName, baudRate) { }


        /* Invokes PpsChanged action when the PPS pin has changed level */
        protected override void OnStatusChange(ModemStatus mask, 
            ModemStatus state)
        {                        
            if (mask.Rlsd && (PpsChanged != null))
            {
                PpsChanged(state.Rlsd);
            }
            base.OnStatusChange(mask, state);
        }

        protected override bool AfterOpen()
        {
            if (PortOpened != null)
            {
                PortOpened();
            }
            return base.AfterOpen();
        }

        new public ModemStatus GetModemStatus()
        {
            return base.GetModemStatus();
        }

        /* Returns the logical level of the PPS (pin 1) */
        public bool GetPpsLevel()
        {
            return GetModemStatus().Rlsd;
        }
    }
}
