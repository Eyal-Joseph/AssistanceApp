using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;

namespace Assistance.Plugins;

public class CommitPatternsAnalyzerPlugin : IBasePlugin
{
    [KernelFunction]
    [Description("Analyze commit patterns")]
    public string AnalyzeCommitPatterns(
        [Description("A list of commit messages to analyze, separated by newlines.")]
        string commitMessages)
    {
        if (string.IsNullOrWhiteSpace(commitMessages))
            return "No commit messages provided.";

        var messages = commitMessages.Split('\n');
        var patternCounts = new Dictionary<string, int>
        {
            { "feature", 0 },
            { "fix", 0 },
            { "failure", 0 },
            { "release", 0 },
            { "other", 0 }
        };

        var patternRegex = new Regex(@"^(feature|fix|failure|release)\b", RegexOptions.IgnoreCase);

        foreach (var msg in messages)
        {
            var match = patternRegex.Match(msg.Trim());
            if (match.Success)
            {
                var key = match.Groups[1].Value.ToLower();
                if (patternCounts.ContainsKey(key))
                    patternCounts[key]++;
                else
                    patternCounts["other"]++;
            }
            else
            {
                patternCounts["other"]++;
            }
        }

        return
            $"Commit Pattern Analysis:\n" +
            $"- feature: {patternCounts["feature"]}\n" +
            $"- fix: {patternCounts["fix"]}\n" +
            $"- failure: {patternCounts["failure"]}\n" +
            $"- release: {patternCounts["release"]}\n" +
            $"- other: {patternCounts["other"]}";
    }
}