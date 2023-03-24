using System;

namespace AzureDevOpsBackup.Class
{
    internal class DisplayHelpToConsole
    {
        public static void DisplayGuide(string _currentExeFileName, string AppName, string _vData, string _companyName)
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
            Console.WriteLine("\t--help, /h or /?:    Showing this help text for the tool");
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
            Console.WriteLine("\tis being specified for a custom output folder");
            Console.WriteLine();
            Console.WriteLine($"{AppName}, v." + _vData + " by " + _companyName);
            Console.WriteLine();
        }
    }
}
