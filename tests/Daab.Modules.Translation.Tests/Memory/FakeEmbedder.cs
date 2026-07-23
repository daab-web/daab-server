using Daab.Modules.Translation.Memory;

namespace Daab.Modules.Translation.Tests.Memory;

/// <summary>
/// Deterministic stand-in for the real Gemini embedder. Returns seeded vectors for known inputs
/// (so cosine ordering is fully controlled) and a char-histogram otherwise. Counts calls so tests
/// can prove the exact/fuzzy tiers short-circuit without embedding.
/// </summary>
internal sealed class FakeEmbedder(
    IReadOnlyDictionary<string, float[]>? seeded = null,
    int dimension = 32
) : IEmbedder
{
    public int CallCount { get; private set; }

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken)
    {
        CallCount++;
        if (seeded is not null && seeded.TryGetValue(text, out var vector))
        {
            return Task.FromResult(vector);
        }

        var histogram = new float[dimension];
        foreach (char c in text)
        {
            histogram[c % dimension] += 1f;
        }
        var norm = MathF.Sqrt(histogram.Sum(x => x * x));
        if (norm > 0)
        {
            for (int i = 0; i < dimension; i++)
            {
                histogram[i] /= norm;
            }
        }
        return Task.FromResult(histogram);
    }
}
