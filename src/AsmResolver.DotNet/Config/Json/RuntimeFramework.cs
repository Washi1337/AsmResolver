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

#if !NET5_0_OR_GREATER
        internal static RuntimeFramework FromJsonNode(JSONNode node)
        {
            var result = new RuntimeFramework
            {
                Name = node.HasKey("name") && !node["name"].IsNull ? node["name"].Value : null,
                Version = node.HasKey("version") && !node["version"].IsNull ? node["version"].Value : null
            };

            return result;
        }
#endif

        /// <inheritdoc />
        public override string ToString() => $"Name: {Name}, Version: {Version}";
    }
}
