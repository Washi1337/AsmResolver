#if !NET5_0_OR_GREATER

using System;
using System.Globalization;
using System.Text;

namespace AsmResolver.DotNet.Config.Json;

internal static class RuntimeConfigurationJsonSerializer
{
    public static string Serialize(RuntimeConfiguration configuration)
    {
        var root = new JSONObject();
        if (configuration.RuntimeOptions is { } options)
            root["runtimeOptions"] = SerializeOptions(options);

        return root.ToString(2);
    }

    private static JSONObject SerializeOptions(RuntimeOptions options)
    {
        var result = new JSONObject();

        if (options.TargetFrameworkMoniker is { } moniker)
            result["tfm"] = moniker;

        if (options.Framework is { } framework)
            result["framework"] = SerializeFramework(framework);

        if (options.Frameworks is { } frameworks)
        {
            var array = new JSONArray();

            for (int i = 0; i < frameworks.Count; i++)
                array.Add(SerializeFramework(frameworks[i]));

            result["frameworks"] = array;
        }

        if (options.IncludedFrameworks is { } includedFrameworks)
        {
            var array = new JSONArray();

            for (int i = 0; i < includedFrameworks.Count; i++)
                array.Add(SerializeFramework(includedFrameworks[i]));

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
        {
            var node = new JSONObject();

            foreach (var property in properties)
                node[property.Key] = SerializeProperty(property.Value);

            result["configProperties"] = node;
        }

        return result;
    }

    private static JSONNode SerializeFramework(RuntimeFramework? framework)
    {
        if (framework is null)
            return JSONNull.CreateOrGet();

        var result = new JSONObject();
        if (framework.Name is { } name)
            result["name"] = name;

        if (framework.Version is { } version)
            result["version"] = version;

        return result;
    }

    private static JSONNode SerializeProperty(object? value)
    {
        return value switch
        {
            null => JSONNull.CreateOrGet(),
            string text => new JSONString(text),
            bool boolean => new JSONBool(boolean),
            double number when double.IsNaN(number) || double.IsInfinity(number) =>
                throw new ArgumentException("Cannot serialize NaN or infinity as a JSON number."),
            float number when float.IsNaN(number) || float.IsInfinity(number) =>
                throw new ArgumentException("Cannot serialize NaN or infinity as a JSON number."),
            sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal =>
                new ConfigurationNumber((IFormattable) value),
            _ => throw new NotSupportedException(
                $"Cannot serialize configuration property value of type '{value.GetType()}'.")
        };
    }

    private sealed class ConfigurationNumber : JSONNumber
    {
        private readonly string _value;

        internal ConfigurationNumber(IFormattable value)
            : base(0)
        {
            // SimpleJSON stores numbers as doubles. Preserve the original numeric representation when writing.
            string? format = value is double
                ? "G17"
                : value is float ? "G9" : null;
            _value = value.ToString(format, CultureInfo.InvariantCulture);
        }

        internal override void WriteToStringBuilder(
            StringBuilder builder,
            int indent,
            int indentIncrement,
            JSONTextMode mode)
        {
            builder.Append(_value);
        }
    }
}

#endif
