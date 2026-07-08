namespace Daab.Modules.Translation.Memory;

/// <summary>Pure ranking maths for the fuzzy and semantic tiers.</summary>
internal static class Similarity
{
    public static double CosineSimilarity(float[] left, float[] right)
    {
        if (left.Length != right.Length)
        {
            throw new ArgumentException("Vectors must have equal length.");
        }

        double dot = 0;
        double leftNorm = 0;
        double rightNorm = 0;
        for (int i = 0; i < left.Length; i++)
        {
            dot += (double)left[i] * right[i];
            leftNorm += (double)left[i] * left[i];
            rightNorm += (double)right[i] * right[i];
        }

        return leftNorm == 0 || rightNorm == 0
            ? 0
            : dot / (Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm));
    }

    /// <summary>Edit-distance similarity in 0..1 (1 = identical). Fuzzy tier runs on tagged strings.</summary>
    public static double NormalizedLevenshtein(string a, string b)
    {
        if (a.Length == 0 && b.Length == 0)
        {
            return 1.0;
        }
        int max = Math.Max(a.Length, b.Length);
        return 1.0 - (double)Levenshtein(a, b) / max;
    }

    /// <summary>
    /// Maximal Marginal Relevance: greedily pick items that are relevant to the query yet
    /// dissimilar to those already picked, so K isn't filled with near-duplicates.
    /// </summary>
    public static IReadOnlyList<T> SelectMmr<T>(
        float[] query,
        IReadOnlyList<T> candidates,
        int k,
        double lambda,
        Func<T, float[]> vector
    )
    {
        k = Math.Min(k, candidates.Count);
        if (k <= 0)
        {
            return [];
        }

        var relevance = new double[candidates.Count];
        for (int i = 0; i < candidates.Count; i++)
        {
            relevance[i] = CosineSimilarity(query, vector(candidates[i]));
        }

        var selected = new List<int>(k);
        var remaining = new List<int>(candidates.Count);
        for (int i = 0; i < candidates.Count; i++)
        {
            remaining.Add(i);
        }

        while (selected.Count < k && remaining.Count > 0)
        {
            int best = remaining[0];
            double bestScore = double.NegativeInfinity;
            foreach (int i in remaining)
            {
                double penalty = 0;
                foreach (int j in selected)
                {
                    penalty = Math.Max(
                        penalty,
                        CosineSimilarity(vector(candidates[i]), vector(candidates[j]))
                    );
                }
                double score = lambda * relevance[i] - (1 - lambda) * penalty;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }

            selected.Add(best);
            remaining.Remove(best);
        }

        return selected.Select(i => candidates[i]).ToList();
    }

    static int Levenshtein(string a, string b)
    {
        if (a.Length == 0)
        {
            return b.Length;
        }
        if (b.Length == 0)
        {
            return a.Length;
        }

        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];
        for (int j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (int i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + cost
                );
            }
            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }
}
