using System.Text.Json;
using System.Text.Json.Nodes;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Daab.Modules.Translation.Service;

/// <summary>Parses the model's JSON-mode output: a flat array of translated block strings.</summary>
internal static class ModelResponse
{
    public static Fin<IReadOnlyList<string>> ParseArray(string json)
    {
        try
        {
            if (JsonNode.Parse(json) is not JsonArray array)
            {
                return Error.New(502, "Model output was not a JSON array");
            }

            var items = new List<string>(array.Count);
            foreach (var element in array)
            {
                if (element is null)
                {
                    return Error.New(502, "Model output array contained a null element");
                }

                items.Add(element.GetValue<string>());
            }

            return FinSucc<IReadOnlyList<string>>(items);
        }
        catch (JsonException ex)
        {
            return Error.New(502, $"Model output was not valid JSON: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return Error.New(502, $"Model output array had a non-string element: {ex.Message}");
        }
    }
}
