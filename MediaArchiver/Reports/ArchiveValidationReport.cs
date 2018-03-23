using System;
namespace MediaArchiver.Reports
{
    public class ArchiveValidationReport
    {
        public bool Successful { get; set; }
        public TimeSpan TotalElapsedTime { get; set; }
        public int TotalFilesToProcess { get; set; }
        public int TotalProcessedFiles { get; set; }

        public override string ToString()
        {
            var totalDigitCount = TotalFilesToProcess.ToString().Length;
            var perComplete = (double)TotalProcessedFiles / TotalFilesToProcess * 100;
            var avg = TotalElapsedTime.TotalMilliseconds / TotalProcessedFiles;
            var msg = $"[{perComplete.ToString("##0.0").PadLeft(6, ' ')} %][{TotalProcessedFiles.ToString().PadLeft(totalDigitCount, ' ')} of {TotalFilesToProcess}][ {TotalElapsedTime.TotalSeconds.ToString("0.0").PadLeft(4, ' ')} s][avg:{avg.ToString("0")} ms/p][ok: {Successful}]";
            return msg;
        }
    }
}
