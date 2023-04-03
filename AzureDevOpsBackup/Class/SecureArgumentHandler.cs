using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace AzureDevOpsBackup.Class
{
    public class SecureArgumentHandler
    {
        private byte[] key;
        private byte[] IV;

        public SecureArgumentHandler()
        {
            // Generate a random key and IV to use for encryption and decryption
            using (Aes aes = Aes.Create())
            {
                key = aes.Key;
                IV = aes.IV;
            }
        }

        public byte[] Encrypt(string value)
        {
            byte[] encryptedValue;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(value);
                        }
                        encryptedValue = msEncrypt.ToArray();
                    }
                }
            }
            return encryptedValue;
        }

        public string Decrypt(byte[] secureValue)
        {
            byte[] decryptedValue;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = IV;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(secureValue))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            decryptedValue = Encoding.UTF8.GetBytes(srDecrypt.ReadToEnd());
                        }
                    }
                }
            }
            return Encoding.UTF8.GetString(decryptedValue);
        }
    }
}