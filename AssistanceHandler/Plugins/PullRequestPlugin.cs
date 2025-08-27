namespace Assistance.Plugins;

using LibGit2Sharp;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AzureDevOpsPullRequest
{
    public int PullRequestId { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public string Url { get; set; }
}

public class PullRequestPlugin : IBasePlugin
{
    HttpClient _client;
    string _personalAccessToken;

    [KernelFunction]
    [Description("Function to Set the Personal Access Token")]
    string GetPersonalAccessToken(
        [Description("Personal Access Token")]
        string personalAccessToken)
    {

        _client = new HttpClient();
        var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        _personalAccessToken = personalAccessToken;

        return $"Personal Access Token is {_personalAccessToken}";
    }

    [KernelFunction]
    [Description("Function to Get Active Pull Requests")]
    public async Task<string> GetActivePullRequestsAsync(
        [Description("Pull Request repository")]
        string repo)
    {
        if(string.IsNullOrEmpty(_personalAccessToken))
        {
            return "Personal Access Token is not set.";
        }

        if (_client == null)
            GetPersonalAccessToken(_personalAccessToken);

        string url = $"https://mickeymouse/tfs/DefaultCollection/Spitfire/_apis/git/repositories/{repo}/pullrequests?searchCriteria.status=active&api-version=7.1-preview.1&_a=Active";

        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to fetch work item. Status code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();

        var workItem = JsonSerializer.Deserialize<JsonElement>(content);
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        var values = root.GetProperty("value");

        var pullRequests = new List<AzureDevOpsPullRequest>();

        foreach (var pr in values.EnumerateArray())
        {
            pullRequests.Add(new AzureDevOpsPullRequest
            {
                PullRequestId = pr.GetProperty("pullRequestId").GetInt32(),
                Title = pr.GetProperty("title").GetString(),
                Status = pr.GetProperty("status").GetString(),
                Url = pr.GetProperty("url").GetString()
            });
        }

        var sb = new StringBuilder();

        foreach (var pullRequest in pullRequests)
        {
            sb.AppendLine($"Pull Request #{pullRequest.PullRequestId}: {pullRequest.Title} - {pullRequest.Status}");
        }

        return sb.ToString();
    }
}
