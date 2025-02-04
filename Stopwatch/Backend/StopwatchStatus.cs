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

        public string FileTitlePrefix { get; set; }

        public bool LapMode { get; set; }

        public bool ClearFileOnReset { get; set; }

        public List<long> Laps { get; set; }

        public string TimeFormat { get; set; }

        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public StopwatchStatus()
        {
            Filename = String.Empty;
            ClearFileOnReset = false;
            Laps = new List<long>();
            TimeFormat = HelperUtils.DEFAULT_TIME_FORMAT;
            FileTitlePrefix = String.Empty;

        }

        public long GetSeconds()
        {
            return (long)sw.Elapsed.TotalSeconds;
        }

        public long GetMiliseconds()
        {
            return (long)sw.Elapsed.TotalMilliseconds;
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