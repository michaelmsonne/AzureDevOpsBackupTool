using System;
using System.Diagnostics;

namespace AzureDevOpsBackup.Class
{
    internal class ApplicationRequirements
    {
        public static void SystemCheck()
        {
            // Test IsLongPathsEnabled
            IsLongPathsEnabled();

            // Test Is Long Paths Enabled for Application
            IsLongPathsEnabledApplication();
        }

        public static bool IsLongPathsEnabled()
        {
            try
            {
                // Check if the LongPathsEnabled registry key exists and is set to 1
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\FileSystem");
                // ReSharper disable once PossibleNullReferenceException
                int value = (int)key.GetValue("LongPathsEnabled", 0);
                return value == 1;
            }
            catch (Exception ex)
            {
                // If an exception occurs, assume Long Paths are not enabled
                Console.WriteLine("An error occurred while trying to enable Long Paths for Windows. Please contact your system administrator for assistance. Error: " + ex);
                return false;
            }
        }

        public static void IsLongPathsEnabledApplication()
        {
            try
            {
                // Enable Long Paths for the application
                AppContext.SetSwitch("Switch.System.IO.UseLegacyPathHandling", false);
            }
            catch (Exception ex)
            {
                // If an exception occurs, assume Long Paths are not enabled
                Console.WriteLine("An error occurred while trying to enable Long Paths for the application. Please contact your system administrator for assistance. Error: " + ex);
            }
        }
    }
}