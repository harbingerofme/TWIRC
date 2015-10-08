/*
 * Contains a class to run a thread to generate the random joypad
 * events
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWIRC
{
    public class ButtonMasher
    {
        Logger RNGLogger;
        LuaServer RNGLuaServer;
        public System.Windows.Forms.Timer timer_RNG;
        Dictionary<string, LuaServer.EmuClientHandler> RNGEmulators;
        double[] thisBias;
        double[] defaultBias;
        int numvals;
        public int[] stats;
        bool running;

        public ButtonMasher(Logger thelogger, int this_numvals, LuaServer whereToDump, Dictionary<string, LuaServer.EmuClientHandler> _RNGEmulators)//, Dictionary<string, LuaServer.EmuClientHandler> clienttable)
        {
            numvals = this_numvals;
            defaultBias = new double[] { 1.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
            thisBias = new double[] { 1.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
            stats = new int[] { 0, 0, 0, 0, 0, 0, 0 };
            Array.Copy(defaultBias,thisBias,7);
            //RNGEmulators = clienttable;
            RNGLogger = thelogger;
            RNGLuaServer = whereToDump;
            RNGEmulators = _RNGEmulators;

            timer_RNG = new System.Windows.Forms.Timer();
            timer_RNG.Interval = 40;
            timer_RNG.Tick += new System.EventHandler(workingMethod);

        }

        private void workingMethod(object obj, EventArgs e)
        {
            bool ishold = false;
            string lasthold = "";
            int holdtime = 0;
            if (RNGLuaServer.get_client_count() == 0) return;
            String cmd;
            int nextRNG = doRNG();

            if (!ishold && RNGesus.Next(8) == 3 && nextRNG < 4) // do we hold?
            {
                //RNGLogger.WriteLine("Doing hold: " + nextRNG);
                ishold = true;
                lasthold = nextRNG.ToString();
                holdtime = 1 + RNGesus.Next(7);
                cmd = "HOLD:";

            }
            else
            {
                cmd = "PRESS:";
            }

            if (ishold)
            {
                //RNGLogger.WriteLine("holding!" + holdtime + "     " + lasthold);
                holdtime--;
                cmd = "HOLD:" + lasthold;
            }
            else
            {
                cmd += nextRNG;
            }

            if (holdtime < 1)
            {
                ishold = false;
            }
            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {

                //RNGLogger.addLog("RNG-manually", 0, "rngagege");
                try
                {
                    rngclient.sendCommand(cmd);
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }

            }
        }

        Random RNGesus = new Random();

        public string rngTest(int numrolls)
        {

            int[] results = new int[11];
            Single[] percents = new Single[11];
            for (int i = 0; i < numrolls; i++)
            {
                results[doRNG()]++;
            }

            for (int i = 0; i < 11; i++)
            {
                percents[i] = 100 * (Single)results[i] / (Single)numrolls;
            }

            return "RNGtest " + String.Join(", ", percents);

        }


        public void setBias(double[] newbias)
        {
            try
            {
                Array.Copy(newbias, thisBias, 7);
            }
            catch { }
        }
        
        public void setDefaultBias(double[] newbias)
        {
            string s = "";
            foreach(double d in newbias)
            {
                s += Math.Round(d, 4) + " ";
            }
            RNGLogger.addLog("RNGesus",1,"Setting default bias to: " + s);
            Array.Copy(newbias, defaultBias, 7 );
        }
        public double[] getDefaultBias()
        {
            double [] tempbias = new double[7];
            Array.Copy(defaultBias, tempbias,7);
            return tempbias;
        }

        public double[] getCurrentBias()
        {
            double[] tempbias = new double[7];
            Array.Copy(thisBias, tempbias, 7);
            return tempbias;
        }


        public void doDecay()
        {
            for (int i = 0; i < numvals; i++)
            {
               thisBias[i] = (4 * thisBias[i] + defaultBias[i]) / 5;
            }
        }


        public int doRNG()
        {
            int winner = 0;
            double[] losers = new double[11]; //table of results of rolls
            double nextval; // temporary value store for the roll

            for (int i = 0; i < numvals; i++) // 0 through howmany - 1
            {
                nextval = RNGesus.NextDouble() * thisBias[i];  //get next random value
                losers[i] = nextval;  //add it to the array
                if (nextval > losers[winner]) // if higher than previous winner, update winner. will be y
                {
                    winner = i;
                    losers[winner] = nextval;
                }
            }
            stats[winner]++;
            return winner;
        }

    }
}
