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
        /// Indicates no special metadata builder flags.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Indicates indices into the #Blob stream should be preserved whenever possible during the construction
        /// of the metadata directory. 
        /// </summary>
        PreserveBlobIndices = 1,
        
        /// <summary>
        /// Indicates indices into the #GUID stream should be preserved whenever possible during the construction
        /// of the metadata directory. 
        /// </summary>
        PreserveGuidIndices = 2,
        
        /// <summary>
        /// Indicates indices into the #Strings stream should be preserved whenever possible during the construction
        /// of the metadata directory. 
        /// </summary>
        PreserveStringIndices = 4,
        
        /// <summary>
        /// Indicates indices into the #US stream should be preserved whenever possible during the construction
        /// of the metadata directory. 
        /// </summary>
        PreserveUserStringIndices = 8,
        
        /// <summary>
        /// Indicates indices into the type references table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveTypeReferenceIndices = 16,
    }
}