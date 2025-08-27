using Microsoft.SemanticKernel.Text;
using System.Text;

namespace Assistance.DataEmbedding;

public class DataReader
{
    public static List<string> SplitIntoChunks(string content, int maxTokens = 1000)
    {
        var chunks = new List<string>();
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new StringBuilder();
        var currentTokens = 0;

        foreach (var word in words)
        {
            // Rough token estimation (1 word ≈ 1.3 tokens)
            var wordTokens = (int)Math.Ceiling(word.Length / 4.0);

            if (currentTokens + wordTokens > maxTokens && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
                currentTokens = 0;
            }

            currentChunk.Append(word + " ");
            currentTokens += wordTokens;
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks;
    }

    public static double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must have the same length");

        var dotProduct = 0.0;
        var magnitudeA = 0.0;
        var magnitudeB = 0.0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        magnitudeA = Math.Sqrt(magnitudeA);
        magnitudeB = Math.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0;

        return dotProduct / (magnitudeA * magnitudeB);
    }
}