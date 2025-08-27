using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;


public class FunctionInvocationFilter : IFunctionInvocationFilter
{
    private readonly List<string> _functionInvocationList;

  public FunctionInvocationFilter(List<string> functionInvocationList)
    {
        _functionInvocationList = functionInvocationList;
    }
    
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {

        _functionInvocationList.Add(context.Function.Name);

        // Call next filter in pipeline or actual function.
        await next(context);

        //// Check which function invocation mode is used.
        //if (context.IsStreaming)
        //{
        //    // Return IAsyncEnumerable<string> result in case of streaming mode.
        //    var enumerable = context.Result.GetValue<IAsyncEnumerable<string>>();
        //    context.Result = new FunctionResult(context.Result, OverrideStreamingDataAsync(enumerable!));
        //}
        //else
        //{
        //    // Return just a string result in case of non-streaming mode.
        //    var data = context.Result.GetValue<string>();
        //    context.Result = new FunctionResult(context.Result, OverrideNonStreamingData(data));
        //}
    }

    private async IAsyncEnumerable<string> OverrideStreamingDataAsync(IAsyncEnumerable<string> data)
    {
        await foreach (var item in data)
        {
            yield return $"{item} - updated from filter";
        }
    }

    private string OverrideNonStreamingData(string data)
    {
        return $"{data}";
    }
}