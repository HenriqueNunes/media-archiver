using System;
namespace MediaArchiver.Reports
{
    public class ArchiveProgressReport
    {

        public int TotalFilesInSource { get; set; }
        public int TotalFilesToProcess { get; set; }
        public int TotalProcessedFiles { get; set; }
        public int TotalFilesAdded { get; set; }
        public int TotalExcludedDuplicateFiles { get; set; }

        public int TotalImageFilesAdded { get; set; }
        public int TotalVideoFilesAdded { get; set; }
        public int TotalOtherFilesAdded { get; set; }

        public long TotalBytesAdded { get; set; }
        public long TotalVideoBytesAdded { get; set; }
        public long TotalImageBytesAdded { get; set; }
        public long TotalOtherBytesAdded { get; set; }
        public long TotalBytesExcluded { get; set; }

        public TimeSpan TotalElapsedTime { get; set; }


        public ArchiveProgressReport() { }

        private void SignalFileAdded(long fileSizeInBytes)
        {
            TotalProcessedFiles += 1;
            TotalFilesAdded += 1;
            TotalBytesAdded += fileSizeInBytes;
        }
        public void SignalImageFileAdded(long fileSizeInBytes)
        {
            TotalImageFilesAdded += 1;
            TotalImageBytesAdded += fileSizeInBytes;
            SignalFileAdded(fileSizeInBytes);
        }
        public void SignalVideoFileAdded(long fileSizeInBytes)
        {
            TotalVideoFilesAdded += 1;
            TotalVideoBytesAdded += fileSizeInBytes;
            SignalFileAdded(fileSizeInBytes);
        }
        public void SignalOtherFileAdded(long fileSizeInBytes)
        {
            TotalOtherFilesAdded += 1;
            TotalOtherBytesAdded += fileSizeInBytes;
            SignalFileAdded(fileSizeInBytes);
        }
        public void SignalFileExcludedDuplicate(long fileSizeInBytes)
        {
            TotalExcludedDuplicateFiles += 1;
            TotalProcessedFiles += 1;
            TotalBytesExcluded += fileSizeInBytes;
        }

        public override string ToString()
        {
            var totalDigitCount = TotalFilesToProcess.ToString().Length;
            var perComplete = (double)TotalProcessedFiles / TotalFilesToProcess * 100;
            var avg = TotalProcessedFiles <= 0 ? 0 : TotalElapsedTime.TotalMilliseconds / TotalProcessedFiles;
            var msg = $"[{perComplete.ToString("##0.0").PadLeft(6, ' ')} %] [{TotalElapsedTime.TotalSeconds.ToString("0.0").PadLeft(4, ' ')} s ({TotalElapsedTime.FormatForConsoleReport()})]";
            msg += $" [avg: {avg.ToString("0")} ms/i]";
            msg += $" [{TotalProcessedFiles.ToString().PadLeft(totalDigitCount, ' ')} of {TotalFilesToProcess}]";
            msg += $" [arc: {TotalFilesAdded.ToString("#,###,###,##0")} ({(TotalBytesAdded / (1024 * 1024)).ToString("#,###,###,##0")} mb)]";
            msg += $" [i: {TotalImageFilesAdded.ToString("#,###,###,##0")} ({(TotalImageBytesAdded / (1024 * 1024)).ToString("#,###,###,##0")} mb)]";
            msg += $" [v: {TotalVideoFilesAdded.ToString("#,###,###,##0")} ({(TotalVideoBytesAdded / (1024 * 1024)).ToString("#,###,###,##0")} mb)]";
            msg += $" [oth: {TotalOtherFilesAdded.ToString("#,###,###,##0")} ({(TotalOtherBytesAdded / (1024 * 1024)).ToString("#,###,###,##0")} mb]";
            msg += $" [excl: {TotalExcludedDuplicateFiles.ToString("#,###,###,##0")} ({(TotalBytesExcluded / (1024 * 1024)).ToString("#,###,###,##0")} mb)]";

            return msg;
        }

    }

}
