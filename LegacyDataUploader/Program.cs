using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace LegacyDataUploader
{
    static class Program
    {
        /// <summary>
        /// The file format used by this program is obsolete, it won't
        /// read the new files
        /// </summary>
        [STAThread]
        static void Main()
        {
            Process process = Process.GetCurrentProcess();
            process.PriorityClass = ProcessPriorityClass.BelowNormal;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
