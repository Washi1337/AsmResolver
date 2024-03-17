using System;
using System.Collections.Generic;

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
        /// Gets an optional array (added in .NET Core 3.0) that allows multiple frameworks to be specified.
        /// </summary>
        public List<RuntimeFramework>? IncludedFrameworks
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
        public List<string>? AdditionalProbingPaths
        {
            get;
            set;
        }

#if !NET5_0_OR_GREATER
        internal static RuntimeOptions FromJsonNode(JSONNode node)
        {
            var result = new RuntimeOptions();

            if (node.HasKey("tfm"))
                result.TargetFrameworkMoniker = node["tfm"].Value;

            if (node.HasKey("framework"))
                result.Framework = RuntimeFramework.FromJsonNode(node["framework"]);

            if (node.HasKey("includedFrameworks"))
            {
                result.IncludedFrameworks = new List<RuntimeFramework>();
                foreach (var item in node["includedFrameworks"].Values)
                    result.IncludedFrameworks.Add(RuntimeFramework.FromJsonNode(item));
            }

            if (node.HasKey("applyPatches"))
                result.ApplyPatches = node["ApplyPatches"].AsBool;

            if (node.HasKey("rollForwardOnNoCandidateFx"))
                result.RollForwardOnNoCandidateFx = node["rollForwardOnNoCandidateFx"].AsInt;

            if (node.HasKey("additionalProbingPaths"))
            {
                result.AdditionalProbingPaths = new List<string>();
                foreach (var item in node["additionalProbingPaths"].Values)
                    result.AdditionalProbingPaths.Add(item.Value);
            }

            if (node.HasKey("configProperties"))
            {
                result.ConfigProperties = new Dictionary<string, object?>();
                foreach (var item in node["configProperties"])
                    result.ConfigProperties[item.Key] = InterpretValue(item.Value);
            }

            return result;
        }

        private static object? InterpretValue(JSONNode node) => node.Tag switch
        {
            JSONNodeType.String => node.Value,
            JSONNodeType.Number => node.AsInt,
            JSONNodeType.NullValue => null,
            JSONNodeType.Boolean => node.AsBool,
            _ => null
        };
#endif

        /// <summary>
        /// Gets a collection of all frameworks specified in the configuration.
        /// </summary>
        public IEnumerable<RuntimeFramework> GetAllFrameworks()
        {
            if (Framework is { } framework)
                yield return framework;

            if (IncludedFrameworks is { } frameworks)
            {
                foreach (var includedFramework in frameworks)
                    yield return includedFramework;
            }
        }
    }
}
