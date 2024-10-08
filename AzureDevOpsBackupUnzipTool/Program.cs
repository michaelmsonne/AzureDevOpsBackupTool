﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using AzureDevOpsBackupUnzipTool.Class;
using Newtonsoft.Json;
using static AzureDevOpsBackupUnzipTool.Class.FileLogger;

namespace AzureDevOpsBackupUnzipTool
{
    public class RootObject
    {
        public int Count { get; set; }
        public List<Item> Value { get; set; }
    }

    public class Item
    {
        public string ObjectId { get; set; }
        public string GitObjectType { get; set; }
        public string CommitId { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Get application information
            ApplicationInfo.GetExeInfo();

            // Log start of program to log
            ApplicationStatus.ApplicationStartMessage();

            // Set Global Logfile properties for log
            DateFormat = "dd-MM-yyyy";
            DateTimeFormat = "dd-MM-yyyy HH:mm:ss";
            WriteOnlyErrorsToEventLog = false;
            WriteToEventLog = false;
            WriteToFile = true;

            // Create log folder if not exist
            LocalFolderTasks.CreateLogFolder();
            
            // Check if parameters have been provided and contains one of
            if (args.Length == 0 || args.Contains("--help") || args.Contains("/h") || args.Contains("/?") || args.Contains("/info") || args.Contains("/about"))
            {
                // If none arguments
                if (args.Length == 0)
                {
                    // No arguments have been provided
                    Message("ERROR: No arguments is provided - try again!", EventType.Error, 1001);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR: No arguments is provided - try again!\n");
                    Console.ResetColor();

                    // Show help to console
                    DisplayHelpToConsole.DisplayGuide();

                    // Log
                    Message($"Showed help to Console - Exciting {Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName + "!", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Showed help to Console - Exciting {Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName + "!\n");
                    Console.ResetColor();

                    // End application
                    Environment.Exit(1);
                }

                // If wants help
                if (args.Contains("--help") || args.Contains("/h") || args.Contains("/?"))
                {
                    // Show help to console
                    DisplayHelpToConsole.DisplayGuide();

                    // Log
                    Message($"Showed help to Console - Exciting {Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName + "!", EventType.Information, 1000);

                    // Reset color
                    Console.ResetColor();

                    // End application
                    Environment.Exit(1);
                }

                // If wants information about application
                if (args.Contains("/info") || args.Contains("/about"))
                {
                    // Show information about application to console
                    DisplayHelpToConsole.DisplayInfo();

                    // Log
                    Message($"Showed information about application to Console - Exciting {Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName + "!", EventType.Information, 1000);

                    // Reset color
                    Console.ResetColor();

                    // End application
                    Environment.Exit(1);
                }
            }

            // Cleanup old log files
            LocalLogCleanup.CleanupLogs();

            // Check for required Args for application will work
            string[] requiredArgs = { "--zipFile", "--jsonFile", "--output" };

            foreach (var requiredArg in requiredArgs)
            {
                if (!args.Contains(requiredArg))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: Missing required argument '{requiredArg}'");
                    Console.ResetColor();
                    return;
                }
            }

            // Check if the required arguments are provided
            if (args.Length < 3)
            {
                // Show help to console
                DisplayHelpToConsole.DisplayGuide();
                return;
            }

            // Get the required arguments from the command line arguments and save them to variables
            string zipFilePath = args[Array.IndexOf(args, "--zipFile") + 1];
            string jsonFilePath = args[Array.IndexOf(args, "--jsonFile") + 1];
            string outputDirectory = args[Array.IndexOf(args, "--output") + 1];
            
            //Try to unzip the project form the zip file and metadata file
            try
            {
                // Do
                UnzipProject(zipFilePath, outputDirectory, jsonFilePath);
            }
            catch (Exception ex)
            {
                // Log
                Message($"An error occurred: {ex.Message}", EventType.Error, 1001);
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // Log end of program to console
            ApplicationStatus.ApplicationEndMessage();
        }

        private static void UnzipProject(string zipFilePath, string outputDirectory, string jsonFilePath)
        {
            // Log start of the unzipping process
            Message("Starting the unzipping process...", EventType.Information, 1000);
            Console.WriteLine("Starting the unzipping process...");

            // Read JSON data
            string jsonData = File.ReadAllText(jsonFilePath);
            var rootObject = JsonConvert.DeserializeObject<RootObject>(jsonData);
            var items = rootObject.Value;

            // Log the number of items to be processed
            Message($"Number of items to process: {items.Count}", EventType.Information, 1000);
            Console.WriteLine($"Number of items to process: {items.Count}");

            // Initialize a counter for processed items
            int processedItemCount = 0;

            // Open the zip archive
            using (var archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (var item in items)
                {
                    // Increment the counter
                    processedItemCount++;

                    // Get the destination path
                    string destinationPath = Path.GetFullPath(Path.Combine(outputDirectory, item.Path.TrimStart('/')));

                    // Log the item being processed with the counter
                    Message($"Processing item {processedItemCount}/{items.Count}: {item.Path} (Type: {item.GitObjectType})", EventType.Information, 1000);
                    Console.WriteLine($"Processing item {processedItemCount}/{items.Count}: {item.Path} (Type: {item.GitObjectType})");

                    // Check if the item is a folder or a file
                    if (item.GitObjectType == "tree")
                    {
                        // If folder data
                        Console.WriteLine($"Unzipping Git repository folder data: '{destinationPath}'...");
                        Message($"Unzipping Git repository folder data: '{destinationPath}'...", EventType.Information, 1000);

                        try
                        {
                            // Create backup folder if not exist
                            Directory.CreateDirectory(destinationPath);

                            // Log
                            Message("Output folder is created: '" + destinationPath + "'", EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Output folder is created: '" + destinationPath + "'");
                            Console.ResetColor();
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Log
                            Message("! Unable to create folder to store the backups: '" + destinationPath + "'. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Unable to create folder to store the backups: '" + destinationPath + "'. Make sure the account you use to run this tool has write rights to this location.");
                            Console.ResetColor();
                        }
                        catch (Exception e)
                        {
                            // Error when create backup folder
                            Message("Exception caught when trying to create output folder - error: " + e, EventType.Error, 1001);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("{0} Exception caught.", e);
                            Console.ResetColor();
                        }
                    }
                    else if (item.GitObjectType == "blob")
                    {
                        // If file data
                        Console.WriteLine($"Unzipping Git repository file data on disk: '{destinationPath}'...");
                        Message($"Unzipping Git repository file data on disk: '{destinationPath}'...", EventType.Information, 1000);

                        // Extract the file
                        var entry = archive.GetEntry(item.ObjectId);

                        // Check if the entry is not null
                        if (entry != null)
                        {
                            try
                            {
                                entry.ExtractToFile(destinationPath, true);

                                // Log
                                Message($"Unzipped Git repository file data on disk: '{destinationPath}'", EventType.Information, 1000);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Unzipped Git repository file data on disk: '{destinationPath}'");
                                Console.ResetColor();
                            }
                            catch (UnauthorizedAccessException)
                            {
                                // Log
                                Message("! Unable to create folder to store the backups: '" + destinationPath + "'. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Unable to create folder to store the backups: '" + destinationPath + "'. Make sure the account you use to run this tool has write rights to this location.");
                                Console.ResetColor();
                            }
                            catch (Exception e)
                            {
                                // Error when create backup folder
                                Message("Exception caught when trying to create output folder - error: " + e, EventType.Error, 1001);
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("{0} Exception caught.", e);
                                Console.ResetColor();
                            }
                        }
                        // If the entry is null
                        else
                        {
                            // Log
                            Message($"Entry with ObjectId '{item.ObjectId}' not found in the zip archive.", EventType.Error, 1001);
                            Console.WriteLine($"Entry with ObjectId '{item.ObjectId}' not found in the zip archive.");
                        }
                    }
                }
            }

            // Log end of the unzipping process
            Message("Unzipping process completed.", EventType.Information, 1000);
            Console.WriteLine("Unzipping process completed.");
        }
    }
}