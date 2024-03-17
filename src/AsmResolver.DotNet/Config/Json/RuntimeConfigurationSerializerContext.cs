#if NET5_0_OR_GREATER

using System.Text.Json.Serialization;

namespace AsmResolver.DotNet.Config.Json
{
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(RuntimeConfiguration))]
    internal partial class RuntimeConfigurationSerializerContext : JsonSerializerContext
    {
    }
}

#endif
