using AsmResolver.IO;

namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Provides members for serializing and deserializing resource data of user-defined types.
    /// </summary>
    public interface IResourceDataSerializer
    {
        /// <summary>
        /// Writes a resource object to the output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to.</param>
        /// <param name="type">The type of the resource.</param>
        /// <param name="value">The object to serialize.</param>
        void Serialize(IBinaryStreamWriter writer, ResourceType type, object? value);

        /// <summary>
        /// Reads a resource object from the input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="type">The type of the resource.</param>
        object? Deserialize(in BinaryStreamReader reader, ResourceType type);
    }
}
