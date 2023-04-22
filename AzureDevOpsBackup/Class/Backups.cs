using System;
using System.IO;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    internal class Backups
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
                DateTime createdTime = new DirectoryInfo(dir).CreationTime;

                // Find folders from days to keep
                if (createdTime < DateTime.Now.AddDays(-days))
                {
                    try
                    {
                        // Do work
                        Folders.DeleteDirectory(dir);

                        // Count files
                        Globals._totalBackupsIsDeleted++;

                        // Log
                        Message("Deleted old backup folder: " + dir + ".", EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Deleted old backup folder: " + dir + ".");
                        Console.ResetColor();

                        // Set state
                        backupsToDelete = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Message("Unable to delete old backup folder: " + dir + ". Make sure the account you use to run this tool has delete rights to this location.", EventType.Error, 1001);
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
                DateTime createdTime = new DirectoryInfo(dir).CreationTime;

                // Find folders from days to keep
                if (createdTime < DateTime.Now.AddDays(-30))
                {
                    try
                    {
                        // Do work
                        Folders.DeleteDirectory(dir);

                        // Count files
                        Globals._totalBackupsIsDeleted++;

                        // Log
                        Message("Deleted old backup folder: " + dir, EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Deleted old backup folder: " + dir);
                        Console.ResetColor();

                        // Set state
                        backupsToDelete = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Message("Unable to delete old backup folder: " + dir + ". Make sure the account you use to run this tool has delete rights to this location.", EventType.Error, 1001);
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
    }
}