using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using AzureDevOpsBackup.Class;
using static AzureDevOpsBackup.Class.FileLogger;
using System.Net;

// ReSharper disable RedundantAssignment
// ReSharper disable NotAccessedVariable

namespace AzureDevOpsBackup
{
    internal struct Project
    {
        public string Name;
    }

    internal struct Projects
    {
        public List<Project> Value;
    }

    internal struct Repo
    {
        public string Id;
        public string Name;
    }

    internal struct Repos
    {
        public List<Repo> Value;
    }

    internal struct Item
    {
        public string ObjectId;
        public string GitObjectType;
        // public string CommitId;
        public string Path;
        public bool IsFolder;
        public string Url;
    }

    internal struct Items
    {
        public int Count;
        public List<Item> Value;
    }

    struct Branch
    {
        public string Name;
        public string ObjectId;
        public string Url;
    }

    struct Branches
    {
        public int Count;
        public List<Branch> Value;
    }

    internal class Program
    {
        private static bool _cleanUpState;

        private static void Main(string[] args)
        {
            // Global variabels
            int projectCount = 0;
            int totalFilesIsBackupUnZipped = 0;
            int totalBlobFilesIsBackup = 0;
            int totalTreeFilesIsBackup = 0;
            int repoItemsCount = 0;
            int repoCount = 0;
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
            bool noProjectsToBackup = false;
            bool isOutputFolderContainFiles = false;

            // Check requirements for tool
            Requirements.SystemCheck();

            // Get key to use for encryption
            var key = SecureArgumentHandlerToken.GetComputerId();

            // Get application data to later use in tool
            AssemblyCopyrightAttribute copyright = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute;
            // ReSharper disable once PossibleNullReferenceException
            Globals._copyrightData = copyright.Copyright;
            Globals._vData = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
            
            var attributes = typeof(Program).GetTypeInfo().Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute));
            var assemblyTitleAttribute = attributes.SingleOrDefault() as AssemblyTitleAttribute;

            // Set application name in code
            Globals.AppName = assemblyTitleAttribute?.Title;
            Globals._currentExeFileName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName);

            var fileName = Assembly.GetEntryAssembly()?.Location;
            if (fileName != null)
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(fileName);
                Globals._companyName = versionInfo.CompanyName;
            }

            // Start timer for runtime
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            DateTime startTime = DateTime.Now; // get current time as start time

            // Log start of program
            Globals.ApplicationStartMessage();

            // Set Global Logfile properties
            FileLogger.DateFormat = "dd-MM-yyyy";
            DateTimeFormat = "dd-MM-yyyy HH:mm:ss";
            WriteOnlyErrorsToEventLog = false;
            WriteToEventLog = false;
            WriteToFile = true;
            
            // Log
            Message("Loaded log configuration into the program: " + Globals.AppName, EventType.Information, 1000);

            // Check for required Args for application will work
            string[] requiredArgs = { "--token", "--org", "--backup", "--server", "--port", "--from", "--to" };
            
            // Check if parameters have been provided and Contains one of
            if (args.Length == 0 || args.Contains("--help") || args.Contains("/h") || args.Contains("/?") || args.Contains("/info") || args.Contains("/about") || args.Contains("--tokenfile"))
            {
                // If none arguments
                if (args.Length == 0)
                {
                    // No arguments have been provided
                    Message("ERROR: No arguments is provided - try again!", EventType.Error, 1001);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR: No arguments is provided - try again!\n");
                    Console.ResetColor();

                    DisplayHelpToConsole.DisplayGuide();

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
                    DisplayHelpToConsole.DisplayGuide();

                    Message($"Showed help to Console - Exciting {Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName + "!", EventType.Information, 1000);

                    Console.ResetColor();

                    // End application
                    Environment.Exit(1);
                }

                // If wants information about application
                if (args.Contains("/info") || args.Contains("/about"))
                {
                    DisplayHelpToConsole.DisplayInfo();

                    Message($"Showed information about application to Console - Exciting {Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName + "!", EventType.Information, 1000);

                    Console.ResetColor();

                    // End application
                    Environment.Exit(1);
                }
                
                // If wants to save token data as file for encryption
                if (args.Contains("--tokenfile"))
                {
                    // Get data fon console
                    string tokentoencrypt = args[Array.IndexOf(args, "--tokenfile") + 1];
                    
                    // Encrypt data
                    SecureArgumentHandlerToken.EncryptAndSaveToFile(key, tokentoencrypt);

                    Message($"Saved information about token to file - Exciting {Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName + "!", EventType.Information, 1000);

                    Console.ResetColor();

                    // End application
                    Environment.Exit(1);
                }
            }

            // Log
            Message("Checking if the 7 required arguments is present (--token, --org, --backup, --server, --port, --from, --to)", EventType.Information, 1000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Checking if the 7 required arguments is present (--token, --org, --backup, --server, --port, --from, --to)...");
            Console.ResetColor();

            var i = args.Length > 1;
            switch (i)
            {
                // Parse the provided arguments
                // ParseArguments(args);
                // If okay do some work
                case true when args.Intersect(requiredArgs).Count() == 7:
                //case true when args.Intersect(requiredArgs).Count() == 6:
                    {
                        // Startup log entry
                        Message("Checked if the 7 required arguments is present (--token, --org, --backup, --server, --port, --from, --to) - all is fine!", EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Checked if the 7 required arguments is present (--token, --org, --backup, --server, --port, --from, --to) - all is fine!");
                        Console.ResetColor();

                        // Set the email priority level based on command line argument
                        int priorityIndex = Array.IndexOf(args, "--priority");
                        if (priorityIndex != -1 && args.Length > priorityIndex + 1)
                        {
                            string priorityArg = args[priorityIndex + 1].ToLower();
                            switch (priorityArg)
                            {
                                case "low":
                                    Globals.EmailPriority = System.Net.Mail.MailPriority.Low;
                                    break;
                                case "high":
                                    Globals.EmailPriority = System.Net.Mail.MailPriority.High;
                                    break;
                                default:
                                    Message("Invalid email priority argument. Defaulting to normal Mail Priority.", EventType.Warning, 1000);
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Invalid email priority argument. Defaulting to normal Mail Priority.");
                                    break;
                            }
                            Message("Email report priority arguments is set to: " + Globals.EmailPriority, EventType.Information, 1000);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Email report priority arguments is set to: " + Globals.EmailPriority);
                        }

                        // Start URL parse to AIP access
                        Message("Starting connection to Azure DevOps API from data provided from arguments", EventType.Information, 1000);
                        Console.WriteLine("Starting connection to Azure DevOps API from data provided from arguments");

                        // Base GET API
                        const string version = "api-version=7.0";
                        string baseUrl = "https://dev.azure.com/" + args[Array.IndexOf(args, "--org") + 1] + "/";
                        
                        SecureArgumentHandler handler = new SecureArgumentHandler();

                        // Get the values of the --token and --org arguments
                        string token = args[Array.IndexOf(args, "--token") + 1];

                        // If set to use token file
                        if (token == "token.bin")
                        {
                            // Read the token information from the -tokentofile
                            token = SecureArgumentHandlerToken.DecryptFromFile(key);
#if DEBUG
                            //Console.WriteLine($"Decrypted string for token = {token}");
                            //Console.ReadKey();
#endif
                        }

                        // Encrypt the value
                        byte[] encryptedToken = handler.Encrypt(token);
                        
                        /*
                         * Clear string token
                         */
                        // Get the length of the string
                        int length = token.Length;

                        // Create a byte array with the same length as the string
                        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);

                        // Overwrite the bytes with zeros
                        Array.Clear(tokenBytes, 0, length);

                        // Set the string to null
                        token = null;
                        /*
                         * End clear string token
                         */

                        // Convert the encrypted values to base64 strings
                        string encryptedTokenString = Convert.ToBase64String(encryptedToken);

                        // Build the auth and baseUrl strings using the encrypted values
                        string auth = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{""}:{handler.Decrypt(Convert.FromBase64String(encryptedTokenString))}"));

                        // Set global value
                        Globals._orgName = args[Array.IndexOf(args, "--org") + 1];

                        // URL parse to API access done
                        Message("Base URL is for Organization is: " + baseUrl, EventType.Information, 1000);
                        Console.WriteLine("Base URL is for Organization is: " + baseUrl);

                        // Get output folder to backup (not with date stamp for backup folder name)
                        string outBackupDir = args[Array.IndexOf(args, "--backup") + 1] + "\\";

                        // Set output folder name
                        string todaysdate = DateTime.Now.ToString("dd-MM-yyyy-(HH-mm)");
                        string outDir = $"{outBackupDir}{todaysdate}\\";

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
                                Globals._errors++;
                            }
                            catch (Exception e)
                            {
                                // Error when create backup folder
                                Message("Exception caught when trying to create output folder - error: " + e, EventType.Error, 1001);
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("{0} Exception caught.", e);
                                Console.ResetColor();

                                // Count errors
                                Globals._errors++;
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
                        
                        // Get connection status from REST API
                        var checkConnectionToAzureDevOps = new RestClient(baseUrl + "_apis/projects?" + version);
                        var checkConnectionToAzureDevOpsGet = new RestRequest(Method.GET);

                        // Connect
                        checkConnectionToAzureDevOpsGet.AddHeader("Authorization", auth);
                        IRestResponse responsecheckConnectionToAzureDevOpsGet = checkConnectionToAzureDevOps.Execute(checkConnectionToAzureDevOpsGet);

                        if (responsecheckConnectionToAzureDevOpsGet.StatusCode == HttpStatusCode.OK)
                        {
                            Message("Connected successfully to Azure DevOps organization '" + Globals._orgName + "'...", EventType.Information, 1000);
                            Console.WriteLine("Connected successfully to Azure DevOps organization '" + Globals._orgName + "'...");
                        }
                        else
                        {
                            // Handle error cases
                            Console.WriteLine("Failed to connected successfully to the Azure DevOps organization '" + Globals._orgName + "' via REST API");
                            Console.WriteLine("Response Status: " + responsecheckConnectionToAzureDevOpsGet.StatusCode);
                            Console.WriteLine("Response Content: " + responsecheckConnectionToAzureDevOpsGet.Content);

                            Message("Failed to connected successfully to the Azure DevOps organization '" + Globals._orgName + "' via REST API", EventType.Error, 1001);
                            Message("Response Status: " + responsecheckConnectionToAzureDevOpsGet.StatusCode, EventType.Error, 1001);
                            Message("Response Content: " + responsecheckConnectionToAzureDevOpsGet.Content, EventType.Error, 1001);

                            Message("Exiting...", EventType.Information, 1000);
                            Console.WriteLine("Exiting...");

                            Environment.Exit(1);
                        }

                        // Save log entry
                        Message("Getting information form Azure DevOps organization " + Globals._orgName + "...", EventType.Information, 1000);
                        Console.WriteLine("Getting information form Azure DevOps organization " + Globals._orgName + "...");

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

                        // Projects
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
                                    // Count total repos got
                                    repoCount++;

                                    // Log
                                    Message("Getting information about Git repository is project: " + repo.Name, EventType.Information, 1000);
                                    Console.WriteLine("Getting information about Git repository in project: " + repo.Name);
                                    
                                    // Branches
                                    var branches = new RestClient(baseUrl + "_apis/git/repositories/" + repo.Id + "/refs?" + version);
                                    var requestBranches = new RestRequest(Method.GET);
                                    requestBranches.AddHeader("Authorization", auth);
                                    IRestResponse responseBranches = branches.Execute(requestBranches);
                                    Branches branchResponse = JsonConvert.DeserializeObject<Branches>(responseBranches.Content);
                                    
                                    foreach (var branch in branchResponse.Value)
                                    {
                                        // Get branch name
                                        string branchName = branch.Name.Replace("refs/heads/", "");

                                        // Get data to find in specific branch
                                        var clientItems = new RestClient(baseUrl + "_apis/git/repositories/" + repo.Id + "/items?recursionlevel=full&" + version + "&versionDescriptor.versionType=Branch&versionDescriptor.version=" + branchName);
                                        var requestItems = new RestRequest(Method.GET);

                                        // List name for projects to list for email report list
                                        repocountelements.Add(repo.Name + $" ('{branchName}' branch)");

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
                                            repoitemscountelements.Add(repo.Name + $" ('{branchName}' branch)");

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
                                                //using (FileStream fs = new FileStream(outDir + project.Name + "_" + repo.Name + "_blob.zip", FileMode.Create))
                                                using (FileStream fs = new FileStream(outDir + project.Name + "_" + repo.Name + $"_{branchName}_blob.zip", FileMode.Create))
                                                {
                                                    fs.Write(data, 0, data.Length);
                                                }

                                                // Log
                                                Message("Saved file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_blob.zip", EventType.Information, 1000);
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine("Saved file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_blob.zip");
                                                Console.ResetColor();

                                                // Count files there is downloaded
                                                totalBlobFilesIsBackup++;

                                                //Set backup status
                                                isBackupOk = true;
                                                isBackupOkAndUnZip = false;
                                            }
                                            catch (UnauthorizedAccessException)
                                            {
                                                Message("Unable to write the backup file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_blob.zip. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine("Unable to write the backup file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_blob.zip. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                                Console.ResetColor();

                                                // Count errors
                                                Globals._errors++;
                                            }
                                            catch (Exception e)
                                            {
                                                // Error
                                                Message("Exception caught when trying to save file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_blob.zip - error: " + e, EventType.Error, 1001);
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine("Exception caught when trying to save file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_blob.zip - error: " + e);
                                                Console.ResetColor();

                                                // Set backup status
                                                isBackupOk = false;
                                                isBackupOkAndUnZip = false;

                                                // Count errors
                                                Globals._errors++;
                                            }

                                            //Save to disk - _tree.json
                                            try
                                            {
                                                // Save file to disk
                                                //File.WriteAllText(outDir + project.Name + "_" + repo.Name + "_tree.json", responseItems.Content);
                                                File.WriteAllText(outDir + project.Name + "_" + repo.Name + $"_{branchName}_tree.json", responseItems.Content);
                                                
                                                // Log
                                                Message("Saved file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_tree.json", EventType.Information, 1000);
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine("Saved file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_tree.json");
                                                Console.ResetColor();

                                                // Count files there is downloaded
                                                totalTreeFilesIsBackup++;

                                                // Set backup status
                                                isBackupOk = true;
                                                isBackupOkAndUnZip = false;
                                            }
                                            catch (UnauthorizedAccessException)
                                            {
                                                Message("Unable to write the backup file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_tree.json. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine("Unable to write the backup file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_tree.json. Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                                Console.ResetColor();

                                                // Count errors
                                                Globals._errors++;
                                            }
                                            catch (Exception e)
                                            {
                                                // Error
                                                Message("Exception caught when trying to save file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_tree.json - error: " + e, EventType.Error, 1001);
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine("Exception caught when trying to save file to disk: " + outDir + project.Name + "_" + repo.Name + $"_{branchName}_tree.json - error: " + e);
                                                Console.ResetColor();

                                                // Set backup status
                                                isBackupOk = false;
                                                isBackupOkAndUnZip = false;

                                                // Count errors
                                                Globals._errors++;
                                            }

                                            // If args is set to unzip files
                                            if (Array.Exists(args, argument => argument == "--unzip"))
                                            {
                                                // If need to unzip data from .zip and .json (files details is in here)
                                                Message("Checking if folder exists before unzip: " + outDir + project.Name + "_" + repo.Name, EventType.Information, 1000);
                                                Console.WriteLine("Checking if folder exists before unzip: " + outDir + project.Name + "_" + repo.Name);

                                                // Check if folder to unzip exists

                                                string localRepoDirectory = outDir + project.Name + "_" + repo.Name + $"_{branchName}";

                                                //if (Directory.Exists(outDir + project.Name + "_" + repo.Name))
                                                if (Directory.Exists(localRepoDirectory))
                                                {
                                                    // Check if an old folder exists, then try to delete it
                                                    try
                                                    {
                                                        // Do work
                                                        Directory.Delete(localRepoDirectory, true);

                                                        // Log
                                                        Message("Folder exists before unzip: " + localRepoDirectory + ", deleting folder...", EventType.Information, 1000);
                                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                                        Console.WriteLine("Folder exists before unzip: " + localRepoDirectory + ", deleting folder...");
                                                        Console.ResetColor();

                                                        // Set backup status
                                                        isBackupOkAndUnZip = true;
                                                    }
                                                    catch (UnauthorizedAccessException)
                                                    {
                                                        Message("Unable to delete folder under unzip: " + localRepoDirectory + ". Make sure the account you use to run this tool has delete rights to this location.", EventType.Error, 1001);
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine("Unable to delete folder under unzip: " + localRepoDirectory + ". Make sure the account you use to run this tool has delete rights to this location.");
                                                        Console.ResetColor();

                                                        // Count errors
                                                        Globals._errors++;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        // Error
                                                        Message("Exception caught when trying to delete folder when unzip: " + localRepoDirectory + " - error: " + e, EventType.Error, 1001);
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine("Exception caught when trying to delete folder when unzip: " + localRepoDirectory + " - error: " + e);
                                                        Console.ResetColor();

                                                        // Set backup status
                                                        isBackupOkAndUnZip = false;

                                                        // Count errors
                                                        Globals._errors++;
                                                    }

                                                    // Check if done delete folder
                                                    if (!Directory.Exists(localRepoDirectory))
                                                    {
                                                        // Log
                                                        Console.ForegroundColor = ConsoleColor.Green;
                                                        Console.WriteLine($"Directory " + localRepoDirectory + " is deleted successfully");
                                                        Console.ResetColor();
                                                        Message("Directory " + localRepoDirectory + " is deleted successfully", EventType.Information, 1000);

                                                        // Set backup status
                                                        isBackupOkAndUnZip = true;
                                                    }
                                                    else
                                                    {
                                                        // Log
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine($"Directory " + localRepoDirectory + " is not deleted successfully - see logs for more information");
                                                        Console.ResetColor();
                                                        Message("Directory " + localRepoDirectory + " is not deleted successfully - see logs for more information", EventType.Error, 1001);

                                                        // Set backup status
                                                        isBackupOkAndUnZip = false;

                                                        // Count errors
                                                        Globals._errors++;
                                                    }
                                                }
                                                else
                                                {
                                                    // if not folder to unzip exists
                                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                                    Console.WriteLine($"Folder to unzip files from does not exists: " + localRepoDirectory);
                                                    Console.ResetColor();
                                                    Message("Folder to unzip files from does not exists: " + localRepoDirectory, EventType.Warning, 1001);
                                                }

                                                // Do work to start over - create folder to files
                                                // Create folder when not exists
                                                try
                                                {
                                                    // Do work
                                                    Directory.CreateDirectory(localRepoDirectory);

                                                    // Log
                                                    Message(
                                                        "Checked if folder exists before unzip: " + localRepoDirectory + " - The folder does not exist, creating folder: " + localRepoDirectory, EventType.Information, 1000);
                                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                                    Console.WriteLine("Checked if folder exists before unzip: " + localRepoDirectory + " - The folder does not exist, creating folder: " + localRepoDirectory);
                                                    Console.ResetColor();

                                                    // Set backup status
                                                    isBackupOkAndUnZip = true;
                                                }
                                                catch (UnauthorizedAccessException)
                                                {
                                                    Message("Unable to create folder under unzipping: " + localRepoDirectory + ". Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("Unable to create folder under unzipping: " + localRepoDirectory + ". Make sure the account you use to run this tool has write rights to this location.");
                                                    Console.ResetColor();

                                                    // Count errors
                                                    Globals._errors++;
                                                }
                                                catch (Exception e)
                                                {
                                                    // Error
                                                    Message("Exception caught when trying to creating folder: " + localRepoDirectory + " - error: " + e, EventType.Error, 1001);
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("{0} Exception caught.", e);
                                                    Console.ResetColor();

                                                    // Set backup status
                                                    isBackupOkAndUnZip = false;
                                                    Globals._errors++;
                                                }

                                                // Get files from .zip folder to unzip
                                                //ZipArchive archive = ZipFile.OpenRead(outDir + project.Name + "_" + repo.Name + "_blob.zip");
                                                ZipArchive archive = ZipFile.OpenRead(outDir + project.Name + "_" + repo.Name + $"_{branchName}_blob.zip");
                                                
                                                foreach (Item item in items.Value)
                                                {
                                                    // Work on all files/folders
                                                    if (item.IsFolder)
                                                    {
                                                        // If folder data
                                                        Message("Unzipping Git repository folder data: " + localRepoDirectory + item.Path, EventType.Information, 1000);
                                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                                        Console.WriteLine("Unzipping Git repository folder data: " + localRepoDirectory + item.Path);
                                                        Console.ResetColor();

                                                        try
                                                        {
                                                            // Do work
                                                            Directory.CreateDirectory(localRepoDirectory + item.Path);

                                                            // Log
                                                            Console.ForegroundColor = ConsoleColor.Green;
                                                            Console.WriteLine($"Created folder: " + localRepoDirectory + item.Path);
                                                            Console.ResetColor();
                                                            Message("Created folder: " + localRepoDirectory + item.Path, EventType.Information, 1000);

                                                            // Set backup status
                                                            isBackupOkAndUnZip = true;
                                                        }
                                                        catch (UnauthorizedAccessException)
                                                        {
                                                            Message("Unable to create folder under unzipping: " + localRepoDirectory + item.Path + ". Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                                            Console.ForegroundColor = ConsoleColor.Red;
                                                            Console.WriteLine("Unable to create folder under unzipping: " + localRepoDirectory + item.Path + ". Make sure the account you use to run this tool has write rights to this location.");
                                                            Console.ResetColor();

                                                            // Count errors
                                                            Globals._errors++;
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            // Log
                                                            Message("Exception caught when trying to create folder: " + localRepoDirectory + item.Path + " - error: " + e, EventType.Error, 1001);
                                                            Console.ForegroundColor = ConsoleColor.Red;
                                                            Console.WriteLine("{0} Exception caught.", e);
                                                            Console.ResetColor();

                                                            // Set backup status
                                                            isBackupOkAndUnZip = false;

                                                            // Count errors
                                                            Globals._errors++;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // If file data
                                                        try
                                                        {
                                                            //Try to save data to disk
                                                            archive.GetEntry(item.ObjectId).ExtractToFile(localRepoDirectory + item.Path, true);

                                                            // Log
                                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                                            Console.WriteLine($"Unzipping Git repository file data on disk: " + localRepoDirectory + item.Path);
                                                            Console.ResetColor();
                                                            Message("Unzipping Git repository file data on disk: " + localRepoDirectory + item.Path, EventType.Information, 1000);

                                                            // Count
                                                            totalFilesIsBackupUnZipped++;

                                                            // Set backup status
                                                            isBackupOkAndUnZip = true;
                                                        }
                                                        catch (UnauthorizedAccessException)
                                                        {
                                                            Message("Unable to create file under unzipping: " + localRepoDirectory + item.Path + ". Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                                                            Console.ForegroundColor = ConsoleColor.Red;
                                                            Console.WriteLine("Unable to create file under unzipping: " + localRepoDirectory + item.Path + ". Make sure the account you use to run this tool has write rights to this location.");
                                                            Console.ResetColor();

                                                            // Count errors
                                                            Globals._errors++;
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            // Error
                                                            Message("Exception caught when trying to create data on disk: " + localRepoDirectory + item.Path + " - error: " + e, EventType.Error, 1001);
                                                            Console.ForegroundColor = ConsoleColor.Green;
                                                            Console.WriteLine("Exception caught when trying to create data on disk: " + localRepoDirectory + item.Path + " - error: " + e);
                                                            Console.ResetColor();

                                                            // Set backup status
                                                            isBackupOkAndUnZip = false;

                                                            // Count errors
                                                            Globals._errors++;
                                                        }
                                                    }
                                                }

                                                // When done unzip
                                                Message("Unzipping Git repository: " + localRepoDirectory + " is now done", EventType.Information, 1000);
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine("Unzipping Git repository: " + localRepoDirectory + " is now done\n");
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
                            }

                            // When done backup
                            Message("No more projets to work with for now...", EventType.Information, 1000);
                            Message("Done with " + repoCount + " project(s) in Azure DevOps", EventType.Information, 1000);
                            Message("Done with " + repoItemsCount + " repositories to backup in folder: " + outDir + " on host: " + Environment.MachineName, EventType.Information, 1000);
                            Message("Processed files to backup from Git repos (total unzipped if specified): " + totalFilesIsBackupUnZipped, EventType.Information, 1000);
                            Message("Processed files to backup from Git repos (blob files (.zip files)) (all branches): " + totalBlobFilesIsBackup, EventType.Information, 1000);
                            Message("Processed files to backup from Git repos (tree files (.json files)) (all branches): " + totalTreeFilesIsBackup, EventType.Information, 1000);

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("== No more projects to work with for now ==\n");
                            Console.WriteLine("Done with " + repoCount + " project(s) in Azure DevOps");
                            Console.WriteLine("Done with " + repoItemsCount + " repositories to backup in folder: " + outDir + " on host: " + Environment.MachineName);
                            Console.WriteLine("Processed files to backup from Git repos (total unzipped if specified): " + totalFilesIsBackupUnZipped);
                            Console.WriteLine("Processed files to backup from Git repos (blob files (.zip files)) (all branches): " + totalBlobFilesIsBackup);
                            Console.WriteLine("Processed files to backup from Git repos (tree files (.json files)) (all branches): " + totalTreeFilesIsBackup);
                            Console.ResetColor();

                            // Stop timer
                            stopWatch.Stop();
                            DateTime endTime = DateTime.Now; // get current time as end time

                            // Get the elapsed time as a TimeSpan value.
                            TimeSpan ts = stopWatch.Elapsed;

                            // Format and display the TimeSpan value.
                            // string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                ts.Hours, ts.Minutes, ts.Seconds,
                                ts.Milliseconds / 10);

                            Globals._startTime = startTime.ToString("dd-MM-yyyy HH:mm:ss"); // convert start time to string
                            Globals._endTime = endTime.ToString("dd-MM-yyyy HH:mm:ss"); // convert end time to string

                            // Log
                            Message("Backup Run Time: " + elapsedTime, EventType.Information, 1000);
                            Message("Backup start Time: " + Globals._startTime, EventType.Information, 1000);
                            Message("Backup end Time: " + Globals._endTime, EventType.Information, 1000);
                            
                            Console.WriteLine("\nBackup Run Time: " + elapsedTime);
                            Console.WriteLine("\nBackup start Time: " + Globals._startTime);
                            Console.WriteLine("\nBackup end Time: " + Globals._endTime);

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
                                        Backups.DaysToKeepBackupsDefault(outBackupDir);
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
                                        Backups.DaysToKeepBackups(outBackupDir, daysToKeepBackups);
                                    }
                                }
                                else
                                {
                                    // Set default
                                    Backups.DaysToKeepBackupsDefault(outBackupDir);
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
                                Backups.DaysToKeepBackupsDefault(outBackupDir);
                            }

                            // Cleanup old log files
                            CleanupLog.CleanupLogs();

                            // Get email status text from job status
                            if (isBackupOk)
                            {
                                // If unzipped or not
                                emailStatusMessage = isBackupOkAndUnZip ? "Success and unzipped" : "Success, not unzipped";
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
                            //DateTime endTime = DateTime.Now; // get current time as end time
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
                                Files.DeleteZipAndJson(outDir);

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
                            Message($"Processed Git project(s) in Azure DevOps (total) status:" + repoCountStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Processed Git project(s) in Azure DevOps (total) status: " + repoCountStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            if (isBackupOk)
                            {
                                repoCountStatusText = "Good";

                                // Log
                                Message($"Processed Git project(s) in Azure DevOps (total) status: " + repoCountStatusText, EventType.Information, 1000);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Processed Git repos in Azure DevOps (total) status: " + repoCountStatusText);
                                Console.ResetColor();
                            }
                            else
                            {
                                repoCountStatusText = "Warning!";

                                // Log
                                Message($"Processed Git project(s) in Azure DevOps (total) status: " + repoCountStatusText, EventType.Warning, 1001);
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
                            Message($"Processed Git repos in project(s) a backup is made of from Azure DevOps status: " + repoItemsCountStatusText, EventType.Warning, 1001);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Processed Git repos in project(s) a backup is made of from Azure DevOps status: " + repoItemsCountStatusText);
                            Console.ResetColor();
                        }
                        else
                        {
                            if (isBackupOk)
                            {
                                repoItemsCountStatusText = "Good";

                                // Log
                                Message($"Processed Git repos in project(s) a backup is made of from Azure DevOps status: " + repoItemsCountStatusText, EventType.Information, 1000);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Processed Git repos in project(s) a backup is made of from Azure DevOps status: " + repoItemsCountStatusText);
                                Console.ResetColor();
                            }
                            else
                            {
                                repoItemsCountStatusText = "Warning!";

                                // Log
                                Message($"Processed Git repos in project(s) a backup is made of from Azure DevOps status: " + repoItemsCountStatusText, EventType.Warning, 1001);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Processed Git repos in project(s) a backup is made of from Azure DevOps status: " + repoItemsCountStatusText);
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
                                totalFilesIsBackupUnZippedStatusText = "Good (and unzip is OK)";

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
                                totalBlobFilesIsBackupStatusText = "Good";

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
                                totalTreeFilesIsBackupStatusText = "Good";

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
                        if (Globals._totalFilesIsDeletedAfterUnZipped != totalFilesThereShouldBeDeleted)
                        {
                            if (Globals._totalFilesIsDeletedAfterUnZipped != 0)
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
                                totalFilesIsDeletedAfterUnZippedStatusText = isBackupOk ? "Good - not set to cleanup!" : "Warning - nothing to backup!";

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
                                totalFilesIsDeletedAfterUnZippedStatusText = "Good (set to cleanup, and matched the total files downloaded)";

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
                        if (Globals._numZip != 0)
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
                        if (Globals._numJson != 0)
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
                        if (Globals._totalBackupsIsDeleted != 0)
                        {
                            totalBackupsIsDeletedStatusText = "Good - deleted " + Globals._totalBackupsIsDeleted + " old backup(s) from backup folder";

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
                        Message("Checking if directory " + outDir + " contains files", EventType.Information, 1000);

                        // Check if done for status in mail report
                        if (Directory.Exists(outDir) && (Directory.EnumerateFiles(outDir, "*.zip").FirstOrDefault() != null))
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
                                Globals._errors++;
                            }
                        }

                        // Get status for output folder
                        if (isOutputFolderContainFiles)
                        {
                            isOutputFolderContainFilesStatusText = "Checked - folder is containing original downloaded files";

                            if (Folders.CheckIfHaveSubfolders(outDir))
                            {
                                isOutputFolderContainFilesStatusText += ", but has also subfolders with unzipped backup(s)";
                            }
                        }
                        else
                        {
                            isOutputFolderContainFilesStatusText = "Checked - folder is NOT containing original downloaded files";
                            if (Folders.CheckIfHaveSubfolders(outDir))
                            {
                                isOutputFolderContainFilesStatusText += ", but has subfolders with unzipped backup(s)";
                            }
                        }

                        // Log
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(isOutputFolderContainFilesStatusText);
                        Console.ResetColor();
                        Message(isOutputFolderContainFilesStatusText, EventType.Information, 1000);
                        
                        // Count backups in backup folder
                        Backups.CountCurrentNumersOfBackup(outBackupDir);

                        // Log
                        Message($"Getting status for tasks for email report is done", EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Getting status for tasks for email report is done");

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Message($"Parsing, processing and collecting data for email report", EventType.Information, 1000);
                        Console.WriteLine($"\nParsing, processing and collecting data for email report");
                        Console.ResetColor();

                        // If args is set to old mail report layout
                        var useSimpleMailReportLayout = Array.Exists(args, argument => argument == "--simpelreport");

                        // Send status email and parse data to function
                        ReportSender.SendEmail(server, serverPort, emailFrom, emailTo, emailStatusMessage, repocountelements,
                            repoitemscountelements, repoCount, repoItemsCount, totalFilesIsBackupUnZipped, totalBlobFilesIsBackup,
                            totalTreeFilesIsBackup, outDir, elapsedTime, Globals._errors, Globals._totalFilesIsDeletedAfterUnZipped,
                            Globals._totalBackupsIsDeleted, daysToKeepBackups, repoCountStatusText, repoItemsCountStatusText,
                            totalFilesIsBackupUnZippedStatusText, totalBlobFilesIsBackupStatusText, totalTreeFilesIsBackupStatusText,
                            totalFilesIsDeletedAfterUnZippedStatusText, letOverZipFilesStatusText, letOverJsonFilesStatusText,
                            totalBackupsIsDeletedStatusText, useSimpleMailReportLayout, isOutputFolderContainFilesStatusText,
                            isDaysToKeepNotDefaultStatusText, Globals._startTime, Globals._endTime,
                            Globals._deletedFilesAfterUnzip, Globals._checkForLeftoverFilesAfterCleanup); 

                        Console.ForegroundColor = ConsoleColor.Green;
                        Message($"Parsing, processing and collecting data for email report is done", EventType.Information, 1000);
                        Console.WriteLine($"Parsing, processing and collecting data for email report is done");
                        Console.ResetColor();
                        break;
                    }

                // Not do the work
                case true:
                    // Log
                    Message("Some of the 7 required arguments is missing: --token, --org, --backup, --server, --port, --from and --to!", EventType.Error, 1001);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nSome of the 7 required arguments is missing: --token, --org, --backup, --server, --port, --from and --to!");
                    Console.ResetColor();
                    break;
            }

            // Log end of program
            Globals.ApplicationEndMessage();
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

            if (args.Contains("--backup"))
            {
                outFolder = args[args.IndexOf("--backup") + 1];
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
    }
}