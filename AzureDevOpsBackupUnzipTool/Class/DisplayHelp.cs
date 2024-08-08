using System;

namespace AzureDevOpsBackupUnzipTool.Class
{
    internal class DisplayHelpToConsole
    {
        public static void DisplayGuide()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"\t{Globals._currentExeFileName} --zipFile <zipFile> --jsonFile <jsonFile> --output <output>");
            Console.WriteLine();
            Console.WriteLine("Description:");
            Console.WriteLine("\tAzure DevOps Backup unzip tool for Git Projects (single repos to unzip from metadata) from the Azure DevOps API.");
            Console.WriteLine("\t(Part of Azure DevOps Backup tool from Michael Morten Sonne)");
            Console.WriteLine();
            Console.WriteLine("\tWhile the code is perfectly safe on the Azure infrastructure, there are cases where a centralized");
            Console.WriteLine("\tlocal backup of all projects and repositories is needed. These might include Corporate Policies,");
            Console.WriteLine("\tDisaster Recovery and Business Continuity Plans.");
            Console.WriteLine();
            Console.WriteLine("Parameter List:");
            Console.WriteLine("  Mandatory:");
            Console.WriteLine("\t--zipFile:           Name of the .zip folder to rename GUID´s to file or folders");
            Console.WriteLine("\t--jsonFile:          Name of the .json file with the metadata in to rename GUID´s to files and folders");
            Console.WriteLine("\t--output:            Folder to unzip data into");
            Console.WriteLine();
            Console.WriteLine("  Optional:");
            Console.WriteLine("\t--help, /h or /?:    Showing this help text for the tool");
            Console.WriteLine("\t--info or /about:    Showing information about the tool");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine($"\t{Globals._currentExeFileName} --zipFile \"C:\\Temp\\blob.zip\" --jsonFile \"C:\\Temp\\tree.json\" --output \"C:\\Temp\\Unzipped\"");
            Console.WriteLine();
            Console.WriteLine("Output:");
            Console.WriteLine("\tData from Git repo is renamed to the right filenames or folders based on the metadata from the Azure DevOps API.");
            Console.WriteLine("\tMapping the files together with the structure of the repo.");
            Console.WriteLine();
        }

        public static void DisplayInfo()
        {
            Console.WriteLine("Description:");
            Console.WriteLine("\tAzure DevOps Backup unzip tool for Git Projects (single repos to unzip from metadata) from the Azure DevOps API.");
            Console.WriteLine();
            Console.WriteLine("\tWhile the code is perfectly safe on the Azure infrastructure, there are cases where a centralized");
            Console.WriteLine("\tlocal backup of all projects and repositories is needed. These might include Corporate Policies,");
            Console.WriteLine("\tDisaster Recovery and Business Continuity Plans.");
            Console.WriteLine();
            Console.WriteLine("\tAzure DevOps is a cloud service to manage source code and collaborate between development teams.");
            Console.WriteLine("\tIt integrates perfectly with both Visual Studio and Visual Studio Code and other IDE´s and tools");
            Console.WriteLine("\tthere is using the 'Git'.");
            Console.WriteLine();
            Console.WriteLine("My blog:");
            Console.WriteLine("\thttps://blog.sonnes.cloud");
            Console.WriteLine();
            Console.WriteLine("My Website:");
            Console.WriteLine("\thttps://sonnes.cloud");
            Console.WriteLine();
            Console.WriteLine("See Microsoft´s website for more information about Azure DevOps:");
            Console.WriteLine("\thttps://azure.microsoft.com/en-us/products/devops");
            Console.WriteLine();
        }
    }
}