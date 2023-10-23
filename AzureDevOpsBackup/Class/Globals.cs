using System;
using System.Net.Mail;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    public static class Globals
    {
        public static string _currentExeFileName;
        public static int _errors;
        public static string _vData;
        public static string _companyName;
        public static string _orgName;
        public static string _startTime;
        public static string _endTime;
        public static int _totalFilesIsDeletedAfterUnZipped;
        public static int _numZip;
        public static int _numJson;
        public static bool _checkForLeftoverFilesAfterCleanup;
        public static bool _deletedFilesAfterUnzip;
        public static string AppName;
        public static string _copyrightData;
        public static int _totalBackupsIsDeleted;
        public static string _fileAttachedIneMailReport;
        public static MailPriority EmailPriority = MailPriority.Normal;
        public static int _currentBackupsInBackupFolderCount;
        public static int _oldLogFilesToDeleteCount;
        public static bool _oldLogfilesToDelete;

        public static void ApplicationStartMessage()
        {
            // Log start of program
            Message($"Welcome to {AppName}, v." + _vData + " by " + _companyName, EventType.Information, 1000);
            Console.WriteLine($"\nWelcome to {AppName}, v." + _vData + " by " + _companyName + "\n");
        }
        public static void ApplicationEndMessage()
        {
            // Log end of program
            Message($"End of application - {AppName}, v." + _vData, EventType.Information, 1000);
            Console.WriteLine($"\nEnd of application - {AppName}, v. {_vData}\n");
        }
    }
}