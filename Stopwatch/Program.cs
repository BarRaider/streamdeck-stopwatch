﻿using BarRaider.SdTools;
using BRUtils;
using System;

namespace Stopwatch
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            // Uncomment this line of code to allow for debugging
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            SDWrapper.Run(args, new UpdateHandler());
        }
    }
}
