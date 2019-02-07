using BarRaider.SdTools;
using CommandLine;
using System;
using System.Collections.Generic;

namespace Stopwatch
{
    class Program
    {
        static void Main(string[] args)
        {
            // Uncomment this line of code to allow for debugging
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            List<PluginActionId> supportedActionIds = new List<PluginActionId>();
            supportedActionIds.Add(new PluginActionId("com.barraider.stopwatch", typeof(StopwatchTimer)));

            SDWrapper.Run(args, supportedActionIds.ToArray());
        }
    }
}
