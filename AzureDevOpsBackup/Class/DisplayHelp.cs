using System;

namespace AzureDevOpsBackup.Class
{
    internal class DisplayHelpToConsole
    {
        public static void DisplayGuide()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"\t{Globals._currentExeFileName} --token <token> --org <organization> --backup <folder> --server <smtpserver> ");
            Console.WriteLine("\t--port <25> --from <fromemail> --to <toemail> ");
            Console.WriteLine();
            Console.WriteLine("Description:");
            Console.WriteLine("\tAzure DevOps Backup for Git Projects and is using the API for Azure DevOps");
            Console.WriteLine();
            Console.WriteLine("\tWhile the code is perfectly safe on the Azure infrastructure, there are cases where a centralized");
            Console.WriteLine("\tlocal backup of all projects and repositories is needed. These might include Corporate Policies,");
            Console.WriteLine("\tDisaster Recovery and Business Continuity Plans.");
            Console.WriteLine();
            Console.WriteLine("Parameter List:");
            Console.WriteLine("  Mandatory:");
            Console.WriteLine("\t--token <token>:     Token to access the API in Azure DevOps (raw token data)");
            Console.WriteLine("\t--token token.bin:   Will use data to access the API in Azure DevOps from encrypted token.bin file");
            Console.WriteLine("\t--tokenfile <token>: Save a token to access the API in Azure DevOps to an encrypted token.bin file");
            Console.WriteLine("\t\t\t     (use before using the '--token token.bin' argument!)");
            Console.WriteLine("\t--org:               Name of the organization in Azure DevOps");
            Console.WriteLine("\t--backup:            Folder where to store the backup(s) - folder with timestamp will be created");
            Console.WriteLine("\t--server:            IP address or DNS name of the SMTP server");
            Console.WriteLine("\t--port:              The port for the SMTP server");
            Console.WriteLine("\t--from:              The email address the report is send from");
            Console.WriteLine("\t--toemail:           The email address the report is send to");
            Console.WriteLine("  Optional:");
            Console.WriteLine("\t--unzip:             Unzip downloaded .zip and .json files in --backup (optional)");
            Console.WriteLine("\t--cleanup:           Delete downloaded .zip and .json files in --backup after unzip (optional)");
            Console.WriteLine("\t--daystokeepbackup:  Number of days to keep backups for in --backup. Backups older than this will");
            Console.WriteLine("\t\t\t     be deleted (default is 30 dayes) (optional)");
            Console.WriteLine("\t--simpelreport:      If set the email report layout there is send is simple, if not set it use the default");
            Console.WriteLine("\t\t\t     report layout");
            Console.WriteLine("\t--priority:          Set the email report priority to other then default (normal)");
            Console.WriteLine("\t  high:              Set the email report priority to 'high'");
            Console.WriteLine("\t  low:               Set the email report priority to 'low'");
            Console.WriteLine();
            Console.WriteLine("\t--help, /h or /?:    Showing this help text for the tool");
            Console.WriteLine("\t--info or /about:    Showing information about the tool");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine($"\t{Globals._currentExeFileName} --token XXX... --org OrgName --backup C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local\n");
            Console.WriteLine($"\t{Globals._currentExeFileName} --token XXX... --org OrgName --backup C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local --unzip\n");
            Console.WriteLine($"\t{Globals._currentExeFileName} --token XXX... --org OrgName --backup C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local --unzip --cleanup\n");
            Console.WriteLine($"\t{Globals._currentExeFileName} --token XXX... --org OrgName --backup C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local --unzip --daystokeepbackup 50\n");
            Console.WriteLine($"\t{Globals._currentExeFileName} --token XXX... --org OrgName --backup C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local --unzip --simpelreport\n");
            Console.WriteLine($"\t{Globals._currentExeFileName} --token XXX... --org OrgName --backup C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local --unzip --simpelreport --priority high\n");
            Console.WriteLine($"\t{Globals._currentExeFileName} --token token.bin --org OrgName --backup C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local --unzip --simpelreport --priority high");
            Console.WriteLine();
            Console.WriteLine("Output:");
            Console.WriteLine("\tA timestamped folder containing the backup will be created within this directory unless --backup");
            Console.WriteLine("\tis being specified for a custom output folder and then what you set from above parameters");
            Console.WriteLine();
            Console.WriteLine($"{Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName);
            Console.WriteLine();
        }

        public static void DisplayInfo()
        {
            Console.WriteLine("Description:");
            Console.WriteLine("\tAzure DevOps Backup for Git Projects and is using the API for Azure DevOps");
            Console.WriteLine();
            Console.WriteLine("\tWhile the code is perfectly safe on the Azure infrastructure, there are cases where a centralized");
            Console.WriteLine("\tlocal backup of all projects and repositories is needed. These might include Corporate Policies,");
            Console.WriteLine("\tDisaster Recovery and Business Continuity Plans.");
            Console.WriteLine();
            Console.WriteLine("\tAzure DevOps is a cloud service to manage source code and collaborate between development teams.");
            Console.WriteLine("\tIt integrates perfectly with both Visual Studio and Visual Studio Code and other IDE´s and tools");
            Console.WriteLine("\tthere is using the 'Git'.");
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