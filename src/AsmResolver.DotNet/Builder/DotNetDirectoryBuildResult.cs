using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Describes the result of a construction of a <see cref="IDotNetDirectory"/>. from a <see cref="ModuleDefinition"/>.
    /// </summary>
    public class DotNetDirectoryBuildResult
    {
        /// <summary>
        /// Creates a new instance of teh <see cref="DotNetDirectoryBuildResult"/> class.
        /// </summary>
        /// <param name="directory">The constructed directory.</param>
        /// <param name="mapping">A dictionary defining a mapping between members and their new metadata tokens.</param>
        public DotNetDirectoryBuildResult(IDotNetDirectory directory, IReadOnlyDictionary<IMetadataMember, MetadataToken> mapping)
        {
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
            TokenMapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        }

        /// <summary>
        /// Gets the constructed .NET data directory.
        /// </summary>
        public IDotNetDirectory Directory
        {
            get;
        }

        /// <summary>
        /// Gets a dictionary that maps metadata members to their newly assigned tokens.
        /// </summary>
        public IReadOnlyDictionary<IMetadataMember, MetadataToken> TokenMapping
        {
            get;
        }
    }
}