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




        public ButtonMasher(Logger thelogger, int this_numvals)//, Dictionary<string, LuaServer.EmuClientHandler> clienttable)
        {
            numvals = this_numvals;
            defaultBias = new double[] { 1.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
            thisBias = new double[] { 1.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
            Array.Copy(defaultBias,thisBias,7);
            //RNGEmulators = clienttable;
            RNGLogger = thelogger;

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

        public string oldRngTest(int numrolls, int biasval)
        {
            int[] results = new int[11];
            Single[] percents = new Single[11];
            for (int i = 0; i < numrolls; i++)
            {
                results[doRNG2(biasval)]++; //neutral bias
            }

            for (int i = 0; i < 11; i++)
            {
                percents[i] = 100 * (Single)results[i] / (Single)numrolls;
            }

            return String.Join("   \t", percents);

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
            Array.Copy(newbias, defaultBias, 7 );
        }
        public double[] getDefaultBias()
        {
            return defaultBias;
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
            return winner;
        }

        private int doRNG2(int rbias)
        {
            //Random, rand, 1, 200 ; 100% in 0.5% steps
            int rand = RNGesus.Next(1, 201);

            // 15% a; 10% b; 5% start;
            // -> 70% for directions -> 17.5% per direction (no directional bias)
            // 16.5% per dir -> 66% for all -> 4% remain

            int result = -1;

            //enum thebias {NEUTRAL, UP, DOWN, LEFT, RIGHT, UP_LEFT, DOWN_LEFT, UP_RIGHT, DOWN_RIGHT};
            int up = 0;
            int down = 1;
            int left = 2;
            int right = 3;
            int A = 4;
            int B = 5;
            int start = 6;

            int UP = 7;
            int DOWN = 8;
            int LEFT = 9;
            int RIGHT = 10;
            int DOWN_LEFT = 11;
            int DOWN_RIGHT = 12;
            int UP_LEFT = 13;
            int UP_RIGHT = 14;
            int START = 16;
            //int NEUTRAL = 20;

            //int rbias = NEUTRAL;

            if (rand > 170)// 15% A
            {
                result = A;
            }
            else if (rand > 150)// 10% B
            {
                result = B;
            }
            else if (rand > 140) // 5% Start
            {
                result = start;
            }
            else if (rand > 107)// 17% Left
            {
                result = left;
            }
            else if (rand > 74) // 17% Right
            {
                result = right;
            }
            else if (rand > 41)// 17% Up
            {
                result = up;
            }
            else if (rand > 8) // 17% Down
            {
                result = down;
            }
            else // Left/Right/Up/Down depending on BIAS
            {
                if (rbias == LEFT)
                {
                    result = left;
                }
                else if (rbias == RIGHT)
                {
                    result = right;
                }
                else if (rbias == UP)
                {
                    result = up;
                }
                else if (rbias == DOWN)
                {
                    result = down;
                }
                else if (rbias == UP_LEFT)
                {
                    rand = RNGesus.Next(1, 3);
                    if (rand == 1)
                    {
                        result = left;
                    }
                    else if (rand == 2)
                    {
                        result = up;
                    }

                }
                else if (rbias == UP_RIGHT)
                {
                    rand = RNGesus.Next(1, 3);

                    if (rand == 1)
                    {
                        result = right;
                    }
                    else if (rand == 2)
                    {
                        result = up;
                    }

                }
                else if (rbias == DOWN_LEFT)
                {
                    rand = RNGesus.Next(1, 3);

                    if (rand == 1)
                    {
                        result = left;
                    }
                    else if (rand == 2)
                    {
                        result = down;
                    }
                }
                else if (rbias == DOWN_RIGHT)
                {
                    rand = RNGesus.Next(1, 3);

                    if (rand == 1)
                    {
                        result = right;
                    }
                    else if (rand == 2)
                    {
                        result = down;
                    }
                }
                else if (rbias == START)
                {
                    result = start;
                }
                else // bias = NEUTRAL / undefined (initially)
                {
                    rand = RNGesus.Next(1, 5);
                    if (rand == 1)
                    {
                        result = left;
                    }
                    else if (rand == 2)
                    {
                        result = right;
                    }
                    else if (rand == 3)
                    {
                        result = up;
                    }
                    else if (rand == 4)
                    {
                        result = down;
                    }
                }
            }

            return result;
        }
    }
}
