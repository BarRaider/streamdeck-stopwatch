using BarRaider.SdTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Stopwatch.Backend
{
    internal class StopwatchManager
    {
        #region Private members
        private static StopwatchManager instance = null;
        private static readonly object objLock = new object();

        private readonly Dictionary<string, StopwatchStatus> dicCounters = new Dictionary<string, StopwatchStatus>();
        private readonly Timer tmrStopwatchCounter;

        #endregion

        #region Constructors

        public static StopwatchManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                lock (objLock)
                {
                    if (instance == null)
                    {
                        instance = new StopwatchManager();
                    }
                    return instance;
                }
            }
        }

        private StopwatchManager()
        {
            tmrStopwatchCounter = new Timer
            {
                Interval = 1000
            };
            tmrStopwatchCounter.Elapsed += TmrStopwatchCounter_Elapsed;
            tmrStopwatchCounter.Start();
        }

        #endregion

        #region Public Methods

        public void StartStopwatch(StopwatchSettings settings)
        {
            InitializeStopwatch(settings);
            if (settings.ResetOnStart)
            {
                ResetStopwatch(settings);
            }
            else // Stopwatch was paused, need to resume
            {
                var status = dicCounters[settings.StopwatchId];
                status.StartTime = DateTime.Now.AddSeconds(-1 * status.GetSeconds());
            }
            dicCounters[settings.StopwatchId].IsEnabled = true;
        }

        public void StopStopwatch(string stopwatchId)
        {
            if (dicCounters.ContainsKey(stopwatchId))
            {
                dicCounters[stopwatchId].EndTime = DateTime.Now;
                dicCounters[stopwatchId].IsEnabled = false;
            }
        }

        public void ResetStopwatch(StopwatchSettings settings)
        {
            InitializeStopwatch(settings);
            dicCounters[settings.StopwatchId].StartTime = DateTime.Now;
            dicCounters[settings.StopwatchId].Laps.Clear();
            dicCounters[settings.StopwatchId].EndTime = null;

            // Clear file contents
            if (settings.ClearFileOnReset)
            {
                SaveTimerToFile(settings.FileName, "");
            }
        }

        public void RecordLap(string stopwatchId)
        {
            dicCounters[stopwatchId].Laps.Add(DateTime.Now);
        }

        public List<DateTime> GetLaps(string stopwatchId)
        {
            return dicCounters[stopwatchId].Laps;
        }

        public long GetStopwatchTime(string stopwatchId)
        {
            if (!dicCounters.ContainsKey(stopwatchId))
            {
                return 0;
            }
            return dicCounters[stopwatchId].GetSeconds();
        }

        public DateTime GetStopwatchStartTime(string stopwatchId)
        {
            if (!dicCounters.ContainsKey(stopwatchId))
            {
                return DateTime.MinValue;
            }
            return dicCounters[stopwatchId].StartTime;
        }

        public bool IsStopwatchEnabled(string stopwatchId)
        {
            if (!dicCounters.ContainsKey(stopwatchId))
            {
                return false;
            }
            return dicCounters[stopwatchId].IsEnabled;
        }

        public void TouchTimerFile(string filename)
        {
            SaveTimerToFile(filename, "00:00");
        }

        #endregion

        #region Private Methods

        private void TmrStopwatchCounter_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (string key in dicCounters.Keys)
            {
                if (dicCounters[key].IsEnabled)
                {
                    WriteTimerToFile(key);
                }
            }
        }

        private void WriteTimerToFile(string counterKey)
        {
            long total, minutes, seconds, hours;
            var stopwatchDate = dicCounters[counterKey];

            if (String.IsNullOrEmpty(stopwatchDate.Filename))
            {
                return;
            }


            total = (long)(DateTime.Now - stopwatchDate.StartTime).TotalSeconds;
            minutes = total / 60;
            seconds = total % 60;
            hours = minutes / 60;
            minutes %= 60;

            string hoursStr = (hours > 0) ? $"{hours.ToString("00")}:" : "";
            SaveTimerToFile(stopwatchDate.Filename, $"{hoursStr}{minutes.ToString("00")}:{seconds.ToString("00")}");
        }

        private void SaveTimerToFile(string fileName, string text)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(fileName))
                {

                    File.WriteAllText(fileName, text);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Error Saving value: {text} to counter file: {fileName} : {ex}");
            }
        }

        private void InitializeStopwatch(StopwatchSettings settings)
        {
            string stopwatchId = settings.StopwatchId;
            if (!dicCounters.ContainsKey(stopwatchId))
            {
                dicCounters[stopwatchId] = new StopwatchStatus();
            }

            dicCounters[stopwatchId].Filename = settings.FileName;
            dicCounters[stopwatchId].ClearFileOnReset = settings.ClearFileOnReset;
            dicCounters[stopwatchId].LapMode = settings.LapMode;
        }

        #endregion
    }
}
