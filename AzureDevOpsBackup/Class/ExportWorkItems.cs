using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using static AzureDevOpsBackup.Class.FileLogger;

namespace AzureDevOpsBackup.Class
{
    public class ExportWorkItems
    {
        public static async Task ExportWorkItemsToCsv(string pat, string devOpsOrgName, string outputFileName)
        {
            var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));
            var baseUrl = $"https://dev.azure.com/{devOpsOrgName}/";

            // Create RestClient with base URL
            var client = new RestClient(baseUrl);

            try
            {
                // Get projects
                var projectsUrl = $"_apis/projects?api-version=7.2-preview.4";
                var projectsRequest = new RestRequest(projectsUrl, Method.Get);
                projectsRequest.AddHeader("Authorization", $"Basic {token}");

                Message($"Calling API to get projects from {baseUrl}{projectsUrl}", EventType.Information, 1020);

                var projectsResponse = await client.GetAsync(projectsRequest);

                if (projectsResponse.StatusCode == HttpStatusCode.OK)
                {
                    Message($"Connected successfully to Azure DevOps organization '{devOpsOrgName}'.", EventType.Information, 1000);
                    Console.WriteLine($"Connected successfully to Azure DevOps organization '{devOpsOrgName}'.");

                    var projectsJson = projectsResponse.Content;
                    var projects = JsonConvert.DeserializeObject<dynamic>(projectsJson);

                    // Get teams
                    var projectId = projects[0].id.ToString();
                    var teamsUrl = $"{projectId}/teams?api-version=7.2-preview.3";
                    var teamsRequest = new RestRequest(teamsUrl, Method.Get);
                    teamsRequest.AddHeader("Authorization", $"Basic {token}");

                    Message($"Calling API to get teams for project {projectId} from {baseUrl}{teamsUrl}", EventType.Information, 1021);

                    var teamsResponse = await client.GetAsync(teamsRequest);

                    if (teamsResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var teamsJson = teamsResponse.Content;
                        var teams = JsonConvert.DeserializeObject<dynamic>(teamsJson);

                        // Get work items
                        var teamId = teams[0].id.ToString();
                        var workItemsUrl = $"{projectId}/{teamId}/_apis/wit/wiql?api-version=7.2-preview.2";
                        var query = new
                        {
                            query = "Select [System.Id], [System.Title], [System.State], [System.TeamProject],[System.Parent] From WorkItems order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc"
                        };
                        var queryJson = JsonConvert.SerializeObject(query);
                        var workItemsRequest = new RestRequest(workItemsUrl, Method.Post);
                        workItemsRequest.AddHeader("Authorization", $"Basic {token}");
                        workItemsRequest.AddHeader("Content-Type", "application/json");
                        workItemsRequest.AddParameter("application/json", queryJson, ParameterType.RequestBody);

                        Message($"Calling API to get work items for team {teamId} in project {projectId} from {baseUrl}{workItemsUrl}", EventType.Information, 1022);

                        var workItemsResponse = await client.PostAsync(workItemsRequest);

                        if (workItemsResponse.StatusCode == HttpStatusCode.OK)
                        {
                            var workItemsJson = workItemsResponse.Content;
                            var workItems = JsonConvert.DeserializeObject<dynamic>(workItemsJson);

                            var objectCollection = new List<dynamic>();

                            foreach (var workItem in workItems.workItems)
                            {
                                var url = $"{workItem.url}?$expand=all";
                                var workItemRequest = new RestRequest(url, Method.Get);
                                workItemRequest.AddHeader("Authorization", $"Basic {token}");

                                Message($"Calling API to get details for work item {workItem.id} from {url}", EventType.Information, 1023);

                                var workItemResponse = await client.GetAsync(workItemRequest);
                                var workItemDetailsJson = workItemResponse.Content;

                                if (workItemResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    var workItemDetails = JsonConvert.DeserializeObject<dynamic>(workItemDetailsJson);

                                    var obj = new
                                    {
                                        Id = workItemDetails.id,
                                        Title = workItemDetails.fields["System.Title"],
                                        AreaPath = workItemDetails.fields["System.AreaPath"],
                                        TeamProject = workItemDetails.fields["System.TeamProject"],
                                        IterationPath = workItemDetails.fields["System.IterationPath"],
                                        WorkItemType = workItemDetails.fields["System.WorkItemType"],
                                        State = workItemDetails.fields["System.State"],
                                        Reason = workItemDetails.fields["System.Reason"],
                                        AssignedTo = workItemDetails.fields["System.AssignedTo"],
                                        CreatedDate = workItemDetails.fields["System.CreatedDate"],
                                        CreatedBy = workItemDetails.fields["System.CreatedBy"],
                                        ChangedDate = workItemDetails.fields["System.ChangedDate"],
                                        ChangedBy = workItemDetails.fields["System.ChangedBy"],
                                        CommentCount = workItemDetails.fields["System.CommentCount"],
                                        BoardColumn = workItemDetails.fields["System.BoardColumn"],
                                        BoardColumnDone = workItemDetails.fields["System.BoardColumnDone"],
                                        Description = workItemDetails.fields["System.Description"],
                                        Parent = workItemDetails.fields["System.Parent"]
                                    };

                                    objectCollection.Add(obj);
                                }
                                else
                                {
                                    Message($"Error: {workItemResponse.StatusCode}", EventType.Error, 1024);
                                    Message($"Response content: {workItemDetailsJson}", EventType.Error, 1025);
                                    throw new HttpRequestException($"Request for work item details failed with status code {workItemResponse.StatusCode}");
                                }
                            }

                            // Write to CSV
                            using (var writer = new StreamWriter(outputFileName))
                            {
                                var header = "Id;Title;AreaPath;TeamProject;IterationPath;WorkItemType;State;Reason;AssignedTo;CreatedDate;CreatedBy;ChangedDate;ChangedBy;CommentCount;BoardColumn;BoardColumnDone;Description;Parent";
                                writer.WriteLine(header);

                                foreach (var obj in objectCollection)
                                {
                                    var line = $"{obj.Id};{obj.Title};{obj.AreaPath};{obj.TeamProject};{obj.IterationPath};{obj.WorkItemType};{obj.State};{obj.Reason};{obj.AssignedTo};{obj.CreatedDate};{obj.CreatedBy};{obj.ChangedDate};{obj.ChangedBy};{obj.CommentCount};{obj.BoardColumn};{obj.BoardColumnDone};{obj.Description};{obj.Parent}";
                                    writer.WriteLine(line);
                                }
                            }

                            Message($"Export completed successfully. Data saved to {outputFileName}", EventType.Information, 1026);
                        }
                        else
                        {
                            Message($"Error: {workItemsResponse.StatusCode}", EventType.Error, 1027);
                            Message($"Response content: {workItemsResponse.Content}", EventType.Error, 1028);
                            throw new HttpRequestException($"Request for work items failed with status code {workItemsResponse.StatusCode}");
                        }
                    }
                    else
                    {
                        Message($"Error: {teamsResponse.StatusCode}", EventType.Error, 1029);
                        Message($"Response content: {teamsResponse.Content}", EventType.Error, 1030);
                        throw new HttpRequestException($"Request for teams failed with status code {teamsResponse.StatusCode}");
                    }
                }
                else
                {
                    Message($"Error: {projectsResponse.StatusCode}", EventType.Error, 1031);
                    Message($"Response content: {projectsResponse.Content}", EventType.Error, 1032);
                    throw new HttpRequestException($"Request for projects failed with status code {projectsResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Message($"An error occurred: {ex.Message}", EventType.Error, 1033);
            }
        }
    }
}
