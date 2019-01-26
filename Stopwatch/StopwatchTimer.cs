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
    public class StopwatchTimer : IPluginable
    {
        private class InspectorSettings : SettingsBase
        {
            public static InspectorSettings CreateDefaultSettings()
            {
                InspectorSettings instance = new InspectorSettings();
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
        private InspectorSettings settings;
        private bool keyPressed = false;
        private DateTime keyPressStart;
        private long stopwatchSeconds;

        #endregion

        #region Public Methods

        public StopwatchTimer(streamdeck_client_csharp.StreamDeckConnection connection, string action, string context, JObject settings)
        {
            if (settings == null || settings.Count == 0)
            {
                this.settings = InspectorSettings.CreateDefaultSettings();
            }
            else
            {
                this.settings = settings.ToObject<InspectorSettings>();
            }

            this.settings.StreamDeckConnection = connection;
            this.settings.ActionId = action;
            this.settings.ContextId = context;
            ResetCounter();
        }

        public void KeyPressed()
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

        public void KeyReleased()
        {
            keyPressed = false;
        }

        public void OnTick()
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

            settings.SetTitleAsync($"{hours.ToString("00")}{delimiter}{minutes.ToString("00")}\n{seconds.ToString("00")}");
        }

        public void Dispose()
        {
            PauseStopwatch();
        }

        public void UpdateSettings(JObject payload)
        {
            if (payload["property_inspector"] != null)
            {
                switch (payload["property_inspector"].ToString().ToLower())
                {
                    case "propertyinspectorconnected":
                        settings.SendToPropertyInspectorAsync();
                        break;

                    case "propertyinspectorwilldisappear":
                        settings.SetSettingsAsync();
                        break;

                    case "updatesettings":
                        settings.Multiline = (bool)payload["multiline"];
                        settings.ResumeOnClick = (bool)payload["resumeOnClick"];
                        settings.SetSettingsAsync();
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
