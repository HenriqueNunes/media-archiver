using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MediaArchiver
{
    class Program
    {

        private enum ExecutionTaskEnum
        {
            None,
            CountArchivedItems,
            CountSourceItems,
            VerifyArchiveHashes,
            Archive
        }


        static void Main(string[] args)
        {

            //var abort = true;
            //if (abort) return;

            var task = ExecutionTaskEnum.Archive;
            //var sourceRootFolder = @"/users/hnunes/downloads/[NO.BACKUPS!]/TP001/test-source";
            //var destinationRootFolder = @"/users/hnunes/downloads/[NO.BACKUPS!]/TP001/test-destination";


            //var sourceRootFolder = @"/users/hnunes/downloads/[NO.BACKUPS!]/100CANON";
            //var sourceRootFolder = @"/Volumes/SAMSUNG 1TB/Teste";


            //***************************

            //var sourceRootFolder = @"/Volumes/SAMSUNG 1TB/Fotos.Tmp/100CANON-tmp";
            //var sourceRootFolder = @"/Volumes/SAMSUNG 1TB/Fotos.100CANON";
            //var sourceRootFolder = @"/Volumes/SAMSUNG 1TB/[Fotos]/[Fotos].AXX.Fotos/Fotografias";
            //var sourceRootFolder = @"/Volumes/SAMSUNG 1TB/[Fotos]/[Fotos].AXX.Fotos/Fotos";
            //var sourceRootFolder = @"/Volumes/SAMSUNG 1TB/[Fotos]/[Fotos].AXX.Fotos/F.Movies";
            //var sourceRootFolder = @"/Volumes/SAMSUNG 1TB/[Fotos]";
            //var sourceRootFolder = @"/Volumes/SAMSUNG 1TB/Fotos.Tmp";
            //var sourceRootFolder = @"/Volumes/SAMSUNG 1TB/Teste";
            //var sourceRootFolder = @"/Volumes/Casa-001/Fotos.Tmp";
            //var sourceRootFolder = @"/Volumes/Casa-001/[Fotos]";

            var sourceRootFolder = @"/Volumes/Casa-001/Fotos.100CANON";
            var destinationRootFolder = @"/Volumes/ex-PS4/[archive]";
            //[Fotos].AXX.Fotos

            var nonArcFiles = Directory.EnumerateFiles(destinationRootFolder, "*.*", SearchOption.AllDirectories).Where(fn => !Path.GetFileName(fn).StartsWith(".", StringComparison.InvariantCultureIgnoreCase));
            foreach (var item in nonArcFiles)
            {
                if (item.Contains("/[arc]")) continue;
                Console.WriteLine(item);
            }

            Archiver.ArchiveOptionsEnum options = Archiver.ArchiveOptionsEnum.None;

            var archiver = Archiver.CreateArchiver(source: sourceRootFolder, destination: destinationRootFolder);

            Logger.Log($"[source: {archiver.Source}]");
            Logger.Log($"[destination: {archiver.Destination}]");
            Logger.Log($"[task: {task.ToString()}] [options: {options.ToString()}]");
            switch (task)
            {
                case ExecutionTaskEnum.CountSourceItems:
                    var countSourceReport = archiver.CountSourceItems();
                    Logger.Log(countSourceReport.ToString());
                    break;
                case ExecutionTaskEnum.CountArchivedItems:
                    var countArchiverReport = archiver.CountArchivedItems();
                    Logger.Log(countArchiverReport.ToString());
                    break;
                case ExecutionTaskEnum.VerifyArchiveHashes:
                    var progressNotifierForVerify = new Progress<Reports.ArchiveValidationReport>((value) => { Logger.Log(value.ToString()); });
                    var archiverReportForVerify = archiver.VerifyArchiveHashes(progressNotifierForVerify);
                    Logger.Log(archiverReportForVerify.ToString());
                    break;
                case ExecutionTaskEnum.Archive:
                    if (options.HasFlag(Archiver.ArchiveOptionsEnum.SimulateOnly)) Logger.Log("[Simulating...]");
                    var beforeCountArchiverReport = archiver.CountArchivedItems();
                    Logger.Log("[before]" + beforeCountArchiverReport.ToString());

                    var progressNotifier = new Progress<Reports.ArchiveProgressReport>((value) => { Logger.Log(value.ToString()); });
                    var archiverReport = archiver.Archive(progressNotifier, options);
                    Logger.Log(archiverReport.ToString());

                    var afterCountArchiverReport = archiver.CountArchivedItems();
                    Logger.Log("[after] " + afterCountArchiverReport.ToString());
                    Logger.Log("[diff]  " + afterCountArchiverReport.GetDiffReport(beforeCountArchiverReport).ToString());
                    if (options.HasFlag(Archiver.ArchiveOptionsEnum.SimulateOnly)) Logger.Log("[Simulation done!]");
                    break;
                default:
                    break;
            }

            //progressNotifier.ProgressChanged += (_, value) => { };




            //var archiverTask =  archiver.AchiveAsync(progressNotifier);
            //Task.WaitAll(archiverTask);
            Logger.Log("[Done!]");

        }

    }
}
