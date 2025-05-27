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
            Console.WriteLine("\tRegulatory Compliance: Some industries require regular backups for compliance purposes.");
            Console.WriteLine("\tAccidental Deletion: Backups help recover data lost due to accidental deletion or corruption.");
            Console.WriteLine("\tMigration: Local backups can assist in migrating projects to other platforms.");
            Console.WriteLine("\tAudit Trail: Backups provide a historical record for auditing and tracking changes.");
            Console.WriteLine();
            Console.WriteLine("Parameter List:");
            Console.WriteLine("  Mandatory:");
            Console.WriteLine("\t--token <token>:     Token to access the API in Azure DevOps (raw token data)");
            Console.WriteLine("\t     or token.bin:   Will use data to access the API in Azure DevOps from encrypted token.bin file");
            Console.WriteLine("\t--org:               Name of the organization in Azure DevOps");
            Console.WriteLine("\t     --oldurl:       Specify this option if you are using the old organization URL format");
            Console.WriteLine(
                "\t\t\t     (https://organization.visualstudio.com) if you have not updated your organization URL");
            Console.WriteLine("\t\t\t     to the new format (https://dev.azure.com/{organization}) (Add argument to set $true)");
            Console.WriteLine("\t--backup:            Folder where to store the backup(s) - folder with timestamp will be created");
            Console.WriteLine("\t\t\t     and this is a 'snapshot' if only useing the REST API (for a full ´Git backup' add");
            Console.WriteLine("\t\t\t     '--fullgitbackup' also)");
            Console.WriteLine("\t--server:            IP address or DNS name of the SMTP server");
            Console.WriteLine("\t--nossl:             No SSL for the mail server");
            Console.WriteLine("\t--port:              The port for the SMTP server");
            Console.WriteLine("\t--from:              The email address the report is send from");
            Console.WriteLine("\t--to:                The email address the report is send to - support multiple recipients with");
            Console.WriteLine("\t\t\t     argument separated by comma");
            Console.WriteLine("  Optional:");
            Console.WriteLine("\t--tokenfile <token>: Save a token to access the API in Azure DevOps to an encrypted token.bin file");
            Console.WriteLine("\t\t\t     (use this before using the '--token token.bin' argument!)");
            Console.WriteLine("\t--unzip:             Unzip downloaded .zip and .json files in --backup (optional)");
            Console.WriteLine("\t--cleanup:           Delete downloaded .zip and .json files in --backup after unzip (optional)");
            Console.WriteLine("\t--daystokeepbackup:  Number of days to keep backups for in --backup. Backups older than this will");
            Console.WriteLine("\t\t\t     be deleted (default is 30 dayes) (optional)");
            Console.WriteLine("\t--simpelreport:      If set the email report layout there is send is simple, if not set it use the default");
            Console.WriteLine("\t\t\t     report layout");
            Console.WriteLine("\t--noattatchlog:      Set the email report to not attach the logfile in the mail report sent");
            Console.WriteLine("\t--priority:          Set the email report priority to other then default (normal)");
            Console.WriteLine("\t  high:              Set the email report priority to 'high'");
            Console.WriteLine("\t  low:               Set the email report priority to 'low'");
            Console.WriteLine("\t--healthcheck:       Option to test connectivity to Azure DevOps REST API and backup folder write access");
            Console.WriteLine("\t\t\t     (optional)");
            Console.WriteLine("\t--fullgitbackup:     Also perform a full git clone --mirror backup of each repository (with all history,");
            Console.WriteLine("\t\t\t     branches, git history etc.) in a '<project>.git' folder (optional)");
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
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local --unzip --simpelreport --priority low\n");
            Console.WriteLine($"\t{Globals._currentExeFileName} --token token.bin --org OrgName --backup C:\\Backup --server smtp.domain.local");
            Console.WriteLine("\t--port 25 --from from@domain.local --to reports@domain.local,admin@domain.local --unzip --noattatchlog");
            Console.WriteLine();
            Console.WriteLine("Output:");
            Console.WriteLine("\tA timestamped folder containing the backup will be created within this directory unless --backup");
            Console.WriteLine("\tis being specified for a custom output folder and then what you set from above parameters");
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