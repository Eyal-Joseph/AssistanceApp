using Microsoft.SemanticKernel;

/// <summary>
/// Example of auto function invocation filter which terminates function calling process as soon as we have the desired result.
/// </summary>
public sealed class EarlyTerminationFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        // Call the function first.
        await next(context);

        // Get a function result from context.
        var result = context.Result.GetValue<string>();

        // If the result meets the condition, terminate the process.
        // Otherwise, the function calling process will continue.
        if (result == "desired result")
        {
            context.Terminate = true;
        }
    }
}