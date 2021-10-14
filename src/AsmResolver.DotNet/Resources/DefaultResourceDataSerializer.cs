using System;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IResourceDataSerializer"/> interface, that represents
    /// custom user-defined data as byte arrays.
    /// </summary>
    public class DefaultResourceDataSerializer : IResourceDataSerializer
    {
        /// <summary>
        /// Gets the default instance of the <see cref="DefaultResourceDataSerializer"/> class.
        /// </summary>
        public static DefaultResourceDataSerializer Instance
        {
            get;
        } = new();

        /// <inheritdoc />
        public void Serialize(IBinaryStreamWriter writer, ResourceType type, object? value)
        {
            switch (value)
            {
                case null:
                    return;

                case byte[] data:
                    writer.WriteBytes(data);
                    break;

                default:
                    throw new NotSupportedException($"Invalid or unsupported object type {value.GetType()}.");
            }
        }

        /// <inheritdoc />
        public object? Deserialize(in BinaryStreamReader reader, ResourceType type) => reader.ReadToEnd();
    }
}
