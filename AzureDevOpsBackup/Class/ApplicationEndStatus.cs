using System;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    internal class ApplicationEndStatus
    {
        public static void ApplicationStartMessage()
        {
            // Log start of program
            Message($"Welcome to {Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName, EventType.Information, 1000);
            Console.WriteLine($"\nWelcome to {Globals.AppName}, v." + Globals._vData + " by " + Globals._companyName + "\n");
        }
        public static void ApplicationEndMessage()
        {
            // Log end of program
            Message($"End of application - {Globals.AppName}, v." + Globals._vData + "\n", EventType.Information, 1000);
            Console.WriteLine($"\nEnd of application - {Globals.AppName}, v. {Globals._vData}\n");
        }
    }
}
