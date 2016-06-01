/* Represents a serial port */

using System;

namespace Utils.GPS.SerialGPS
{
    public interface ISerial
    {
        /* Called when the PPS pin changes the state */
        event Action<bool> PpsChanged;

        /* Called when a byte is received */
        event Action<byte> DataReceived;

        /* Called after the port is opened */
        event Action PortOpened;

        /* Open the serial port */
        void Open();

        /* Close the serial port*/
        void Close();

        /* Returns the PPS pin state */
        bool GetPpsLevel();
    }
}
