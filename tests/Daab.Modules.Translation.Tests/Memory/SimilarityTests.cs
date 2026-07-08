using Daab.Modules.Translation.Memory;

namespace Daab.Modules.Translation.Tests.Memory;

public sealed class SimilarityTests
{
    [Fact]
    public void Cosine_IdenticalVectors_IsOne() =>
        Assert.Equal(1.0, Similarity.CosineSimilarity([1f, 2f, 3f], [1f, 2f, 3f]), 6);

    [Fact]
    public void Cosine_OrthogonalVectors_IsZero() =>
        Assert.Equal(0.0, Similarity.CosineSimilarity([1f, 0f], [0f, 1f]), 6);

    [Fact]
    public void Cosine_ZeroVector_IsZero() =>
        Assert.Equal(0.0, Similarity.CosineSimilarity([0f, 0f], [1f, 1f]));

    [Fact]
    public void Cosine_MismatchedLengths_Throws() =>
        Assert.Throws<ArgumentException>(() => Similarity.CosineSimilarity([1f], [1f, 2f]));

    [Fact]
    public void Levenshtein_Identical_IsOne() =>
        Assert.Equal(1.0, Similarity.NormalizedLevenshtein("abc", "abc"));

    [Fact]
    public void Levenshtein_BothEmpty_IsOne() =>
        Assert.Equal(1.0, Similarity.NormalizedLevenshtein("", ""));

    [Fact]
    public void Levenshtein_SmallEdit_StaysAboveThreshold() =>
        Assert.True(
            Similarity.NormalizedLevenshtein(
                "The quick brown fox jumps",
                "The quick brown fox jumped"
            ) >= 0.8
        );

    [Fact]
    public void Levenshtein_LargeEdit_FallsBelowThreshold() =>
        Assert.True(Similarity.NormalizedLevenshtein("alpha", "completely different") < 0.8);

    [Fact]
    public void Mmr_AvoidsNearDuplicates()
    {
        float[][] candidates =
        [
            [1f, 0f, 0f],
            [1f, 0f, 0f],
            [0f, 1f, 0f],
        ];
        var selected = Similarity.SelectMmr([1f, 0f, 0f], candidates, 2, 0.3, v => v);

        Assert.Same(candidates[0], selected[0]);
        Assert.Same(candidates[2], selected[1]);
    }

    [Fact]
    public void Mmr_FewerCandidatesThanK_ReturnsAll()
    {
        float[][] candidates =
        [
            [1f, 0f],
        ];
        Assert.Single(Similarity.SelectMmr([1f, 0f], candidates, 5, 0.7, v => v));
    }

    [Fact]
    public void Mmr_NonPositiveK_ReturnsEmpty()
    {
        float[][] candidates =
        [
            [1f, 0f],
        ];
        Assert.Empty(Similarity.SelectMmr([1f, 0f], candidates, 0, 0.7, v => v));
    }
}
