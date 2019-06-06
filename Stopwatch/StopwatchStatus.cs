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

        public StopwatchStatus()
        {
            Counter = 0;
            IsEnabled = false;
        }
    }
}
