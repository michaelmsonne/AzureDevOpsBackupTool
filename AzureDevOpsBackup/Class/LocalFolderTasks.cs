using System;
using System.IO;
using System.Linq;

namespace AzureDevOpsBackup.Class
{
    internal class LocalFolderTasks
    {
        public static bool CheckIfHaveSubfolders(string path)
        {
            if (Directory.GetDirectories(path).Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }
            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

        // Function to sanitize directory name
        public static string SanitizeDirectoryName(string directoryName)
        {
            // Remove any potentially dangerous characters from the directory name
            return Path.GetInvalidPathChars().Aggregate(directoryName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}