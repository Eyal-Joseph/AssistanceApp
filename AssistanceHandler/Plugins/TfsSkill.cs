using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace Assistance.Plugins;

public class TfsSkill : IBasePlugin
{
    HttpClient _client;
    private string _personalAccessToken;

    [KernelFunction]
    [Description("Function to Get Personal Access Token")]
    string GetPersonalAccessToken(
        [Description("Personal Access Token")]
        string personalAccessToken)
    {
        _personalAccessToken = personalAccessToken;

        _client = new HttpClient();
        var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        return $"Personal Access Token is {_personalAccessToken}";
    }

    [KernelFunction]
    [Description("Get details of a TFS work item by its ID")]
    public async Task<string> GetWorkItemByIdAsync([Description("Work item ID")] string workItemId)
    {

        if (string.IsNullOrEmpty(_personalAccessToken))
        {
            return "Personal Access Token is not set.";
        }

        if (string.IsNullOrWhiteSpace(workItemId))
        {
            throw new ArgumentException("Work item ID cannot be null or empty.");
        }
        string tfsBaseUrl = "https://mickeymouse/tfs/DefaultCollection/Spitfire";
        var _tfsBaseUrl = tfsBaseUrl.TrimEnd('/');

        var url = $"{_tfsBaseUrl}/_apis/wit/workitems/{workItemId}?api-version=7.0";

        var response = await _client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to fetch work item. Status code: {response.StatusCode}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var workItem = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

        // Extract relevant fields (e.g., title, state, assigned to, type, severity, priority, created date)
        var title = workItem.GetProperty("fields").GetProperty("System.Title").GetString();
        var state = workItem.GetProperty("fields").GetProperty("System.State").GetString();
        var assignedTo = workItem.GetProperty("fields").GetProperty("System.AssignedTo").GetProperty("displayName").GetString();
        var workItemType = workItem.GetProperty("fields").GetProperty("System.WorkItemType").GetString();
        var severity = workItem.GetProperty("fields").TryGetProperty("Microsoft.VSTS.Common.Severity", out var severityProperty)
            ? severityProperty.GetString()
            : "Not Specified";
        var priority = workItem.GetProperty("fields").TryGetProperty("Microsoft.VSTS.Common.Priority", out var priorityProperty)
            ? priorityProperty.GetInt32().ToString()
            : "Not Specified";
        var createdDate = workItem.GetProperty("fields").GetProperty("System.CreatedDate").GetDateTime().ToString("yyyy-MM-dd HH:mm:ss");

        return $"Work Item ID: {workItemId}\n" +
               $"Title: {title}\n" +
               $"State: {state}\n" +
               $"Assigned To: {assignedTo}\n" +
               $"Type: {workItemType}\n" +
               $"Severity: {severity}\n" +
               $"Priority: {priority}\n" +
               $"Created Date: {createdDate}";
    }
}

