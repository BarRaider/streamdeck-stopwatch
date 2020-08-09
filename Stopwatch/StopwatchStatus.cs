using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopwatch
{
    public class StopwatchStatus
    {
        public System.Diagnostics.Stopwatch Stopwatch { get; set; }

        public string Filename { get; set; }

        public bool LapMode { get; set; }

        public bool ClearFileOnReset { get; set; }

        public List<TimeSpan> Laps { get; set; }

        public StopwatchStatus()
        {
            Stopwatch = new System.Diagnostics.Stopwatch();
            Filename = String.Empty;
            ClearFileOnReset = false;
            Laps = new List<TimeSpan>();
        }
    }
}
