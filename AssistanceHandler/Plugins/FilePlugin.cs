using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Assistance.Plugins;

public class FilePlugin : IBasePlugin
{
    [KernelFunction]
    [Description("Save Json to file")]
    public string SaveJsonToFile(
        [Description("Json data to save")]
        string json,
        [Description("File Name")]
        string fileName)
    {
        try
        {
            fileName = DateTime.Now.ToString("dd_MM_yyyy_HH_mm-") + fileName;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "ExportedData", fileName);
            File.WriteAllText(filePath, json);
            return $"Json saved to {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error saving Json to file: {ex.Message}";
        }
    }
}