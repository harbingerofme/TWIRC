using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
//using System.Threading;

namespace TWIRC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>


        static Logger RNGLogger;
        static RNGWindow mainWindow;
        static HarbBot HarbBot;
        //static DBHandler RNGDB;
        


        [STAThread] static void Main()
        {



            RNGLogger = new Logger();
            RNGLogger.addLog("Main()", 0, "Logger object created");
#if OFFLINE
            RNGLogger.addLog("Main()", 0, "Working in offline mode, no IRC connection will be made!");
#endif

#if !OFFLINE
            HarbBot = new HarbBot(RNGLogger);
#endif

            //RNGDB = new DBHandler("rngppbot.sqlite", RNGLogger);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainWindow = new RNGWindow(RNGLogger, HarbBot);
            
            Application.Run(mainWindow);


        }
    }
}
