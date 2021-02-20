using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AsmResolver.DotNet.Config.Json
{
    /// <summary>
    /// Provides settings created when building a project.
    /// </summary>
    public class RuntimeOptions
    {
        /// <summary>
        /// Indicates configuration properties to configure the runtime and the framework
        /// </summary>
        public IDictionary<string, object> ConfigProperties
        {
            get;
        } = new Dictionary<string, object>();

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
        public IList<RuntimeFramework> Frameworks
        {
            get;
        } = new List<RuntimeFramework>();

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
        public int RollForwardOnNoCandidateFx
        {
            get;
            set;
        }

        /// <summary>
        /// Optional property which specifies additional paths to consider when looking for dependencies.
        /// The value is either a single string, or an array of strings.
        /// </summary>
        public IList<string> AdditionalProbingPaths
        {
            get;
        } = new List<string>();
    }
}
