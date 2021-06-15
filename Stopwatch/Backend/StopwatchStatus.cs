using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopwatch.Backend
{
    public class StopwatchStatus
    {
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; } = null;

        public bool IsEnabled { get; set; }

        public string Filename { get; set; }

        public bool LapMode { get; set; }

        public bool ClearFileOnReset { get; set; }

        public List<DateTime> Laps { get; set; }

        public StopwatchStatus()
        {
            StartTime = DateTime.Now;
            IsEnabled = false;
            Filename = String.Empty;
            ClearFileOnReset = false;
            Laps = new List<DateTime>();
        }

        public long GetSeconds()
        {
            if (IsEnabled || !EndTime.HasValue)
            {
                return (long)(DateTime.Now - StartTime).TotalSeconds;
            }
            else
            {
                return (long)(EndTime.Value - StartTime).TotalSeconds;
            }
        }
    }
}