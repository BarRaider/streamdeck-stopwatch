using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Stopwatch.Backend
{
    public class StopwatchStatus
    {
        public bool IsEnabled => sw?.IsRunning ?? false;

        public string Filename { get; set; }

        public bool LapMode { get; set; }

        public bool ClearFileOnReset { get; set; }

        public List<long> Laps { get; set; }

        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public StopwatchStatus()
        {
            Filename = String.Empty;
            ClearFileOnReset = false;
            Laps = new List<long>();
        }

        public long GetSeconds()
        {
            return (long)sw.Elapsed.TotalSeconds;
        }

        public void Start()
        {
            sw.Start();
        }

        public void Stop()
        {
            sw.Stop();
        }

        public void Reset()
        {
            sw = new System.Diagnostics.Stopwatch();
        }

    }
}