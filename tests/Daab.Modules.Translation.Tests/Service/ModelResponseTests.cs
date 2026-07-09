using Daab.Modules.Translation.Service;

namespace Daab.Modules.Translation.Tests.Service;

public sealed class ModelResponseTests
{
    [Fact]
    public void ParseArray_ArrayOfStrings_ReturnsThem()
    {
        var result = ModelResponse.ParseArray("[\"a\", \"b\"]").ExpectSucc();

        Assert.Equal(["a", "b"], result);
    }

    [Fact]
    public void ParseArray_EmptyArray_ReturnsEmpty() =>
        Assert.Empty(ModelResponse.ParseArray("[]").ExpectSucc());

    [Fact]
    public void ParseArray_NotAnArray_Fails() =>
        Assert.Contains(
            "not a JSON array",
            ModelResponse.ParseArray("{\"a\":1}").ExpectFail().Message,
            StringComparison.Ordinal
        );

    [Fact]
    public void ParseArray_NonStringElement_Fails() =>
        Assert.Contains(
            "non-string",
            ModelResponse.ParseArray("[1, 2]").ExpectFail().Message,
            StringComparison.Ordinal
        );

    [Fact]
    public void ParseArray_NullElement_Fails() =>
        Assert.Contains(
            "null element",
            ModelResponse.ParseArray("[null]").ExpectFail().Message,
            StringComparison.Ordinal
        );

    [Fact]
    public void ParseArray_Malformed_Fails() =>
        Assert.Contains(
            "not valid JSON",
            ModelResponse.ParseArray("[\"a\"").ExpectFail().Message,
            StringComparison.Ordinal
        );
}
