using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Assistance.Plugins;

public class TimePlugin : IBasePlugin
{
    [KernelFunction]
    [Description("Get the current time in the format 'HH:mm:ss'")]
    public string GetCurrentTime()
    {
        return DateTime.Now.ToString("HH:mm:ss");
    }

    [KernelFunction]
    [Description("Get the current date in the format 'yyyy-MM-dd'")]
    public string GetCurrentDate()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }
}