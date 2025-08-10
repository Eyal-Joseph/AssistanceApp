using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Embeddings;

namespace AssistanceHandler
{
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public class GoogleAssistanceHandler : AssistanceHandlerBase
    {
        protected override void CreateBuilder()
        {

            var modelName = Configuration["GeminiModelName"] ?? throw new ApplicationException("ModelName not found");
            var embedding = Configuration["GeminiEmbeddingModel"] ??
                            throw new ApplicationException("EmbeddingModel not found");
            var apiKey = Configuration["GeminiApiKey"] ?? throw new ApplicationException("ApiKey not found");

            var httpClient = new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            });


            // Create kernel with Google AI connector
            Builder = Kernel.CreateBuilder()
                .AddGoogleAIGeminiChatCompletion(modelName, apiKey, httpClient: httpClient)
                .AddGoogleAIEmbeddingGenerator(embedding, apiKey, httpClient: httpClient);

            Builder.Services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
            {
                return new GoogleAITextEmbeddingGenerationService(embedding, apiKey, httpClient: httpClient);
            });

            base.CreateBuilder();
        }
      

        protected override void StartChat()
        {
            _executionSettings = new GeminiPromptExecutionSettings()
            {
                ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions // Enable function calling
            };
            base.StartChat();
        }

        //public async Task<string> GetReplyAsync(string request)
        //{
        //    Console.ForegroundColor = ConsoleColor.Cyan;
        //    Console.Write($"Me > {request}");
        //    Console.ResetColor();

        //    _history.AddUserMessage(request!);

        //    var streamingResponse =
        //        _chatCompletionService.GetStreamingChatMessageContentsAsync(
        //            _history,
        //            _executionSettings,
        //            _kernel);

        //    Console.ForegroundColor = ConsoleColor.Green;
        //    Console.Write("Agent > ");
        //    Console.ResetColor();

        //    var fullResponse = "";
        //    try
        //    {
        //        fullResponse = await ProcessUserInputAsync(request!);
        //    }
        //    catch (HttpOperationException ex)
        //    {
        //        foreach (var function in functionInvocationList.ToList())
        //        {
        //            var response = _kernel.InvokePromptAsync(function, new(_executionSettings)).Result;
        //            fullResponse += response.ToString().Replace("\n", " ").Replace("\r", " ");

        //        }
        //        functionInvocationList.Clear();
        //        Console.ForegroundColor = ConsoleColor.Yellow;
        //        Console.Write(fullResponse);
        //        Console.ResetColor();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Unexpected error: {ex.Message}");
        //        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        //    }

        //    Console.WriteLine();

        //    _history.AddMessage(AuthorRole.Assistant, fullResponse);
        //    return fullResponse;
        //}

        //async Task<string> ProcessUserInputAsync(string userInput)
        //{
        //    functionInvocationList.Clear();
        //    _history.AddUserMessage(userInput!);

        //    var streamingResponse =
        //        _chatCompletionService.GetStreamingChatMessageContentsAsync(
        //            _history,
        //            _executionSettings,
        //            _kernel);

        //    Console.ForegroundColor = ConsoleColor.Green;
        //    Console.Write("Agent > ");
        //    Console.ResetColor();

        //    var fullResponse = "";

        //    await foreach (var chunk in streamingResponse)
        //    {
        //        Console.ForegroundColor = ConsoleColor.Yellow;
        //        Console.Write(chunk.Content);
        //        Console.ResetColor();
        //        fullResponse += chunk.Content;
        //    }
        //    return fullResponse;
        //}
    }
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

}
