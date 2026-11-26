using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#if NET5_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace AsmResolver.DotNet.Config.Json
{
    /// <summary>
    /// Provides settings created when building a project.
    /// </summary>
    public class RuntimeOptions
    {
        /// <summary>
        /// Creates a new empty runtime options instance.
        /// </summary>
        public RuntimeOptions()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RuntimeOptions"/> class.
        /// </summary>
        /// <param name="tfm">The target framework moniker.</param>
        /// <param name="framework">The framework implementation description.</param>
        public RuntimeOptions(string tfm, RuntimeFramework framework)
        {
            TargetFrameworkMoniker = tfm;
            Framework = framework;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RuntimeOptions"/> class.
        /// </summary>
        /// <param name="tfm">The target framework moniker.</param>
        /// <param name="runtimeName">The name of the runtime.</param>
        /// <param name="runtimeVersion">The version of the runtime.</param>
        public RuntimeOptions(string tfm, string runtimeName, string runtimeVersion)
        {
            TargetFrameworkMoniker = tfm;
            Framework = new RuntimeFramework(runtimeName, runtimeVersion);
        }

        /// <summary>
        /// Indicates configuration properties to configure the runtime and the framework
        /// </summary>
        /// <remarks>
        /// On legacy targets, JSON values are represented by their corresponding CLR types.
        /// Numbers that cannot be represented without losing precision or changing their original
        /// representation are stored as objects whose <see cref="object.ToString"/> returns the raw JSON value.
        /// </remarks>
#if NET5_0_OR_GREATER
        public Dictionary<string, JsonElement>? ConfigProperties
#else
        public Dictionary<string, object?>? ConfigProperties
#endif
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the optional string value which specifies the Target Framework Moniker.
        /// </summary>
        #if NET5_0_OR_GREATER
        [JsonPropertyName("tfm")]
        #endif
        public string? TargetFrameworkMoniker
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the shared framework to use when activating the application.
        /// </summary>
        /// <remarks>
        /// The presence of this section (or another framework in the new frameworks section) indicates
        /// that the application is a framework-dependent app.
        /// </remarks>
        public RuntimeFramework? Framework
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an array (added in .NET Core 3.0) of shared frameworks when activating the application.
        /// </summary>
        /// <remarks>
        /// The presence of this section (or another framework in the new "framework" section) indicates
        /// that the application is a framework-dependent app.
        /// </remarks>
        public List<RuntimeFramework?>? Frameworks
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an optional array (added in .NET Core 3.0) that allows multiple frameworks to be specified.
        /// </summary>
        public List<RuntimeFramework?>? IncludedFrameworks
        {
            get;
            set;
        }

        /// <summary>
        /// When false, the most compatible framework version previously found is used. When it is
        /// unspecified or true, the framework from either the same or a higher version that differs only
        /// by the patch field will be used.
        /// </summary>
        public bool? ApplyPatches
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that determines the roll-forward behavior.
        /// </summary>
        public int? RollForwardOnNoCandidateFx
        {
            get;
            set;
        }

        /// <summary>
        /// Optional property which specifies additional paths to consider when looking for dependencies.
        /// The value is either a single string, or an array of strings.
        /// </summary>
#if NET5_0_OR_GREATER
        [JsonConverter(typeof(ProbingPathsConverter))]
#endif
        public List<string?>? AdditionalProbingPaths
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets additional properties to include in the runtime configuration.
        /// </summary>
        /// <remarks>
        /// The dictionary must not contain keys matching known JSON property names.
        /// </remarks>
#if NET5_0_OR_GREATER
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditionalProperties
        {
            get;
            set;
        }
#else
        public IDictionary<string, object?>? AdditionalProperties
        {
            get;
            set;
        }
#endif

#if !NET5_0_OR_GREATER
        internal static RuntimeOptions FromJsonNode(JSONNode node)
        {
            var result = new RuntimeOptions();

            if (node.HasKey("tfm") && !node["tfm"].IsNull)
                result.TargetFrameworkMoniker = node["tfm"].Value;

            if (node.HasKey("framework") && !node["framework"].IsNull)
                result.Framework = RuntimeFramework.FromJsonNode(node["framework"]);

            if (node.HasKey("frameworks") && !node["frameworks"].IsNull)
            {
                result.Frameworks = new List<RuntimeFramework?>();
                foreach (var item in node["frameworks"].Values)
                    result.Frameworks.Add(item.IsNull ? null : RuntimeFramework.FromJsonNode(item));
            }

            if (node.HasKey("includedFrameworks") && !node["includedFrameworks"].IsNull)
            {
                result.IncludedFrameworks = new List<RuntimeFramework?>();
                foreach (var item in node["includedFrameworks"].Values)
                    result.IncludedFrameworks.Add(item.IsNull ? null : RuntimeFramework.FromJsonNode(item));
            }

            if (node.HasKey("applyPatches") && !node["applyPatches"].IsNull)
                result.ApplyPatches = node["applyPatches"].AsBool;

            if (node.HasKey("rollForwardOnNoCandidateFx") && !node["rollForwardOnNoCandidateFx"].IsNull)
                result.RollForwardOnNoCandidateFx = node["rollForwardOnNoCandidateFx"].AsInt;

            if (node.HasKey("additionalProbingPaths") && !node["additionalProbingPaths"].IsNull)
            {
                result.AdditionalProbingPaths = new List<string?>();
                var paths = node["additionalProbingPaths"];
                if (paths.IsString)
                {
                    result.AdditionalProbingPaths.Add(paths.Value);
                }
                else if (paths.IsArray)
                {
                    foreach (var item in paths.Values)
                    {
                        if (!item.IsString && !item.IsNull)
                            throw new FormatException("Expected a string or null in 'additionalProbingPaths'.");

                        result.AdditionalProbingPaths.Add(item.IsNull ? null : item.Value);
                    }
                }
                else
                {
                    throw new FormatException("Expected a string or an array of strings for 'additionalProbingPaths'.");
                }
            }

            if (node.HasKey("configProperties") && !node["configProperties"].IsNull)
            {
                result.ConfigProperties = new Dictionary<string, object?>();
                foreach (var item in node["configProperties"])
                    result.ConfigProperties[item.Key] = InterpretValue(item.Value);
            }

            result.AdditionalProperties = JsonExtensionData.Read(
                node,
                "tfm",
                "framework",
                "frameworks",
                "includedFrameworks",
                "applyPatches",
                "rollForwardOnNoCandidateFx",
                "additionalProbingPaths",
                "configProperties"
            );

            return result;
        }

        internal static object? InterpretValue(JSONNode node) => node.Tag switch
        {
            JSONNodeType.String => node.Value,
            JSONNodeType.Number => InterpretNumber(node),
            JSONNodeType.NullValue => null,
            JSONNodeType.Boolean => node.AsBool,
            JSONNodeType.Array => node.Linq.Select(item => InterpretValue(item.Value)).ToList(),
            JSONNodeType.Object => node.Linq.ToDictionary(item => item.Key, item => InterpretValue(item.Value)),
            _ => null
        };

        private static object InterpretNumber(JSONNode node)
        {
            string value = node.Value;
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue)
                && intValue.ToString(CultureInfo.InvariantCulture) == value)
                return intValue;

            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long longValue)
                && longValue.ToString(CultureInfo.InvariantCulture) == value)
                return longValue;

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong ulongValue)
                && ulongValue.ToString(CultureInfo.InvariantCulture) == value)
                return ulongValue;

            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal decimalValue)
                && decimalValue.ToString(CultureInfo.InvariantCulture) == value)
                return decimalValue;

            // Preserve the original JSON literal if none of the conversions round-trip.
            return node;
        }
#endif

        /// <summary>
        /// Gets a collection of all frameworks specified in the configuration.
        /// </summary>
        public IEnumerable<RuntimeFramework> GetAllFrameworks()
        {
            if (Framework is { } framework)
                yield return framework;

            if (Frameworks is { } frameworks)
            {
                foreach (var includedFramework in frameworks)
                {
                    if (includedFramework is not null)
                        yield return includedFramework;
                }
            }

            if (IncludedFrameworks is { } includedFrameworks)
            {
                foreach (var includedFramework in includedFrameworks)
                {
                    if (includedFramework is not null)
                        yield return includedFramework;
                }
            }
        }
    }
}
