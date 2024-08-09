using System.IO;

namespace AzureDevOpsBackupUnzipTool.Class
{
    internal class Files
    {
        public static string LogFilePath
        {
            get
            {
                // Root folder for log files
                var logfilePathvar = ProgramDataFilePath + @"\Log\Unzip tool";
                return logfilePathvar;
            }
        }

        public static string ProgramDataFilePath
        {
            get
            {
                // Root path for program data
                var currentDirectory = Directory.GetCurrentDirectory();
                var programDataFilePathvar = currentDirectory;
                return programDataFilePathvar;
            }
        }
    }
}