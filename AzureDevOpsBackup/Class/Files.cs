using System;
using System.IO;
using System.Linq;
using System.Threading;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    internal class Files
    {
        public static string LogFilePath
        {
            get
            {
                // Root folder for log files
                var logfilePathvar = ProgramDataFilePath + @"\Log";
                return logfilePathvar;
            }
        }

        public static string ProgramDataFilePath
        {
            get
            {
                // Root path for program data
                var currentDirectory = Directory.GetCurrentDirectory();
                var programDataFilePathvar = currentDirectory;
                return programDataFilePathvar;
            }
        }

        public static string TokenFilePath
        {
            get
            {
                // Root folder for log files
                var tokenFilePathvar = ProgramDataFilePath + @"\token.bin";
                return tokenFilePathvar;
            }
        }

        // If args is set to delete original downloaded .zip and .json files
        // Get output folder from backup with date for folder to backup to
        public static void DeleteZipAndJson(string outDir)
        {
            // Find files to delete if needed - deleting downloaded .zip and .json files when unzipped
            string[] fileExtensions = new[] { ".zip", ".json" };
            DirectoryInfo di = new DirectoryInfo(outDir);
            FileInfo[] files = di.GetFiles().Where(p => fileExtensions.Contains(p.Extension)).ToArray();

            // Set wait
            Thread.Sleep(3000);

            // Do work
            try
            {
                foreach (var file in files)
                {
                    // Try to delete file(s)
                    DeleteFileAndWait(file.FullName, outDir);
                }
                //break; // When done we can break loop
            }
            catch (Exception ex)
            {
                // Log
                Message("Error: " + ex, EventType.Error, 1001);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex);
                Console.ResetColor();
            }

            // Check for letovers if any
            if (Globals._checkForLeftoverFilesAfterCleanup)
            {
                int numZip = di.GetFiles("*.zip").Length;
                int numJson = di.GetFiles("*.json").Length;

                Globals._numJson = numJson;
                Globals._numZip = numZip;
            }

            // Delete downloaded files after unzip
            Globals._deletedFilesAfterUnzip = true;
        }

        public static void DeleteFileAndWait(string filepath, string outDir, int timeout = 30000)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var fw = new FileSystemWatcher(Path.GetDirectoryName(filepath), Path.GetFileName(filepath)))
            using (var mre = new ManualResetEventSlim())
            {
                fw.EnableRaisingEvents = true;
                fw.Deleted += (sender, e) =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    mre.Set();
                };

                try
                {
                    // Try to delete the files there is leftover
                    File.Delete(filepath);

                    // Count files
                    Globals._totalFilesIsDeletedAfterUnZipped++;

                    // Log
                    Message("Deleted downloaded file: " + filepath + " in backup folder: " + outDir, EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Deleted downloaded file: " + filepath + " in backup folder: " + outDir);
                    Console.ResetColor();
                }
                catch (UnauthorizedAccessException)
                {
                    Message("Unable to delete downloaded file: " + filepath + " in backup folder: " + outDir + ". Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unable to delete downloaded file: " + filepath + " in backup folder: " + outDir + ". Make sure the account you use to run this tool has write rights to this location.");
                    Console.ResetColor();

                    // Count errors
                    Globals._errors++;
                }
                catch (Exception e)
                {
                    // Error if cant delete file(s)
                    Message(
                        "Exception caught when trying to delete downloaded .zip and .json files in backup folder: " +
                        outDir + " - error: " + e, EventType.Error, 1001);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        "Exception caught when trying to delete downloaded .zip and .json files in backup folder: " +
                        outDir + " - error: " + e);
                    Console.ResetColor();

                    // Add error to counter
                    Globals._errors++;

                    // Check for letovers when error
                    Globals._checkForLeftoverFilesAfterCleanup = true;
                }

                // Wait for work
                mre.Wait(timeout);
            }
        }
    }
}
