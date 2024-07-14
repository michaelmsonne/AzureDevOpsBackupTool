using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    public class DevOpsTask
    {
        public static async Task<string> GetDevOpsTask(string url, string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Message($"Calling API to get work item details from {url}", EventType.Information, 1000);

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Message($"Error: {response.StatusCode}", EventType.Error, 1001);
                    Message($"Response content: {content}", EventType.Error, 1002);
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }

                // Check if content is HTML
                if (content.TrimStart().StartsWith("<"))
                {
                    Message("Error: The response is in HTML format, indicating a possible issue with the request.", EventType.Error, 1003);
                    Message($"Response content: {content}", EventType.Error, 1004);
                    throw new HttpRequestException("The response is in HTML format, indicating a possible issue with the request.");
                }

                return content;
            }
        }
    }
}
