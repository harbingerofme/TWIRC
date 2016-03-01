using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;

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
        static DatabaseScheduler dbSched;
        const string VERSION = "2.0.0";
        


        [STAThread] static void Main()
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;


            RNGLogger = new Logger();
            RNGLogger.addLog("PROGRAM", 0, "Logger object created");
            RNGLogger.addLog("PROGRAM", 0, "Running TWIRC " + VERSION + ".");

            RNGEmulators = new Dictionary<string, LuaServer.EmuClientHandler>(); //List of connected emulators
            
            RNGLuaServer = new LuaServer(RNGLogger, RNGEmulators);
            RNGLuaServer.Run();

            RNGesus = new ButtonMasher(RNGLogger, 7,RNGLuaServer,RNGEmulators); // 6 buttons

            dbConn = new DatabaseConnector(RNGLogger);
            dbSched = new DatabaseScheduler(dbConn);

            HarbBot = new HarbBot(RNGLogger, RNGesus,RNGLuaServer, dbConn);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //mainWindow = new RNGWindow(RNGLogger, RNGLuaServer, RNGEmulators, RNGesus, biasWindow, HarbBot);
            mainWindow = new MainWindow(HarbBot, RNGLogger, dbConn, RNGLuaServer, dbSched, RNGesus, RNGEmulators);
            
            Application.Run(mainWindow);


        }
    }
}
