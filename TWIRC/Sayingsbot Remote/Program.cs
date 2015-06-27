using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Sayingsbot_Remote
{
    static class Program
    {

        public static Logger RemoteLogger;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            RemoteLogger = new Logger();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain(RemoteLogger));
        }
    }
}
