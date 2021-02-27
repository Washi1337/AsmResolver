using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        public Dictionary<string, JsonElement> ConfigProperties
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the optional string value which specifies the Target Framework Moniker.
        /// </summary>
        [JsonPropertyName("tfm")]
        public string TargetFrameworkMoniker
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
        public RuntimeFramework Framework
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an optional array (added in .NET Core 3.0) that allows multiple frameworks to be specified.
        /// </summary>
        public List<RuntimeFramework> IncludedFrameworks
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
        public List<string> AdditionalProbingPaths
        {
            get;
            set;
        }

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
