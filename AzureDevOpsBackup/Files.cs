namespace AzureDevOpsBackup
{
    internal class Files
    {
        public static string LogFilePath
        {
            get
            {
                // Root folder for log files
                var logfilePathvar = ProgramDataFilePath + @"\Log";
                return logfilePathvar;
            }
        }
        public static string ProgramDataFilePath
        {
            get
            {
                // Root path for program data
                var currentDirectory = System.IO.Directory.GetCurrentDirectory();
                var programDataFilePathvar = currentDirectory;
                return programDataFilePathvar;
            }
        }
    }
}
