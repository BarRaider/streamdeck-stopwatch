using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stopwatch.Backend;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Stopwatch.Actions
{
    [PluginActionId("com.barraider.stopwatch")]
    public class StopwatchTimerAction : PluginBase
    {

        //---------------------------------------------------
        //          BarRaider's Hall Of Fame
        // OneMouseGaming - Tip: $3.33
        // Subscriber: stea1e
        // Subscriber: leer_12345
        //---------------------------------------------------
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    ResumeOnClick = false,
                    Multiline = false,
                    ClearFileOnReset = false,
                    LapMode = false,
                    FileName = String.Empty,
                    SharedId = String.Empty,
                    PauseImageFile = String.Empty,
                    EnabledImageFile = String.Empty
                };

                return instance;
            }

            [JsonProperty(PropertyName = "resumeOnClick")]
            public bool ResumeOnClick { get; set; }

            [JsonProperty(PropertyName = "multiline")]
            public bool Multiline { get; set; }

            [JsonProperty(PropertyName = "fileName")]
            public string FileName { get; set; }

            [JsonProperty(PropertyName = "clearFileOnReset")]
            public bool ClearFileOnReset { get; set; }

            [JsonProperty(PropertyName = "lapMode")]
            public bool LapMode { get; set; }

            [JsonProperty(PropertyName = "sharedId")]
            public string SharedId { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "pauseImageFile")]
            public string PauseImageFile { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "enabledImageFile")]
            public string EnabledImageFile { get; set; }

        }

        #region Private members

        private const int RESET_COUNTER_KEYPRESS_LENGTH = 600;
        private readonly string[] DEFAULT_IMAGES = new string[]
        {
            @"images\bg@2x.png",
            @"images\bgEnabled.png"
        };

        private readonly PluginSettings settings;
        private bool keyPressed = false;
        private bool longKeyPress = false;
        private DateTime keyPressStart;
        private string stopwatchId;
        private readonly System.Timers.Timer tmrOnTick = new System.Timers.Timer();
        private Image pauseImage;
        private Image enabledImage;


        #endregion

        #region PluginBase Methods

        public StopwatchTimerAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            InitializeSettings();
            Connection.OnSendToPlugin += Connection_OnSendToPlugin;

            tmrOnTick.Interval = 250;
            tmrOnTick.Elapsed += TmrOnTick_Elapsed;
            tmrOnTick.Start();
        }


        public override void Dispose()
        {
            tmrOnTick.Stop();
            Connection.OnSendToPlugin -= Connection_OnSendToPlugin;
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            string fileName = settings.FileName;
            Tools.AutoPopulateSettings(settings, payload.Settings);
            InitializeSettings();
            if (fileName != settings.FileName)
            {
                StopwatchManager.Instance.TouchTimerFile(settings.FileName);
            }
            SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        { }


        public override void KeyPressed(KeyPayload payload)
        {
            // Used for long press
            keyPressStart = DateTime.Now;
            keyPressed = true;
            longKeyPress = false;

            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");

            if (payload.IsInMultiAction)
            {
                HandleMultiActionKeyPress(payload.UserDesiredState);
                return;
            }

            if (StopwatchManager.Instance.IsStopwatchEnabled(stopwatchId)) // Stopwatch is already running
            {
                if (settings.LapMode)
                {
                    StopwatchManager.Instance.RecordLap(stopwatchId);
                }
                else
                {
                    PauseStopwatch();
                }
            }
            else // Stopwatch is already paused
            {
                HandleStopwatchResume();
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
            keyPressed = false;
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Released");
        }

        // Using timer instead
        public override void OnTick() { }

        #endregion

        #region Private methods

        private void ResetCounter()
        {
            StopwatchManager.Instance.ResetStopwatch(new StopwatchSettings() { StopwatchId = stopwatchId, FileName = settings.FileName, ClearFileOnReset = settings.ClearFileOnReset, LapMode = settings.LapMode, ResetOnStart = !settings.ResumeOnClick });
        }

        private void ResumeStopwatch()
        {
            StopwatchManager.Instance.StartStopwatch(new StopwatchSettings() { StopwatchId = stopwatchId, FileName = settings.FileName, ClearFileOnReset = settings.ClearFileOnReset, LapMode = settings.LapMode, ResetOnStart = !settings.ResumeOnClick });
        }

        private void CheckIfResetNeeded()
        {
            if (!keyPressed)
            {
                return;
            }

            // Long key press
            if ((DateTime.Now - keyPressStart).TotalMilliseconds > RESET_COUNTER_KEYPRESS_LENGTH && !longKeyPress)
            {
                longKeyPress = true;
                PauseStopwatch();
                if (settings.LapMode)
                {
                    DateTime startTime = StopwatchManager.Instance.GetStopwatchStartTime(stopwatchId);
                    List<DateTime> laps = StopwatchManager.Instance.GetLaps(stopwatchId);
                    List<string> lapStr = new List<string>();

                    foreach (DateTime lap in laps)
                    {
                        lapStr.Add(SecondsToReadableFormat((long)(lap - startTime).TotalSeconds, ":", false));
                    }
                    SaveToClipboard(string.Join("\n", lapStr.ToArray()));

                }
                ResetCounter();
            }
        }

        private void SaveToClipboard(string str)
        {
            if (str == null)
            {
                return;
            }

            Thread t = new Thread(() =>
            {
                System.Windows.Forms.Clipboard.SetText(str);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }


        private void PauseStopwatch()
        {
            StopwatchManager.Instance.StopStopwatch(stopwatchId);
        }

        private string SecondsToReadableFormat(long total, string delimiter, bool secondsOnNewLine)
        {
            long minutes, seconds, hours;
            minutes = total / 60;
            seconds = total % 60;
            hours = minutes / 60;
            minutes %= 60;

            return $"{hours.ToString("00")}{delimiter}{minutes.ToString("00")}{(secondsOnNewLine ? "\n" : delimiter)}{seconds.ToString("00")}";
        }

        private async void TmrOnTick_Elapsed(object sender, ElapsedEventArgs e)
        {
            string delimiter = settings.Multiline ? "\n" : ":";

            // Stream Deck calls this function every second, 
            // so this is the best place to determine if we need to reset (versus the internal timer which may be paused)
            CheckIfResetNeeded();

            long total = StopwatchManager.Instance.GetStopwatchTime(stopwatchId);
            await Connection.SetTitleAsync(SecondsToReadableFormat(total, delimiter, true));
            await Connection.SetImageAsync(StopwatchManager.Instance.IsStopwatchEnabled(stopwatchId) ? enabledImage : pauseImage);
        }
        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private void InitializeSettings()
        {
            if (String.IsNullOrWhiteSpace(settings.SharedId))
            {
                stopwatchId = Connection.ContextId;
            }
            else
            {
                stopwatchId = settings.SharedId;
            }

            PrefetchImages();
        }

        private void Connection_OnSendToPlugin(object sender, BarRaider.SdTools.Wrappers.SDEventReceivedEventArgs<BarRaider.SdTools.Events.SendToPlugin> e)
        {
            var payload = e.Event.Payload;

            Logger.Instance.LogMessage(TracingLevel.INFO, "OnSendToPlugin called");
            if (payload["property_inspector"] != null)
            {
                switch (payload["property_inspector"].ToString().ToLowerInvariant())
                {
                    case "loadsavepicker":
                        string propertyName = (string)payload["property_name"];
                        string pickerTitle = (string)payload["picker_title"];
                        string pickerFilter = (string)payload["picker_filter"];
                        string fileName = PickersUtil.Pickers.SaveFilePicker(pickerTitle, null, pickerFilter);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            if (!PickersUtil.Pickers.SetJsonPropertyValue(settings, propertyName, fileName))
                            {
                                Logger.Instance.LogMessage(TracingLevel.ERROR, "Failed to save picker value to settings");
                            }
                            SaveSettings();
                        }
                        break;
                }
            }
        }

        private void PrefetchImages()
        {
            if (pauseImage != null)
            {
                pauseImage.Dispose();
                pauseImage = null;
            }
            pauseImage = TryLoadCustomImage(settings.PauseImageFile, DEFAULT_IMAGES[0]);

            if (enabledImage != null)
            {
                enabledImage.Dispose();
                enabledImage = null;
            }
            enabledImage = TryLoadCustomImage(settings.EnabledImageFile, DEFAULT_IMAGES[1]);
        }

        private Image TryLoadCustomImage(string customImageFileName, string defaultImageFileName)
        {
            if (String.IsNullOrEmpty(customImageFileName))
            {
                return Image.FromFile(defaultImageFileName);
            }
            else if (!File.Exists(customImageFileName))
            {

                Logger.Instance.LogMessage(TracingLevel.WARN, $"{this.GetType()} Custom image not found: {customImageFileName}");
                return Image.FromFile(defaultImageFileName);
            }
            else
            {
                return Image.FromFile(customImageFileName);
            }
        }

        private void HandleStopwatchResume()
        {
            if (!settings.ResumeOnClick)
            {
                ResetCounter();
            }

            ResumeStopwatch();
        }

        private void HandleMultiActionKeyPress(uint state)
        {
            switch (state) // 0 = Toggle, 1 = Start, 2 = Stop
            {
                case 0:
                    if (StopwatchManager.Instance.IsStopwatchEnabled(stopwatchId))
                    {
                        PauseStopwatch();
                    }
                    else
                    {
                        HandleStopwatchResume();
                    }
                    break;
                case 1:
                    if (!StopwatchManager.Instance.IsStopwatchEnabled(stopwatchId))
                    {
                        HandleStopwatchResume();
                    }
                    break;
                case 2:
                    if (StopwatchManager.Instance.IsStopwatchEnabled(stopwatchId))
                    {
                        PauseStopwatch();
                    }
                    break;
                default:
                    Logger.Instance.LogMessage(TracingLevel.ERROR, $"{this.GetType()} HandleMultiActionKeyPress: Unsupported state {state}");
                    break;
            }
        }
        #endregion
    }
}
