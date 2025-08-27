using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Assistance.Plugins;

public class FunFilters : IBasePlugin
{
    [KernelFunction]
    [Description("Rewrites text as if spoken by a famous character.")]
    public string RewriteAsCharacter(
        [Description("The original text")] string text,
        [Description("The character to impersonate (e.g., Yoda, Gandalf, Batman)")] string character)
    {
        return $"""
        Rewrite the following text in the voice of {character}. Use their unique speech patterns and vocabulary.

        Original Text:
        "{text}"
        """;
    }
}