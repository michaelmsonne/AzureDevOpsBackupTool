using RestSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureDevOpsBackup.Class
{
    /// <summary>
    /// Client for interacting with the Azure DevOps REST API.
    /// Provides methods to retrieve projects, repositories, branches, items, and download blobs.
    /// </summary>
    internal class AzureDevOpsApiClient
    {
        // Base URL for the Azure DevOps organization or project.
        private readonly string _baseUrl;
        // Authorization header value for Basic authentication using a Personal Access Token (PAT).
        private readonly string _authHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureDevOpsApiClient"/> class.
        /// </summary>
        /// <param name="baseUrl">The base URL of the Azure DevOps organization (e.g., https://dev.azure.com/yourorg/).</param>
        /// <param name="pat">The Personal Access Token for authentication.</param>
        public AzureDevOpsApiClient(string baseUrl, string pat)
        {
            _baseUrl = baseUrl;
            _authHeader = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{pat}"));
        }

        /// <summary>
        /// Retrieves the list of projects from Azure DevOps.
        /// </summary>
        /// <param name="apiVersion">The API version query string (e.g., "api-version=6.0").</param>
        /// <returns>A <see cref="RestResponse"/> containing the projects data.</returns>
        public async Task<RestResponse> GetProjectsAsync(string apiVersion)
        {
            var client = new RestClient(_baseUrl + $"_apis/projects?{apiVersion}");
            var request = new RestRequest { Method = Method.Get };
            request.AddHeader("Authorization", _authHeader);
            return await client.ExecuteAsync(request);
        }

        /// <summary>
        /// Retrieves the list of repositories for a given project.
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project.</param>
        /// <param name="apiVersion">The API version query string.</param>
        /// <returns>A <see cref="RestResponse"/> containing the repositories data.</returns>
        public async Task<RestResponse> GetReposAsync(string projectName, string apiVersion)
        {
            var client = new RestClient(_baseUrl + $"{projectName}/_apis/git/repositories?{apiVersion}");
            var request = new RestRequest { Method = Method.Get };
            request.AddHeader("Authorization", _authHeader);
            return await client.ExecuteAsync(request);
        }

        /// <summary>
        /// Retrieves the list of branches for a given repository.
        /// </summary>
        /// <param name="repoId">The ID of the repository.</param>
        /// <param name="apiVersion">The API version query string.</param>
        /// <returns>A <see cref="RestResponse"/> containing the branches data.</returns>
        public async Task<RestResponse> GetBranchesAsync(string repoId, string apiVersion)
        {
            var client = new RestClient(_baseUrl + $"_apis/git/repositories/{repoId}/refs?{apiVersion}");
            var request = new RestRequest { Method = Method.Get };
            request.AddHeader("Authorization", _authHeader);
            return await client.ExecuteAsync(request);
        }

        /// <summary>
        /// Retrieves the list of items (files and folders) in a repository branch.
        /// </summary>
        /// <param name="repoId">The ID of the repository.</param>
        /// <param name="branchName">The name of the branch.</param>
        /// <param name="apiVersion">The API version query string.</param>
        /// <returns>A <see cref="RestResponse"/> containing the items data.</returns>
        public async Task<RestResponse> GetItemsAsync(string repoId, string branchName, string apiVersion)
        {
            var client = new RestClient(_baseUrl + $"_apis/git/repositories/{repoId}/items?recursionlevel=full&{apiVersion}&versionDescriptor.versionType=Branch&versionDescriptor.version={branchName}");
            var request = new RestRequest { Method = Method.Get };
            request.AddHeader("Authorization", _authHeader);
            return await client.ExecuteAsync(request);
        }

        /// <summary>
        /// Downloads blobs (files) from a repository as a ZIP stream.
        /// </summary>
        /// <param name="repoId">The ID of the repository.</param>
        /// <param name="objectIds">A list of object IDs (blob IDs) to download.</param>
        /// <param name="apiVersion">The API version query string.</param>
        /// <returns>A <see cref="Stream"/> containing the ZIP file with the requested blobs.</returns>
        public Stream DownloadBlobs(string repoId, List<string> objectIds, string apiVersion)
        {
            var client = new RestClient(_baseUrl + $"_apis/git/repositories/{repoId}/blobs?{apiVersion}");
            var request = new RestRequest { Method = Method.Post };
            request.AddHeader("Authorization", _authHeader);
            request.AddHeader("Accept", "application/zip");
            request.AddJsonBody(objectIds);
            return client.DownloadStream(request);
        }
    }
}