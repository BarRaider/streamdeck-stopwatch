using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopwatch
{
    public class StopwatchStatus
    {
        public long Counter { get; set; }

        public bool IsEnabled { get; set; }

        public string Filename { get; set; }

        public bool LapMode { get; set; }

        public bool ClearFileOnReset { get; set; }

        public List<long> Laps { get; set; }

        public StopwatchStatus()
        {
            Counter = 0;
            IsEnabled = false;
            Filename = String.Empty;
            ClearFileOnReset = false;
            Laps = new List<long>();
        }
    }
}
