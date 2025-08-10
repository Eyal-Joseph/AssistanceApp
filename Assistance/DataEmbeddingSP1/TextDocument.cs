namespace Assistance.DataEmbedding;

public class TextDocument
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Source { get; set; }
}

public class VectorRecord
{
    public string Id { get; set; }
    public string Content { get; set; }
    public float[] Embedding { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

public class SearchResult
{
    public string Id { get; set; }
    public string Content { get; set; }
    public double Similarity { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

public class VectorStoreStats
{
    public int TotalVectors { get; set; }
    public Dictionary<string, int> CollectionCounts { get; set; }
    public long TotalSizeBytes { get; set; }
}