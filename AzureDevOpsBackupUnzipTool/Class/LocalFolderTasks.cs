using System;
using System.IO;
using System.Linq;
using static AzureDevOpsBackupUnzipTool.Class.FileLogger;

namespace AzureDevOpsBackupUnzipTool.Class
{
    internal class LocalFolderTasks
    {
        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }
            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

        public static void CreateLogFolder()
        {
            // If logfile path not exist, create it
            if (!Directory.Exists(Files.LogFilePath))
            {
                try
                {
                    // Create log folder if not exist
                    Directory.CreateDirectory(Files.LogFilePath);

                    // Log
                    Message("Log folder is created: '" + Files.LogFilePath + "'", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Log folder is created: '" + Files.LogFilePath + "'");
                    Console.ResetColor();
                }
                catch (UnauthorizedAccessException)
                {
                    // Log
                    Message("! Unable to create folder to store the logs: '" + Files.LogFilePath + "'. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unable to create folder to store the logs: '" + Files.LogFilePath + "'. Make sure the account you use to run this tool has write rights to this location.");
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    // Error when create logs folder
                    Message("Exception caught when trying to create logs folder - error: " + e, EventType.Error, 1001);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} Exception caught.", e);
                    Console.ResetColor();
                }
            }
        }

        // Function to sanitize directory name
        public static string SanitizeDirectoryName(string directoryName)
        {
            // Remove any potentially dangerous characters from the directory name
            return Path.GetInvalidPathChars().Aggregate(directoryName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}