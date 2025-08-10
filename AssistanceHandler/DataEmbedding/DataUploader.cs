using Assistance.Models;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;

namespace Assistance.DataEmbedding;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class DataUploader(VectorStore vectorStore, ITextEmbeddingGenerationService textEmbeddingGenerator)
{
    public async Task UploadToVectorStore(string collectionName, IEnumerable<TextChunk> textChunk)
    {
        var collection = vectorStore.GetCollection<string, TextChunk>(collectionName);
        await collection.EnsureCollectionExistsAsync();

        foreach (var chunk in textChunk)
        {
            Console.WriteLine($"Generating embedding for paragraph: {chunk.ParagraphId}");
            var embedding = await textEmbeddingGenerator.GenerateEmbeddingAsync(chunk.Text);
            chunk.TextEmbedding = embedding;

            Console.WriteLine($"Upserting chunk to vector store: {chunk.Key}");
            await collection.UpsertAsync(chunk);
        }
    }
}

#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.