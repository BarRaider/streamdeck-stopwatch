using BarRaider.SdTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Stopwatch
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
            dicCounters[settings.StopwatchId].Stopwatch.Start();
        }

        public void StopStopwatch(string stopwatchId)
        {
            if (dicCounters.ContainsKey(stopwatchId))
            {
                dicCounters[stopwatchId].Stopwatch.Stop();
            }
        }

        public void ResetStopwatch(StopwatchSettings settings)
        {
            InitializeStopwatch(settings);
            dicCounters[settings.StopwatchId].Stopwatch.Reset();
            dicCounters[settings.StopwatchId].Laps.Clear();

            // Clear file contents
            if (settings.ClearFileOnReset)
            {
                SaveTimerToFile(settings.FileName, "00:00:00");
            }
        }

        public void RecordLap(string stopwatchId)
        {
            dicCounters[stopwatchId].Laps.Add(dicCounters[stopwatchId].Stopwatch.Elapsed);
        }

        public List<TimeSpan> GetLaps(string stopwatchId)
        {
            return dicCounters[stopwatchId].Laps;
        }

        public TimeSpan GetStopwatchTime(string stopwatchId)
        {
            if (!dicCounters.ContainsKey(stopwatchId))
            {
                return new TimeSpan(0);
            }
            
            return dicCounters[stopwatchId].Stopwatch.Elapsed;
        }

        public bool IsStopwatchEnabled(string stopwatchId)
        {
            if (!dicCounters.ContainsKey(stopwatchId))
            {
                return false;
            }
            return dicCounters[stopwatchId].Stopwatch.IsRunning;
        }

        public void TouchTimerFile(string filename, string startTime)
        {
            SaveTimerToFile(filename, startTime);
        }

        #endregion

        #region Private Methods

        private void TmrStopwatchCounter_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (string key in dicCounters.Keys)
            {
                if (dicCounters[key].Stopwatch.IsRunning)
                {
                    WriteTimerToFile(key);
                }
            }
        }

        private void WriteTimerToFile(string counterKey)
        {
            var stopwatchDate = dicCounters[counterKey];

            if (String.IsNullOrEmpty(stopwatchDate.Filename))
            {
                return;
            }

            TimeSpan ts = stopwatchDate.Stopwatch.Elapsed;
            SaveTimerToFile(stopwatchDate.Filename, $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}");
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

                var times = settings.StartTime.Split(':');
                if (!int.TryParse(times[0], out int hours))
                {
                    hours = 0;
                }
                if (!int.TryParse(times[1], out int minutes))
                {
                    minutes = 0;
                }
                if (!int.TryParse(times[2], out int seconds))
                {
                    seconds = 0;
                }
                dicCounters[stopwatchId].Stopwatch.StartOffset = new TimeSpan(hours, minutes, seconds);
            }

            dicCounters[stopwatchId].Filename = settings.FileName;
            dicCounters[stopwatchId].ClearFileOnReset = settings.ClearFileOnReset;
            dicCounters[stopwatchId].LapMode = settings.LapMode;
        }

        #endregion
    }
}
