using System;
using System.Collections.Generic;
using System.Globalization;

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
        /// Gets or sets the framework version roll-forward policy.
        /// </summary>
        public string? RollForward
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether framework patch versions can be applied.
        /// </summary>
        public bool? ApplyPatches
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the legacy framework roll-forward policy.
        /// </summary>
        public int? RollForwardOnNoCandidateFx
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
            if (!node.IsObject)
                throw new FormatException("Expected an object for a framework reference.");

            if (node.HasKey("name") && !node["name"].IsNull && !node["name"].IsString)
                throw new FormatException("Expected a string for 'name'.");

            if (node.HasKey("version") && !node["version"].IsNull && !node["version"].IsString)
                throw new FormatException("Expected a string for 'version'.");

            var result = new RuntimeFramework
            {
                Name = node.HasKey("name") && !node["name"].IsNull ? node["name"].Value : null,
                Version = node.HasKey("version") && !node["version"].IsNull ? node["version"].Value : null
            };

            if (node.HasKey("rollForward") && !node["rollForward"].IsNull)
            {
                var value = node["rollForward"];
                if (!value.IsString)
                    throw new FormatException("Expected a string for 'rollForward'.");

                result.RollForward = value.Value;
            }

            if (node.HasKey("applyPatches") && !node["applyPatches"].IsNull)
            {
                var value = node["applyPatches"];
                if (!value.IsBoolean)
                    throw new FormatException("Expected a boolean for 'applyPatches'.");

                result.ApplyPatches = value.AsBool;
            }

            if (node.HasKey("rollForwardOnNoCandidateFx") && !node["rollForwardOnNoCandidateFx"].IsNull)
            {
                var value = node["rollForwardOnNoCandidateFx"];
                if (!value.IsNumber
                    || !int.TryParse(value.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int policy))
                {
                    throw new FormatException("Expected a 32-bit integer for 'rollForwardOnNoCandidateFx'.");
                }

                result.RollForwardOnNoCandidateFx = policy;
            }

            result.AdditionalProperties = JsonExtensionData.Read(
                node,
                "name",
                "version",
                "rollForward",
                "applyPatches",
                "rollForwardOnNoCandidateFx"
            );

            return result;
        }
#endif

        /// <inheritdoc />
        public override string ToString() => $"Name: {Name}, Version: {Version}";
    }
}
