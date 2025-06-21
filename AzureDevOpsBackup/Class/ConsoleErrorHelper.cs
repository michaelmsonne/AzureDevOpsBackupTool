using System;

namespace AzureDevOpsBackup.Class
{
    /// <summary>
    /// Helper class for displaying error messages in the console with red color formatting.
    /// </summary>
    public static class ConsoleErrorHelper
    {
        /// <summary>
        /// Displays an error message indicating that the Azure DevOps PAT has expired,
        /// along with instructions for resolving the issue.
        /// </summary>
        public static void ShowExpiredPatError()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("  ERROR: Your Azure DevOps Personal Access Token (PAT) has expired.");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("  How to fix:");
            Console.WriteLine("    1. Create a new PAT in Azure DevOps.");
            Console.WriteLine("    2. Run this tool with: '--tokenfile <yourNewPAT>' to generate a new token.bin file.");
            Console.WriteLine("    3. Use: '--token token.bin' for future runs on this machine.");
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Displays an error message when token decryption fails,
        /// including the provided reason and possible causes and solutions.
        /// </summary>
        /// <param name="message">The specific error message or reason for decryption failure.</param>
        public static void ShowTokenDecryptError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("  ERROR: Failed to decrypt token from 'token.bin'");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine($"  Reason: {message}");
            Console.WriteLine();
            Console.WriteLine("  Possible causes:");
            Console.WriteLine("    • The token.bin file was not created on this machine.");
            Console.WriteLine("    • The token.bin file is corrupted or incomplete.");
            Console.WriteLine("    • The encryption key (hardware ID) does not match.");
            Console.WriteLine();
            Console.WriteLine("  How to fix:");
            Console.WriteLine("    1. Delete the old token.bin file.");
            Console.WriteLine("    2. Run this tool with '--tokenfile <yourPAT>' on this machine to generate a new token.bin.");
            Console.WriteLine("    3. Use: '--token token.bin' for future runs on this machine.");
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Displays an error message for unexpected errors encountered while reading the token.bin file.
        /// </summary>
        /// <param name="message">The specific error message or reason for the read failure.</param>
        public static void ShowTokenReadError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("  ERROR: Unexpected error while reading token.bin");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine($"  Reason: {message}");
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}