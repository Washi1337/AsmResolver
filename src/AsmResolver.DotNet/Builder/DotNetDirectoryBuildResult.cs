using System;
using AsmResolver.PE.DotNet;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Describes the result of a construction of a <see cref="DotNetDirectory"/>. from a <see cref="ModuleDefinition"/>.
    /// </summary>
    public class DotNetDirectoryBuildResult
    {
        /// <summary>
        /// Creates a new instance of teh <see cref="DotNetDirectoryBuildResult"/> class.
        /// </summary>
        /// <param name="directory">The constructed directory.</param>
        /// <param name="mapping">An object defining a mapping between members and their new metadata tokens.</param>
        public DotNetDirectoryBuildResult(DotNetDirectory directory, ITokenMapping mapping)
        {
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
            TokenMapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        }

        /// <summary>
        /// Gets the constructed .NET data directory.
        /// </summary>
        public DotNetDirectory Directory
        {
            get;
        }

        /// <summary>
        /// Gets an object that maps metadata members to their newly assigned tokens.
        /// </summary>
        public ITokenMapping TokenMapping
        {
            get;
        }
    }
}
