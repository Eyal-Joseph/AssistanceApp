using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Assistance.Plugins;

public class DateTimePlugin
{
    [KernelFunction]
    [Description("Get Current Time")]
    public string GetCurrentTime()
    {
        return DateTime.Now.ToString("HH:mm:ss");
    }

    [KernelFunction]
    [Description("Get Current Date")]
    public string GetCurrentDate()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }
}