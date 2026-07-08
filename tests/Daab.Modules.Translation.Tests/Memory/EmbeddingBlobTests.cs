using Daab.Modules.Translation.Persistence;

namespace Daab.Modules.Translation.Tests.Memory;

public sealed class EmbeddingBlobTests
{
    [Fact]
    public void EncodeDecode_RoundTripsVector()
    {
        float[] vector = [1.5f, -2.25f, 0f, 3.0f];
        Assert.Equal(vector, EmbeddingBlob.Decode(EmbeddingBlob.Encode(vector)));
    }

    [Fact]
    public void Decode_LengthNotMultipleOfFour_Throws() =>
        Assert.Throws<ArgumentException>(() => EmbeddingBlob.Decode(new byte[3]));
}
