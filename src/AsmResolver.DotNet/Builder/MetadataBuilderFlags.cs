using System;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides members for describing the behaviour of the .NET directory builder.
    /// </summary>
    [Flags]
    public enum MetadataBuilderFlags
    {
        /// <summary>
        /// Indicates indices into the blob stream should be preserved whenever possible during the construction
        /// of the metadata directory. 
        /// </summary>
        PreserveBlobIndices = 1
    }
}