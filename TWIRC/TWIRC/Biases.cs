using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNGBot
{
    public static class Biases
    {

        //ewww
                                    // LT    DN    UP    RT
        public static double[] leftDown =   { 1.20, 1.20, 1.00, 1.00, 0.96, 0.92, 0.82 };
        public static double[] down =       { 1.00, 1.28, 1.00, 1.00, 0.96, 0.92, 0.82 };
        public static double[] downRight =  { 1.00, 1.20, 1.00, 1.20, 0.96, 0.92, 0.82 };
        public static double[] left =       { 1.28, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
        public static double[] neutral =    { 1.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
        public static double[] right =      { 1.00, 1.00, 1.00, 1.28, 0.96, 0.92, 0.82 };
        public static double[] upLeft =     { 1.20, 1.00, 1.20, 1.00, 0.96, 0.92, 0.82 };
        public static double[] up =         { 1.00, 1.00, 1.28, 1.00, 0.96, 0.92, 0.82 };
        public static double[] upRight =    { 1.00, 1.00, 1.20, 1.20, 0.96, 0.92, 0.82 };

        public static double[] getBias(string biasname)
        {
            switch (biasname.ToLower())
            {
                case "leftdown":
                case "left-down":
                    return leftDown;
                case "down":
                     return down;
                case "downright":
                case "down-right":
                    return downRight;
                case "left":
                    return left;
                case "neutral":
                    return neutral;
                case "right":
                    return right;
                case "upleft":
                case "up-left":
                    return upLeft;
                case "up":
                    return up;
                case "upright":
                case "up-right":
                    return upRight;
                default:
                    return neutral;

            
            }
        }

        public static double[] getBias(int biasname)
        {
            switch (biasname)
            {
                case 1:
                    return leftDown;
                case 2:
                    return down;
                case 3:
                    return downRight;
                case 4:
                    return left;
                case 5:
                    return neutral;
                case 6:
                    return right;
                case 7:
                    return upLeft;
                case 8:
                    return up;
                case 9:
                    return upRight;
                default:
                    return neutral;


            }

        }

    }
}
