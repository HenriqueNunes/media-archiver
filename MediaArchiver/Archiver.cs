using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using MediaArchiver.Reports;


namespace MediaArchiver
{
    public sealed class Archiver
    {
        [Flags]
        public enum ArchiveOptionsEnum
        {
            None = 0,
            SimulateOnly = 1 << 0,
            KeepSourceStructure = 1 << 1,
            UseInboxFolder = 1 << 2
        }


        public string Source
        {
            get;
        }
        public string Destination
        {
            get;
        }
        public List<string> ValidImageExtensions { get; set; }
        public List<string> ValidVideoExtensions { get; set; }


        private Archiver()
        {
            ValidImageExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
            ValidVideoExtensions = new List<string> { ".mov", ".mpeg", ".avi", ".mkv", ".m4v", ".mpg" };
        }

        private Archiver(string source, string destination) : this()
        {
            Source = source;
            Destination = destination;
        }

        public static Archiver CreateArchiver(string source, string destination)
        {
            return new Archiver(source, destination);
        }

        public CountReport CountSourceItems()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var progressReport = new CountReport() { TotalElapsedTime = stopWatch.Elapsed };

            var filesToCheck = Directory.EnumerateFiles(Source, "*.*", SearchOption.AllDirectories)
                                        .Where(fn => !Path.GetFileName(fn).StartsWith(".", StringComparison.InvariantCultureIgnoreCase));

            foreach (var fn in filesToCheck)
            {
                var fInfo = new FileInfo(fn);

                var fileExtension = Path.GetExtension(fn).ToLowerInvariant();
                if (ValidImageExtensions.Contains(fileExtension))
                {
                    progressReport.SignalImageFile(fInfo.Length);
                }
                else if (ValidVideoExtensions.Contains(fileExtension))
                {
                    progressReport.SignalVideoFile(fInfo.Length);
                }
                else
                {
                    progressReport.SignalOtherFile(fInfo.Length, fileExtension);
                }
            }

            stopWatch.Stop();
            progressReport.TotalElapsedTime = stopWatch.Elapsed;
            return progressReport;
        }

        public CountReport CountArchivedItems()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var progressReport = new CountReport() { TotalElapsedTime = stopWatch.Elapsed };

            //progressReport.TotalFiles = Directory.EnumerateFiles(Destination, "*.*", SearchOption.AllDirectories)
            //.Count(fn => !Path.GetFileName(fn).StartsWith(".", StringComparison.InvariantCultureIgnoreCase));

            //var filesToCheck = Directory.EnumerateFiles(Destination, "[arc]*.*", SearchOption.AllDirectories);
            var filesToCheck = Directory.EnumerateFiles(Destination, "*.*", SearchOption.AllDirectories)
                                        .Where(fn => !Path.GetFileName(fn).StartsWith(".", StringComparison.InvariantCultureIgnoreCase));

            foreach (var fn in filesToCheck)
            {
                var fInfo = new FileInfo(fn);

                if (!Path.GetFileName(fn).StartsWith("[arc]", StringComparison.InvariantCultureIgnoreCase))
                {
                    progressReport.SignalNonValidFile(fInfo.Length);
                    continue;
                }

                var fileExtension = Path.GetExtension(fn).ToLowerInvariant();
                if (ValidImageExtensions.Contains(fileExtension))
                {
                    progressReport.SignalImageFile(fInfo.Length);
                }
                else if (ValidVideoExtensions.Contains(fileExtension))
                {
                    progressReport.SignalVideoFile(fInfo.Length);
                }
                else
                {
                    progressReport.SignalOtherFile(fInfo.Length, fileExtension);
                }
            }

            stopWatch.Stop();
            progressReport.TotalElapsedTime = stopWatch.Elapsed;
            return progressReport;
        }

        public ArchiveValidationReport VerifyArchiveHashes(IProgress<ArchiveValidationReport> progress)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var progressReport = new ArchiveValidationReport() { TotalElapsedTime = stopWatch.Elapsed, Successful = true };

            var filesToCheck = Directory.EnumerateFiles(Destination, "[arc]*.*", SearchOption.AllDirectories);

            progressReport.TotalFilesToProcess = filesToCheck.Count();

            foreach (var fn in filesToCheck)
            {
                if (stopWatch.Elapsed - progressReport.TotalElapsedTime > TimeSpan.FromMilliseconds(1000))
                {
                    // send progress notification
                    progressReport.TotalElapsedTime = stopWatch.Elapsed;
                    progress.Report(progressReport);
                }

                var hashOnfileName = Path.GetFileName(fn).Substring("[arc]".Length, 44);
                var calculatedHash = FileProcessor.MakeFileNameSafeHash(FileProcessor.CalculateHash(fn));
                if (calculatedHash != hashOnfileName)
                {
                    progressReport.Successful = false;
                    Logger.Log($"[Critical Event][Hashes do NOT match!] [{fn}][fHash: {hashOnfileName}][calcHash: {calculatedHash}]");
                    //Console.WriteLine($"[Critical Event][Hashes do NOT match!] [{fn}][fHash: {hashOnfileName}][calcHash: {calculatedHash}]");
                }
                progressReport.TotalProcessedFiles++;
            }

            stopWatch.Stop();
            progressReport.TotalElapsedTime = stopWatch.Elapsed;
            return progressReport;
        }

        public ArchiveProgressReport Archive(IProgress<ArchiveProgressReport> progress, ArchiveOptionsEnum options = ArchiveOptionsEnum.SimulateOnly)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var progressReport = new ArchiveProgressReport() { TotalElapsedTime = stopWatch.Elapsed };

            var ptCultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("pt-PT");

            var validExtensions = new List<string>();
            validExtensions.AddRange(ValidImageExtensions);
            validExtensions.AddRange(ValidVideoExtensions);

            progressReport.TotalFilesInSource = Directory.EnumerateFiles(Source, "*.*", SearchOption.AllDirectories)
                                                            .Count(fn => !Path.GetFileName(fn).StartsWith(".", StringComparison.InvariantCultureIgnoreCase));
            progressReport.TotalFilesToProcess = FileProcessor.EnumerateSpecificFiles(Source, validExtensions, SearchOption.AllDirectories).Count();

            var sourceFiles = FileProcessor.EnumerateSpecificFiles(Source, validExtensions, SearchOption.AllDirectories);

            foreach (var srcFile in sourceFiles)
            {
                if (stopWatch.Elapsed - progressReport.TotalElapsedTime > TimeSpan.FromMilliseconds(1000))
                {
                    // send progress notification
                    progressReport.TotalElapsedTime = stopWatch.Elapsed;
                    progress.Report(progressReport);
                }

                var fInfo = new FileInfo(srcFile);
                var srcHash = FileProcessor.CalculateHash(srcFile);
                var srcHashFileSafe = FileProcessor.MakeFileNameSafeHash(srcHash);

                var fileExtension = Path.GetExtension(srcFile).ToLowerInvariant();


                var estimatedFileDate = fInfo.CreationTime.Year <= 1970 ? fInfo.LastWriteTime : fInfo.CreationTime;

                if (estimatedFileDate.Year <= 1980 || (estimatedFileDate > DateTime.Now.AddDays(1)))
                {
                    var ddd = fInfo.LastWriteTime;
                    var zzz = ddd;
                }

                //if (fInfo.CreationTime.Year <= 1980)
                //{
                //    var xx = "";
                //}

                if (ValidImageExtensions.Any(ext => ext.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var dt = GetBestDateForImage(srcFile);
                    if (dt.HasValue) estimatedFileDate = dt.Value;
                }


                var destFile = FindFileInDestination(srcHashFileSafe);
                //if (destFile != null && srcHash != FileProcessor.CalculateHash(destFile.FullName))
                //{
                //    Console.WriteLine($"[Critical Event] [{srcFile}][{srcHash}][{destFile.FullName}] Hashes(filename) match but Hashes(real non file-name-safe) do NOT match!");
                //}

                if (destFile != null)
                {
                    progressReport.SignalFileExcludedDuplicate(fInfo.Length);
                }
                else
                {
                    var isImage = ValidImageExtensions.Contains(fileExtension);
                    var isVideo = ValidVideoExtensions.Contains(fileExtension);

                    var destFolder = $"{Destination}";
                    if (options.HasFlag(ArchiveOptionsEnum.UseInboxFolder)) { destFolder = Path.Combine(destFolder, "inbox"); }
                    if (options.HasFlag(ArchiveOptionsEnum.KeepSourceStructure))
                    {
                        destFolder = Path.Combine(destFolder, Path.GetDirectoryName(Path.GetRelativePath(Source, srcFile)));
                    }
                    else
                    {
                        var mediaType = isImage ? "fotos" : (isVideo ? "videos" : "misc");
                        destFolder = Path.Combine(destFolder, mediaType, estimatedFileDate.Year.ToString(), ptCultureInfo.DateTimeFormat.GetMonthName(estimatedFileDate.Month));
                        //destFolder = $"/{mediaType}/{estimatedFileDate.Year}/{estimatedFileDate.Month.ToString().PadLeft(2, '0')}";
                    }


                    var destfFilename = $"[arc]{srcHashFileSafe}-{fInfo.Name}";

                    if (!options.HasFlag(ArchiveOptionsEnum.SimulateOnly))
                    {
                        if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);
                        File.Copy(srcFile, Path.Combine(destFolder, destfFilename), overwrite: false);
                    }

                    if (isImage) { progressReport.SignalImageFileAdded(fInfo.Length); }
                    else if (isVideo) { progressReport.SignalVideoFileAdded(fInfo.Length); }
                    else { progressReport.SignalOtherFileAdded(fInfo.Length); }
                }
            }

            stopWatch.Stop();
            progressReport.TotalElapsedTime = stopWatch.Elapsed;
            return progressReport;
        }

        private Nullable<DateTime> GetBestDateForImage(string fileName)
        {
            try
            {
                var metadataDirectories = MetadataExtractor.ImageMetadataReader.ReadMetadata(fileName);
                var dtValue = "";
                var subIfdDirectory = metadataDirectories.OfType<MetadataExtractor.Formats.Exif.ExifSubIfdDirectory>().FirstOrDefault();
                if (subIfdDirectory != null) dtValue = subIfdDirectory?.GetDescription(MetadataExtractor.Formats.Exif.ExifDirectoryBase.TagDateTime);
                if (String.IsNullOrWhiteSpace(dtValue))
                {
                    var ifd0Directory = metadataDirectories.OfType<MetadataExtractor.Formats.Exif.ExifIfd0Directory>().FirstOrDefault();
                    if (ifd0Directory != null) dtValue = ifd0Directory?.GetDescription(MetadataExtractor.Formats.Exif.ExifDirectoryBase.TagDateTime);
                }
                if (!String.IsNullOrWhiteSpace(dtValue))
                {
                    var dateParts = dtValue.Substring(0, 10).Split(':', StringSplitOptions.None);
                    var timeParts = dtValue.Substring(11, 8).Split(':', StringSplitOptions.None);
                    var dt = new DateTime(year: int.Parse(dateParts[0]), month: int.Parse(dateParts[1]), day: int.Parse(dateParts[2]), hour: int.Parse(timeParts[0]), minute: int.Parse(timeParts[1]), second: int.Parse(timeParts[2]));
                    if (dt > new DateTime(1980, 01, 01) && dt < DateTime.Now) return dt;
                }
                //var iptcDirectory = metadataDirectories.OfType<MetadataExtractor.Formats.Iptc.IptcDirectory>().FirstOrDefault();
                //if (iptcDirectory != null) dtValue = iptcDirectory?.GetDescription(MetadataExtractor.Formats.Iptc.IptcDirectory.TagDateCreated);
            }
            catch (Exception ex)
            {
                Logger.Log($"[Error][{fileName}] GetBestDateForImage crasched! - {ex.Message}");
                //Console.WriteLine($"[Error][{fileName}] GetBestDateForImage crasched! - {ex.Message}");
            }


            return null;
        }

        private FileInfo FindFileInDestination(string fileHash)
        {
            var destFileName = Directory.EnumerateFiles(Destination, "[arc]*.*", SearchOption.AllDirectories)
                                        .FirstOrDefault(fn => Path.GetFileName(fn).Substring("[arc]".Length, 44) == fileHash);

            if (!String.IsNullOrWhiteSpace(destFileName)) return new FileInfo(destFileName);

            return null;
        }

    }
}