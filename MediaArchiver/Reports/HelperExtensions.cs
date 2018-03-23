using System;
namespace MediaArchiver.Reports
{
    public static class HelperExtensions
    {
        public static string FormatForConsoleReport(this TimeSpan value)
        {
            if (value.TotalMinutes> 2){
                return value.TotalMinutes.ToString("0.0 min");
            }
            if (value.TotalSeconds > 10){
                return value.TotalSeconds.ToString("0.0 sec");
            }
            return value.TotalMilliseconds.ToString("0.0 ms");
        }
    }
}
