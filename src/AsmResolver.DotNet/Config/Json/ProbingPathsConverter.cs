#if NET5_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsmResolver.DotNet.Config.Json;

// The .NET 6 JSON source generator omits nullable element annotations when constructing converters.
#nullable disable annotations
internal sealed class ProbingPathsConverter : JsonConverter<List<string>>
#nullable restore annotations
{
    /// <inheritdoc />
    public override List<string?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new List<string?>
            {
                reader.GetString()
            };
        }

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected a string or an array of strings for 'additionalProbingPaths'.");

        var result = new List<string?>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return result;

            if (reader.TokenType is not (JsonTokenType.String or JsonTokenType.Null))
                throw new JsonException("Expected a string or null in 'additionalProbingPaths'.");

            result.Add(reader.GetString());
        }

        throw new JsonException("Unterminated probing paths array.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, List<string?> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        for (int i = 0; i < value.Count; i++)
            writer.WriteStringValue(value[i]);

        writer.WriteEndArray();
    }
}

#endif
