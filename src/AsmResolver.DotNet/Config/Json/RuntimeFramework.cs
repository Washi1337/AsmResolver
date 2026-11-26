using System.Collections.Generic;

#if NET5_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace AsmResolver.DotNet.Config.Json
{
    /// <summary>
    /// Provides version information about a framework implementation.
    /// </summary>
    public class RuntimeFramework
    {
        /// <summary>
        /// Creates a new empty runtime framework description.
        /// </summary>
        public RuntimeFramework()
        {
        }

        /// <summary>
        /// Creates a new runtime framework description.
        /// </summary>
        public RuntimeFramework(string name, string version)
        {
            Name = name;
            Version = version;
        }

        /// <summary>
        /// Gets or sets the name of the framework implementation.
        /// </summary>
        public string? Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version number of the framework implementation.
        /// </summary>
        public string? Version
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
        internal static RuntimeFramework FromJsonNode(JSONNode node)
        {
            var result = new RuntimeFramework
            {
                Name = node.HasKey("name") && !node["name"].IsNull ? node["name"].Value : null,
                Version = node.HasKey("version") && !node["version"].IsNull ? node["version"].Value : null
            };

            result.AdditionalProperties = JsonExtensionData.Read(
                node,
                "name",
                "version"
            );

            return result;
        }
#endif

        /// <inheritdoc />
        public override string ToString() => $"Name: {Name}, Version: {Version}";
    }
}
