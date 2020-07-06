namespace AsmResolver.PE.DotNet.Resources
{
    /// <summary>
    /// Represents a data directory containing the data of all manifest resources stored in a .NET module.
    /// </summary>
    public abstract class DotNetResourcesDirectory : SegmentBase
    {
        /// <summary>
        /// Gets the manifest resource data by its offset.
        /// </summary>
        /// <param name="offset">The offset of the resource data, relative to the start of the data directory.</param>
        /// <returns>The data, or <c>null</c> if the offset is not a valid offset.</returns>
        public abstract byte[] GetManifestResourceData(uint offset);

        /// <summary>
        /// Gets a reader starting at the beginning of the resource data referenced by the provided offset.
        /// </summary>
        /// <param name="offset">The offset of the resource data, relative to the start of the data directory.</param>
        /// <returns>The data reader, or <c>null</c> if the offset is not a valid offset.</returns>
        public abstract IBinaryStreamReader CreateManifestResourceReader(uint offset);
    }
}