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


        static Dictionary<string,LuaServer.EmuClientHandler> RNGEmulators;
        static Logger RNGLogger;
        static LuaServer RNGLuaServer;
        static ButtonMasher RNGesus;
        static MainWindow mainWindow;
        static HarbBot HarbBot;
        static DatabaseConnector dbConn;
        const string VERSION = "2.0.0";
        


        [STAThread] static void Main()
        {



            RNGLogger = new Logger();
            RNGLogger.addLog("PROGRAM", 0, "Logger object created");
#if OFFLINE
            RNGLogger.addLog("Main()", 0, "Working in offline mode, no IRC connection will be made!");
#endif

            RNGEmulators = new Dictionary<string, LuaServer.EmuClientHandler>(); //List of connected emulators
            
            RNGLuaServer = new LuaServer(RNGLogger, RNGEmulators);
            RNGLuaServer.Run();

            RNGesus = new ButtonMasher(RNGLogger, 7); // 6 buttons

            HarbBot = new HarbBot(RNGLogger, RNGesus,RNGLuaServer);

            dbConn = new DatabaseConnector(RNGLogger);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //mainWindow = new RNGWindow(RNGLogger, RNGLuaServer, RNGEmulators, RNGesus, biasWindow, HarbBot);
            mainWindow = new MainWindow(HarbBot, RNGLogger, dbConn, RNGLuaServer);
            
            Application.Run(mainWindow);


        }
    }
}
