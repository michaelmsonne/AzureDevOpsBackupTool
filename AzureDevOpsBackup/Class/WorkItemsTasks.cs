using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Util;
using System.IO;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    internal class WorkItemsTasks
    {
        private readonly string _personalAccessToken;
        private readonly string _organization;
        private readonly string _project;

        // How to get work items from Azure DevOps using REST API where input is: personalAccessToken, organization and the project

        public WorkItemsTasks(string personalAccessToken, string organization, string project)
        {
            _personalAccessToken = personalAccessToken;
            _organization = organization;
            _project = project;

            if (string.IsNullOrEmpty(_personalAccessToken) || string.IsNullOrEmpty(_organization) || string.IsNullOrEmpty(_project))
            {
                throw new ArgumentException("Personal Access Token, Organization and Project are required.");
            }
        }

        public async Task<WorkItemsDetailResult> GetWorkItemsDetail(string[] ids)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}")));

                var url = $"{_organization}{_project}/_apis/wit/workitems?ids={string.Join(",", ids)}&api-version=5.1";
                Console.WriteLine($"Requesting URL: {url}");

                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    Console.WriteLine($"Response Status Code: {response.StatusCode}");
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorResponse}");
                    }
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<WorkItemsDetailResult>(responseBody);
                }
            }
        }
        /*
        public async Task<WorkItem[]> GetCurrentIterationPBIs()
        {
            var query = "Select [System.Id], [System.Title], [System.State] From WorkItems Where [System.WorkItemType] = 'Product Backlog Item' AND [State] <> 'Closed' AND [State] <> 'Removed' AND [Iteration Path] = @CurrentIteration order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc";
            var result = await MakeWITQuery<WITResult>(query);
            return result.workItems;
        }*/

        public async Task<WorkItemRelation[]> GetChildWorkItemsFromParentWorkItemId(string Id)
        {
            var query = $"select * from WorkItemLinks where ([System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward') and (Source.[System.Id] = {Id}) order by [System.Id] mode (ReturnMatchingChildren)";
            var result = await MakeWITQuery<WITRelationsResult>(query);
            return result.workItemRelations;
        }

        public async Task<T> MakeWITQuery<T>(string query)
        {
            var queryObj = new { query };

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}")));

                var url = $"{_organization}{_project}/_apis/wit/wiql?api-version=5.1";
                Console.WriteLine($"Posting to URL: {url} with query: {JsonConvert.SerializeObject(queryObj)}");

                using (HttpResponseMessage response = await client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(queryObj), Encoding.UTF8, "application/json")))
                {
                    Console.WriteLine($"Response Status Code: {response.StatusCode}");
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorResponse}");
                    }
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(responseBody);
                }
            }
        }

        public static async Task SaveWorkItems(string baseUrl, string projectName, string auth, string outDirSaveToDisk)
        {
            // Construct the URL for the Azure DevOps REST API call
            string url = $"{baseUrl}/{projectName}/_apis/wit/wiql?" + Globals.APIversion;
            string query = "{\"query\":\"Select [System.Id], [System.Title], [System.State], [System.TeamProject],[System.Parent] From WorkItems order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc\"}";

            using (var client = new HttpClient())
            {
                // Set up HttpClient with authentication and headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{auth}")));

                // Make the POST request
                HttpResponseMessage response = await client.PostAsync(url, new StringContent(query, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    // Output response data
                    Console.WriteLine($"Response: {response}");
                    // Parse the response and save or process the work items
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Assuming you have a class that matches the JSON structure of your response
                    var workItems = JsonConvert.DeserializeObject<WorkItemsResponse>(responseContent);
                    try
                    {
                        

                        // Save or process the work items
                        // For example, save to disk
                        string filePath = Path.Combine(outDirSaveToDisk, "workitems.json");
                        // Replace the direct call to WriteAllTextAsync with Task.Run wrapping WriteAllText
                        await Task.Run(() => File.WriteAllText(filePath, responseContent));
                        Message("Saved: " + filePath, EventType.Information, 1000);
                    }
                    catch (JsonReaderException ex)
                    {
                        Console.WriteLine("Failed to parse JSON response.");
                        Console.WriteLine($"Response Content: {responseContent}");
                    }                    
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {errorContent}");
                    return;
                }
            }
        }

        public class WorkItemsResponse
        {
            public List<WorkItem> WorkItems { get; set; }
        }

        public class WorkItem
        {
            public int Id { get; set; }
            public string Title { get; set; }
            // Add other properties as needed
        }
        /*public static void SaveWorkItems(string baseUrl, string project, string auth, string outDirSaveToDisk)
        {
            /*var azureHelper = new WorkItemsTasks(auth, baseUrl, project);

            var workItems = azureHelper.GetCurrentIterationPBIs().Result;

            if (workItems == null || workItems.Length == 0)
            {
                Console.WriteLine("No work items found.");
                return;
            }

            var workItemIds = workItems.Select(wi => wi.id.ToString()).ToArray();
            var workItemsDetails = azureHelper.GetWorkItemsDetail(workItemIds).Result;

            var csv = new StringBuilder();
            csv.AppendLine("ID,Title,State,AssignedTo,CreatedDate,ChangedDate");

            foreach (var workItem in workItemsDetails.workItems)
            {
                var fields = workItem.fields;
                csv.AppendLine($"{workItem.id},{fields.Title},{fields.State},{fields.AssignedTo},{fields.CreatedDate},{fields.ChangedDate}");
            }

            var filePath = Path.Combine(outDirSaveToDisk, "workitems.csv");
            File.WriteAllText(filePath, csv.ToString());
            Console.WriteLine($"Work items saved to {filePath}");}*/
    }
    public class WorkItemsDetailResult
    {
        public WorkItem[] workItems { get; set; }
    }

    public class WorkItem
    {
        public int id { get; set; }
        public Fields fields { get; set; }
    }





    public class Fields
    {
        [JsonProperty("System.Title")]
        public string Title { get; set; }
        [JsonProperty("System.State")]
        public string State { get; set; }
        [JsonProperty("System.AssignedTo")]
        public string AssignedTo { get; set; }
        [JsonProperty("System.CreatedDate")]
        public DateTime CreatedDate { get; set; }
        [JsonProperty("System.ChangedDate")]
        public DateTime ChangedDate { get; set; }
        // Add other fields as needed
    }

    public class WITResult
    {
        public WorkItem[] workItems { get; set; }
    }

    public class WorkItemRelation
    {
        // Define properties based on expected data structure
    }

    public class WITRelationsResult
    {
        public WorkItemRelation[] workItemRelations { get; set; }
    }
}