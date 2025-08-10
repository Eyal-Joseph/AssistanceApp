using Microsoft.SemanticKernel.Embeddings;

namespace Assistance.DataEmbedding;


public class VectorStoreService
{
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly Dictionary<string, VectorRecord> _vectorStore;

    public VectorStoreService(ITextEmbeddingGenerationService embeddingService)
    {
        _embeddingService = embeddingService;
        _vectorStore = new Dictionary<string, VectorRecord>();
    }

    /// <summary>
    /// Upload text documents to vector store with embeddings
    /// </summary>
    /// <param name="documents">List of documents to upload</param>
    /// <param name="collectionName">Name of the collection</param>
    /// <returns>Number of documents uploaded</returns>
    public async Task<int> UploadToVectorStore(
        List<TextDocument> documents,
        string collectionName = "default")
    {
        var uploadedCount = 0;

        foreach (var doc in documents)
        {
            try
            {
                // Split document into chunks if it's too large
                var chunks = DataReader.SplitIntoChunks(doc.Content, maxTokens: 1000);

                foreach (var (chunk, index) in chunks.Select((c, i) => (c, i)))
                {
                    // Generate embedding for the chunk
                    var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk);

                    // Create unique ID for the chunk
                    var chunkId = $"{doc.Id}_{index}";

                    // Create vector record
                    var vectorRecord = new VectorRecord
                    {
                        Id = chunkId,
                        Content = chunk,
                        Embedding = embedding.ToArray(),
                        Metadata = new Dictionary<string, object>
                        {
                            ["collection"] = collectionName,
                            ["sourceDocument"] = doc.Id,
                            ["title"] = doc.Title,
                            ["chunkIndex"] = index,
                            ["uploadedAt"] = DateTime.UtcNow,
                            ["contentLength"] = chunk.Length
                        }
                    };

                    // Store in vector store
                    _vectorStore[chunkId] = vectorRecord;
                    uploadedCount++;
                }

                Console.WriteLine($"✓ Uploaded document: {doc.Title} ({chunks.Count} chunks)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to upload document {doc.Title}: {ex.Message}");
            }
        }

        Console.WriteLine($"📊 Total uploaded: {uploadedCount} chunks from {documents.Count} documents");
        return uploadedCount;
    }

    /// <summary>
    /// Upload text files from a directory
    /// </summary>
    /// <param name="directoryPath">Path to directory containing text files</param>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="fileExtensions">File extensions to include (default: .txt, .md)</param>
    /// <returns>Number of documents uploaded</returns>
    public async Task<int> UploadFromDirectory(
        string directoryPath,
        string collectionName = "default",
        string[] fileExtensions = null)
    {
        fileExtensions ??= new[] { ".txt", ".md", ".json" };

        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
            .Where(f => fileExtensions.Contains(Path.GetExtension(f).ToLower()))
            .ToList();

        Console.WriteLine($"📁 Found {files.Count} files to process");

        var documents = new List<TextDocument>();

        foreach (var filePath in files)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);

                documents.Add(new TextDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = fileName,
                    Content = content,
                    Source = filePath
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to read file {filePath}: {ex.Message}");
            }
        }

        return await UploadToVectorStore(documents, collectionName);
    }

    /// <summary>
    /// Upload text files from a directory
    /// </summary>
    /// <param name="filePath">Path of the file</param>
    /// <param name="collectionName">Name of the collection</param>
    /// <returns>Number of documents uploaded</returns>
    public async Task<int> UploadFile(
        string filePath,
        string collectionName = "default")
    {
        var documents = new List<TextDocument>();

        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            documents.Add(new TextDocument
            {
                Id = Guid.NewGuid().ToString(),
                Title = fileName,
                Content = content,
                Source = filePath
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to read file {filePath}: {ex.Message}");
        }

        return await UploadToVectorStore(documents, collectionName);
    }

    /// <summary>
    /// Search for similar content in the vector store
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="collectionName">Collection to search in</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="minSimilarity">Minimum similarity threshold (0-1)</param>
    /// <returns>List of similar documents</returns>
    public async Task<List<SearchResult>> SearchSimilar(
        string query,
        string collectionName = "default",
        int limit = 5,
        double minSimilarity = 0.5)
    {
        // Generate embedding for the query
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);
        var queryVector = queryEmbedding.ToArray();

        // Calculate similarity with all vectors in the collection
        var similarities = new List<SearchResult>();

        foreach (var record in _vectorStore.Values)
        {
            // Filter by collection if specified
            if (collectionName != "default" &&
                record.Metadata.TryGetValue("collection", out var collection) &&
                collection.ToString() != collectionName)
                continue;

            // Calculate cosine similarity
            var similarity = DataReader.CalculateCosineSimilarity(queryVector, record.Embedding);

            if (similarity >= minSimilarity)
            {
                similarities.Add(new SearchResult
                {
                    Id = record.Id,
                    Content = record.Content,
                    Similarity = similarity,
                    Metadata = record.Metadata
                });
            }
        }

        // Sort by similarity (descending) and take top results
        return similarities
            .OrderByDescending(s => s.Similarity)
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Get vector store statistics
    /// </summary>
    /// <returns>Statistics about the vector store</returns>
    public VectorStoreStats GetStats()
    {
        var collections = _vectorStore.Values
            .GroupBy(v => v.Metadata.GetValueOrDefault("collection", "default"))
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        return new VectorStoreStats
        {
            TotalVectors = _vectorStore.Count,
            CollectionCounts = collections,
            TotalSizeBytes = _vectorStore.Values.Sum(v => v.Content.Length + v.Embedding.Length * sizeof(float))
        };
    }

    
}


