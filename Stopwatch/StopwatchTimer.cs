using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Stopwatch
{
    [PluginActionId("com.barraider.stopwatch")]
    public class StopwatchTimer : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.ResumeOnClick = false;
                instance.Multiline = false;

                return instance;
            }

            [JsonProperty(PropertyName = "resumeOnClick")]
            public bool ResumeOnClick { get; set; }

            [JsonProperty(PropertyName = "multiline")]
            public bool Multiline { get; set; }
        }

        #region Private members

        private const int RESET_COUNTER_KEYPRESS_LENGTH = 1;

        private PluginSettings settings;
        private bool keyPressed = false;
        private DateTime keyPressStart;
        private string stopwatchId;

        #endregion

        #region PluginBase Methods

        public StopwatchTimer(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(settings));
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
            stopwatchId = Connection.ContextId;
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            // New in StreamDeck-Tools v2.0:
            Tools.AutoPopulateSettings(settings, payload.Settings);
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {}


        public override void KeyPressed(KeyPayload payload)
        {
            // Used for long press
            keyPressStart = DateTime.Now;
            keyPressed = true;

            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");

            if (StopwatchManager.Instance.IsStopwatchEnabled(stopwatchId))
            {
                PauseStopwatch();
            }
            else
            {
                if (!settings.ResumeOnClick)
                {
                    ResetCounter();
                }

                ResumeStopwatch();
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
            keyPressed = false;
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Released");
        }

        public async override void OnTick()
        {
            long total, minutes, seconds, hours;
            string delimiter = settings.Multiline ? "\n" : ":";

            // Stream Deck calls this function every second, 
            // so this is the best place to determine if we need to reset (versus the internal timer which may be paused)
            CheckIfResetNeeded();

            total = StopwatchManager.Instance.GetStopwatchTime(stopwatchId);
            minutes = total / 60;
            seconds = total % 60;
            hours = minutes / 60;
            minutes = minutes % 60;

            await Connection.SetTitleAsync($"{hours.ToString("00")}{delimiter}{minutes.ToString("00")}\n{seconds.ToString("00")}");
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        #endregion

        #region Private methods

        private void ResetCounter()
        {
            StopwatchManager.Instance.ResetStopwatch(stopwatchId);
        }

        private void ResumeStopwatch()
        {
            bool reset = !settings.ResumeOnClick;
            StopwatchManager.Instance.StartStopwatch(stopwatchId, reset);
        }

        private void CheckIfResetNeeded()
        {
            if (!keyPressed)
            {
                return;
            }

            if ((DateTime.Now - keyPressStart).TotalSeconds > RESET_COUNTER_KEYPRESS_LENGTH)
            {
                PauseStopwatch();
                ResetCounter();
            }
        }
        
        private void PauseStopwatch()
        {
            Stopwatch.StopwatchManager.Instance.StopStopwatch(stopwatchId);
        }
        #endregion
    }
}
