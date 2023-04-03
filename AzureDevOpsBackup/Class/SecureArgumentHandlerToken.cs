using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    public class SecureArgumentHandlerToken
    {
        public static string EncryptAndSaveToFile(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] encryptedBytes;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        encryptedBytes = memoryStream.ToArray();
                    }
                }
            }
            //File.WriteAllBytes("data.bin", encryptedBytes);

            try
            {
                // Try to save the file
                File.WriteAllBytes(Files.TokenFilePath, encryptedBytes);
                
                // Log
                Message("Saved encrypted token to file: " + Files.TokenFilePath + " in application folder: " + Files.ProgramDataFilePath,
                    EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Saved encrypted token to file: " + Files.TokenFilePath + " in application folder: " + Files.ProgramDataFilePath + Environment.NewLine);
                Console.ResetColor();
            }
            catch (UnauthorizedAccessException)
            {
                Message("Unable to save encrypted token to file: " + Files.TokenFilePath + " in application folder: " + Files.ProgramDataFilePath + ". Make sure the account you use to run this tool has write rights to this location.", EventType.Error, 1001);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unable to save encrypted token to file: " + Files.TokenFilePath + " in application folder: " + Files.ProgramDataFilePath + ". Make sure the account you use to run this tool has write rights to this location.");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                // Error if cant delete file(s)
                Message(
                    "Exception caught when trying to save encrypted token to file: " + Files.TokenFilePath + " in application folder: " + Files.ProgramDataFilePath + " - error: " + e, EventType.Error, 1001);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exception caught when trying to save encrypted token to file: " + Files.TokenFilePath + " in application folder: " + Files.ProgramDataFilePath + " - error: " + e);
                Console.ResetColor();
            }

            //File.WriteAllBytes(Files.TokenFilePath, encryptedBytes);
            return encryptedBytes.ToString();
        }

        public static string DecryptFromFile(string key)
        {
            byte[] iv = new byte[16];
            //byte[] encryptedBytes = File.ReadAllBytes("data.bin");
            byte[] encryptedBytes = File.ReadAllBytes(Files.TokenFilePath);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        public static string GetComputerId()
        {
            string processorId = "", motherboardId = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select ProcessorId from Win32_Processor");
            foreach (ManagementObject item in searcher.Get())
            {
                processorId = item["ProcessorId"].ToString();
            }

            searcher = new ManagementObjectSearcher("select SerialNumber from Win32_BaseBoard");
            foreach (ManagementObject item in searcher.Get())
            {
                motherboardId = item["SerialNumber"].ToString();
            }

            string computerId = processorId + motherboardId;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(computerId));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 16; i++) // Iterate through the first 16 bytes (32 characters) of the hash value
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
