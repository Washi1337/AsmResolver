#if !NET5_0_OR_GREATER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace AsmResolver.DotNet.Config.Json;

internal static class RuntimeConfigurationJsonSerializer
{
    private const int MaximumDepth = 64;

    public static string Serialize(RuntimeConfiguration configuration)
    {
        var root = SerializeProperties(configuration.AdditionalProperties);
        if (configuration.RuntimeOptions is { } options)
            root["runtimeOptions"] = SerializeOptions(options);

        return root.ToString(2);
    }

    private static JSONObject SerializeOptions(RuntimeOptions options)
    {
        var result = SerializeProperties(options.AdditionalProperties, 1);

        if (options.TargetFrameworkMoniker is { } moniker)
            result["tfm"] = moniker;

        if (options.Framework is { } framework)
            result["framework"] = SerializeFramework(framework);

        if (options.Frameworks is { } frameworks)
        {
            var array = new JSONArray();

            for (int i = 0; i < frameworks.Count; i++)
                array.Add(SerializeFramework(frameworks[i], 3));

            result["frameworks"] = array;
        }

        if (options.IncludedFrameworks is { } includedFrameworks)
        {
            var array = new JSONArray();

            for (int i = 0; i < includedFrameworks.Count; i++)
                array.Add(SerializeFramework(includedFrameworks[i], 3));

            result["includedFrameworks"] = array;
        }

        if (options.ApplyPatches is { } applyPatches)
            result["applyPatches"] = applyPatches;

        if (options.RollForwardOnNoCandidateFx is { } rollForward)
            result["rollForwardOnNoCandidateFx"] = rollForward;

        if (options.AdditionalProbingPaths is { } paths)
        {
            var array = new JSONArray();

            for (int i = 0; i < paths.Count; i++)
                array.Add(paths[i]);

            result["additionalProbingPaths"] = array;
        }

        if (options.ConfigProperties is { } properties)
            result["configProperties"] = SerializeProperties(properties, 2);

        return result;
    }

    private static JSONNode SerializeFramework(RuntimeFramework? framework, int depth = 2)
    {
        if (framework is null)
            return JSONNull.CreateOrGet();

        var result = SerializeProperties(framework.AdditionalProperties, depth);
        if (framework.Name is { } name)
            result["name"] = name;

        if (framework.Version is { } version)
            result["version"] = version;

        return result;
    }

    private static JSONNode SerializeProperty(object? value, int depth)
    {
        return value switch
        {
            null => JSONNull.CreateOrGet(),
            JSONNode node => node,
            string text => new JSONString(text),
            bool boolean => new JSONBool(boolean),
            double number when double.IsNaN(number) || double.IsInfinity(number) =>
                throw new ArgumentException("Cannot serialize NaN or infinity as a JSON number."),
            float number when float.IsNaN(number) || float.IsInfinity(number) =>
                throw new ArgumentException("Cannot serialize NaN or infinity as a JSON number."),
            double number => new JSONNumber(number.ToString("G17", CultureInfo.InvariantCulture)),
            float number => new JSONNumber(number.ToString("G9", CultureInfo.InvariantCulture)),
            sbyte or byte or short or ushort or int or uint or long or ulong or decimal =>
                new JSONNumber(((IFormattable) value).ToString(null, CultureInfo.InvariantCulture)),
            IDictionary<string, object?> properties => SerializeProperties(properties, depth),
            IDictionary properties => SerializeProperties(GetDictionaryEntries(properties), depth),
            IEnumerable values => SerializeArray(values, depth),
            _ => throw new NotSupportedException(
                $"Cannot serialize configuration property value of type '{value.GetType()}'.")
        };
    }

    private static IEnumerable<KeyValuePair<string, object?>> GetDictionaryEntries(IDictionary properties)
    {
        foreach (object? key in properties.Keys)
        {
            if (key is not string name)
                throw new NotSupportedException("Configuration property dictionaries must have string keys.");

            yield return new KeyValuePair<string, object?>(name, properties[key]);
        }
    }

    private static JSONObject SerializeProperties(
        IEnumerable<KeyValuePair<string, object?>>? properties,
        int depth = 0)
    {
        if (depth >= MaximumDepth)
        {
            throw new InvalidOperationException(
                $"Cannot serialize configuration properties nested more than {MaximumDepth} levels deep.");
        }

        var result = new JSONObject();
        if (properties is null)
            return result;

        foreach (var property in properties)
            result[property.Key] = SerializeProperty(property.Value, depth + 1);

        return result;
    }

    private static JSONArray SerializeArray(IEnumerable values, int depth)
    {
        if (depth >= MaximumDepth)
        {
            throw new InvalidOperationException(
                $"Cannot serialize configuration properties nested more than {MaximumDepth} levels deep.");
        }

        var result = new JSONArray();

        foreach (object? value in values)
            result.Add(SerializeProperty(value, depth + 1));

        return result;
    }
}

#endif
