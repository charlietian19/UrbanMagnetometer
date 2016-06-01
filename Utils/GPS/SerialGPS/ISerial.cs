/* Represents a serial port */

using System;
using PInvokeSerialPort;

namespace Utils.GPS.SerialGPS
{
    public interface ISerial
    {
        /* Called when any status pin changes the state */
        event Action<ModemStatus, ModemStatus> StatusChanged;

        /* Called when a byte is received */
        event Action<byte> DataReceived;

        /* Called after the port is opened */
        event Action PortOpened;

        /* Open the serial port */
        void Open();

        /* Close the serial port*/
        void Close();

        /* Returns the modem status bits */
        ModemStatus GetModemStatus();
    }
}
