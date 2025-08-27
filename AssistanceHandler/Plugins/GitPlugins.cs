using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Microsoft.SemanticKernel;

namespace Assistance.Plugins;

public class GitPlugins : IBasePlugin
{
    string _repoPath;

    [KernelFunction]
    [Description("Function to retrieve the latest commits")]
    string GetCommits(
        [Description("number of commits")]
        int numberOfCommits)
    {
        if (string.IsNullOrEmpty(_repoPath))
        {
            return "Repository path is required";
        }
        using var repo = new Repository(_repoPath);

        var sb = new StringBuilder();

        foreach (var commit in repo.Commits.Take(numberOfCommits))
        {
            sb.AppendLine($"{commit.Committer.When} {commit.Author.Name}: {commit.MessageShort} {commit.Sha}");
        }

        return sb.ToString();
    }

    [KernelFunction]
    [Description("Function to set the repository path")]
    string SetRepoPath(
        [Description("Repository path")]
        string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "Repository path is required";
        }
        if (!Directory.Exists(path))
        {
            return "Repository path does not exist";
        }
        _repoPath = path;
        return $"Repository path set to {path}";
    }

    [KernelFunction]
    [Description("Function to retrieve the latest commits by user name")]
    string GetCommitsByUserName(
        [Description("number of commits")]
        int numberOfCommits,
        [Description("user name")]
        string userName)
    {
        if (string.IsNullOrEmpty(_repoPath))
        {
            return "Repository path is required";
        }
        using var repo = new Repository(_repoPath);
        var sb = new StringBuilder();
        foreach (var commit in repo.Commits
                     .Where(c => c.Author.Name.Contains(userName, StringComparison.OrdinalIgnoreCase))
                     .Take(numberOfCommits))
        {
            sb.AppendLine($"{commit.Author.When} {commit.Author.Name}: {commit.MessageShort} {commit.Sha}");
        }
        return sb.ToString();
    }

    [KernelFunction]
    [Description("Get the related work item for a specific commit")]
    public Task<string> GetWorkItemForCommitAsync([Description("Commit SHA")] string commitSha)
    {
        if (!Repository.IsValid(_repoPath))
        {
            throw new InvalidOperationException("The specified path is not a valid Git repository.");
        }

        using var repo = new Repository(_repoPath);
        var commit = repo.Commits.FirstOrDefault(c => c.Sha.StartsWith(commitSha, StringComparison.OrdinalIgnoreCase));

        if (commit == null)
        {
            throw new ArgumentException($"Commit with SHA '{commitSha}' not found.");
        }

        // Extract work item ID from the commit message using a regex (e.g., #1234 or WI-5678)
        var workItemRegex = new Regex(@"#(\d+)|WI-(\d+)", RegexOptions.IgnoreCase);
        var match = workItemRegex.Match(commit.Message);

        if (!match.Success)
        {
            return Task.FromResult("No related work item found in the commit message.");
        }

        var workItemId = match.Value;
        return Task.FromResult($"Related work item: {workItemId}");
    }
}