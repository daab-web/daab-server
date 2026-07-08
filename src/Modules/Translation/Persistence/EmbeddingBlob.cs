using System.Runtime.InteropServices;

namespace Daab.Modules.Translation.Persistence;

/// <summary>Packs an embedding vector into a compact SQLite BLOB (native-endian) and back.</summary>
internal static class EmbeddingBlob
{
    public static byte[] Encode(float[] vector) => MemoryMarshal.AsBytes(vector.AsSpan()).ToArray();

    public static float[] Decode(byte[] bytes)
    {
        if (bytes.Length % sizeof(float) != 0)
        {
            throw new ArgumentException(
                $"Embedding blob length {bytes.Length} is not a multiple of {sizeof(float)}.",
                nameof(bytes)
            );
        }
        return MemoryMarshal.Cast<byte, float>(bytes.AsSpan()).ToArray();
    }
}
