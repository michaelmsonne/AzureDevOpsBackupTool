using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    public class DevOpsProjectTeamTasks
    {
        public static async Task<string> GetDevOpsProjectTeamTasks(string devOpsOrgName, string token, string projectId, string team)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var query = new
                {
                    query = "Select [System.Id], [System.Title], [System.State], [System.TeamProject],[System.Parent] From WorkItems order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc"
                };

                var queryJson = JsonConvert.SerializeObject(query);
                var content = new StringContent(queryJson, Encoding.UTF8, "application/json");

                var url = $"https://dev.azure.com/{devOpsOrgName}/{projectId}/{team}/_apis/wit/wiql?api-version=7.2-preview.2";

                Message($"Calling API to get team tasks from {url}", EventType.Information, 1015);

                var response = await client.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Message($"Error: {response.StatusCode}", EventType.Error, 1016);
                    Message($"Response content: {responseBody}", EventType.Error, 1017);
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }

                // Check if content is HTML
                if (responseBody.TrimStart().StartsWith("<"))
                {
                    Message("Error: The response is in HTML format, indicating a possible issue with the request.", EventType.Error, 1018);
                    Message($"Response content: {responseBody}", EventType.Error, 1019);
                    throw new HttpRequestException("The response is in HTML format, indicating a possible issue with the request.");
                }

                return responseBody;
            }
        }
    }
}
