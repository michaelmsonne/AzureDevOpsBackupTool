using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    public class DevOpsProjectTeams
    {
        public static async Task<string> GetDevOpsProjectTeams(string devOpsOrgName, string token, string projectId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json-patch+json"));

                var url = $"https://dev.azure.com/{devOpsOrgName}/_apis/projects/{projectId}/teams?api-version=7.2-preview.3";

                Message($"Calling API to get teams from {url}", EventType.Information, 1010);

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Message($"Error: {response.StatusCode}", EventType.Error, 1011);
                    Message($"Response content: {content}", EventType.Error, 1012);
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }

                // Check if content is HTML
                if (content.TrimStart().StartsWith("<"))
                {
                    Message("Error: The response is in HTML format, indicating a possible issue with the request.", EventType.Error, 1013);
                    Message($"Response content: {content}", EventType.Error, 1014);
                    throw new HttpRequestException("The response is in HTML format, indicating a possible issue with the request.");
                }

                return content;
            }
        }
    }
}
