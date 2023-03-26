using System;
using System.Diagnostics;
using System.IO;
using System.Security;

namespace AzureDevOpsBackup.Class
{
    internal class FileLogger
    {
        // Control if saves log to logfile
        public static bool WriteToFile { get; set; } = true;

        // Control if saves log to Windows eventlog
        public static bool WriteToEventLog { get; set; } = true;

        public static bool WriteOnlyErrorsToEventLog { get; set; } = true;

        // Sets the App name for the log function
        //public static string AppName { get; set; } = Globals.AppName; // "Unknown",;

        // Set date format short
        public static string DateFormat { get; set; } = "dd-MM-yyyy";

        // Set date format long
        public static string DateTimeFormat { get; set; } = "dd-MM-yyyy HH:mm:ss";

        // Get logfile path
        public static string GetLogPath(string df)
        {
            return Files.LogFilePath + @"\" + Globals.AppName + " Log " + df + ".log";
        }

        // Get datetime
        public static string GetDateTime(DateTime datetime)
        {
            return datetime.ToString(DateTimeFormat);
        }

        // Get date
        public static string GetDate(DateTime datetime)
        {
            return datetime.ToString(DateFormat);
        }

        // Set event type
        public enum EventType
        {
            Warning,
            Error,
            Information,
        }

        // Add message
        public static void Message(string logText, EventType type, int id)
        {
            var now = DateTime.Now;
            var date = GetDate(now);
            var dateTime = GetDateTime(now);
            var logPath = GetLogPath(date);

            // Set where to save log message to
            if (WriteToFile)
                AppendMessageToFile(logText, type, dateTime, logPath, id);
            if (!WriteToEventLog)
                return;
            AddMessageToEventLog(logText, type, dateTime, logPath, id);
        }

        // Save message to logfile
        private static void AppendMessageToFile(string mess, EventType type, string dtf, string path, int id)
        {
            try
            {
                // Check if file exists else create it
                if (!Directory.Exists(Files.LogFilePath))
                    Directory.CreateDirectory(Files.LogFilePath);

                var str = type.ToString().Length > 7 ? "\t" : "\t\t";
                if (!File.Exists(path))
                {
                    using (var text = File.CreateText(path))
                        text.WriteLine(
                            $"{(object)dtf} - [EventID {(object)id.ToString()}] {(object)type.ToString()}{(object)str}{(object)mess}");
                }
                else
                {
                    using (var streamWriter = File.AppendText(path))
                        streamWriter.WriteLine(
                            $"{(object)dtf} - [EventID {(object)id.ToString()}] {(object)type.ToString()}{(object)str}{(object)mess}");
                }
            }
            catch (Exception ex)
            {
                if (!WriteToEventLog)
                    return;
                AddMessageToEventLog($"Error writing to log file, {ex.Message}", EventType.Error, dtf, path, 0);
                AddMessageToEventLog("Writing log file have been disabled.", EventType.Information, dtf, path, 0);
                WriteToFile = false;
            }
        }

        // Save message to Windows event log
        private static void AddMessageToEventLog(string mess, EventType type, string dtf, string path, int id)
        {
            try
            {
                if (type != EventType.Error && WriteOnlyErrorsToEventLog)
                    return;
                var eventLog = new EventLog("");
                if (!EventLog.SourceExists(Globals.AppName))
                    EventLog.CreateEventSource(Globals.AppName, "Application");
                eventLog.Source = Globals.AppName;
                eventLog.EnableRaisingEvents = true;
                var type1 = EventLogEntryType.Error;
                switch (type)
                {
                    case EventType.Warning:
                        type1 = EventLogEntryType.Warning;
                        break;
                    case EventType.Error:
                        type1 = EventLogEntryType.Error;
                        break;
                    case EventType.Information:
                        type1 = EventLogEntryType.Information;
                        break;
                }
                eventLog.WriteEntry(mess, type1, id);
            }
            catch (SecurityException ex)
            {
                if (WriteToFile)
                {
                    AppendMessageToFile($"Security Exeption: {ex.Message}", EventType.Error, dtf, path, id);
                    AppendMessageToFile("Run this software as Administrator once to solve the problem.", EventType.Information, dtf, path, id);
                    AppendMessageToFile("Event log entries have been disabled.", EventType.Information, dtf, path, id);
                    WriteToEventLog = false;
                }
            }
            catch (Exception ex)
            {
                if (WriteToFile)
                {
                    AppendMessageToFile(ex.Message, EventType.Error, dtf, path, id);
                    AppendMessageToFile("Event log entries have been disabled.", EventType.Information, dtf, path, id);
                    WriteToEventLog = false;
                }
            }
        }
    }
}