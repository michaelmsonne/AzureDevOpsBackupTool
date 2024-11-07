using CommandLine;

namespace AzureDevOpsBackup.Class
{
    public class Options
    {
        [Option('t', "token", HelpText = "Token to access the API in Azure DevOps (raw token data) or token.bin")]
        public string Token { get; set; }

        [Option('o', "org", Required = true, HelpText = "Name of the organization in Azure DevOps")]
        public string Org { get; set; }

        [Option('b', "backup", Required = true, HelpText = "Folder where to store the backup(s) - folder with timestamp will be created")]
        public string Backup { get; set; }

        [Option('s', "server", Required = true, HelpText = "IP address or DNS name of the SMTP server")]
        public string Server { get; set; }

        [Option("nossl", HelpText = "Use no SSL for the email server (optional)")]
        public bool NoSSL { get; set; }

        [Option('p', "port", Required = true, HelpText = "The port for the SMTP server")]
        public int Port { get; set; }

        [Option('f', "from", Required = true, HelpText = "The email address the report is sent from")]
        public string From { get; set; }

        [Option("to", Required = true, HelpText = "The email address the report is sent to")]
        public string ToEmail { get; set; }

        [Option("tokenfile", HelpText = "Save a token to access the API in Azure DevOps to an encrypted token.bin file")]
        public string TokenFile { get; set; }

        [Option("unzip", HelpText = "Unzip downloaded .zip and .json files in --backup (optional)")]
        public bool Unzip { get; set; }

        [Option("cleanup", HelpText = "Delete downloaded .zip and .json files in --backup after unzip (optional)")]
        public bool Cleanup { get; set; }

        [Option("daystokeepbackup", HelpText = "Number of days to keep backups for in --backup. Backups older than this will be deleted (default is 30 days) (optional)")]
        public int DaysToKeepBackup { get; set; }

        [Option("simpelreport", HelpText = "If set, the email report layout that is sent is simple, if not set, it uses the default report layout")]
        public bool SimpleReport { get; set; }

        [Option("priority", Default = "normal", HelpText = "Set the email report priority to other than default (normal).\n  high: Set the email report priority to 'high'\n  low: Set the email report priority to 'low'")]
        public string Priority { get; set; }

        [Option('h', "help", HelpText = "Display this help message")]
        public bool Help { get; set; }

        [Option("info", HelpText = "Show information about the tool")]
        public bool Info { get; set; }
    }
}
