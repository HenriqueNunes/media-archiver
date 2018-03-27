using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaArchiver.Reports
{
    public class CountReport
    {
        public TimeSpan TotalElapsedTime { get; set; }

        public long TotalFiles { get; private set; } = 0;
        public long TotalValidFiles { get; private set; } = 0;
        public long TotalImageFiles { get; private set; } = 0;
        public long TotalVideoFiles { get; private set; } = 0;
        public long TotalOtherFiles { get; private set; } = 0;

        public long TotalBytes { get; private set; } = 0;
        public long TotalValidBytes { get; private set; } = 0;
        public long TotalVideoBytes { get; private set; } = 0;
        public long TotalImageBytes { get; private set; } = 0;
        public long TotalOtherBytes { get; private set; } = 0;

        public List<string> OtherExtensions { get; private set; } = new List<string>();



        private void SignalFile(long fileSizeInBytes)
        {
            TotalFiles++;
            TotalBytes += fileSizeInBytes;
        }
        private void SignalValidFile(long fileSizeInBytes)
        {
            SignalFile(fileSizeInBytes);
            TotalValidFiles++;
            TotalValidBytes += fileSizeInBytes;
        }

        public void SignalImageFile(long fileSizeInBytes)
        {
            SignalValidFile(fileSizeInBytes);
            TotalImageFiles++;
            TotalImageBytes += fileSizeInBytes;
        }
        public void SignalVideoFile(long fileSizeInBytes)
        {
            SignalValidFile(fileSizeInBytes);
            TotalVideoFiles++;
            TotalVideoBytes += fileSizeInBytes;
        }
        public void SignalOtherFile(long fileSizeInBytes, string fileExtension)
        {
            SignalValidFile(fileSizeInBytes);
            TotalOtherFiles++;
            TotalOtherBytes += fileSizeInBytes;
            fileExtension = fileExtension.ToLowerInvariant();
            if (!OtherExtensions.Contains(fileExtension)) OtherExtensions.Add(fileExtension);
        }
        public void SignalNonValidFile(long fileSizeInBytes)
        {
            SignalFile(fileSizeInBytes);
        }

        public CountReport GetDiffReport(CountReport comparedToThis)
        {
            return new CountReport
            {
                TotalFiles = this.TotalFiles - comparedToThis.TotalFiles,
                TotalValidFiles = this.TotalValidFiles - comparedToThis.TotalValidFiles,
                TotalImageFiles = this.TotalImageFiles - comparedToThis.TotalImageFiles,
                TotalVideoFiles = this.TotalVideoFiles - comparedToThis.TotalVideoFiles,
                TotalOtherFiles = this.TotalOtherFiles - comparedToThis.TotalOtherFiles,
                TotalBytes = this.TotalBytes - comparedToThis.TotalBytes,
                TotalValidBytes = this.TotalValidBytes - comparedToThis.TotalValidBytes,
                TotalImageBytes = this.TotalImageBytes - comparedToThis.TotalImageBytes,
                TotalVideoBytes = this.TotalVideoBytes - comparedToThis.TotalVideoBytes,
                TotalOtherBytes = this.TotalOtherBytes - comparedToThis.TotalOtherBytes,
                TotalElapsedTime = this.TotalElapsedTime - comparedToThis.TotalElapsedTime
            };
        }

        public override string ToString()
        {
            var totalDigitCount = TotalFiles.ToString().Length;
            var msg = $"[arc-count][{TotalElapsedTime.FormatForConsoleReport()}]";
            msg += $" [total: {TotalFiles.ToString("#,###,###,##0")} ({(TotalBytes / (1024 * 1024)).ToString("#,###,###,##0")} mb)]";
            msg += $" [t-valid: {TotalValidFiles.ToString("#,###,###,##0")} ({(TotalValidBytes / (1024 * 1024)).ToString("#,###,###,##0")} mb)]";
            msg += $" [i: {TotalImageFiles.ToString("#,###,###,##0")} ({(TotalImageBytes / (1024 * 1024)).ToString("#,###,###,##0")} mb)]";
            msg += $" [v: {TotalVideoFiles.ToString("#,###,###,##0")} ({(TotalVideoBytes / (1024 * 1024)).ToString("#,###,###,##0")} mb)]";
            msg += $" [o: {TotalOtherFiles.ToString("#,###,###,##0")} ({(TotalOtherBytes / (1024 * 1024)).ToString("#,###,###,##0")} mb)]";
            msg += $" [o-ext:{OtherExtensions.Aggregate("", (acc, next) => acc + "  " + next)}]";
            return msg;
        }
    }
}
