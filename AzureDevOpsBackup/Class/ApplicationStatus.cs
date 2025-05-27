using System;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    internal class ApplicationStatus
    {
        public static void ApplicationStartMessage()
        {
            // Log start of program
            Message($"Welcome to {ApplicationGlobals.AppName}, v." + ApplicationGlobals._vData + " by " + ApplicationGlobals._companyName, EventType.Information, 1000);
            Console.WriteLine($"\nWelcome to {ApplicationGlobals.AppName}, v." + ApplicationGlobals._vData + " by " + ApplicationGlobals._companyName + "\n");
        }
        public static void ApplicationEndMessage()
        {
            // Log end of program
            Message($"End of application - {ApplicationGlobals.AppName}, v." + ApplicationGlobals._vData + "\n", EventType.Information, 1000);
            Console.WriteLine($"\nEnd of application - {ApplicationGlobals.AppName}, v. {ApplicationGlobals._vData}\n");
        }
    }
}
