using System.Net.Mail;

namespace AzureDevOpsBackup.Class
{
    public static class ApplicationGlobals
    {
        public static string _currentExeFileName; // The name of the current executable file
        public static int _errors; // The number of errors that have occurred
        public static string _vData; // The version data of the current executable file
        public static string _companyName; // The company name of the current executable file
        public static string _orgName; // The organization name of Azure DevOps to backup
        public static string _startTime; // The time the backup started
        public static string _endTime; // The time the backup ended
        public static int _totalFilesIsDeletedAfterUnZipped; // The total number of files deleted after unzipping
        public static int _numZip; // The number of zip files
        public static int _numJson; // The number of json files
        public static bool _checkForLeftoverFilesAfterCleanup; // Check for leftover files after cleanup
        public static bool _deletedFilesAfterUnzip; // Delete files after unzipping
        public static bool _nossl; // No SSL for the email server
        public static string AppName; // The name of the application
        public static string _copyrightData; // The copyright data of the application
        public static int _totalBackupsIsDeleted; // The total number of backups deleted
        public static string _fileAttachedIneMailReport; // The file attached in the email report (name)
        public static MailPriority EmailPriority = MailPriority.Normal; // The priority of the email report (default is normal)
        public static int _currentBackupsInBackupFolderCount; 
        public static int _oldLogFilesToDeleteCount;
        public static bool _oldLogfilesToDelete;
        public const string APIversion = "api-version=7.0"; // See more at: https://learn.microsoft.com/en-us/rest/api/azure/devops/ for information on API versions
        public static string _backupFolder; // The folder to backup to (default is the current directory)
        public static string _sanitizedbackupFolder; // The sanitized folder to backup to (default is the current directory)
        public static string _dateOfToday; // The date of today
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
        public static int _projectCount; // The number of projects in the organization to backup
        public static int _totalFilesIsBackupUnZipped; // The total number of files in the backups unzipped
        public static int _totalBlobFilesIsBackup; // The total number of blob files in the backups
        public static int _totalTreeFilesIsBackup; // The total number of tree files in the backups
        public static int _repoItemsCount; // The number of items in the repository
        public static int _repoCount; // The number of repositories in the organization
        public static string _emailStatusMessage; // The status message of the email
        public static bool _noAttatchLog; // TODO
        public static bool _doFullGitBackup; // Whether to do a full git backup or not
    }
}