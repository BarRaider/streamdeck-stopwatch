using System;
using System.Collections.Generic;
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

        private Dictionary<string, StopwatchStatus> dicCounters = new Dictionary<string, StopwatchStatus>();
        private Timer tmrStopwatchCounter;

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
            tmrStopwatchCounter = new Timer();
            tmrStopwatchCounter.Interval = 1000;
            tmrStopwatchCounter.Elapsed += TmrStopwatchCounter_Elapsed;
            tmrStopwatchCounter.Start();
        }

        #endregion

        #region Public Methods

        public void StartStopwatch(string stopwatchId, bool resetOnStart)
        {
            if (!dicCounters.ContainsKey(stopwatchId))
            {
                dicCounters[stopwatchId] = new StopwatchStatus();
            }

            if (resetOnStart)
            {
                ResetStopwatch(stopwatchId);
            }
            dicCounters[stopwatchId].IsEnabled = true;
        }

        public void StopStopwatch(string stopwatchId)
        {
            if (dicCounters.ContainsKey(stopwatchId))
            {
                dicCounters[stopwatchId].IsEnabled = false;
            }
        }

        public void ResetStopwatch(string stopwatchId)
        {
            if (!dicCounters.ContainsKey(stopwatchId))
            {
                dicCounters[stopwatchId] = new StopwatchStatus();
            }
            dicCounters[stopwatchId].Counter = 0;
        }

        public long GetStopwatchTime(string stopwatchId)
        {
            if (!dicCounters.ContainsKey(stopwatchId))
            {
                return 0;
            }
            return dicCounters[stopwatchId].Counter;
        }

        public bool IsStopwatchEnabled(string stopwatchId)
        {
            if (!dicCounters.ContainsKey(stopwatchId))
            {
                return false;
            }
            return dicCounters[stopwatchId].IsEnabled;
        }

        #endregion

        #region Private Methods

        private void TmrStopwatchCounter_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (string key in dicCounters.Keys)
            {
                if (dicCounters[key].IsEnabled)
                {
                    dicCounters[key].Counter++;
                }
            }
        }

        #endregion
    }
}
