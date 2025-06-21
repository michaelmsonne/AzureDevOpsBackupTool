using System;
using System.IO;
using RestSharp;
using System.Net;

namespace AzureDevOpsBackup.Class
{
    internal class HealthChecker
    {
        /// <summary>
        /// Checks if the Azure DevOps API is reachable with the given organization and PAT.
        /// </summary>
        public static bool CheckAzureDevOpsApi(string org, string pat, string apiVersion, out string error)
        {
            error = null;
            try
            {
                string baseUrl = $"https://dev.azure.com/{org}/";
                string auth = "Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{pat}"));
                var client = new RestClient(baseUrl + $"_apis/projects?{apiVersion}");
                var request = new RestRequest { Method = Method.Get };
                request.AddHeader("Authorization", auth);

                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    return true;

                error = $"Status: {response.StatusCode}, Content: {response.Content}";
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Checks if the backup folder is writable.
        /// </summary>
        public static bool CheckBackupFolderWriteAccess(string backupFolder, out string error)
        {
            error = null;
            try
            {
                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                string testFile = Path.Combine(backupFolder, "healthcheck_test.txt");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}