using System;
using System.IO;

namespace AzureDevOpsBackup.Class
{
    internal class Folders
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
    }
}