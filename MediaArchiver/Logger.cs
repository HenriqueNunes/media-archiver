using System;
using System.IO;

namespace MediaArchiver
{
    public static class Logger
    {
        private static string _logIntanceFileName = null;

        private static void Initialize()
        {
            if (_logIntanceFileName == null)
            {
                var dt = DateTime.Now;
                _logIntanceFileName = $"execution-logs/[arc]-[{dt.ToString("yyyy-MM-dd")}]-[{dt.ToString("HHmmss")}].log";
            }
        }

        public static void Log(string message)
        {
            Initialize();
            var dt = DateTime.Now;
            var msgPrefix = $"[{dt.ToString("HH:mm:ss")}]";
            try
            {
                File.AppendAllText(_logIntanceFileName, $"{msgPrefix} {message}{Environment.NewLine}");
            }
            catch (Exception) { msgPrefix += "[!]"; }
            Console.WriteLine($"{msgPrefix} {message}");
        }
    }
}
