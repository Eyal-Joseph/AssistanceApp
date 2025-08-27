using Assistance.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using MongoDB.Driver;

namespace AssistanceHandler;

public abstract class AssistanceHandlerBase : IAssistanceHandler
{
    protected ChatHistory _history { get; set; }
    protected PromptExecutionSettings _executionSettings;
    protected IKernelBuilder Builder { get; set; }
    protected IChatCompletionService _chatCompletionService;
    protected string _collectionName = "SampleDataCollection";
    protected List<string> functionInvocationList = new();
    protected IConfigurationRoot Configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
        .Build();

    private readonly string _mongoConnectionString = "mongodb://root:password@mongodb:27017/?authSource=admin"; // Update if needed
    private readonly string _mongoDbName = "AssistanceDb";
    private readonly string _mongoCollectionName = "AssistanceCollection";

    protected List<string> fileList = new()
    {
        "SampleData/Elena-Adam-facts.txt",
        "SampleData/Noa-Daniel-facts.txt"
    };

    string sqlConnectionString =
        "Server=sqlserver,1433;Database=VectorStore;User Id=sa;Password=StrongPassw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";

    protected Kernel _kernel;
    private readonly string _vectorStoreType;

    public AssistanceHandlerBase(string vectorStoreType)
    {
        Console.WriteLine($"VectorStoreType = {vectorStoreType}");

        _vectorStoreType = vectorStoreType;
        CreateBuilder();
        AddPlugins();
        StartChat();
    }

    protected virtual void CreateBuilder()
    {
        Console.WriteLine($"CreateBuilder");

        if (_vectorStoreType.ToLower().Contains("mongo"))
        {
            Builder.Services.AddSingleton<IMongoDatabase>(sp =>
            {
                var mongoClient = new MongoClient(_mongoConnectionString);
                return mongoClient.GetDatabase(_mongoDbName);
            });
            Builder.Services.AddMongoVectorStore();
        }

        else if (_vectorStoreType.ToLower().Contains("sql"))
        {
            Builder.Services.AddSqlServerVectorStore(connectionStringProvider: serviceProvider => sqlConnectionString);
        }
        else
        {
            Builder.Services.AddInMemoryVectorStore();
        }






        _kernel = Builder.Build();
    }


    protected virtual void AddPlugins()
    {
        Console.WriteLine($"AddPlugins");

        var pluginTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IBasePlugin).IsAssignableFrom(t))
            .ToList();

        var embeddingService = _kernel.Services.GetService<ITextEmbeddingGenerationService>();
        var vectorStore = _kernel.GetRequiredService<VectorStore>();
        var searchPlugin = new SearchPlugin(vectorStore, embeddingService);
        _kernel.Plugins.AddFromObject(searchPlugin);

        foreach (var pluginType in pluginTypes)
        {
            var pluginInstance = Activator.CreateInstance(pluginType);
            if (pluginInstance != null)
            {
                _kernel.Plugins.AddFromObject(pluginInstance);
            }
        }

        _kernel.FunctionInvocationFilters.Add(new FunctionInvocationFilter(functionInvocationList));

    }

    protected virtual void StartChat()
    {
        Console.WriteLine($"StartChat");

        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        _history = new ChatHistory();

        var systemMessage = "For every query:\n" +
                            "Always try to invoke Plugin to retrieve relevant answer.\n" +
                            "Do not add OK." +
                            "Keep answers concise and grounded in the retrieved material.";

        //var systemMessage = "You are a RAG‐enabled assistant. For every query:\n" +
        //                    "1. Always try to invoke the “SearchPlugin” to retrieve relevant text chunks.\n" +
        //                    "2. Base your answer on those chunks whenever possible.\n" +
        //                    $"3.If no specific collection is defined, use the collection named {_collectionName}" +
        //                    "Do not add OK." +
        //                    "Keep answers concise and grounded in the retrieved material.";

        _history.AddSystemMessage(systemMessage);
    }

    public async Task<string> GetReplyAsync(string request)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"Me > {request}");
        Console.ResetColor();

        _history.AddUserMessage(request!);

        var streamingResponse =
            _chatCompletionService.GetStreamingChatMessageContentsAsync(
                _history,
                _executionSettings,
                _kernel);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Agent > ");
        Console.ResetColor();

        var fullResponse = "";
        try
        {
            fullResponse = await ProcessUserInputAsync(request!);
        }
        catch (HttpOperationException ex)
        {
            foreach (var function in functionInvocationList.ToList())
            {
                var response = _kernel.InvokePromptAsync(function, new(_executionSettings)).Result;
                fullResponse += response.ToString().Replace("\n", " ").Replace("\r", " ");

            }
            functionInvocationList.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(fullResponse);
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine();

        _history.AddMessage(AuthorRole.Assistant, fullResponse);
        return fullResponse;
    }

    async Task<string> ProcessUserInputAsync(string userInput)
    {
        functionInvocationList.Clear();
        _history.AddUserMessage(userInput!);

        var streamingResponse =
            _chatCompletionService.GetStreamingChatMessageContentsAsync(
                _history,
                _executionSettings,
                _kernel);

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
}