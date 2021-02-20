namespace AsmResolver.DotNet.Config.Json
{
    /// <summary>
    /// Provides version information about a framework implementation.
    /// </summary>
    public class RuntimeFramework
    {
        /// <summary>
        /// Gets or sets the name of the framework implementation.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version number of the framework implementation.
        /// </summary>
        public string Version
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string ToString() => $"Name: {Name}, Version: {Version}";
    }
}
