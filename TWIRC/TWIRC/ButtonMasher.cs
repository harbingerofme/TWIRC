/*
 * Contains a class to run a thread to generate the random joypad
 * events
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNGBot
{
    public class ButtonMasher
    {
        Logger RNGLogger;
        //Dictionary<string, LuaServer.EmuClientHandler> RNGEmulators;
        double[] thisBias;
        double[] defaultBias;
        int numvals;
        RNGWindow MainWindow;
        public int[] stats;



        public ButtonMasher(Logger thelogger, int this_numvals)//, Dictionary<string, LuaServer.EmuClientHandler> clienttable)
        {
            numvals = this_numvals;
            defaultBias = new double[] { 1.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
            thisBias = new double[] { 1.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
            stats = new int[] { 0, 0, 0, 0, 0, 0, 0 };
            Array.Copy(defaultBias,thisBias,7);
            //RNGEmulators = clienttable;
            RNGLogger = thelogger;

        }

        public void set_MainWindow(RNGWindow NewMainWindow)
        {
            MainWindow = NewMainWindow;
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

            return "RNGtest " + String.Join("   \t", percents);

        }


        public void setBias(double[] newbias)
        {
            RNGLogger.WriteLine("New bias:" + Biases.printBias(newbias));
            try
            {
                Array.Copy(newbias, thisBias, 7);
            }
            catch { }
        }
        
        public void setDefaultBias(double[] newbias)
        {
            RNGLogger.WriteLine("Setting default bias to:" + Biases.printBias(newbias));
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
            RNGLogger.WriteLine("Doing Decay! " + Biases.printBias(thisBias));
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
