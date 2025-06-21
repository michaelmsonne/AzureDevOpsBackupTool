using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    internal class ReportSender
    {
        public static void SendEmail(string serverAddress, bool noSsl, string serverPort, string emailFrom, string emailTo, string emailStatusMessage,
            List<string> repoCountElements, List<string> repoItemsCountElements, int repoCount, int repoItemsCount, int totalFilesIsBackupUnZipped,
            int totalBlobFilesIsBackup, int totalTreeFilesIsBackup, string outDir, string elapsedTime, int errors,
            int totalFilesIsDeletedAfterUnZipped, int totalBackupsIsDeleted, string daysToKeep, string repoCountStatusText, string repoItemsCountStatusText,
            string totalFilesIsBackupUnZippedStatusText, string totalBlobFilesIsBackupStatusText, string totalTreeFilesIsBackupStatusText,
            string totalFilesIsDeletedAfterUnZippedStatusText, string letOverZipFilesStatusText, string letOverJsonFilesStatusText, string totalBackupsIsDeletedStatusText,
            bool useSimpleMailReportLayout, bool noAttatchLog, string isOutputFolderContainFilesStatusText, string isDaysToKeepNotDefaultStatusText, string startTime, string endTime, bool deletedFilesAfterUnzip,
            bool checkForLeftoverFilesAfterCleanup, bool doFullGitBackup)
        {
            var serverPortStr = serverPort;
            string mailBody;
            //if (mailBody == null) throw new ArgumentNullException(nameof(mailBody));

            //Parse data to list from list of repo.name
            var listrepocountelements = "<h3>List of Git repositories in Azure DevOps:</h3>∘ " + string.Join("<br>∘ ", repoCountElements);
            var listitemscountelements = "<h3>List of Git repositories in Azure DevOps a backup is performed of:</h3>∘ " + string.Join("<br>∘ ", repoItemsCountElements);
            var letOverJsonFiles = 0;
            var letOverZipFiles = 0;

            // Add subject if cleanup after unzip is set
            if (deletedFilesAfterUnzip)
            {
                emailStatusMessage += " (and cleaned up downloaded files)";
            }

            // It error count is over 0 add warning in email subject
            if (errors > 0)
            {
                emailStatusMessage += " - but with warning(s)";
            }

            // Get leftover files is needed (if had error(s))
            if (checkForLeftoverFilesAfterCleanup)
            {
                letOverJsonFiles = ApplicationGlobals._numJson;
                letOverZipFiles = ApplicationGlobals._numZip;
            }

            // If args is set to old mail report layout
            // Build the Git backup message if needed
            string gitBackupMsg = "";
            if (doFullGitBackup)
            {
                gitBackupMsg = useSimpleMailReportLayout
                    ? "<p><b>Full Git backup (--fullgitbackup) was ENABLED for this run.</b></p>"
                    : "<br><p style=\"color:green;\"><b>Full Git backup (--fullgitbackup) was ENABLED for this run.</b></p>";
            }

            // If args is set to old mail report layout
            if (useSimpleMailReportLayout)
            {
                mailBody =
                    $"<hr><h2>Your {ApplicationGlobals.AppName} of organization '{ApplicationGlobals._orgName}' is: {emailStatusMessage}</h2><hr><p><h3>Details:</h3><p>" +
                    $"<p>Processed Git project(s) in Azure DevOps (total): <b>{repoCount}</b><br>" +
                    $"Processed Git repos in project(s) a backup is made of from Azure DevOps (all branches): <b>{repoItemsCount}</b><p>" +
                    $"Processed files to backup from Git repos (total unzipped if specified): <b>{totalFilesIsBackupUnZipped}</b><br>" +
                    $"Processed files to backup from Git repos (blob files (.zip files)) (all branches): <b>{totalBlobFilesIsBackup}</b><br>" +
                    $"Processed files to backup from Git repos (tree files (.json files)) (all branches): <b>{totalTreeFilesIsBackup}</b><p>" +
                    $"See the attached logfile for the backup(s) today: <b>'{ApplicationGlobals.AppName} Log " + DateTime.Today.ToString("dd-MM-yyyy") + ".log'</b>.<p>" +
                    $"Total Run Time is: \"{elapsedTime}\"<br>" +
                    $"Backup start Time: \"{startTime}\"<br>" +
                    $"Backup end Time: \"{endTime}\"<br>" +
                    "<h3>Download cleanup (if specified):</h3><p>" +
                    $"Deleted original downloaded <b>.zip</b> and <b>.json</b> files in backup folder: <b>{totalFilesIsDeletedAfterUnZipped}</b><br>" +
                    $"Leftovers for original downloaded <b>.zip</b> files in backup folder (error(s) when try to delete): <b>{letOverZipFiles}</b><br>" +
                    $"Leftovers for original downloaded <b>.json</b> files in backup folder (error(s) when try to delete): <b>{letOverJsonFiles}</b><p>" +
                    $"<h3>Backup location:</h3><p>Backed up in folder: <b>\"{outDir}\"</b> on host/server: <b>{Environment.MachineName}</b><br>" +
                    $"Old backups set to keep in backup folder (days): <b>{daysToKeep}</b><br>" +
                    $"Old backups deleted in backup folder: <b>{totalBackupsIsDeleted}</b><br>" +
                    listrepocountelements + "<br>" +
                    listitemscountelements +
                    gitBackupMsg + "</p><hr>" +
                    $"<h3>From Your {ApplicationGlobals.AppName} tool!<o:p></o:p></h3>" + ApplicationGlobals._copyrightData + ", v." + ApplicationGlobals._vData;
            }
            else
            {
                mailBody =
                    $"<hr/><h2>Your {ApplicationGlobals.AppName} of organization '{ApplicationGlobals._orgName}' is: {emailStatusMessage}</h2><hr />" +
                    $"<br><table style=\"border-collapse: collapse; width: 100%; height: 108px;\" border=\"1\">" +
                    $"<tbody><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 33%; height: 18px;\"><strong>Backup task(s):</strong></td>" +
                    $"<td style=\"width: 10%; height: 18px;\"><strong>File(s):</strong></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\"><strong>Status:</strong></td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 33%; height: 18px;\">Processed Git project(s) in Azure DevOps (total):</td>" +
                    $"<td style=\"width: 10%; height: 18px;\"><b>{repoCount}</b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">{repoCountStatusText}</td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 33%; height: 18px;\">Processed Git repos in project(s) a backup is made of from Azure DevOps (all branches):</td>" +
                    $"<td style=\"width: 10%; height: 18px;\"><b>{repoItemsCount}</b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">{repoItemsCountStatusText}</td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 33%; height: 18px;\">Processed files to backup from Git repos (total unzipped if specified):</td>" +
                    $"<td style=\"width: 10%; height: 18px;\"><b>{totalFilesIsBackupUnZipped}</b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">{totalFilesIsBackupUnZippedStatusText}</td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 33%; height: 18px;\">Processed files to backup from Git repos (blob files (.zip files)) (all branches):</td>" +
                    $"<td style=\"width: 10%; height: 18px;\"><b>{totalBlobFilesIsBackup}</b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">{totalBlobFilesIsBackupStatusText}</td></tr><tr>" +
                    $"<td style=\"width: 33%;\">Processed files to backup from Git repos (tree files (.json files)) (all branches):</td>" +
                    $"<td style=\"width: 10%;\"><b>{totalTreeFilesIsBackup}</b></td>" +
                    $"<td style=\"width: 33.3333%;\">{totalTreeFilesIsBackupStatusText}</td></tr></tbody></table><br><table style=\"border-collapse: collapse; width: 100%; height: 108px;\" border=\"1\"><tbody><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 33%; height: 18px;\"><strong>Download cleanup (if specified):</strong></td>" +
                    $"<td style=\"width: 10%; height: 18px;\"><strong>File(s):</strong></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\"><strong>Status:</strong></td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 33%; height: 18px;\">Deleted original downloaded .zip and .json files in backup folder:</td>" +
                    $"<td style=\"width: 10%; height: 18px;\"><b>{totalFilesIsDeletedAfterUnZipped}</b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">{totalFilesIsDeletedAfterUnZippedStatusText}</td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 33%; height: 18px;\">Leftovers for original downloaded .zip files in backup folder (error(s) when try to delete):</td>" +
                    $"<td style=\"width: 10%; height: 18px;\"><b>{letOverZipFiles}</b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">{letOverZipFilesStatusText}</td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 33%; height: 18px;\">Leftovers for original downloaded .json files in backup folder (error(s) when try to delete):</td>" +
                    $"<td style=\"width: 10%; height: 18px;\"><b>{letOverJsonFiles}</b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">{letOverJsonFilesStatusText}</td></tr></tbody></table><br><table style=\"border-collapse: collapse; width: 100%; height: 108px;\" border=\"1\"><tr>" +
                    $"<td style=\"width: 21%; height: 18px;\"><strong>Backup:</strong></td>" +
                    $"<td style=\"width: 22%; height: 18px;\"><strong>Info:</strong></td>" +
                    $"<td style=\"width: 33%; height: 18px;\"><strong>Status:</strong></td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 21%; height: 18px;\">Backup folder:</td>" +
                    $"<td style=\"width: 22%; height: 18px;\"><strong><b>\"{outDir}\"</b></b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">{isOutputFolderContainFilesStatusText}</td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 21%; height: 18px;\">Backup host:</td>" +
                    $"<td style=\"width: 22%; height: 18px;\"><b>{Environment.MachineName}</b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">  </td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 21%; height: 18px;\">Old backup(s) set to keep in backup folder (days):</td>" +
                    $"<td style=\"width: 22%; height: 18px;\"><b>{daysToKeep}</b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">{isDaysToKeepNotDefaultStatusText}</td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 21%; height: 18px;\">Number of current backups in backup folder:</td>" +
                    $"<td style=\"width: 22%; height: 18px;\"><b>{ApplicationGlobals._currentBackupsInBackupFolderCount}</b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">Multiple backups can be created at the same day, so there can be more backups then days set to keep</td></tr><tr style=\"height: 18px;\">" +
                    $"<td style=\"width: 21%; height: 18px;\">Old backup(s) deleted in backup folder:</td>" +
                    $"<td style=\"width: 22%; height: 18px;\"><b>{totalBackupsIsDeleted}</b></td>" +
                    $"<td style=\"width: 33.3333%; height: 18px;\">{totalBackupsIsDeletedStatusText}</td></tr></table>" +
                    $"<p>See the attached logfile for the backup(s) today: <b>'{ApplicationGlobals.AppName} Log " + DateTime.Today.ToString("dd-MM-yyyy") + ".log'</b>.<o:p></o:p></p>" +
                    $"<p>Total Run Time is: \"{elapsedTime}\"<br>" +
                    $"Backup start Time: \"{startTime}\"<br>" +
                    $"Backup end Time: \"{endTime}\"</p><hr/>" +
                    listrepocountelements + "<br>" +
                    listitemscountelements + "</p>" +
                    gitBackupMsg + "<br><hr>" +
                    $"<h3>From Your {ApplicationGlobals.AppName} tool!<o:p></o:p></h3>" + ApplicationGlobals._copyrightData + ", v." + ApplicationGlobals._vData;
            }

            // Create mail
            var message = new MailMessage();
            message.From = new MailAddress(emailFrom);

            // Split the emailTo string by commas and add each address to the To collection
            var emailAddresses = emailTo.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var address in emailAddresses)
            {
                message.To.Add(address.Trim());
            }

            // Set email subject
            message.Subject = "[" + emailStatusMessage + $"] - {ApplicationGlobals.AppName} status - (" + totalBlobFilesIsBackup +
                              " Git projects backed up), " + errors + " issues(s) - (backups to keep (days): " + daysToKeep +
                              ", backup(s) deleted: " + totalBackupsIsDeleted + ")";
            
            // Set email body
            message.Body = mailBody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;

            // Set email priority level based on command-line argument
            message.Priority = ApplicationGlobals.EmailPriority;
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.None;
            message.BodyTransferEncoding = TransferEncoding.QuotedPrintable;

            // ReSharper disable once UnusedVariable
            var isParsable = Int32.TryParse(serverPortStr, out var serverPortNumber);
            using (var client = new SmtpClient(serverAddress, serverPortNumber)
            {
                EnableSsl = !noSsl,
                UseDefaultCredentials = true
            })
            {
                Message("Created email report and parsed data", EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Created email report and parsed data");
                Console.ResetColor();

                // Check if we should attach the logfile to the email report or not
                if (noAttatchLog)
                {
                    // Log
                    Message("No logfile attached to email report!", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("No logfile attached to email report!");
                    Console.ResetColor();
                }
                else
                {
                    // Get all the files in the log dir for today

                    // Log
                    Message("Finding logfile for today to attach in email report...", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Finding logfile for today to attach in email report...");
                    Console.ResetColor();

                    // Get filename to find
                    var filePaths = Directory.GetFiles(Files.LogFilePath,
                        $"{ApplicationGlobals.AppName} Log " + DateTime.Today.ToString("dd-MM-yyyy") + "*.*");

                    // Get the files that their extension are .log or .txt
                    var files = filePaths.Where(filePath =>
                        Path.GetExtension(filePath).Contains(".log") || Path.GetExtension(filePath).Contains(".txt"));

                    // Loop through the files enumeration and attach each file in the mail.
                    foreach (var file in files)
                    {
                        ApplicationGlobals._fileAttachedIneMailReport = file;

                        // Log
                        Message("Found logfile for today:", EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Found logfile for today:");
                        Console.ResetColor();

                        // Full file name
                        var fileName = ApplicationGlobals._fileAttachedIneMailReport;
                        var fi = new FileInfo(fileName);

                        // Get File Name
                        var justFileName = fi.Name;
                        Console.WriteLine("File name: " + justFileName);
                        Message("File name: " + justFileName, EventType.Information, 1000);

                        // Get file name with full path
                        var fullFileName = fi.FullName;
                        Console.WriteLine("Full file name: " + fullFileName);
                        Message("Full file name: " + fullFileName, EventType.Information, 1000);

                        // Get file extension
                        var extn = fi.Extension;
                        Console.WriteLine("File Extension: " + extn);
                        Message("File Extension: " + extn, EventType.Information, 1000);

                        // Get directory name
                        var directoryName = fi.DirectoryName;
                        Console.WriteLine("Directory name: " + directoryName);
                        Message("Directory name: " + directoryName, EventType.Information, 1000);

                        // File Exists ?
                        var exists = fi.Exists;
                        Console.WriteLine("File exists: " + exists);
                        Message("File exists: " + exists, EventType.Information, 1000);
                        if (fi.Exists)
                        {
                            // Get file size
                            var size = fi.Length;
                            Console.WriteLine("File Size in Bytes: " + size);
                            Message("File Size in Bytes: " + size, EventType.Information, 1000);

                            // File ReadOnly ?
                            var isReadOnly = fi.IsReadOnly;
                            Console.WriteLine("Is ReadOnly: " + isReadOnly);
                            Message("Is ReadOnly: " + isReadOnly, EventType.Information, 1000);

                            // Creation, last access, and last write time
                            var creationTime = fi.CreationTime;
                            Console.WriteLine("Creation time: " + creationTime);
                            Message("Creation time: " + creationTime, EventType.Information, 1000);
                            var accessTime = fi.LastAccessTime;
                            Console.WriteLine("Last access time: " + accessTime);
                            Message("Last access time: " + accessTime, EventType.Information, 1000);
                            var updatedTime = fi.LastWriteTime;
                            Console.WriteLine("Last write time: " + updatedTime + "\n");
                            Message("Last write time: " + updatedTime, EventType.Information, 1000);
                        }

                        // TODO Do not add more to logfile here - file is locked!
                        var attachment = new Attachment(file);

                        // Attach file to email
                        message.Attachments.Add(attachment);
                    }

                    // Log
                    Message("Logfile attached to email report!", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Logfile attached to email report!");
                    Console.ResetColor();
                }

                //Try to send email status email
                try
                {
                    // Send the email
                    client.Send(message);

                    // Release files for the email
                    message.Dispose();
                    // TODO logfile is not locked from here - you can add logs to logfile again from here!

                    // Log
                    Message("Email notification is send to '" + emailTo + "' at '" + DateTime.Now.ToString("dd-MM-yyyy (HH-mm)") + "' with priority " + ApplicationGlobals.EmailPriority + "!", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Email notification is send to '" + emailTo + "' at '" + DateTime.Now.ToString("dd-MM-yyyy (HH-mm)") + "' with priority " + ApplicationGlobals.EmailPriority + "!");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    // Log
                    Message("Sorry, we are unable to send email notification of your presence. Please try again! Error: " + ex, EventType.Error, 1001);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Sorry, we are unable to send email notification of your presence. Please try again! Error: " + ex);
                    Console.ResetColor();
                }
            }


            /*using (var client = new SmtpClient(serverAddress, serverPortNumber) { EnableSsl = true, UseDefaultCredentials = true })
            {
                Message("Created email report and parsed data", EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Created email report and parsed data");
                Console.ResetColor();

                // Check if we should attach the logfile to the email report or not
                if (noAttatchLog)
                {
                    // Log
                    Message("No logfile attached to email report!", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("No logfile attached to email report!");
                    Console.ResetColor();
                }
                else
                {
                    // Get all the files in the log dir for today

                    // Log
                    Message("Finding logfile for today to attach in email report...", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Finding logfile for today to attach in email report...");
                    Console.ResetColor();

                    // Get filename to find
                    var filePaths = Directory.GetFiles(Files.LogFilePath,
                        $"{Globals.AppName} Log " + DateTime.Today.ToString("dd-MM-yyyy") + "*.*");

                    // Get the files that their extension are .log or .txt
                    var files = filePaths.Where(filePath =>
                        Path.GetExtension(filePath).Contains(".log") || Path.GetExtension(filePath).Contains(".txt"));

                    // Loop through the files enumeration and attach each file in the mail.
                    foreach (var file in files)
                    {
                        Globals._fileAttachedIneMailReport = file;

                        // Log
                        Message("Found logfile for today:", EventType.Information, 1000);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Found logfile for today:");
                        Console.ResetColor();

                        // Full file name
                        var fileName = Globals._fileAttachedIneMailReport;
                        var fi = new FileInfo(fileName);

                        // Get File Name
                        var justFileName = fi.Name;
                        Console.WriteLine("File name: " + justFileName);
                        Message("File name: " + justFileName, EventType.Information, 1000);

                        // Get file name with full path
                        var fullFileName = fi.FullName;
                        Console.WriteLine("Full file name: " + fullFileName);
                        Message("Full file name: " + fullFileName, EventType.Information, 1000);

                        // Get file extension
                        var extn = fi.Extension;
                        Console.WriteLine("File Extension: " + extn);
                        Message("File Extension: " + extn, EventType.Information, 1000);

                        // Get directory name
                        var directoryName = fi.DirectoryName;
                        Console.WriteLine("Directory name: " + directoryName);
                        Message("Directory name: " + directoryName, EventType.Information, 1000);

                        // File Exists ?
                        var exists = fi.Exists;
                        Console.WriteLine("File exists: " + exists);
                        Message("File exists: " + exists, EventType.Information, 1000);
                        if (fi.Exists)
                        {
                            // Get file size
                            var size = fi.Length;
                            Console.WriteLine("File Size in Bytes: " + size);
                            Message("File Size in Bytes: " + size, EventType.Information, 1000);

                            // File ReadOnly ?
                            var isReadOnly = fi.IsReadOnly;
                            Console.WriteLine("Is ReadOnly: " + isReadOnly);
                            Message("Is ReadOnly: " + isReadOnly, EventType.Information, 1000);

                            // Creation, last access, and last write time
                            var creationTime = fi.CreationTime;
                            Console.WriteLine("Creation time: " + creationTime);
                            Message("Creation time: " + creationTime, EventType.Information, 1000);
                            var accessTime = fi.LastAccessTime;
                            Console.WriteLine("Last access time: " + accessTime);
                            Message("Last access time: " + accessTime, EventType.Information, 1000);
                            var updatedTime = fi.LastWriteTime;
                            Console.WriteLine("Last write time: " + updatedTime + "\n");
                            Message("Last write time: " + updatedTime, EventType.Information, 1000);
                        }

                        // TODO Do not add more to logfile here - file is locked!
                        var attachment = new Attachment(file);

                        // Attach file to email
                        message.Attachments.Add(attachment);
                    }

                    // Log
                    Message("Logfile attached to email report!", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Logfile attached to email report!");
                    Console.ResetColor();
                }

                //Try to send email status email
                try
                {
                    // Send the email
                    client.Send(message);

                    // Release files for the email
                    message.Dispose();
                    // TODO logfile is not locked from here - you can add logs to logfile again from here!

                    // Log
                    Message("Email notification is send to '" + emailTo + "' at '" + DateTime.Now.ToString("dd-MM-yyyy (HH-mm)") + "' with priority " + Globals.EmailPriority + "!", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Email notification is send to '" + emailTo + "' at '" + DateTime.Now.ToString("dd-MM-yyyy (HH-mm)") + "' with priority " + Globals.EmailPriority + "!");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    // Log
                    Message("Sorry, we are unable to send email notification of your presence. Please try again! Error: " + ex, EventType.Error, 1001);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Sorry, we are unable to send email notification of your presence. Please try again! Error: " + ex);
                    Console.ResetColor();
                }
            }*/
        }
    }
}