using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Config.Json;

internal static class JsonExtensionData
{
#if !NET5_0_OR_GREATER
    public static Dictionary<string, object?>? Read(JSONNode node, params string[] knownProperties)
    {
        var result = new Dictionary<string, object?>();

        foreach (var item in node)
        {
            if (Array.IndexOf(knownProperties, item.Key) >= 0)
                continue;

            result.Add(item.Key, RuntimeOptions.InterpretValue(item.Value));
        }

        return result.Count > 0 ? result : null;
    }
#endif

    /// <summary>
    /// Verifies that no extension properties conflict with known properties.
    /// </summary>
    public static void Validate<T>(IDictionary<string, T>? properties, params string[] knownProperties)
    {
        if (properties is null)
            return;

        foreach (string name in properties.Keys)
        {
            if (Array.IndexOf(knownProperties, name) >= 0)
            {
                throw new ArgumentException(
                    $"'{name}' is a known property and cannot be used as an extension property.");
            }
        }
    }
}
