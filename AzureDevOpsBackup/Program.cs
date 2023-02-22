using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Diagnostics;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using RestSharp;
using static AzureDevOpsBackup.FileLogger;
// ReSharper disable RedundantAssignment
// ReSharper disable NotAccessedVariable

namespace AzureDevOpsBackup
{
    struct Project
    {
        public string Name;
    }
    struct Projects
    {
        public List<Project> Value;
    }
    struct Repo
    {
        public string Id;
        public string Name;
    }
    struct Repos
    {
        public List<Repo> Value;
    }
    struct Item
    {
        public string ObjectId;
        public string GitObjectType;
        public string CommitId;
        public string Path;
        public bool IsFolder;
        public string Url;
    }
    struct Items
    {
        public int Count;
        public List<Item> Value;
    }

    class Program
    {
        static int _totalFilesIsDeletedAfterUnZipped;
        static int _errors;
        static int _numZip;
        static int _numJson;
        static int _totalBackupsIsDeleted;
        static bool _checkForLeftoverFilesAfterCleanup;
        static bool _deletedFilesAfterUnzip;
        private static bool _cleanUpState;

        public static string _currentExeFileName;
        public static string _fileAttachedIneMailReport;
        public static string _vData;
        public static string _companyName;

        static void Main(string[] args)
        {
            // Global variabels
            int projectCount = 0;
            int totalFilesIsBackupUnZipped = 0;
            int totalBlobFilesIsBackup = 0;
            int totalTreeFilesIsBackup = 0;
            int repoItemsCount = 0;
            int repoCount = 0;
            int oldLogFilesToDeleteCount = 0;
            var emailStatusMessage = "";
            string server = null;
            string serverPort = null;
            string emailFrom = null;
            string emailTo = null;
            string elapsedTime = null;
            string daysToKeepBackups = null;
            string repoCountStatusText;
            string repoItemsCountStatusText = "";
            string totalFilesIsBackupUnZippedStatusText;
            string totalBlobFilesIsBackupStatusText;
            string totalTreeFilesIsBackupStatusText;
            string totalFilesIsDeletedAfterUnZippedStatusText;
            string letOverZipFilesStatusText;
            string letOverJsonFilesStatusText;
            string totalBackupsIsDeletedStatusText;
            string isOutputFolderContainFilesStatusText;
            string isDaysToKeepNotDefaultStatusText = null;

            // Set default status for backup job
            bool isBackupOk = false;
            bool isBackupOkAndUnZip = false;
            bool oldLogfilesToDelete = false;
            bool noProjectsToBackup = false;
            bool isOutputFolderContainFiles = false;

            // Get application data to later use in tool
            AssemblyCopyrightAttribute copyright = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute;
            // ReSharper disable once PossibleNullReferenceException
            var copyrightdata = copyright.Copyright;
            _vData = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
            var attributes = typeof(Program).GetTypeInfo().Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute));
            var assemblyTitleAttribute = attributes.SingleOrDefault() as AssemblyTitleAttribute;
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()?.Location);
            _companyName = versionInfo.CompanyName;

            // Start timer for runtime
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Set application name in code
            AppName = assemblyTitleAttribute?.Title;
            _currentExeFileName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName);

            // Start of program
            Message($"Welcome to {AppName}, v." + _vData + " by " + _companyName, EventType.Information, 1000);
            Console.WriteLine($"\nWelcome to {AppName}, v." + _vData + " by " + _companyName + "\n");
            
            // Set Global Logfile properties
            FileLogger.DateFormat = "dd-MM-yyyy";
            DateTimeFormat = "dd-MM-yyyy HH:mm:ss";
            WriteOnlyErrorsToEventLog = false;
            WriteToEventLog = false;
            WriteToFile = true;
            Message("Loaded log configuration into the program: " + AppName, EventType.Information, 1000);
            
            // Check for required Args for application will work
            string[] requiredArgs = { "--token", "--org", "--outdir", "--server", "--port", "--from", "--to" };

            // Log
            Message("Checking if the 7 required arguments is present (--token, --org, --outdir, --server, --port, --from, --to)", EventType.Information, 1000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Checking if the 7 required arguments is present (--token, --org, --outdir, --server, --port, --from, --to)...");
            Console.ResetColor();

            // Check if parameters have been provided
            if (args.Length == 0)
            {
                // No arguments have been provided
                Message("ERROR: No arguments is provided - try again!", EventType.Error, 1001);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: No arguments is provided - try again!\n");
                Console.ResetColor();

                // Show help
                DisplayHelp();

                Message($"Showed help to Console - Exciting {AppName}, v." + _vData + " by " + _companyName + "!", EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Showed help to Console - Exciting {AppName}, v." + _vData + " by " + _companyName + "!\n");
                Console.ResetColor();

                // End application
                Environment.Exit(1);
            }

            switch (args.Length > 0)
            {
                // Parse the provided arguments
                //ParseArguments(args);
                // If okay do some work
                case true when args.Intersect(requiredArgs).Count() == 7:
                {
                    // Startup log entry
                    Message("Checked if the 7 required arguments is present (--token, --org, --outdir, --server, --port, --from, --to) - all is fine!", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Checked if the 7 required arguments is present (--token, --org, --outdir, --server, --port, --from, --to) - all is fine!");
                    Console.ResetColor();

                    // Start URL parse to AIP access
                    Message("Starting connection to Azure DevOps API from data provided from arguments", EventType.Information, 1000);
                    Console.WriteLine("Starting connection to Azure DevOps API from data provided from arguments");

                    // Base GET API
                    //const string version = "api-version=5.1-preview.1";
                    const string version = "api-version=7.0";
                    string auth = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", args[Array.IndexOf(args, "--token") + 1])));
                    string baseUrl = "https://dev.azure.com/" + args[Array.IndexOf(args, "--org") + 1] + "/";

                    // URL parse to API access done
                    Message("Base URL is for Organization is: " + baseUrl, EventType.Information, 1000);
                    Console.WriteLine("Base URL is for Organization is: " + baseUrl);

                    // Get output folder to backup (not with date stamp for backup folder name)
                    string outBackupDir = args[Array.IndexOf(args, "--outdir") + 1] + "\\";

                    // Set output folder name
                    string todaysdate = DateTime.Now.ToString("dd-MM-yyyy-(HH-mm)");
                    string outDir = outBackupDir + todaysdate + "\\";

                    // Output folder to backup to (without date stamp for backup) done
                    Message("Output folder is: " + outDir, EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Output folder is: " + outDir);
                    Console.ResetColor();

                    // Check if folder to backup (with date stamp for today) exist, else create it
                    if (!Directory.Exists(outDir))
                    {
                        Message("Output folder does not exists - trying to create...: " + outDir, EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Output folder does not exists - trying to create..." + outDir);
                        Console.ResetColor();
                        try
                        {
                            // Create backup folder if not exist
                            Directory.CreateDirectory(outDir);

                            // Log
                            Message("Output folder is created: " + outDir, EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Output folder is created: " + outDir);
                            Console.ResetColor();
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Message("Unable to create folder to store the backups: " + outDir + ". Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Unable to create folder to store the backups: " + outDir + ". Make sure the account you use to run this tool has write rights to this location.");
                            Console.ResetColor();

                            // Count errors
                            _errors++;
                        }
                        catch (Exception e)
                        {
                            // Error when create backup folder
                            Message("Exception caught when trying to create output folder - error: " + e, EventType.Error, 1001);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("{0} Exception caught.", e);
                            Console.ResetColor();

                            // Count errors
                            _errors++;
                        }
                    }
                    else
                    {
                        // Backup folder exists - will not create a new folder
                        Message("Output folder exists (will not create it again): " + outDir, EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Output folder exists (will not create it again): " + outDir);
                        Console.ResetColor();
                    }

                    // Save log entry
                    Message("Getting information about Git projects...", EventType.Information, 1000);
                    Console.WriteLine("Getting information about Git projects...");

                    // Get Git projects from REST API
                    var clientProjects = new RestClient(baseUrl + "_apis/projects?" + version);
                    var requestProjects = new RestRequest(Method.GET);

                    requestProjects.AddHeader("Authorization", auth);
                    IRestResponse responseProjects = clientProjects.Execute(requestProjects);
                    Projects projects = JsonConvert.DeserializeObject<Projects>(responseProjects.Content);

                    // Get Git project details from project to backup
                    List<string> repocountelements = new List<string>();
                    List<string> repoitemscountelements = new List<string>();

                    if (projects.Value != null)
                    {
                        // If there is not 0 Git projects > do work
                        foreach (Project project in projects.Value)
                        {
                            // Count projects
                            projectCount++;

                            // Log
                            Message("Getting information about Git project: " + project.Name, EventType.Information, 1000);
                            Console.WriteLine("Getting information about Git project: " + project.Name);

                            // Repos
                            var clientRepos = new RestClient(baseUrl + project.Name + "/_apis/git/repositories?" + version);
                            var requestRepos = new RestRequest(Method.GET);
                            requestRepos.AddHeader("Authorization", auth);
                            IRestResponse responseRepos = clientRepos.Execute(requestRepos);
                            Repos repos = JsonConvert.DeserializeObject<Repos>(responseRepos.Content);

                            // Get info about repos in projects
                            foreach (Repo repo in repos.Value)
                            {
                                // List data about repos in projects
                                repocountelements.Add(repo.Name);

                                // Count total repos got
                                repoCount++;

                                // Log
                                Message("Getting information about Git repository: " + repo.Name, EventType.Information, 1000);
                                Console.WriteLine("Getting information about Git repository: " + repo.Name);

                                var clientItems = new RestClient(baseUrl + "_apis/git/repositories/" + repo.Id + "/items?recursionlevel=full&" + version);
                                var requestItems = new RestRequest(Method.GET);

                                requestItems.AddHeader("Authorization", auth);
                                IRestResponse responseItems = clientItems.Execute(requestItems);
                                Items items = JsonConvert.DeserializeObject<Items>(responseItems.Content);

                                // Get info about repos in projects, files
                                Message("Getting information about Git repository: " + repo.Name + " is done, items count in here is: " + items.Count, EventType.Information, 1000);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Getting information about Git repository: " + repo.Name + " is done, items count in here is: " + items.Count);
                                Console.ResetColor();

                                // If more then 0 is returned
                                if (items.Count > 0)
                                {
                                    // If repos have files, get it

                                    // Count files
                                    repoItemsCount++;

                                    var clientBlob = new RestClient(baseUrl + "_apis/git/repositories/" + repo.Id + "/blobs?" + version);
                                    var requestBlob = new RestRequest(Method.POST);

                                    // List data about repos in projects
                                    repoitemscountelements.Add(repo.Name);

                                    // Log
                                    Message("Getting client blob for Git repository: " + repo.Name + " from API", EventType.Information, 1000);
                                    Console.WriteLine("Getting client blob for Git repository: " + repo.Name + " from API");

                                    // Get the files from repo to storage
                                    requestBlob.AddJsonBody(items.Value.Where(itm => itm.GitObjectType == "blob").Select(itm => itm.ObjectId).ToList());
                                    requestBlob.AddHeader("Authorization", auth);
                                    requestBlob.AddHeader("Accept", "application/zip");

                                    // Save to disk - _blob.zip
                                    try
                                    {
                                        // Save file to disk
                                        //clientBlob.DownloadData(requestBlob).SaveAs(outDir + project.Name + "_" + repo.Name + "_blob.zip");

                                        // Save file to disk
                                        byte[] data = clientBlob.DownloadData(requestBlob);
                                        using (FileStream fs = new FileStream(outDir + project.Name + "_" + repo.Name + "_blob.zip", FileMode.Create))
                                        {
                                            fs.Write(data, 0, data.Length);
                                        }

                                        // Log
                                        Message("Saved file to disk: " + outDir + project.Name + "_" + repo.Name + "_blob.zip", EventType.Information, 1000);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("Saved file to disk: " + outDir + project.Name + "_" + repo.Name + "_blob.zip");
                                        Console.ResetColor();

                                        // Count files there is downloaded
                                        totalBlobFilesIsBackup++;

                                        //Set backup status
                                        isBackupOk = true;
                                        isBackupOkAndUnZip = false;
                                    }
                                    catch (UnauthorizedAccessException)
                                    {
                                        Message("Unable to write the backup file to disk: " + outDir + project.Name + "_" + repo.Name + "_blob.zip. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Unable to write the backup file to disk: " + outDir + project.Name + "_" + repo.Name + "_blob.zip. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                        Console.ResetColor();

                                        // Count errors
                                        _errors++;
                                    }
                                    catch (Exception e)
                                    {
                                        // Error
                                        Message("Exception caught when trying to save file to disk: " + outDir + project.Name + "_" + repo.Name + "_blob.zip - error: " + e, EventType.Error, 1001);
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Exception caught when trying to save file to disk: " + outDir + project.Name + "_" + repo.Name + "_blob.zip - error: " + e);
                                        Console.ResetColor();

                                        // Set backup status
                                        isBackupOk = false;
                                        isBackupOkAndUnZip = false;

                                        // Count errors
                                        _errors++;
                                    }

                                    //Save to disk - _tree.json
                                    try
                                    {
                                        // Save file to disk
                                        File.WriteAllText(outDir + project.Name + "_" + repo.Name + "_tree.json", responseItems.Content);

                                        // Log
                                        Message("Saved file to disk: " + outDir + project.Name + "_" + repo.Name + "_tree.json", EventType.Information, 1000);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("Saved file to disk: " + outDir + project.Name + "_" + repo.Name + "_tree.json");
                                        Console.ResetColor();

                                        // Count files there is downloaded
                                        totalTreeFilesIsBackup++;

                                        // Set backup status
                                        isBackupOk = true;
                                        isBackupOkAndUnZip = false;
                                    }
                                    catch (UnauthorizedAccessException)
                                    {
                                        Message("Unable to write the backup file to disk: " + outDir + project.Name + "_" + repo.Name + "_tree.json. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Unable to write the backup file to disk: " + outDir + project.Name + "_" + repo.Name + "_tree.json. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                        Console.ResetColor();

                                        // Count errors
                                        _errors++;
                                    }
                                    catch (Exception e)
                                    {
                                        // Error
                                        Message("Exception caught when trying to save file to disk: " + outDir + project.Name + "_" + repo.Name + "_tree.json - error: " + e, EventType.Error, 1001);
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Exception caught when trying to save file to disk: " + outDir + project.Name + "_" + repo.Name + "_tree.json - error: " + e);
                                        Console.ResetColor();

                                        // Set backup status
                                        isBackupOk = false;
                                        isBackupOkAndUnZip = false;

                                        // Count errors
                                        _errors++;
                                    }

                                    // If args is set to unzip files
                                    if (Array.Exists(args, argument => argument == "--unzip"))
                                    {
                                        // If need to unzip data from .zip and .json (files details is in here)
                                        Message("Checking if folder exists before unzip: " + outDir + project.Name + "_" + repo.Name, EventType.Information, 1000);
                                        Console.WriteLine("Checking if folder exists before unzip: " + outDir + project.Name + "_" + repo.Name);

                                        // Check if folder to unzip exists
                                        if (Directory.Exists(outDir + project.Name + "_" + repo.Name))
                                        {
                                            // Check if an old folder exists, then try to delete it
                                            try
                                            {
                                                // Do work
                                                Directory.Delete(outDir + project.Name + "_" + repo.Name, true);

                                                // Log
                                                Message("Folder exists before unzip: " + outDir + project.Name + "_" + repo.Name + ", deleting folder...", EventType.Information, 1000);
                                                Console.ForegroundColor = ConsoleColor.Yellow;
                                                Console.WriteLine("Folder exists before unzip: " + outDir + project.Name + "_" + repo.Name + ", deleting folder...");
                                                Console.ResetColor();

                                                // Set backup status
                                                isBackupOkAndUnZip = true;
                                            }
                                            catch (UnauthorizedAccessException)
                                            {
                                                Message("Unable to delete folder under unzip: " + outDir + project.Name + "_" + repo.Name + ". Make sure the account you use to run this tool has delete rights to this location.", EventType.Error, 1001);
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine("Unable to delete folder under unzip: " + outDir + project.Name + "_" + repo.Name + ". Make sure the account you use to run this tool has delete rights to this location.");
                                                Console.ResetColor();

                                                // Count errors
                                                _errors++;
                                            }
                                            catch (Exception e)
                                            {
                                                // Error
                                                Message("Exception caught when trying to delete folder when unzip: " + outDir + project.Name + "_" + repo.Name + " - error: " + e, EventType.Error, 1001);
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine("Exception caught when trying to delete folder when unzip: " + outDir + project.Name + "_" + repo.Name + " - error: " + e);
                                                Console.ResetColor();

                                                // Set backup status
                                                isBackupOkAndUnZip = false;

                                                // Count errors
                                                _errors++;
                                            }

                                            // Check if done delete folder
                                            if (!Directory.Exists(outDir + project.Name + "_" + repo.Name))
                                            {
                                                // Log
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine($"Directory " + outDir + project.Name + "_" + repo.Name + " is deleted successfully");
                                                Console.ResetColor();
                                                Message("Directory " + outDir + project.Name + "_" + repo.Name + " is deleted successfully", EventType.Information, 1000);

                                                // Set backup status
                                                isBackupOkAndUnZip = true;
                                            }
                                            else
                                            {
                                                // Log
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine($"Directory " + outDir + project.Name + "_" + repo.Name + " is not deleted successfully - see logs for more information");
                                                Console.ResetColor();
                                                Message("Directory " + outDir + project.Name + "_" + repo.Name + " is not deleted successfully - see logs for more information", EventType.Error, 1001);

                                                // Set backup status
                                                isBackupOkAndUnZip = false;

                                                // Count errors
                                                _errors++;
                                            }
                                        }
                                        else
                                        {
                                            // if not folder to unzip exists
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine($"Folder to unzip files from does not exists: " + outDir + project.Name + "_" + repo.Name);
                                            Console.ResetColor();
                                            Message("Folder to unzip files from does not exists: " + outDir + project.Name + "_" + repo.Name, EventType.Warning, 1001);
                                        }

                                        // Do work to start over - create folder to files
                                        // Create folder when not exists
                                        try
                                        {
                                            // Do work
                                            Directory.CreateDirectory(outDir + project.Name + "_" + repo.Name);

                                            // Log
                                            Message(
                                                "Checked if folder exists before unzip: " + outDir + project.Name + "_" +
                                                repo.Name + " - The folder does not exist, creating folder: " + outDir +
                                                project.Name + "_" + repo.Name, EventType.Information, 1000);
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine("Checked if folder exists before unzip: " + outDir +
                                                              project.Name + "_" + repo.Name +
                                                              " - The folder does not exist, creating folder: " + outDir +
                                                              project.Name + "_" + repo.Name);
                                            Console.ResetColor();

                                            // Set backup status
                                            isBackupOkAndUnZip = true;
                                        }
                                        catch (UnauthorizedAccessException)
                                        {
                                            Message("Unable to create folder under unzipping: " + outDir + project.Name + "_" + repo.Name + ". Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("Unable to create folder under unzipping: " + outDir + project.Name + "_" + repo.Name + ". Make sure the account you use to run this tool has write rights to this location.");
                                            Console.ResetColor();

                                            // Count errors
                                            _errors++;
                                        }
                                        catch (Exception e)
                                        {
                                            // Error
                                            Message("Exception caught when trying to creating folder: " + outDir + project.Name + "_" + repo.Name + " - error: " + e, EventType.Error, 1001);
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("{0} Exception caught.", e);
                                            Console.ResetColor();

                                            // Set backup status
                                            isBackupOkAndUnZip = false;
                                            _errors++;
                                        }

                                        // Get files from .zip folder to unzip
                                        ZipArchive archive = ZipFile.OpenRead(outDir + project.Name + "_" + repo.Name + "_blob.zip");

                                        foreach (Item item in items.Value)
                                        {
                                            // Work on all files/folders
                                            if (item.IsFolder)
                                            {
                                                // If folder data
                                                Message("Unzipping Git repository folder data: " + outDir + project.Name + "_" + repo.Name + item.Path, EventType.Information, 1000);
                                                Console.ForegroundColor = ConsoleColor.Yellow;
                                                Console.WriteLine("Unzipping Git repository folder data: " + outDir + project.Name + "_" + repo.Name + item.Path);
                                                Console.ResetColor();

                                                try
                                                {
                                                    // Do work
                                                    Directory.CreateDirectory(outDir + project.Name + "_" + repo.Name + item.Path);

                                                    // Log
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine($"Created folder: " + outDir + project.Name + "_" + repo.Name + item.Path);
                                                    Console.ResetColor();
                                                    Message("Created folder: " + outDir + project.Name + "_" + repo.Name + item.Path, EventType.Information, 1000);

                                                    // Set backup status
                                                    isBackupOkAndUnZip = true;
                                                }
                                                catch (UnauthorizedAccessException)
                                                {
                                                    Message("Unable to create folder under unzipping: " + outDir + project.Name + "_" + repo.Name + item.Path + ". Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("Unable to create folder under unzipping: " + outDir + project.Name + "_" + repo.Name + item.Path + ". Make sure the account you use to run this tool has write rights to this location.");
                                                    Console.ResetColor();

                                                    // Count errors
                                                    _errors++;
                                                }
                                                catch (Exception e)
                                                {
                                                    // Log
                                                    Message("Exception caught when trying to create folder: " + outDir + project.Name + "_" + repo.Name + item.Path + " - error: " + e, EventType.Error, 1001);
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("{0} Exception caught.", e);
                                                    Console.ResetColor();

                                                    // Set backup status
                                                    isBackupOkAndUnZip = false;

                                                    // Count errors
                                                    _errors++;
                                                }
                                            }
                                            else
                                            {
                                                // If file data
                                                try
                                                {
                                                    //Try to save data to disk
                                                    archive.GetEntry(item.ObjectId).ExtractToFile(outDir + project.Name + "_" + repo.Name + item.Path, true);

                                                    // Log
                                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                                    Console.WriteLine($"Unzipping Git repository file data on disk: " + outDir + project.Name + "_" + repo.Name + item.Path);
                                                    Console.ResetColor();
                                                    Message("Unzipping Git repository file data on disk: " + outDir + project.Name + "_" + repo.Name + item.Path, EventType.Information, 1000);

                                                    // Count
                                                    totalFilesIsBackupUnZipped++;

                                                    // Set backup status
                                                    isBackupOkAndUnZip = true;
                                                }
                                                catch (UnauthorizedAccessException)
                                                {
                                                    Message("Unable to create file under unzipping: " + outDir + project.Name + "_" + repo.Name + item.Path + ". Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("Unable to create file under unzipping: " + outDir + project.Name + "_" + repo.Name + item.Path + ". Make sure the account you use to run this tool has write rights to this location.");
                                                    Console.ResetColor();

                                                    // Count errors
                                                    _errors++;
                                                }
                                                catch (Exception e)
                                                {
                                                    // Error
                                                    Message("Exception caught when trying to create data on disk: " + outDir + project.Name + "_" + repo.Name + item.Path + " - error: " + e, EventType.Error, 1001);
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine("Exception caught when trying to create data on disk: " + outDir + project.Name + "_" + repo.Name + item.Path + " - error: " + e);
                                                    Console.ResetColor();

                                                    // Set backup status
                                                    isBackupOkAndUnZip = false;

                                                    // Count errors
                                                    _errors++;
                                                }
                                            }
                                        }

                                        // When done unzip
                                        Message("Unzipping Git repository: " + outDir + project.Name + "_" + repo.Name + " is now done", EventType.Information, 1000);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("Unzipping Git repository: " + outDir + project.Name + "_" + repo.Name + " is now done\n");
                                        Console.ResetColor();

                                        // When done release zip files from function
                                        archive.Dispose();
                                    }

                                    // Set backup status
                                    noProjectsToBackup = false;
                                }
                                else
                                {
                                    // If there is nothing in the Git repo/project to backup
                                    Message("Number of items in project " + project.Name + " repository: " + repo.Name + " is 0, nothing to backup", EventType.Information, 1000);
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Number of items in project " + project.Name + " repository: " + repo.Name + " is 0, nothing to backup\n");
                                    Console.ResetColor();

                                    // Set backup status
                                    noProjectsToBackup = true;
                                }
                            }
                        }

                        // When done backup
                        Message("No more projets to work with for now...", EventType.Information, 1000);
                        Message("Done with " + repoCount + " repositories in Azure DevOps", EventType.Information, 1000);
                        Message("Done with " + repoItemsCount + " repositories to backup in folder: " + outDir + " on host: " + Environment.MachineName, EventType.Information, 1000);
                        Message("Processed files to backup from Git repos (total unzipped if specified): " + totalFilesIsBackupUnZipped, EventType.Information, 1000);
                        Message("Processed files to backup from Git repos (blob files (.zip files)): " + totalBlobFilesIsBackup, EventType.Information, 1000);
                        Message("Processed files to backup from Git repos (tree files (.json files)): " + totalTreeFilesIsBackup, EventType.Information, 1000);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("== No more projects to work with for now ==\n");
                        Console.WriteLine("Done with " + repoCount + " repositories in Azure DevOps");
                        Console.WriteLine("Done with " + repoItemsCount + " repositories to backup in folder: " + outDir + " on host: " + Environment.MachineName);
                        Console.WriteLine("Processed files to backup from Git repos (total unzipped if specified): " + totalFilesIsBackupUnZipped);
                        Console.WriteLine("Processed files to backup from Git repos (blob files (.zip files)): " + totalBlobFilesIsBackup);
                        Console.WriteLine("Processed files to backup from Git repos (tree files (.json files)): " + totalTreeFilesIsBackup);
                        Console.ResetColor();

                        // Stop timer
                        stopWatch.Stop();

                        // Get the elapsed time as a TimeSpan value.
                        TimeSpan ts = stopWatch.Elapsed;

                        // Format and display the TimeSpan value.
                        // string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);

                        // Log
                        Message("Backup Run Time: " + elapsedTime, EventType.Information, 1000);
                        Console.WriteLine("\nBackup Run Time: " + elapsedTime);

                        // Parse data
                        server = args[Array.IndexOf(args, "--server") + 1];
                        serverPort = args[Array.IndexOf(args, "--port") + 1];
                        emailFrom = args[Array.IndexOf(args, "--from") + 1];
                        emailTo = args[Array.IndexOf(args, "--to") + 1];

                        // Log details regarding email send
                        Console.WriteLine("Email details:");
                        Console.WriteLine("--server: " + server);
                        Console.WriteLine("--port: " + serverPort);
                        Console.WriteLine("--from: " + emailFrom);
                        Console.WriteLine("--to: " + emailTo + Environment.NewLine);
                        Message("Email details:", EventType.Information, 1000);
                        Message("--server: " + server, EventType.Information, 1000);
                        Message("--port: " + serverPort, EventType.Information, 1000);
                        Message("--from: " + emailFrom, EventType.Information, 1000);
                        Message("--to: " + emailTo, EventType.Information, 1000);

                        // Cleanup old backups
                        Message("Clean up old backups", EventType.Information, 1000);
                        Console.WriteLine("== Clean up old backups ==");

                        if (args.Contains("--daystokeepbackup"))
                        {
                            // If present - do work
                            daysToKeepBackups = args[Array.IndexOf(args, "--daystokeepbackup") + 1];

                            // Check if data is not null
                            if (daysToKeepBackups != null)
                            {
                                // If set to 30 (default) show it - other text if --daystokeepbackup is not set
                                if (daysToKeepBackups == "30")
                                {
                                    // Log
                                    Message($"argument --daystokeepbackup is set to (default) {daysToKeepBackups}", EventType.Information, 1000);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"argument --daystokeepbackup is set to (default) {daysToKeepBackups}");
                                    Console.ResetColor();

                                    // Set status text for email
                                    isDaysToKeepNotDefaultStatusText = "Default number of old backup(s) set to keep in backup folder (days)";

                                    // Log
                                    Message(isDaysToKeepNotDefaultStatusText, EventType.Information, 1000);

                                    // Do work
                                    DaysToKeepBackupsDefault(outBackupDir);
                                }

                                // If --daystokeepbackup is not set to default 30 - show it and do work
                                if (daysToKeepBackups != "30")
                                {
                                    // Log
                                    Message($"argument --daystokeepbackup is not default (30), it is set to {daysToKeepBackups} days", EventType.Information, 1000);
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"argument --daystokeepbackup is not default (30), it is set to {daysToKeepBackups} days");
                                    Console.ResetColor();

                                    // Set status text for email
                                    isDaysToKeepNotDefaultStatusText = "Custom number of old backup(s) set to keep in backup folder (days)";

                                    // Log
                                    Message(isDaysToKeepNotDefaultStatusText, EventType.Information, 1000);

                                    // Do work
                                    DaysToKeepBackups(outBackupDir, daysToKeepBackups);
                                }
                            }
                            else
                            {
                                // Set default
                                DaysToKeepBackupsDefault(outBackupDir);
                            }
                        }
                        else
                        {
                            // Log
                            Message($"argument --daystokeepbackup does not exits - using default backups to keep (30 days)!", EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"\nargument --daystokeepbackup does not exits - using default backups to keep (30 days)!\n");
                            Console.ResetColor();

                            // Do work
                            DaysToKeepBackupsDefault(outBackupDir);
                        }

                        // Cleanup old log files
                        string[] oldfiles = Directory.GetFiles(Files.LogFilePath);

                        // Log
                        Message("Checking for old log file(s) to cleanup...", EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nChecking for old log file(s) to cleanup...");
                        Console.ResetColor();

                        // Loop all files in folder
                        foreach (string file in oldfiles)
                        {
                            FileInfo fi = new FileInfo(file);

                            // Get all last access time back in time
                            if (fi.LastAccessTime < DateTime.Now.AddDays(-30))
                            {
                                try
                                {
                                    // Do work
                                    fi.Delete();

                                    // Log
                                    Message("Deleted old log file: " + fi, EventType.Information, 1000);
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Deleted old log file: " + fi);
                                    Console.ResetColor();

                                    // Set status
                                    oldLogfilesToDelete = true;
                                    oldLogFilesToDeleteCount++;
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    Message("Unable to delete old log file: " + fi + ". Make sure the account you use to run this tool has delete rights to this location.", EventType.Error, 1001);
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Unable to delete old log file: " + fi + ". Make sure the account you use to run this tool has delete rights to this location.");
                                    Console.ResetColor();

                                    // Count errors
                                    _errors++;
                                }
                                catch (Exception ex)
                                {
                                    // Log
                                    Message("Sorry, we are unable to delete old log file: " + fi + "Error: " + ex, EventType.Error, 1001);
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Sorry, we are unable to delete old log file: " + fi + "Error: " + ex);
                                    Console.ResetColor();

                                    // Count errors
                                    _errors++;
                                }
                            }
                        }

                        // Check if there is old log files to delete
                        if (oldLogfilesToDelete)
                        {
                            // Log
                            Message($"There was {oldLogFilesToDeleteCount} old log files to delete (-30 days)", EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"There was {oldLogFilesToDeleteCount} old log files to delete (-30 days)");
                            Console.ResetColor();
                        }
                        else
                        {
                            // Log
                            Message("No old log files to delete (-30 days)", EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("No old log files to delete (-30 days)");
                            Console.ResetColor();
                        }

                        // Get email status text from job status
                        if (isBackupOk)
                        {
                            // If unzipped or not
                            if (isBackupOkAndUnZip)
                            {
                                emailStatusMessage = "Success and unzipped";
                            }
                            else
                            {
                                emailStatusMessage = "Success, not unzipped";
                            }
                        }
                        else
                        {
                            emailStatusMessage = "Failed!";
                        }

                        // Text if no Git projects to backup
                        if (noProjectsToBackup)
                        {
                            emailStatusMessage = "No projects to backup!";
                        }
                    }
                    else
                    {
                        // No projects to backup
                        // Log
                        Message("No information from Azure DevOps for Git projects - nothing to backup!", EventType.Information, 1000);
                        Console.WriteLine("No information from Azure DevOps for Git projects - nothing to backup!");

                        // Stop timer
                        stopWatch.Stop();
                    }

                    // If user set to delete downloaded files (.zip and .json) after unzipped
                    if (Array.Exists(args, argument => argument == "--cleanup"))
                    {
                        // Set state
                        _cleanUpState = true;

                        // If --cleanup was set to and --unzip
                        if (Array.Exists(args, argument => argument == "--unzip"))
                        {
                            // Log
                            Message("Parameter --cleanup and --unzip is set - deleting downloaded .zip and .json files when unzipped...", EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nParameter --cleanup and --unzip is set - deleting downloaded .zip and .json files when unzipped...\n");
                            Console.ResetColor();

                            // Do task
                            Deletezipandjson(outDir);

                            // Log
                            Message("Parameter --cleanup was set - deleted downloaded .zip and .json files after unzipped!", EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\nParameter --cleanup was set - deleted downloaded .zip and .json files after unzipped!\n");
                            Console.ResetColor();
                        }
                        // If --unzip was not set when --cleanup is - do not delete anything!
                        else
                        {
                            // Log
                            Message("Parameter --cleanup is set but NOT --unzip - will not delete any downloaded .zip and .json files as that the only files there is backup of!", EventType.Warning, 1002);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nParameter --cleanup is set but NOT --unzip - will not delete any downloaded .zip and .json files as that the only files there is backup of!\n");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        // Set state
                        _cleanUpState = false;
                    }

                    // Get status email text for Status colums in email report
                    // Log
                    Message($"Getting status for tasks for email report", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Getting status for tasks for email report");
                    Console.ResetColor();

                    // Processed Git repos in Azure DevOps (total):
                    if (repoCount == 0)
                    {
                        repoCountStatusText = "Warning - nothing to backup!";

                        // Log
                        Message($"Processed Git repos in Azure DevOps (total) status:" + repoCountStatusText, EventType.Warning, 1001);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Processed Git repos in Azure DevOps (total) status: " + repoCountStatusText);
                        Console.ResetColor();
                    }
                    else
                    {
                        if (isBackupOk)
                        {
                            repoCountStatusText = "Good!";

                            // Log
                            Message($"Processed Git repos in Azure DevOps (total) status: " + repoCountStatusText, EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Processed Git repos in Azure DevOps (total) status: " + repoCountStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            repoCountStatusText = "Warning!";

                            // Log
                            Message($"Processed Git repos in Azure DevOps (total) status: " + repoCountStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Processed Git repos in Azure DevOps (total) status: " + repoCountStatusText);
                            Console.ResetColor();
                        }
                    }

                    // Processed Git repos a backup is made of from Azure DevOps:
                    if (repoItemsCount == 0)
                    {
                        repoItemsCountStatusText = "Warning - nothing to backup!";

                        // Log
                        Message($"Processed Git repos a backup is made of from Azure DevOps status: " + repoItemsCountStatusText, EventType.Warning, 1001);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Processed Git repos a backup is made of from Azure DevOps status: " + repoItemsCountStatusText);
                        Console.ResetColor();
                    }
                    else
                    {
                        if (isBackupOk)
                        {
                            repoItemsCountStatusText = "Good!";

                            // Log
                            Message($"Processed Git repos a backup is made of from Azure DevOps status: " + repoItemsCountStatusText, EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Processed Git repos a backup is made of from Azure DevOps status: " + repoItemsCountStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            repoItemsCountStatusText = "Warning!";

                            // Log
                            Message($"Processed Git repos a backup is made of from Azure DevOps status: " + repoItemsCountStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Processed Git repos a backup is made of from Azure DevOps status: " + repoItemsCountStatusText);
                            Console.ResetColor();
                        }
                    }

                    // Processed files to backup from Git repos (total unzipped if specified):
                    if (totalFilesIsBackupUnZipped == 0)
                    {
                        totalFilesIsBackupUnZippedStatusText = "Good - nothing to unzip!";

                        // Log
                        Message($"Processed files to backup from Git repos (total unzipped if specified) status: " + totalFilesIsBackupUnZippedStatusText, EventType.Warning, 1001);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Processed files to backup from Git repos (total unzipped if specified) status: " + totalFilesIsBackupUnZippedStatusText);
                        Console.ResetColor();
                    }
                    else
                    {
                        if (isBackupOkAndUnZip)
                        {
                            totalFilesIsBackupUnZippedStatusText = "Good! (and unzip is OK)";

                            // Log
                            Message($"Processed files to backup from Git repos (total unzipped if specified) status: " + totalFilesIsBackupUnZippedStatusText, EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Processed files to backup from Git repos (total unzipped if specified) status: " + totalFilesIsBackupUnZippedStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            totalFilesIsBackupUnZippedStatusText = "Warning on unzip!";

                            // Log
                            Message($"Processed files to backup from Git repos (total unzipped if specified) status: " + totalFilesIsBackupUnZippedStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Processed files to backup from Git repos (total unzipped if specified) status: " + totalFilesIsBackupUnZippedStatusText);
                            Console.ResetColor();
                        }
                    }

                    // Processed files to backup from Git repos (blob files (.zip files)):
                    if (totalBlobFilesIsBackup == 0)
                    {
                        totalBlobFilesIsBackupStatusText = "Warning - nothing to backup!";

                        // Log
                        Message($"Processed files to backup from Git repos (blob files (.zip files) status: " + totalBlobFilesIsBackupStatusText, EventType.Warning, 1001);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Processed files to backup from Git repos (blob files (.zip files) status: " + totalBlobFilesIsBackupStatusText);
                        Console.ResetColor();
                    }
                    else
                    {
                        if (isBackupOk)
                        {
                            totalBlobFilesIsBackupStatusText = "Good!";

                            // Log
                            Message($"Processed files to backup from Git repos (blob files (.zip files) status: " + totalBlobFilesIsBackupStatusText, EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Processed files to backup from Git repos (blob files (.zip files) status: " + totalBlobFilesIsBackupStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            totalBlobFilesIsBackupStatusText = "Warning!";

                            // Log
                            Message($"Processed files to backup from Git repos (blob files (.zip files) status: " + totalBlobFilesIsBackupStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Processed files to backup from Git repos (blob files (.zip files) status: " + totalBlobFilesIsBackupStatusText);
                            Console.ResetColor();
                        }
                    }

                    // Processed files to backup from Git repos (tree files (.json files)):
                    if (totalTreeFilesIsBackup == 0)
                    {
                        totalTreeFilesIsBackupStatusText = "Warning - nothing to backup!";

                        // Log
                        Message($"Processed files to backup from Git repos (tree files (.json files) status: " + totalTreeFilesIsBackupStatusText, EventType.Warning, 1001);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Processed files to backup from Git repos (tree files (.json files) status: " + totalTreeFilesIsBackupStatusText);
                        Console.ResetColor();
                    }
                    else
                    {
                        if (isBackupOk)
                        {
                            totalTreeFilesIsBackupStatusText = "Good!";

                            // Log
                            Message($"Processed files to backup from Git repos (tree files (.json files) status: " + totalTreeFilesIsBackupStatusText, EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Processed files to backup from Git repos (tree files (.json files) status: " + totalTreeFilesIsBackupStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            totalTreeFilesIsBackupStatusText = "Warning!";

                            // Log
                            Message($"Processed files to backup from Git repos (tree files (.json files) status: " + totalTreeFilesIsBackupStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Processed files to backup from Git repos (tree files (.json files) status: " + totalTreeFilesIsBackupStatusText);
                            Console.ResetColor();
                        }
                    }

                    // Deleted original downloaded .zip and .json files in backup folder:
                    int totalFilesThereShouldBeDeleted = totalBlobFilesIsBackup + totalTreeFilesIsBackup;
                    if (_totalFilesIsDeletedAfterUnZipped != totalFilesThereShouldBeDeleted)
                    {
                        if (_totalFilesIsDeletedAfterUnZipped != 0)
                        {
                            totalFilesIsDeletedAfterUnZippedStatusText =
                                "Warning - not all files is deleted and backup is not OK!";

                            // Log
                            Message($"Deleted original downloaded .zip and .json files in backup folder status: " + totalFilesIsDeletedAfterUnZippedStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Deleted original downloaded .zip and .json files in backup folder status: " + totalFilesIsDeletedAfterUnZippedStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            if (isBackupOk)
                            {
                                totalFilesIsDeletedAfterUnZippedStatusText = "Good - not set to cleanup!";
                            }
                            else
                            {
                                totalFilesIsDeletedAfterUnZippedStatusText = "Warning - nothing to backup!";
                            }

                            // Log
                            Message($"Deleted original downloaded .zip and .json files in backup folder status: " + totalFilesIsDeletedAfterUnZippedStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Deleted original downloaded .zip and .json files in backup folder status: " + totalFilesIsDeletedAfterUnZippedStatusText);
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        if (isBackupOkAndUnZip)
                        {
                            totalFilesIsDeletedAfterUnZippedStatusText = "Good! (set to cleanup, and matched the total files downloaded)";

                            // Log
                            Message($"Deleted original downloaded .zip and .json files in backup folder status: " + totalFilesIsDeletedAfterUnZippedStatusText, EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Deleted original downloaded .zip and .json files in backup folder status: " + totalFilesIsDeletedAfterUnZippedStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            totalFilesIsDeletedAfterUnZippedStatusText = "Warning - not all files is deleted!";

                            // Log
                            Message($"Deleted original downloaded .zip and .json files in backup folder status: " + totalFilesIsDeletedAfterUnZippedStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Deleted original downloaded .zip and .json files in backup folder status: " + totalFilesIsDeletedAfterUnZippedStatusText);
                            Console.ResetColor();
                        }
                    }

                    // Leftovers for original downloaded .zip files in backup folder (error(s) when try to delete):
                    if (_numZip != 0)
                    {
                        letOverZipFilesStatusText = "Warning - leftover .zip files!";

                        // Log
                        Message($"Leftovers for original downloaded .zip files in backup folder (error(s) when try to delete) status: " + letOverZipFilesStatusText, EventType.Warning, 1001);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Leftovers for original downloaded .zip files in backup folder (error(s) when try to delete) status: " + letOverZipFilesStatusText);
                        Console.ResetColor();
                    }
                    else
                    {
                        if (isBackupOk)
                        {
                            letOverZipFilesStatusText = "Good (no leftover .zip files)";

                            // Log
                            Message($"Leftovers for original downloaded .zip files in backup folder (error(s) when try to delete) status: " + letOverZipFilesStatusText, EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Leftovers for original downloaded .zip files in backup folder (error(s) when try to delete) status: " + letOverZipFilesStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            letOverZipFilesStatusText = "Warning - backup not OK!";

                            // Log
                            Message($"Leftovers for original downloaded .zip files in backup folder (error(s) when try to delete) status: " + letOverZipFilesStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Leftovers for original downloaded .zip files in backup folder (error(s) when try to delete) status: " + letOverZipFilesStatusText);
                            Console.ResetColor();
                        }
                    }

                    // Leftovers for original downloaded .json files in backup folder (error(s) when try to delete):
                    if (_numJson != 0)
                    {
                        letOverJsonFilesStatusText = "Warning - leftover .json files!";

                        // Log
                        Message($"Leftovers for original downloaded .json files in backup folder (error(s) when try to delete) status: " + letOverJsonFilesStatusText, EventType.Warning, 1001);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Leftovers for original downloaded .json files in backup folder (error(s) when try to delete) status: " + letOverJsonFilesStatusText);
                        Console.ResetColor();
                    }
                    else
                    {
                        if (isBackupOk)
                        {
                            letOverJsonFilesStatusText = "Good (no leftover .json files)";

                            // Log
                            Message($"Leftovers for original downloaded .json files in backup folder (error(s) when try to delete) status: " + letOverJsonFilesStatusText, EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Leftovers for original downloaded .json files in backup folder (error(s) when try to delete) status: " + letOverJsonFilesStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            letOverJsonFilesStatusText = "Warning!";

                            // Log
                            Message($"Leftovers for original downloaded .json files in backup folder (error(s) when try to delete) status: " + letOverJsonFilesStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Leftovers for original downloaded .json files in backup folder (error(s) when try to delete) status: " + letOverJsonFilesStatusText);
                            Console.ResetColor();
                        }
                    }

                    // Old backup(s) deleted in backup folder:
                    if (_totalBackupsIsDeleted != 0)
                    {
                        totalBackupsIsDeletedStatusText = "Good - deleted " + _totalBackupsIsDeleted + " old backup(s) from backup folder";

                        // Log
                        Message($"Old backup(s) deleted in backup folder: status: " + totalBackupsIsDeletedStatusText, EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Old backup(s) deleted in backup folder: status: " + totalBackupsIsDeletedStatusText);
                        Console.ResetColor();
                    }
                    else
                    {
                        if (isBackupOk)
                        {
                            totalBackupsIsDeletedStatusText = "Good - no old backup(s) to delete from backup folder";

                            // Log
                            Message($"Old backup(s) deleted in backup folder status: " + totalBackupsIsDeletedStatusText, EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Old backup(s) deleted in backup folder status: " + totalBackupsIsDeletedStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            totalBackupsIsDeletedStatusText = "Warning!";

                            // Log
                            Message($"Old backup(s) deleted in backup folder status: " + totalBackupsIsDeletedStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Old backup(s) deleted in backup folder status: " + totalBackupsIsDeletedStatusText);
                            Console.ResetColor();
                        }
                    }

                    // Check if output folder exists to email report and folder contains files
                    // Log
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Checking if directory " + outDir + " contains files");
                    Console.ResetColor();
                    Message("Checking if directory " + outDir + " and contains files", EventType.Information, 1000);

                    // Check if done for status in mail report
                    if (Directory.Exists(outDir) && (Directory.EnumerateFiles(outDir, "*.zip", SearchOption.AllDirectories).FirstOrDefault() != null))
                    {
                        // Log
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Directory " + outDir + " contains files");
                        Console.ResetColor();
                        Message("Directory " + outDir + " contains files", EventType.Information, 1000);

                        // Set status
                        isOutputFolderContainFiles = true;
                    }
                    else
                    {
                        if (_cleanUpState)
                        {
                            // Log
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Directory " + outDir + " contains no files - set to cleanup downloaded files - see logs for more information");
                            Console.ResetColor();
                            Message("Directory " + outDir + " contains no files - set to cleanup downloaded files - see logs for more information", EventType.Information, 1000);

                            // Set status
                            isOutputFolderContainFiles = false;
                        }
                        else
                        {
                            // Log
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Directory " + outDir + " is not created successfully and contains no files - see logs for more information");
                            Console.ResetColor();
                            Message("Directory " + outDir + " is not created successfully and contains no files - see logs for more information", EventType.Error, 1001);

                            // Set status
                            isOutputFolderContainFiles = false;

                            // Count errors
                            _errors++;
                        }
                    }

                    // Get status for output folder
                    if (isOutputFolderContainFiles)
                    {
                        isOutputFolderContainFilesStatusText = "Checked - folder is containing original downloaded files";

                        if (CheckIfHaveSubfolders(outDir))
                        {
                            isOutputFolderContainFilesStatusText += ", but has also subfolders with unzipped backup(s)";
                        }
                    }
                    else
                    {
                        isOutputFolderContainFilesStatusText = "Checked - folder is NOT containing original downloaded files";
                        if (CheckIfHaveSubfolders(outDir))
                        {
                            isOutputFolderContainFilesStatusText += ", but has subfolders with unzipped backup(s)";
                        }
                    }

                    // Log
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(isOutputFolderContainFilesStatusText);
                    Console.ResetColor();
                    Message(isOutputFolderContainFilesStatusText, EventType.Information, 1000);

                    // Log
                    Message($"Getting status for tasks for email report is done", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Getting status for tasks for email report is done");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Message($"Parsing, processing and collecting data for email report", EventType.Information, 1000);
                    Console.WriteLine($"\nParsing, processing and collecting data for email report");
                    Console.ResetColor();

                    // If args is set to old mail report layout
                    bool useSimpleMailReportLayout;
                    if (Array.Exists(args, argument => argument == "--simpelreport"))
                    {
                        useSimpleMailReportLayout = true;
                    }
                    else
                    {
                        useSimpleMailReportLayout = false;
                    }
                    
                    // Send status email and parse data to function
                    SendEmail(server, serverPort, emailFrom, emailTo, emailStatusMessage, repocountelements, repoitemscountelements,
                        repoCount, repoItemsCount, totalFilesIsBackupUnZipped, totalBlobFilesIsBackup, totalTreeFilesIsBackup,
                        outDir, elapsedTime, copyrightdata, _vData, _errors, _totalFilesIsDeletedAfterUnZipped, _totalBackupsIsDeleted, daysToKeepBackups,
                        repoCountStatusText, repoItemsCountStatusText, totalFilesIsBackupUnZippedStatusText, totalBlobFilesIsBackupStatusText, totalTreeFilesIsBackupStatusText,
                        totalFilesIsDeletedAfterUnZippedStatusText, letOverZipFilesStatusText, letOverJsonFilesStatusText, totalBackupsIsDeletedStatusText, useSimpleMailReportLayout, isOutputFolderContainFilesStatusText, isDaysToKeepNotDefaultStatusText);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Message($"Parsing, processing and collecting data for email report is done", EventType.Information, 1000);
                    Console.WriteLine($"Parsing, processing and collecting data for email report is done");
                    Console.ResetColor();
                    break;
                }

                // Not do work
                case true:
                    // Log
                    Message("Some of the 7 required arguments is missing: --token, --org, --outdir, --server, --port, --from and --to!", EventType.Error, 1001);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nSome of the 7 required arguments is missing: --token, --org, --outdir, --server, --port, --from and --to!");
                    Console.ResetColor();
                    break;
            }

            // End of program
            Message($"End of application - {AppName}, v." + _vData, EventType.Information, 1000);
            Console.WriteLine($"\nEnd of application - {AppName}, v. {_vData}\n");
        }

        private static bool CheckIfHaveSubfolders(string path)
        {
            if (Directory.GetDirectories(path).Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

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
                        DeleteDirectory(dir);

                        // Count files
                        _totalBackupsIsDeleted++;

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
                        _errors++;
                    }
                    catch (Exception e)
                    {
                        // Error if cant delete file(s)
                        Message("Exception caught when trying to delete old backup folder: " + dir + " - error: " + e, EventType.Error, 1001);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Exception caught when trying to delete old backup folder: " + dir + " - error: " + e);
                        Console.ResetColor();

                        // Add error to counter
                        _errors++;
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
                        DeleteDirectory(dir);

                        // Count files
                        _totalBackupsIsDeleted++;

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
                        _errors++;
                    }
                    catch (Exception e)
                    {
                        // Error if cant delete file(s)
                        Message("Exception caught when trying to delete old backup folder: " + dir + " - error: " + e, EventType.Error, 1001);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Exception caught when trying to delete old backup folder: " + dir + " - error: " + e);
                        Console.ResetColor();

                        // Add error to counter
                        _errors++;
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

        /*/// <summary>
        /// Parses all provided arguments
        /// </summary>
        /// <param name="args">String array with arguments passed to this console application</param>
        private static void ParseArguments(IList<string> args)
        {
            //TODO Use this and cleanup code - coming....
            UseSilentMode = args.Contains("-silent");

            if (args.Contains("-token"))
            {
                Token = args[args.IndexOf("-token") + 1];
            }

            if (args.Contains("--org"))
            {
                OrgName = args[args.IndexOf("--org") + 1];
            }

            if (args.Contains("--outdir"))
            {
                outFolder = args[args.IndexOf("--outdir") + 1];
            }

            if (args.Contains("-o"))
            {
                var outputTo = args[args.IndexOf("-o") + 1];

                // Verify that the target filename does not contain any invalid characters
                try
                {
                    new FileInfo(outputTo);
                    OutputFileName = outputTo;
                }
                catch (ArgumentException)
                {
                    OutputFileName = "ERROR";
                }
                catch (NotSupportedException)
                {
                    OutputFileName = "ERROR";
                }
            }

            if (args.Contains("--server"))
            {
                server = args[args.IndexOf("--server") + 1];
            }

            if (args.Contains("--port"))
            {
                if (int.TryParse(args[args.IndexOf("--port") + 1], out int timeout))
                {
                    Port = timeout;
                }
            }

            if (args.Contains("--from"))
            {
                from = args[args.IndexOf("--from") + 1];
            }

            if (args.Contains("--to"))
            {
                from = args[args.IndexOf("--to") + 1];
            }

            unZip = args.Contains("--unzip");
            cleanUp = args.Contains("--cleanup");

            if (args.Contains("--daystokeepbackup"))
            {
                if (short.TryParse(args[args.IndexOf("--daystokeepbackup") + 1], out short keepBackupsForDays))
                {
                    BackupDaysToKeep = keepBackupsForDays;
                }
            }

            UseHttps = args.Contains("--usessl");

            BackupStatisticsData = !args.Contains("-norrd");
            BackupPackageInfo = !args.Contains("-nopackage");

        }*/

        /// <summary>
        /// Shows the syntax
        /// </summary>
        private static void DisplayHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"\t{_currentExeFileName} --token <token> --org <organization> --outdir <folder> --server <smtpserver> ");
            Console.WriteLine("\t--port <25> --from <fromemail> --to <toemail>");
            Console.WriteLine();
            Console.WriteLine("Description:");
            Console.WriteLine("\tAzure DevOps Backup for Git Projects and is using the API for Azure DevOps");
            Console.WriteLine();
            Console.WriteLine("\tWhile the code is perfectly safe on the Azure infrastructure, there are cases where a centralized");
            Console.WriteLine("\tlocal backup of all projects and repositories is needed. These might include Corporate Policies,");
            Console.WriteLine("\tDisaster Recovery and Business Continuity Plans.");
            Console.WriteLine();
            Console.WriteLine("Parameter List:");
            Console.WriteLine("\t--token:             Token to access the API in Azure DevOps");
            Console.WriteLine("\t--org:               Name of the organization in Azure DevOps");
            Console.WriteLine("\t--outdir:            Folder where to store the backup(s) - folder with timestamp will be created");
            Console.WriteLine("\t--server:            IP address or DNS name of the SMTP server");
            Console.WriteLine("\t--port:              The port for the SMTP server");
            Console.WriteLine("\t--from:              The email address the report is send from");
            Console.WriteLine("\t--toemail:           The email address the report is send to");
            Console.WriteLine("\t--unzip:             Unzip downloaded .zip and .json files in --outdir (optional)");
            Console.WriteLine("\t--cleanup:           Delete downloaded .zip and .json files in --outdir after unzip (optional)");
            Console.WriteLine("\t--daystokeepbackup:  Number of days to keep backups for in --outdir. Backups older than this will");
            Console.WriteLine("\t\t\t     be deleted (default is 30 dayes) (optional)");
            Console.WriteLine("\t--simpelreport:      If set, the email report layout is simple");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine($"\t{_currentExeFileName} --token XXX... --org OrgName --outdir C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local\n");
            Console.WriteLine($"\t{_currentExeFileName} --token XXX... --org OrgName --outdir C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local --unzip\n");
            Console.WriteLine($"\t{_currentExeFileName} --token XXX... --org OrgName --outdir C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local --unzip --cleanup\n");
            Console.WriteLine($"\t{_currentExeFileName} --token XXX... --org OrgName --outdir C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local --unzip --daystokeepbackup 50");
            Console.WriteLine();
            Console.WriteLine("Output:");
            Console.WriteLine("\tA timestamped folder containing the backup will be created within this directory unless --outdir");
            Console.WriteLine("\tis being specified");
            Console.WriteLine();
            Console.WriteLine($"{AppName}, v." + _vData + " by " + _companyName);
            Console.WriteLine();
        }

        // If args is set to delete original downloaded .zip and .json files
        // Get output folder from backup with date for folder to backup to
        public static void Deletezipandjson(string outDir)
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
            if (_checkForLeftoverFilesAfterCleanup)
            {
                int numZip = di.GetFiles("*.zip").Length;
                int numJson = di.GetFiles("*.json").Length;

                _numJson = numJson;
                _numZip = numZip;
            }

            // Delete downloaded files after unzip
            _deletedFilesAfterUnzip = true;
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
                    _totalFilesIsDeletedAfterUnZipped++;

                    // Log
                    Message("Deleted downloaded file: " + filepath + " in backup folder: " + outDir,
                        EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Deleted downloaded file: " + filepath + " in backup folder: " +
                                      outDir);
                    Console.ResetColor();
                }
                catch (UnauthorizedAccessException)
                {
                    Message("Unable to delete downloaded file: " + filepath + " in backup folder: " + outDir + ". Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unable to delete downloaded file: " + filepath + " in backup folder: " + outDir + ". Make sure the account you use to run this tool has write rights to this location.");
                    Console.ResetColor();

                    // Count errors
                    _errors++;
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
                    _errors++;

                    // Check for letovers when error
                    _checkForLeftoverFilesAfterCleanup = true;
                }
                
                // Wait for work
                mre.Wait(timeout);
            }
        }        

        public static void SendEmail(string serveraddress, string serverPort, string emailFrom, string emailTo, string emailStatusMessage,
            List<string> repoCountElements, List<string> repoItemsCountElements, int repoCount, int repoItemsCount, int totalFilesIsBackupUnZipped,
            int totalBlobFilesIsBackup, int totalTreeFilesIsBackup, string outDir, string elapsedTime, string copyrightData, string vData, int errors,
            int totalFilesIsDeletedAfterUnZipped, int totalBackupsIsDeleted, string daysToKeep, string repoCountStatusText, string repoItemsCountStatusText,
            string totalFilesIsBackupUnZippedStatusText, string totalBlobFilesIsBackupStatusText, string totalTreeFilesIsBackupStatusText,
            string totalFilesIsDeletedAfterUnZippedStatusText, string letOverZipFilesStatusText, string letOverJsonFilesStatusText, string totalBackupsIsDeletedStatusText,
            bool useSimpleMailReportLayout, string isOutputFolderContainFilesStatusText, string isDaysToKeepNotDefaultStatusText)
        {
            var serverPortStr = serverPort;
            var mailBody = "";
            if (mailBody == null) throw new ArgumentNullException(nameof(mailBody));

            //Parse data to list from list of repo.name
            var listrepocountelements = "<h3>List of Git repositories in Azure DevOps:</h3>∘ " + string.Join("<br>∘ ", repoCountElements);
            var listitemscountelements = "<h3>List of Git repositories in Azure DevOps a backup is performed (Default branch):</h3>∘ " + string.Join("<br>∘ ", repoItemsCountElements);
            var letOverJsonFiles = 0;
            var letOverZipFiles = 0;

            // Add subject if cleanup after unzip is set
            if (_deletedFilesAfterUnzip)
            {
                emailStatusMessage += " (and cleaned up downloaded files)";
            }

            // It error count is over 0 add warning in email subject
            if (errors > 0)
            {
                emailStatusMessage += " - but with warning(s)";
            }

            // Get leftover files is needed (if had error(s)
            if (_checkForLeftoverFilesAfterCleanup)
            {
                letOverJsonFiles = _numJson;
                letOverZipFiles = _numZip;
            }

            // If args is set to old mail report layout
            if (useSimpleMailReportLayout)
            {
                // Make email body data
                mailBody =
                    $"<hr><h2>Your {AppName} is: {emailStatusMessage}</h2><hr><p><h3>Details:</h3><p>" +
                    $"<p>Processed Git repos in Azure DevOps (total): <b>{repoCount}</b><br>" +
                    $"Processed Git repos a backup is made of from Azure DevOps: <b>{repoItemsCount}</b><p>" +
                    $"Processed files to backup from Git repos (total unzipped if specified): <b>{totalFilesIsBackupUnZipped}</b><br>" +
                    $"Processed files to backup from Git repos (blob files (.zip files)): <b>{totalBlobFilesIsBackup}</b><br>" +
                    $"Processed files to backup from Git repos (tree files (.json files)): <b>{totalTreeFilesIsBackup}</b><p>" +
                    $"See the attached logfile for the backup(s) today: <b>{AppName} Log " + DateTime.Today.ToString("dd-MM-yyyy") + ".log</b>.<p>" +
                    $"Total Backup Run Time is: \"{elapsedTime}\"<br>" +
                    "<h3>Download cleanup (if specified):</h3><p>" +
                    $"Deleted original downloaded <b>.zip</b> and <b>.json</b> files in backup folder: <b>{totalFilesIsDeletedAfterUnZipped}</b><br>" +
                    $"Leftovers for original downloaded <b>.zip</b> files in backup folder (error(s) when try to delete): <b>{letOverZipFiles}</b><br>" +
                    $"Leftovers for original downloaded <b>.json</b> files in backup folder (error(s) when try to delete): <b>{letOverJsonFiles}</b><p>" +
                    $"<h3>Backup location:</h3><p>Backed up in folder: <b>\"{outDir}\"</b> on host/server: <b>{Environment.MachineName}</b><br>" +
                    $"Old backups set to keep in backup folder (days): <b>{daysToKeep}</b><br>" +
                    $"Old backups deleted in backup folder: <b>{totalBackupsIsDeleted}</b><br>" +
                    listrepocountelements + "<br>" +
                    listitemscountelements + "</p><hr>" +
                    $"<h3>From Your {AppName} tool!</h3></b><br>" +
                    copyrightData + ", v." + vData;
            }
            else
            {
                // Make email body data
                mailBody =
                $"<hr/><h2>Your {AppName} is: {emailStatusMessage}</h2><hr />" +
                $"<br><table style=\"border-collapse: collapse; width: 100%; height: 108px;\" border=\"1\">" +
                $"<tbody><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\"><strong>Backup task(s):</strong></td>" +
                $"<td style=\"width: 10%; height: 18px;\"><strong>File(s):</strong></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\"><strong>Status:</strong></td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\">Processed Git repos in Azure DevOps (total):</td>" +
                $"<td style=\"width: 10%; height: 18px;\"><b>{repoCount}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{repoCountStatusText}</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\">Processed Git repos a backup is made of from Azure DevOps:</td>" +
                $"<td style=\"width: 10%; height: 18px;\"><b>{repoItemsCount}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{repoItemsCountStatusText}</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\">Processed files to backup from Git repos (total unzipped if specified):</td>" +
                $"<td style=\"width: 10%; height: 18px;\"><b>{totalFilesIsBackupUnZipped}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{totalFilesIsBackupUnZippedStatusText}</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\">Processed files to backup from Git repos (blob files (.zip files)):</td>" +
                $"<td style=\"width: 10%; height: 18px;\"><b>{totalBlobFilesIsBackup}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{totalBlobFilesIsBackupStatusText}</td></tr><tr>" +
                $"<td style=\"width: 33%;\">Processed files to backup from Git repos (tree files (.json files)):</td>" +
                $"<td style=\"width: 10%;\"><b>{totalTreeFilesIsBackup}</b></td>" +
                $"<td style=\"width: 33.3333%;\">{totalTreeFilesIsBackupStatusText}</td></tr></tbody></table><br><table style=\"border-collapse: collapse; width: 100%; height: 108px;\" border=\"1\"><tbody><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\"><strong>Download cleanup (if specified):</strong></td>" +
                $"<td style=\"width: 10%; height: 18px;\"><strong>File(s):</strong></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\"><strong>Status:</strong></td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\">Deleted original downloaded .zip and .json files in backup folder:</td>" +
                $"<td style=\"width: 10%; height: 18px;\"><b>{totalFilesIsDeletedAfterUnZipped}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{totalFilesIsDeletedAfterUnZippedStatusText}</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\">Leftovers for original downloaded .zip files in backup folder (error(s) when try to delete):</td>" +
                $"<td style=\"width: 10%; height: 18px;\"><b>{letOverZipFiles}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{letOverZipFilesStatusText}</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\">Leftovers for original downloaded .json files in backup folder (error(s) when try to delete):</td>" +
                $"<td style=\"width: 10%; height: 18px;\"><b>{letOverJsonFiles}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{letOverJsonFilesStatusText}</td></tr></tbody></table><br><table style=\"border-collapse: collapse; width: 100%; height: 108px;\" border=\"1\"><tr>" +
                $"<td style=\"width: 21%; height: 18px;\"><strong>Backup:</strong></td>" +
                $"<td style=\"width: 22%; height: 18px;\"><strong>Info:</strong></td>" +
                $"<td style=\"width: 33%; height: 18px;\"><strong>Status:</strong></td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 21%; height: 18px;\">Backup folder:</td>" +
                $"<td style=\"width: 22%; height: 18px;\"><strong><b>\"{outDir}\"</b></b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{isOutputFolderContainFilesStatusText}</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 21%; height: 18px;\">Backup server:</td>" +
                $"<td style=\"width: 22%; height: 18px;\"><b>{Environment.MachineName}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">  </td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 21%; height: 18px;\">Old backup(s) set to keep in backup folder (days):</td>" +
                $"<td style=\"width: 22%; height: 18px;\"><b>{daysToKeep}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{isDaysToKeepNotDefaultStatusText}</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 21%; height: 18px;\">Old backup(s) deleted in backup folder:</td>" +
                $"<td style=\"width: 22%; height: 18px;\"><b>{totalBackupsIsDeleted}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{totalBackupsIsDeletedStatusText}</td></tr></table>" +
                $"<p>See the attached logfile for the backup(s) today: <b>{AppName} Log " + DateTime.Today.ToString("dd-MM-yyyy") + ".log</b>.<o:p></o:p></p>" +
                $"<p>Total Backup Run Time is: \"{elapsedTime}\".</p><hr/>" +
                listrepocountelements + "<br>" +
                listitemscountelements + "</p><br><hr>" +
                $"<h3>From Your {AppName} tool!<o:p></o:p></h3>" + copyrightData + ", v." + vData;
            }

            // Create mail
            var message = new MailMessage(emailFrom, emailTo);
            message.Subject = "[" + emailStatusMessage + $"] - {AppName} status - (" + totalBlobFilesIsBackup +
                              " Git projects backed up), " + errors + " issues(s) - (backups to keep (days): " + daysToKeep +
                              ", backup(s) deleted: " + totalBackupsIsDeleted + ")";

            message.Body = mailBody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            message.Priority = MailPriority.Normal;
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.None;
            message.BodyTransferEncoding = TransferEncoding.QuotedPrintable;
            
            // ReSharper disable once UnusedVariable
            var isParsable = Int32.TryParse(serverPortStr, out var serverPortNumber);
            var client = new SmtpClient(serveraddress, serverPortNumber)
            {
                EnableSsl = true,
                UseDefaultCredentials = true
            };

            // Log
            Message("Created email report and parsed data", EventType.Information, 1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Created email report and parsed data");
            Console.ResetColor();

            // Get all the files in the log dir for today

            // Log
            Message("Finding logfile for today to attach in email report...", EventType.Information, 1000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Finding logfile for today to attach in email report...");
            Console.ResetColor();

            // Get filename to find
            var filePaths = Directory.GetFiles(Files.LogFilePath, $"{AppName} Log " + DateTime.Today.ToString("dd-MM-yyyy") + "*.*");

            // Get the files that their extension are .log or .txt
            var files = filePaths.Where(filePath => Path.GetExtension(filePath).Contains(".log") || Path.GetExtension(filePath).Contains(".txt"));

            // Loop through the files enumeration and attach each file in the mail.
            foreach (var file in files)
            {
                _fileAttachedIneMailReport = file;

                // Log
                Message("Found logfile for today:", EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Found logfile for today:");
                Console.ResetColor();

                // Full file name   
                var fileName = _fileAttachedIneMailReport;
                var fi = new FileInfo(fileName);

                // Get File Name  
                var justFileName = fi.Name;
                Console.WriteLine("File name: " + justFileName);
                Message("File name: " + justFileName, EventType.Information, 1000);
                
                // Get file name with full path   
                var fullFileName = fi.FullName;
                Console.WriteLine("Full file name: " + fullFileName);
                Message("Full file name: " + fullFileName, EventType.Information, 1000);
                
                // Get file extension   
                var extn = fi.Extension;
                Console.WriteLine("File Extension: " + extn);
                Message("File Extension: " + extn, EventType.Information, 1000);
                
                // Get directory name   
                var directoryName = fi.DirectoryName;
                Console.WriteLine("Directory name: " + directoryName);
                Message("Directory name: " + directoryName, EventType.Information, 1000);
                
                // File Exists ?  
                var exists = fi.Exists;
                Console.WriteLine("File exists: " + exists);
                Message("File exists: " + exists, EventType.Information, 1000);
                if (fi.Exists)
                {
                    // Get file size  
                    var size = fi.Length;
                    Console.WriteLine("File Size in Bytes: " + size);
                    Message("File Size in Bytes: " + size, EventType.Information, 1000);
                    
                    // File ReadOnly ?  
                    var isReadOnly = fi.IsReadOnly;
                    Console.WriteLine("Is ReadOnly: " + isReadOnly);
                    Message("Is ReadOnly: " + isReadOnly, EventType.Information, 1000);
                    
                    // Creation, last access, and last write time   
                    var creationTime = fi.CreationTime;
                    Console.WriteLine("Creation time: " + creationTime);
                    Message("Creation time: " + creationTime, EventType.Information, 1000);
                    var accessTime = fi.LastAccessTime;
                    Console.WriteLine("Last access time: " + accessTime);
                    Message("Last access time: " + accessTime, EventType.Information, 1000);
                    var updatedTime = fi.LastWriteTime;
                    Console.WriteLine("Last write time: " + updatedTime + "\n");
                    Message("Last write time: " + updatedTime, EventType.Information, 1000);
                }

                // TODO Do not add more to logfile here - file is locked!
                var attachment = new Attachment(file);

                // Attach file to email
                message.Attachments.Add(attachment);
            }

            //Try to send email status email
            try
            {
                // Send the email
                client.Send(message);
                
                // Release files for the email
                message.Dispose();
                // TODO logfile is not locked from here - you can add logs to logfile again from here!

                // Log
                Message("Email notification is send to " + emailTo + " at " + DateTime.Now.ToString("dd-MM-yyyy (HH-mm)") + "!", EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Email notification is send to " + emailTo + " at " + DateTime.Now.ToString("dd-MM-yyyy (HH-mm)") + "!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                // Log
                Message("Sorry, we are unable to send email notification of your presence. Please try again! Error: " + ex, EventType.Error, 1001);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Sorry, we are unable to send email notification of your presence. Please try again! Error: " + ex);
                Console.ResetColor();
            }
        }
    }
}