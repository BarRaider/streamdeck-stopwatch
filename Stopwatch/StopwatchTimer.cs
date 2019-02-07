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

        private Timer tmrStopwatch;
        private PluginSettings settings;
        private bool keyPressed = false;
        private DateTime keyPressStart;
        private long stopwatchSeconds;

        #endregion

        #region Public Methods

        public StopwatchTimer(SDConnection connection, JObject settings) : base(connection, settings)
        {
            if (settings == null || settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                this.settings = settings.ToObject<PluginSettings>();
            }
            ResetCounter();
        }

        public override void KeyPressed()
        {
            // Used for long press
            keyPressStart = DateTime.Now;
            keyPressed = true;

            if (tmrStopwatch != null && tmrStopwatch.Enabled)
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

        public override void KeyReleased()
        {
            keyPressed = false;
        }

        public async override void OnTick()
        {
            long total, minutes, seconds, hours;
            string delimiter = settings.Multiline ? "\n" : ":";

            // Stream Deck calls this function every second, 
            // so this is the best place to determine if we need to reset (versus the internal timer which may be paused)
            CheckIfResetNeeded();

            total = stopwatchSeconds;
            minutes = total / 60;
            seconds = total % 60;
            hours = minutes / 60;
            minutes = minutes % 60;

            await Connection.SetTitleAsync($"{hours.ToString("00")}{delimiter}{minutes.ToString("00")}\n{seconds.ToString("00")}");
        }

        public override void Dispose()
        {
            PauseStopwatch();
        }

        public async override void UpdateSettings(JObject payload)
        {
            if (payload["property_inspector"] != null)
            {
                switch (payload["property_inspector"].ToString().ToLower())
                {
                    case "propertyinspectorconnected":
                        await Connection.SendToPropertyInspectorAsync(JObject.FromObject(settings));
                        break;

                    case "propertyinspectorwilldisappear":
                        await Connection.SetSettingsAsync(JObject.FromObject(settings));
                        break;

                    case "updatesettings":
                        settings.Multiline = (bool)payload["multiline"];
                        settings.ResumeOnClick = (bool)payload["resumeOnClick"];
                        await Connection.SetSettingsAsync(JObject.FromObject(settings));
                        break;
                }
            }
        }

        #endregion

        #region Private methods

        private void ResetCounter()
        {
            stopwatchSeconds = 0;
        }

        private void ResumeStopwatch()
        {
            if (tmrStopwatch is null)
            {
                tmrStopwatch = new Timer();
                tmrStopwatch.Elapsed += TmrStopwatch_Elapsed;
            }
            tmrStopwatch.Interval = 1000;
            tmrStopwatch.Start();
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

        private void TmrStopwatch_Elapsed(object sender, ElapsedEventArgs e)
        {
            stopwatchSeconds++;
        }

        private void PauseStopwatch()
        {
            tmrStopwatch.Stop();
        }

        #endregion
    }
}
