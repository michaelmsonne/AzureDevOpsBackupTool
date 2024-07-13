using System.Net.Mail;

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
        public const string APIversion = "api-version=7.0"; // https://learn.microsoft.com/en-us/rest/api/azure/devops/
        public static string _backupFolder;
        public static string _sanitizedbackupFolder;
        public static string _dateOfToday;
        public static string _repoCountStatusText;
        public static string _totalFilesIsBackupUnZippedStatusText;
        public static string _totalBlobFilesIsBackupStatusText;
        public static string _totalTreeFilesIsBackupStatusText;
        public static string _totalFilesIsDeletedAfterUnZippedStatusText;
        public static string _letOverZipFilesStatusText;
        public static string _letOverJsonFilesStatusText;
        public static string _totalBackupsIsDeletedStatusText;
        public static string _isOutputFolderContainFilesStatusText;
        public static string _repoItemsCountStatusText;
        public static string _isDaysToKeepNotDefaultStatusText;
        public static int _projectCount;
        public static int _totalFilesIsBackupUnZipped;
        public static int _totalBlobFilesIsBackup;
        public static int _totalTreeFilesIsBackup;
        public static int _repoItemsCount;
        public static int _repoCount;
        public static string _emailStatusMessage;
        public static int _workItemsCount = 0;
    }
}