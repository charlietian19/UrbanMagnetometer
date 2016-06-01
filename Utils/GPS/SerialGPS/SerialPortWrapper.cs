using System;
using PInvokeSerialPort;

/* Wraps PInvokeSerialPort mostly to facilitate testing */

namespace Utils.GPS.SerialGPS
{
    class SerialPortWrapper : SerialPort, ISerial
    {
        public event Action<ModemStatus, ModemStatus> StatusChanged;
        public event Action PortOpened;

        new public void Open()
        {
            base.Open();
        }

        public SerialPortWrapper(string portName)
            : base(portName) { }

        public SerialPortWrapper(string portName, int baudRate) 
            : base(portName, baudRate) { }

        protected override void OnStatusChange(ModemStatus mask, ModemStatus state)
        {            
            base.OnStatusChange(mask, state);
            if (StatusChanged != null)
            {
                StatusChanged(mask, state);
            }
        }

        new public bool AutoReopen
        {
            get { return base.AutoReopen; }
            set { base.AutoReopen = value; }
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
    }
}
