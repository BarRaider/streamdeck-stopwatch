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
        public StopwatchWithOffset Stopwatch { get; set; }

        public string Filename { get; set; }

        public bool LapMode { get; set; }

        public bool ClearFileOnReset { get; set; }

        public List<TimeSpan> Laps { get; set; }

        public StopwatchStatus()
        {
            Stopwatch = new StopwatchWithOffset(new TimeSpan(0));
            Filename = String.Empty;
            ClearFileOnReset = false;
            Laps = new List<TimeSpan>();
        }
    }
}
