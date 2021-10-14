using AsmResolver.Collections;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Represents a set of resources embedded into a ".resources" file.
    /// </summary>
    public class ResourceSet : LazyList<ResourceSetEntry>
    {
        /// <summary>
        /// Gets the magic number that every resource set starts with.
        /// </summary>
        public const uint Magic = 0xBEEFCACE;

        /// <summary>
        /// Gets the version of the file format that is used for this resource set.
        /// </summary>
        public int FormatVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a resource set from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The resource set.</returns>
        public static ResourceSet FromReader(in BinaryStreamReader reader) =>
            FromReader(reader, DefaultResourceDataSerializer.Instance);

        /// <summary>
        /// Reads a resource set from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="serializer">The object responsible for deserializing user-defined types.</param>
        /// <returns>The resource set.</returns>
        public static ResourceSet FromReader(in BinaryStreamReader reader, IResourceDataSerializer serializer) =>
            new SerializedResourceSet(reader, serializer);

        /// <inheritdoc />
        protected override void Initialize()
        {
        }
    }
}
