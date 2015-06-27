using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
//using System.Threading;

namespace SayingsBot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>


        public static Logger RNGLogger;
        public static RNGWindow mainWindow;
        public static HarbBot HarbBot;
        public static NetComm.Host Server;
        //static DBHandler RNGDB;
        


        [STAThread] static void Main()
        {

            Server = new NetComm.Host(8523);

            RNGLogger = new Logger();
            RNGLogger.addLog("Main()", 0, "Logger object created");
#if OFFLINE
            RNGLogger.addLog("Main()", 0, "Working in offline mode, no IRC connection will be made!");
#endif

#if !OFFLINE
            HarbBot = new HarbBot(RNGLogger, Server);
#endif

            //RNGDB = new DBHandler("rngppbot.sqlite", RNGLogger);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainWindow = new RNGWindow(RNGLogger, HarbBot);

            Application.Run(mainWindow);
        }
    }
}
