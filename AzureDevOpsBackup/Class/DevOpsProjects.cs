using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    public class DevOpsProjects
    {
        public static async Task<string> GetDevOpsProjects(string devOpsOrgName, string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var url = $"https://dev.azure.com/{devOpsOrgName}/_apis/projects?api-version=7.2-preview.4";

                Message($"Calling API to get projects from {url}", EventType.Information, 1005);

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Message($"Error: {response.StatusCode}", EventType.Error, 1006);
                    Message($"Response content: {content}", EventType.Error, 1007);
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }

                // Check if content is HTML
                if (content.TrimStart().StartsWith("<"))
                {
                    Message("Error: The response is in HTML format, indicating a possible issue with the request.", EventType.Error, 1008);
                    Message($"Response content: {content}", EventType.Error, 1009);
                    throw new HttpRequestException("The response is in HTML format, indicating a possible issue with the request.");
                }

                return content;
            }
        }
    }
}
