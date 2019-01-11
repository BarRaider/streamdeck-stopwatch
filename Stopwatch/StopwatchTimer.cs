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
    public class StopwatchTimer
    {
        private class InspectorSettings
        {
            public static InspectorSettings CreateDefaultSettings()
            {
                InspectorSettings instance = new InspectorSettings();
                instance.ResumeOnClick = false;
                instance.Multiline = false;

                return instance;
            }

            public async void SendToPropertyInspectorAsync()
            {
                if (StreamDeckConnection != null && !String.IsNullOrEmpty(ContextId) && !String.IsNullOrEmpty(ActionId))
                {
                    await StreamDeckConnection.SendToPropertyInspectorAsync(ActionId, JObject.FromObject(this), this.ContextId);
                }
            }

            public async void SetSettingsAsync()
            {
                if (StreamDeckConnection != null && !String.IsNullOrEmpty(ContextId) && !String.IsNullOrEmpty(ActionId))
                {
                    await StreamDeckConnection.SetSettingsAsync(JObject.FromObject(this), this.ContextId);
                }
            }

            [JsonProperty(PropertyName = "resumeOnClick")]
            public bool ResumeOnClick { get; set; }

            [JsonProperty(PropertyName = "multiline")]
            public bool Multiline { get; set; }

            [JsonIgnore]
            public string ActionId { private get; set; }

            [JsonIgnore]
            public string ContextId { private get; set; }

            [JsonIgnore]
            public streamdeck_client_csharp.StreamDeckConnection StreamDeckConnection { private get; set; }
        }

        #region Public properties

        public long StopwatchSeconds { get; private set; }

        #endregion

        #region Private members

        private Timer tmrStopwatch;
        private InspectorSettings settings;

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

        public void TriggerStopwatch()
        {
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

        public string GetCurrentStopwatchValue()
        {
            long total, minutes, seconds, hours;
            string delimiter = settings.Multiline ? "\n" : ":";

            total = StopwatchSeconds;
            minutes = total / 60;
            seconds = total % 60;
            hours = minutes / 60;
            minutes = minutes % 60;

            return $"{hours.ToString("00")}{delimiter}{minutes.ToString("00")}\n{seconds.ToString("00")}";
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
            StopwatchSeconds = 0;
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

        private void TmrStopwatch_Elapsed(object sender, ElapsedEventArgs e)
        {
            StopwatchSeconds++;
        }

        private void PauseStopwatch()
        {
            tmrStopwatch.Stop();
        }

        #endregion

    }
}
