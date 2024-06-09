using System;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Represents the header of a resource set file.
    /// </summary>
    public readonly struct ResourceManagerHeader : IWritable
    {
        /// <summary>
        /// Gets the magic number that every resource set starts with.
        /// </summary>
        public const uint Magic = 0xBEEFCACE;

        /// <summary>
        /// Gets the default header used for resource set files targeting .NET Framework 2.0.
        /// </summary>
        public static readonly ResourceManagerHeader Default_v2_0_0_0 = new(
            KnownResourceReaderNames.ResourceReader_mscorlib_v2_0_0_0,
            KnownResourceReaderNames.RuntimeResourceSet);

        /// <summary>
        /// Gets the default header used for resource set files targeting .NET Framework 4.0 and higher.
        /// </summary>
        public static readonly ResourceManagerHeader Default_v4_0_0_0 = new(
            KnownResourceReaderNames.ResourceReader_mscorlib_v4_0_0_0,
            KnownResourceReaderNames.RuntimeResourceSet);

        /// <summary>
        /// Gets the default header used for resource set files targeting .NET Framework 4.0 and higher, with
        /// the System.Resources.Extensions.DeserializingResourceReader class as reader.
        /// </summary>
        public static readonly ResourceManagerHeader Deserializing_v4_0_0_0 = new(
            KnownResourceReaderNames.DeserializingResourceReader_SystemResourcesExtensions_v4_0_0_0,
            KnownResourceReaderNames.RuntimeResourceSet_SystemResourcesExtensions_v4_0_0_0);

        /// <summary>
        /// Creates a new instance of the <see cref="ResourceManagerHeader"/>.
        /// </summary>
        /// <param name="resourceReaderName">The full name of the type that is responsible for reading the resource set file.</param>
        /// <param name="resourceSetName">The full name of the type that is responsible for representing the resource set.</param>
        public ResourceManagerHeader(string resourceReaderName, string resourceSetName)
            : this(resourceReaderName.GetBinaryFormatterSize() + resourceSetName.GetBinaryFormatterSize(),
                resourceReaderName,
                resourceSetName)
        {
        }

        private ResourceManagerHeader(uint headerSize, string resourceReaderName, string resourceSetName)
        {
            HeaderSize = headerSize;
            ResourceReaderName = resourceReaderName;
            ResourceSetName = resourceSetName;
        }

        /// <summary>
        /// Gets the number of bytes that make up the resource manager header.
        /// </summary>
        public uint HeaderSize
        {
            get;
        }

        /// <summary>
        /// Gets the full name of the type that is responsible for reading the resource set file.
        /// </summary>
        public string ResourceReaderName
        {
            get;
        }

        /// <summary>
        /// Gets the full name of the type that is responsible for representing the resource set.
        /// </summary>
        public string ResourceSetName
        {
            get;
        }

        /// <summary>
        /// Reads a resource manager header from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Occurs when an invalid magic number is read, or one of the strings is encoded improperly.</exception>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported header version was encountered.</exception>
        public static ResourceManagerHeader FromReader(ref BinaryStreamReader reader)
        {
            uint magic = reader.ReadUInt32();
            if (magic != Magic)
                throw new FormatException($"Invalid magic number 0x{magic:X8}.");

            uint headerVersion = reader.ReadUInt32();
            if (headerVersion != 1)
                throw new NotSupportedException($"Invalid or unsupported header version {headerVersion}.");

            return new ResourceManagerHeader(
                reader.ReadUInt32(),
                reader.ReadBinaryFormatterString(),
                reader.ReadBinaryFormatterString());
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return sizeof(uint)   // Magic
                   + sizeof(uint) // Header version
                   + sizeof(uint) // Header size
                   + HeaderSize;
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer)
        {
            writer.WriteUInt32(Magic);
            writer.WriteUInt32(1);
            writer.WriteUInt32(GetPhysicalSize() - 3*sizeof(uint));
            writer.WriteBinaryFormatterString(ResourceReaderName);
            writer.WriteBinaryFormatterString(ResourceSetName);
        }
    }
}
