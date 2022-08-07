using System.IO;
using System.Text.Json;

namespace AsmResolver.DotNet.Config.Json
{
    /// <summary>
    /// Represents the root object of a runtime configuration, stored in a *.runtimeconfig.json file.
    /// </summary>
    public class RuntimeConfiguration
    {
        /// <summary>
        /// Parses runtime configuration from a JSON file.
        /// </summary>
        /// <param name="path">The path to the runtime configuration file.</param>
        /// <returns>The parsed runtime configuration.</returns>
        public static RuntimeConfiguration? FromFile(string path)
        {
            return FromJson(File.ReadAllText(path));
        }

        /// <summary>
        /// Parses runtime configuration from a JSON string.
        /// </summary>
        /// <param name="json">The raw json string configuration file.</param>
        /// <returns>The parsed runtime configuration.</returns>
        public static RuntimeConfiguration? FromJson(string json)
        {
            return JsonSerializer.Deserialize(json, RuntimeConfigurationSerializerContext.Default.RuntimeConfiguration);
        }

        /// <summary>
        /// Creates a new empty runtime configuration.
        /// </summary>
        public RuntimeConfiguration()
        {
            RuntimeOptions = new();
        }

        /// <summary>
        /// Creates a new runtime configuration with the provided options.
        /// </summary>
        public RuntimeConfiguration(RuntimeOptions options)
        {
            RuntimeOptions = options;
        }

        /// <summary>
        /// Gets or sets the runtime options.
        /// </summary>
        public RuntimeOptions RuntimeOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Serializes the configuration to a JSON string.
        /// </summary>
        /// <returns>The JSON string.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, RuntimeConfigurationSerializerContext.Default.RuntimeConfiguration);
        }

        /// <summary>
        /// Writes the configuration to a file.
        /// </summary>
        /// <param name="path">The path to the JSON output file.</param>
        public void Write(string path)
        {
            File.WriteAllText(path, ToJson());
        }
    }
}
