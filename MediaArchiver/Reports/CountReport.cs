using System;
namespace MediaArchiver.Reports
{
    public class CountReport
    {
        public TimeSpan TotalElapsedTime { get; set; }
        public long TotalFiles { get; set; }
        public long TotalValidFiles { get; set; }
        public long TotalImageFiles { get; set; }
        public long TotalVideoFiles { get; set; }
        public long TotalOtherFiles { get; set; }

        public CountReport GetDiffReport(CountReport comparedToThis)
        {
            return new CountReport
            {
                TotalFiles = this.TotalFiles - comparedToThis.TotalFiles,
                TotalValidFiles = this.TotalValidFiles - comparedToThis.TotalValidFiles,
                TotalImageFiles = this.TotalImageFiles - comparedToThis.TotalImageFiles,
                TotalVideoFiles = this.TotalVideoFiles - comparedToThis.TotalVideoFiles,
                TotalOtherFiles = this.TotalOtherFiles - comparedToThis.TotalOtherFiles
            };
        }

        public override string ToString()
        {
            var totalDigitCount = TotalFiles.ToString().Length;
            var msg = $"[arc-count][{TotalElapsedTime.FormatForConsoleReport()}][total: {TotalFiles.ToString("#,###,###,##0")}][valid: {TotalValidFiles.ToString("#,###,###,##0")}]";
            msg += $"[i: {TotalImageFiles.ToString("#,###,###,##0")}][v: {TotalVideoFiles.ToString("#,###,###,##0")}][m: {TotalOtherFiles.ToString("#,###,###,##0")}]";
            return msg;
        }
    }
}
