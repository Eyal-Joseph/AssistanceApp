using Microsoft.SemanticKernel;

/// <summary>
/// Example of prompt render filter which overrides rendered prompt before sending it to AI.
/// </summary>
public class SafePromptFilter : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        // Example: get function information
        var functionName = context.Function.Name;

        await next(context);

        // Example: override rendered prompt before sending it to AI
        //context.RenderedPrompt = "Get date and get time";
    }
}