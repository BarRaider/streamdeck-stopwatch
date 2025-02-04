using BarRaider.SdTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopwatch.Backend
{
    internal static class HelperUtils
    {
        internal const string DEFAULT_TIME_FORMAT = "hh:mm:ss";
        internal static string FormatTime(long timeInMiliseconds, string timeFormat, bool setNewLineDelimiter)
        {
            string delimiter = setNewLineDelimiter ? "\n" : ":";
            var ts = TimeSpan.FromMilliseconds(timeInMiliseconds);

            string output;
            long hours = ts.Days * 24 + ts.Hours;
            long minutes = hours * 60 + ts.Minutes;
            long seconds = minutes * 60 + ts.Seconds;
            long ms = ts.Milliseconds;
            switch (timeFormat)
            {
                case "d.hh":
                    output = (ts.Days > 0) ? $"{ts.Days:0}.{ts.Hours:00}" : $"{ts.Hours:00}";
                    break;
                case "d.h:mm":
                    output = (ts.Days > 0) ? $"{ts.Days:0}.{ts.Hours:00}{delimiter}{ts.Minutes:00}" : $"{ts.Hours:0}{delimiter}{ts.Minutes:00}";
                    break;
                case "d.hh:mm":
                    output = (ts.Days > 0) ? $"{ts.Days:0}.{ts.Hours:00}{delimiter}{ts.Minutes:00}" : $"{ts.Hours:00}{delimiter}{ts.Minutes:00}";
                    break;
                case "hh:mm:ss":
                    output = (hours > 0) ? $"{hours:00}{delimiter}{ts.Minutes:00}{delimiter}{ts.Seconds:00}" : $"{ts.Minutes:00}{delimiter}{ts.Seconds:00}";
                    break;
                case "h:mm:ss":
                    output = (hours > 0) ? $"{hours:0}{delimiter}{ts.Minutes:00}{delimiter}{ts.Seconds:00}" : $"{ts.Minutes:00}{delimiter}{ts.Seconds:00}";
                    break;
                case "hh:mm":
                    output = $"{hours:00}{delimiter}{ts.Minutes:00}";
                    break;
                case "mm:ss":
                    output = $"{minutes:00}{delimiter}{ts.Seconds:00}";
                    break;
                case "mm:ss.ms":
                    output = $"{minutes:00}{delimiter}{ts.Seconds:00}.{ms}";
                    break;
                case "ss":
                    output = $"{seconds:0}";
                    break;
                case "ss.ms":
                    output = $"{seconds:0}.{ms}";
                    break;
                default:
                    Logger.Instance.LogMessage(TracingLevel.WARN, $"FormatTime: Invalid Format {timeFormat}");
                    return null;
            }
            return output;
        }

        internal static void WriteToFile(string fileName, string text)
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
    }
}
