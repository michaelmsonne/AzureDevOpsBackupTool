using RestSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureDevOpsBackup.Class
{
    internal class AzureDevOpsApiClient
    {
        private readonly string _baseUrl;
        private readonly string _authHeader;

        public AzureDevOpsApiClient(string baseUrl, string pat)
        {
            _baseUrl = baseUrl;
            _authHeader = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{pat}"));
        }

        public async Task<RestResponse> GetProjectsAsync(string apiVersion)
        {
            var client = new RestClient(_baseUrl + $"_apis/projects?{apiVersion}");
            var request = new RestRequest { Method = Method.Get };
            request.AddHeader("Authorization", _authHeader);
            return await client.ExecuteAsync(request);
        }

        public async Task<RestResponse> GetReposAsync(string projectName, string apiVersion)
        {
            var client = new RestClient(_baseUrl + $"{projectName}/_apis/git/repositories?{apiVersion}");
            var request = new RestRequest { Method = Method.Get };
            request.AddHeader("Authorization", _authHeader);
            return await client.ExecuteAsync(request);
        }

        public async Task<RestResponse> GetBranchesAsync(string repoId, string apiVersion)
        {
            var client = new RestClient(_baseUrl + $"_apis/git/repositories/{repoId}/refs?{apiVersion}");
            var request = new RestRequest { Method = Method.Get };
            request.AddHeader("Authorization", _authHeader);
            return await client.ExecuteAsync(request);
        }

        public async Task<RestResponse> GetItemsAsync(string repoId, string branchName, string apiVersion)
        {
            var client = new RestClient(_baseUrl + $"_apis/git/repositories/{repoId}/items?recursionlevel=full&{apiVersion}&versionDescriptor.versionType=Branch&versionDescriptor.version={branchName}");
            var request = new RestRequest { Method = Method.Get };
            request.AddHeader("Authorization", _authHeader);
            return await client.ExecuteAsync(request);
        }

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