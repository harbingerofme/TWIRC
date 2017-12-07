using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SayingsBot
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>


        public static Logger RNGLogger;
        public static RNGWindow mainWindow;
        public static frmDiscord disocrdWindow;
        public static volatile HarbBot HarbBot;
        public static NetComm.Host Server;
        //static DBHandler RNGDB;
        static Thread t = new Thread(new ThreadStart(thread2));

        static void thread2()
        {
            try
            {
                Application.Run(disocrdWindow);
            }
            catch (Exception EEE)
            {
                HarbBot.appendFile(HarbBot.progressLogPATH, EEE.ToString());
            }
        }


        [STAThread] static void Main()
        {
#if DEBUG
            Server = new NetComm.Host(8524);
#else
            Server = new NetComm.Host(8523);
#endif

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
            disocrdWindow = new frmDiscord(RNGLogger, HarbBot);
            try
            {
                t.Start();
                Application.Run(mainWindow);
            } catch (Exception EEE) {
                HarbBot.appendFile(HarbBot.progressLogPATH, EEE.ToString());
            }
        }
    }
}
