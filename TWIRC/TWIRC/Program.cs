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

            RNGEmulators = new Dictionary<string, LuaServer.EmuClientHandler>(); //List of connected emulators
            
            RNGLuaServer = new LuaServer(RNGLogger, RNGEmulators);
            RNGLuaServer.Run();

            RNGesus = new ButtonMasher(RNGLogger);//, RNGEmulators);


            double[] bias1 = { 1.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82};
            double[] bias2 = { 2.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82};
            double[] bias3 = { 1.02, 1.00, 1.02, 1.00, 0.96, 0.92, 0.82};
            double[] bias4 = { 1.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.88};
            double[] bias5 = { 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00};


            RNGesus.setDefaultBias(bias1); //values to average against
            RNGesus.setBias(bias1);

            //RNGDB = new DBHandler("rngppbot.sqlite", RNGLogger);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainWindow = new RNGWindow(RNGLogger, RNGLuaServer, RNGEmulators, RNGesus);
            Application.Run(mainWindow);


        }
    }
}
