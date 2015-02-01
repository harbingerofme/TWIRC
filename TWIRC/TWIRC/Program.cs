using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
//using System.Threading;

namespace RNGBot
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
        static RNGWindow mainWindow; 
        //static DBHandler RNGDB;
        


        [STAThread] static void Main()
        {



            RNGLogger = new Logger();
            RNGLogger.addLog("Main()", 0, "Logger object created");
#if OFFLINE
            RNGLogger.addLog("Main()", 0, "Working in offline mode, no IRC connection will be made!");
#endif

            RNGEmulators = new Dictionary<string, LuaServer.EmuClientHandler>(); //List of connected emulators
            
            RNGLuaServer = new LuaServer(RNGLogger, RNGEmulators);
            RNGLuaServer.Run();

            RNGesus = new ButtonMasher(RNGLogger, 7); // 6 buttons
                              // LT   DN     UP     RT
            double[] bias1 = { 2.25, 1.00, 2.25, 1.00, 0.96, 0.92, 0.82 };
            double[] bias2 = { 1.00, 3.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
            double[] bias3 = { 2.25, 1.00, 2.25, 2.25, 0.96, 0.92, 0.82 };
            double[] bias4 = { 3.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
            double[] bias5 = { 1.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
            double[] bias6 = { 1.00, 1.00, 1.00, 3.00, 0.96, 0.92, 0.82 };
            double[] bias7 = { 1.00, 2.25, 1.00, 2.25, 0.96, 0.92, 0.82 };
            double[] bias8 = { 1.00, 1.00, 3.00, 1.00, 0.96, 0.92, 0.82 };
            double[] bias9 = { 1.00, 2.25, 1.00, 2.25, 0.96, 0.92, 0.82 };


            RNGesus.setDefaultBias(bias5); //values to average against
            RNGesus.setBias(bias5);

            //RNGDB = new DBHandler("rngppbot.sqlite", RNGLogger);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainWindow = new RNGWindow(RNGLogger, RNGLuaServer, RNGEmulators, RNGesus);
            Application.Run(mainWindow);


        }
    }
}
