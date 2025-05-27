using LibGit2Sharp;
using System;
using System.IO;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    internal class LocalBackupsTasks
    {
        public static void DaysToKeepBackups(string outBackupDir, string daysToKeep)
        {
            // If other then 30 days of backup
            bool backupsToDelete = false;
            string input = daysToKeep;
            int days = int.Parse(input);

            // Log
            Message($"Set to keep {daysToKeep} number of backups (day(s)) in backup folder: {outBackupDir}", EventType.Information, 1000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nSet to keep {daysToKeep} number of backups (day(s)) in backup folder: {outBackupDir}\n");
            Console.ResetColor();

            // Loop folders
            foreach (string dir in Directory.GetDirectories(outBackupDir))
            {
                var createdTime = new DirectoryInfo(dir).CreationTime;

                // Find folders from days to keep
                if (createdTime < DateTime.Now.AddDays(-days))
                {
                    try
                    {
                        // Do work
                        LocalFolderTasks.DeleteDirectory(dir);

                        // Count files
                        Globals._totalBackupsIsDeleted++;

                        // Log
                        Message("> Deleted old backup folder: " + dir + ".", EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Deleted old backup folder: " + dir + ".");
                        Console.ResetColor();

                        // Set state
                        backupsToDelete = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Message("! Unable to delete old backup folder: " + dir + ". Make sure the account you use to run this tool has delete rights to this location.", EventType.Error, 1001);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unable to delete old backup folder: " + dir + ". Make sure the account you use to run this tool has delete rights to this location.");
                        Console.ResetColor();

                        // Count errors
                        Globals._errors++;
                    }
                    catch (Exception e)
                    {
                        // Error if cant delete file(s)
                        Message("Exception caught when trying to delete old backup folder: " + dir + " - error: " + e, EventType.Error, 1001);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Exception caught when trying to delete old backup folder: " + dir + " - error: " + e);
                        Console.ResetColor();

                        // Add error to counter
                        Globals._errors++;
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            // If no backups to delete
            if (backupsToDelete == false)
            {
                // Log
                Message("No old backups needed to be deleted form backup folder: " + outBackupDir, EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("No old backups needed to be deleted form backup folder: " + outBackupDir);
                Console.ResetColor();
            }
        }

        public static void DaysToKeepBackupsDefault(string outBackupDir)
        {
            // If default 30 days of backup
            bool backupsToDelete = false;

            // Loop in folder
            foreach (string dir in Directory.GetDirectories(outBackupDir))
            {
                var createdTime = new DirectoryInfo(dir).CreationTime;

                // Find folders from days to keep
                if (createdTime < DateTime.Now.AddDays(-30))
                {
                    try
                    {
                        // Do work
                        LocalFolderTasks.DeleteDirectory(dir);

                        // Count files
                        Globals._totalBackupsIsDeleted++;

                        // Log
                        Message("> Deleted old backup folder: " + dir, EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Deleted old backup folder: " + dir);
                        Console.ResetColor();

                        // Set state
                        backupsToDelete = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Message("! Unable to delete old backup folder: " + dir + ". Make sure the account you use to run this tool has delete rights to this location.", EventType.Error, 1001);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unable to delete old backup folder: " + dir + ". Make sure the account you use to run this tool has delete rights to this location.");
                        Console.ResetColor();

                        // Count errors
                        Globals._errors++;
                    }
                    catch (Exception e)
                    {
                        // Error if cant delete file(s)
                        Message("Exception caught when trying to delete old backup folder: " + dir + " - error: " + e, EventType.Error, 1001);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Exception caught when trying to delete old backup folder: " + dir + " - error: " + e);
                        Console.ResetColor();

                        // Add error to counter
                        Globals._errors++;
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            // If no backups to delete
            if (backupsToDelete == false)
            {
                // Log
                Message("No old backups (default 30 days) needed to be deleted from backup folder: " + outBackupDir, EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("No old backups (default 30 days) needed to be deleted from backup folder: " + outBackupDir + "\n");
                Console.ResetColor();
            }
        }

        public static void CountCurrentNumersOfBackup(string outBackupDir)
        {
            //Count backups in folder
            string searchPattern = "??-??-????-(??-??)";
            int folderCount = 0;
            foreach (var directory in Directory.GetDirectories(outBackupDir, searchPattern, SearchOption.TopDirectoryOnly))
            {
                if (Directory.GetFileSystemEntries(directory).Length > 0)
                {
                    folderCount++;
                }
            }

            // Save count
            Globals._currentBackupsInBackupFolderCount = folderCount;
        }

        public static void BackupRepositoryWithGit(string repoUrl, string backupDir, string pat)
        {
            // Log the start of the backup process
            Console.WriteLine($"[GIT] Starting git backup for repository: '{repoUrl}'");
            Console.WriteLine($"[GIT] Target backup directory: '{backupDir}\\'");
            Message($"[GIT] Starting git backup for repository: '{repoUrl}'", EventType.Information, 1000);
            Message($"[GIT] Target backup directory: '{backupDir}\\'", EventType.Information, 1000);

            var cloneOptions = new CloneOptions();

            // Configure the existing FetchOptions
            cloneOptions.FetchOptions.CredentialsProvider = (url, user, cred) =>
                new UsernamePasswordCredentials
                {
                    Username = "pat", // Use "pat" for Azure DevOps
                    Password = pat
                };

            try
            {
                // Ensure the backup directory exists
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Clone the repository
                Repository.Clone(repoUrl, backupDir, cloneOptions);

                // Log success
                Console.WriteLine($"[GIT] Successfully backed up repository: '{repoUrl}'");
                Console.WriteLine($"[GIT] Backup stored at: '{backupDir}\\'");
                Message($"[GIT] Successfully backed up repository: '{repoUrl}'", EventType.Information, 1000);
                Message($"[GIT] Backup stored at: '{backupDir}\\'", EventType.Information, 1000);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"[GIT] Failed to backup repository: '{repoUrl}'");
                Console.WriteLine($"[GIT ERROR] {ex.Message}");
                Message($"[GIT] Failed to backup repository: '{repoUrl}'", EventType.Error, 1001);
                Message($"[GIT ERROR] {ex.Message}", EventType.Error, 1001);
            }
        }
    }
}