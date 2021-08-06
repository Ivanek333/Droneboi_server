using System;
using System.Collections.Generic;
using System.Text;

namespace Droneboi_Server
{
    class Debug
    {
        public static void Log(string message)
        {
            Console.WriteLine(DateTime.Now.ToLongTimeString() + " >> " + message);
        }
    }
}
