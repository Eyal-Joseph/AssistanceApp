using Assistance.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Embeddings;

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var history = new ChatHistory();
var functionInvocationList = new List<string>();
var _collectionName = "SampleDataCollection";
var fileList = new List<string>()
{
    "SampleData/Elena-Adam-facts.txt",
    "SampleData/Noa-Daniel-facts.txt"
};
var sqlConnectionString = "Server=localhost,1433;Database=VectorStore;User Id=sa;Password=StrongPassw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
    .Build();

var modelName = configuration["ModelName"] ?? throw new ApplicationException("ModelName not found");
var embedding = configuration["EmbeddingModel"] ?? throw new ApplicationException("EmbeddingModel not found");
var apiKey = configuration["ApiKey"] ?? throw new ApplicationException("ApiKey not found");

var httpClient = new HttpClient(new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
});


// Create kernel with Google AI connector
var builder = Kernel.CreateBuilder()
        .AddGoogleAIGeminiChatCompletion(modelName, apiKey, httpClient: httpClient)
        .AddGoogleAIEmbeddingGenerator(embedding, apiKey, httpClient: httpClient);

/*
//Sample 2
// Create InMemory vector store service
//builder.Services.AddInMemoryVectorStore();
*/
//Sample 3
// Create SQL vector store service
builder.Services.AddSqlServerVectorStore(connectionStringProvider: serviceProvider => sqlConnectionString);


builder.Services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
{
    return new GoogleAITextEmbeddingGenerationService(embedding, apiKey, httpClient: httpClient);
});

//builder.Services.AddSingleton<IPromptRenderFilter, SafePromptFilter>();
//builder.Services.AddSingleton<IAutoFunctionInvocationFilter, EarlyTerminationFilter>();
//builder.Services.AddSingleton< IFunctionInvocationFilter, AutoFunctionInvocationFilter>();

//builder.Services.AddLogging(configure => { configure.AddConsole(); configure.SetMinimumLevel(LogLevel.Information); });
var kernel = builder.Build();
kernel.Plugins.AddFromType<DateTimePlugin>();



kernel.FunctionInvocationFilters.Add(new FunctionInvocationFilter(functionInvocationList));

// Register the embedding service
var embeddingService = kernel.Services.GetService<ITextEmbeddingGenerationService>();


var vectorStore = kernel.GetRequiredService<VectorStore>();

//foreach (var file in fileList)
//{
//    var textChunks = DocumentReader.ParseFile(file);
//    var dataUploader = new DataUploader(vectorStore, embeddingService);
//    await dataUploader.UploadToVectorStore(_collectionName, textChunks);
//}

var searchPlugin = new SearchPlugin(kernel, vectorStore, embeddingService);
kernel.Plugins.AddFromObject(searchPlugin);


var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var executionSettings = new GeminiPromptExecutionSettings()
{
    ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions, // Enable function calling
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),    
};
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


var systemMessage = "You are a RAG‐enabled assistant. For every query:\n" +
                    "1. Always try to invoke the “SearchPlugin” to retrieve relevant text chunks.\n" +
                    "2. Base your answer on those chunks whenever possible.\n" +
                    $"3.If no specific collection is defined, use the collection named {_collectionName}" +
                    "Do not add OK." +
                    "Keep answers concise and grounded in the retrieved material.";
history.AddSystemMessage(systemMessage);
// Exported function for processing user input and streaming response
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
async Task<string> ProcessUserInputAsync(string userInput, ChatHistory history, IChatCompletionService chatCompletionService, GeminiPromptExecutionSettings executionSettings, Kernel kernel)
{
    functionInvocationList.Clear();
    history.AddUserMessage(userInput!);

    var streamingResponse =
        chatCompletionService.GetStreamingChatMessageContentsAsync(
            history,
            executionSettings,
            kernel);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Agent > ");
    Console.ResetColor();

    var fullResponse = "";
    
    await foreach (var chunk in streamingResponse)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(chunk.Content);
        Console.ResetColor();
        fullResponse += chunk.Content;
    }
    return fullResponse;
}
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

do
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Me > ");
    Console.ResetColor();

    var userInput = Console.ReadLine();
    if (userInput == "exit")
    {
        break;
    }

    string fullResponse = "";
    try
    {
        fullResponse = await ProcessUserInputAsync(userInput!, history, chatCompletionService, executionSettings, kernel);
    }
    catch (HttpOperationException ex)
    {
        //Console.WriteLine($"HTTP Operation failed:");
        //Console.WriteLine($"Status Code: {ex.StatusCode}");
        //Console.WriteLine($"Message: {ex.Message}");

        // Get the actual response content for debugging
        //if (ex.ResponseContent != null)
        //{
        //    Console.WriteLine($"Response Content: {ex.ResponseContent}");
        //}

        // Handle specific error codes
        //switch (ex.StatusCode)
        //{
        //    case System.Net.HttpStatusCode.BadRequest:
        //        Console.WriteLine("Bad request - check your API key, model name, or request format");
        //        break;
        //    case System.Net.HttpStatusCode.Unauthorized:
        //        Console.WriteLine("Unauthorized - check your API key");
        //        break;
        //    case System.Net.HttpStatusCode.TooManyRequests:
        //        Console.WriteLine("Rate limit exceeded - wait before retrying");
        //        break;
        //    case System.Net.HttpStatusCode.InternalServerError:
        //        Console.WriteLine("Google AI API internal server error");
        //        break;
        //    default:
        //        Console.WriteLine($"Unexpected status code: {ex.StatusCode}");
        //        break;
        //}

        var temphistory = new ChatHistory();
        temphistory.AddSystemMessage(systemMessage);

        foreach (var msg in history)
        {
            if (msg.Role == AuthorRole.System)
                continue;

            if (msg.Content != "")
                temphistory.Add(msg);
                //temphistory.AddUserMessage(msg.Content);
        }

        history.Clear();
        history = temphistory;
        foreach (var function in functionInvocationList.ToList())
        {
            var response = kernel.InvokePromptAsync(function, new(executionSettings)).Result;
            fullResponse += response.ToString().Replace("\n", " ").Replace("\r", " ");
            
        }
        functionInvocationList.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(fullResponse);
        Console.ResetColor();

        //fullResponse = await ProcessUserInputAsync(userInput!, history, chatCompletionService, executionSettings, kernel);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }

    Console.WriteLine();

    history.AddMessage(AuthorRole.Assistant, fullResponse);


} while (true);

#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
