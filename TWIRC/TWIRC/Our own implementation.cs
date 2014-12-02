using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace TWIRC
{ //name will be changed later, care for it.
    class Our_own_implementation
    {
        public static void Main(string[] args)
        {

            //loadInformation();

            Thread.CurrentThread.Name = "Main";
            HarbBot HB = new HarbBot();
       }
    }
}
