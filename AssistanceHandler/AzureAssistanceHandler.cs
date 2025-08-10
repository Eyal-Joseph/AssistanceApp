using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.Google;

namespace AssistanceHandler;
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class AzureAssistanceHandler : AssistanceHandlerBase
{
    protected override void CreateBuilder()
    {

        var modelName = Configuration["AzureModelName"] ?? throw new ApplicationException("ModelName not found");
        var embedding = Configuration["AzureEmbeddingModel"] ?? throw new ApplicationException("ModelName not found");
        var endpoint = Configuration["AzureEndpoint"] ?? throw new ApplicationException("Endpoint not found");
        var apiKey = Configuration["AzureApiKey"] ?? throw new ApplicationException("ApiKey not found");

        Builder = Kernel.CreateBuilder();

        Builder.Services.AddAzureOpenAIChatCompletion(modelName, endpoint, apiKey);
        Builder.Services.AddAzureOpenAIEmbeddingGenerator(modelName, endpoint, apiKey);

        base.CreateBuilder();
    }


    protected override void StartChat()
    {
        _executionSettings = new ()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        base.StartChat();
    }
}

#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
