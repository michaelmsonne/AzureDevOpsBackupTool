using System;
using System.Diagnostics;
using System.IO;
using System.Security;

namespace AzureDevOpsBackup.Class
{
    internal class FileLogger
    {
        public static bool WriteToFile { get; set; } = true;

        public static bool WriteToEventLog { get; set; } = true;

        public static bool WriteOnlyErrorsToEventLog { get; set; } = true;

        public static string AppName { get; set; } = Globals.AppName; // "Unknown",;

        public static string DateFormat { get; set; } = "dd-MM-yyyy";

        public static string DateTimeFormat { get; set; } = "dd-MM-yyyy HH:mm:ss";

        public static string GetLogPath(string df)
        {
            return Files.LogFilePath + @"\" + AppName + " Log " + df + ".log";
        }

        public static string GetDateTime(DateTime datetime)
        {
            return datetime.ToString(DateTimeFormat);
        }

        public static string GetDate(DateTime datetime)
        {
            return datetime.ToString(DateFormat);
        }

        public enum EventType
        {
            Warning,
            Error,
            Information,
        }

        public static void Message(string logText, EventType type, int id)
        {
            var now = DateTime.Now;
            var date = GetDate(now);
            var dateTime = GetDateTime(now);
            var logPath = GetLogPath(date);

            if (WriteToFile)
                AppendMessageToFile(logText, type, dateTime, logPath, id);
            if (!WriteToEventLog)
                return;
            AddMessageToEventLog(logText, type, dateTime, logPath, id);
        }

        private static void AppendMessageToFile(string mess, EventType type, string dtf, string path, int id)
        {
            try
            {
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

        private static void AddMessageToEventLog(string mess, EventType type, string dtf, string path, int id)
        {
            try
            {
                if (type != EventType.Error && WriteOnlyErrorsToEventLog)
                    return;
                var eventLog = new EventLog("");
                if (!EventLog.SourceExists(AppName))
                    EventLog.CreateEventSource(AppName, "Application");
                eventLog.Source = AppName;
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
