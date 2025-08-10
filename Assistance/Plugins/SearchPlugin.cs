using Assistance.Models;
using Microsoft.Extensions.VectorData;
using Microsoft.Identity.Client;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Text;
using System.Collections;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;

namespace Assistance.Plugins;

public class SearchPlugin
{
    public ITextEmbeddingGenerationService _embeddingService;
    public VectorStore _vectorStore;
    public List<string> collectionNamesList = new();

    string _collectionName;
    private readonly Kernel _kernel;


    public SearchPlugin(Kernel kernel, VectorStore vectorStore,
        ITextEmbeddingGenerationService embeddingService)
    {
        this._embeddingService = embeddingService;
        this._vectorStore = vectorStore;
        _kernel = kernel;

        var listCollectionNames = this._vectorStore.ListCollectionNamesAsync();

        foreach (var collectionName in listCollectionNames.ToBlockingEnumerable())
        {
            collectionNamesList.Add(collectionName);
        }
    }

    [KernelFunction]
    [Description("Get all Collections names")]
    string GetAllCollectionsNames()
    {
        // concatenate the collection names into a single string
        if (collectionNamesList.Count == 0)
        {
            return "No collections found in vector store";
        }
        return $"Available collections: {string.Join(", ", collectionNamesList)}";
    }

    [KernelFunction]
    [Description("Function to set the Collection to search in")]
    string SetCollectionName(
        [Description("Collection to search in")]
        string collectionName)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            return "Collection Name is required";
        }
        _collectionName = collectionName;
        return $"Collection Name set to {collectionName}";
    }

    [KernelFunction]
    [Description("Search for data in vector store")]
    public async Task<string> SearchInVectorStore(
        [Description("The search query")] string query,
        [Description("Maximum number of results to return")] int maxResults = 5)
    {

        if (string.IsNullOrEmpty(_collectionName))
        {
            //Console.Write("Please enter the Collection to search in:");
            //var userInput = Console.ReadLine();

            //var result = await _kernel.InvokePromptAsync($"Get only the Collection name from this input: {userInput}");
            //_collectionName = result.ToString().Replace("\n", "").Replace("\r", "");
            return "Collection name  is not set";

        }

        if (!collectionNamesList.Contains(_collectionName))
        {
            return "Collection name  is not set";
        }
        
        var collection = _vectorStore.GetCollection<string, TextChunk>(_collectionName);

        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

        var searchResults = collection.SearchAsync(queryEmbedding, maxResults);

        var resultList = new List<VectorSearchResult<TextChunk>>();
        await foreach (var result in searchResults)
        {
            resultList.Add(result);
        }

        if (!resultList.Any())
        {
            return "No relevant information found for your query in vector store";
        }

        var builder = new StringBuilder();
        builder.AppendLine("### Relevant information about your query:");
        builder.AppendLine();

        foreach (var result in resultList)
        {
            builder.AppendLine($"**File: {result.Record.DocumentName}, Paragraph: {result.Record.ParagraphId}, Relevancy: {result.Score}**");

            builder.AppendLine(result.Record.Text);
            builder.AppendLine();
        }

        return builder.ToString();
    }

    [KernelFunction]
    [Description("Search for data in vector store With Specific Collection name")]
    public async Task<string> SearchInVectorStoreWithSpecificCollection(
        [Description("The search query")] string query,
        [Description("Collection to search in")] string collectionName,
        [Description("Maximum number of results to return")] int maxResults = 5)
    {
        var collection = _vectorStore.GetCollection<string, TextChunk>(collectionName);

        if (!collectionNamesList.Contains(collectionName))
        {
            return $"Collection '{collectionName}' does not exist in vector store. Available collections: {string.Join(", ", collectionNamesList)}";
        }

        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

        var searchResults = collection.SearchAsync(queryEmbedding, maxResults);

        var resultList = new List<VectorSearchResult<TextChunk>>();
        await foreach (var result in searchResults)
        {
            resultList.Add(result);
        }

        if (!resultList.Any())
        {
            return "No relevant information found for your query in vector store";
        }

        var builder = new StringBuilder();
        builder.AppendLine("### Relevant information about your query:");
        builder.AppendLine();

        foreach (var result in resultList)
        {
            builder.AppendLine($"**File: {result.Record.DocumentName}, Paragraph: {result.Record.ParagraphId}, Relevancy: {result.Score}**");

            builder.AppendLine(result.Record.Text);
            builder.AppendLine();
        }

        return builder.ToString();
    }
}