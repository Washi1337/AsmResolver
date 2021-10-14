using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsmResolver.Collections;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Represents a set of resources embedded into a ".resources" file.
    /// </summary>
    public class ResourceSet : LazyList<ResourceSetEntry>
    {
        public ResourceSet()
            : this(ResourceManagerHeader.Framework40Header, 2)
        {
        }

        /// <inheritdoc />
        public ResourceSet(ResourceManagerHeader managerHeader, int formatVersion)
        {
            ManagerHeader = managerHeader;
            FormatVersion = formatVersion;
        }

        /// <summary>
        /// Gets or sets the resource manager header of the set.
        /// </summary>
        public ResourceManagerHeader ManagerHeader
        {
            get;
            set;
        }

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

        /// <summary>
        /// Serializes the resource set and writes it to the provided output stream.
        /// </summary>
        /// <param name="writer">The output stream.</param>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported version is specified in <see cref="FormatVersion"/>.</exception>
        public void Write(IBinaryStreamWriter writer) => Write(writer, DefaultResourceDataSerializer.Instance);

        /// <summary>
        /// Serializes the resource set and writes it to the provided output stream.
        /// </summary>
        /// <param name="writer">The output stream.</param>
        /// <param name="serializer">The object responsible for serializing user-defined types.</param>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported version is specified in <see cref="FormatVersion"/>.</exception>
        public void Write(IBinaryStreamWriter writer, IResourceDataSerializer serializer)
        {
            if (FormatVersion is not 1 and not 2)
                throw new NotSupportedException($"Invalid or unsupported format version {FormatVersion}.");

            using var dataSection = new MemoryStream();
            var dataWriter = new BinaryStreamWriter(dataSection);

            // Entry headers.
            var sortedEntries = Items.OrderBy(item => item.Name).ToArray();
            var entryHeaders = new SortedDictionary<string, ResourceSetEntryHeader>();
            uint entryOffset = 0;

            // Hash table.
            uint[] nameHashes = new uint[Count];
            uint[] nameOffsets = new uint[Count];

            // Type codes.
            var userTypeNames = new List<string>();
            var typeNameToTypeCode = new Dictionary<string, int>();

            // Add all items in the resource set.
            for (int i = 0; i < sortedEntries.Length; i++)
            {
                var item = sortedEntries[i];

                // Determine type code.
                if ((FormatVersion == 1 || item.Type is not IntrinsicResourceType)
                    && !typeNameToTypeCode.TryGetValue(item.Type.FullName, out int typeCode))
                {
                    // New user-defined type. Add it to the list.

                    typeCode = userTypeNames.Count;
                    if (FormatVersion == 2)
                        typeCode += (int) ResourceTypeCode.StartOfUserTypes;

                    typeNameToTypeCode[item.Type.FullName] = typeCode;
                    userTypeNames.Add(item.Type.FullName);
                }
                else if (item.Type is IntrinsicResourceType intrinsicType)
                {
                    // Format allows for intrinsic types to be used.
                    typeCode = (int)intrinsicType.TypeCode;
                }
                else
                {
                    throw new NotSupportedException($"Invalid or unsupported resource type {item.Type.FullName}.");
                }

                // Create and add entry header to hash table.
                var entryHeader = new ResourceSetEntryHeader(item.Name, (uint) dataWriter.Offset);
                entryHeaders.Add(item.Name, entryHeader);
                nameHashes[i] = HashString(item.Name);
                nameOffsets[i] = entryOffset;
                entryOffset += entryHeader.GetPhysicalSize();

                // Write data to data section.
                item.Write(dataWriter, FormatVersion, typeCode, serializer);
            }

            // Write header.
            ManagerHeader.Write(writer);
            writer.WriteInt32(FormatVersion);
            writer.WriteInt32(Count);

            // Write user types.
            writer.WriteInt32(userTypeNames.Count);
            foreach (string typeName in userTypeNames)
                writer.WriteBinaryFormatterString(typeName);

            // Write padding.
            int x = 0;
            while ((writer.Offset & 0b111) != 0)
                writer.WriteByte((byte) "PAD"[x++ % 3]);

            // Write hash table.
            Array.Sort(nameHashes, nameOffsets);
            foreach (uint hash in nameHashes)
                writer.WriteUInt32(hash);
            foreach (uint offset in nameOffsets)
                writer.WriteUInt32(offset);

            // Write location of data section.
            writer.WriteUInt32((uint) (writer.Offset + sizeof(uint) + entryOffset));

            // Write names in name table
            foreach (var header in entryHeaders.Values)
                header.Write(writer);

            // Write data section.
            writer.WriteBytes(dataSection.ToArray());
        }

        private static uint HashString(string key)
        {
            // Reference:
            // https://github.com/dotnet/runtime/blob/97ef512a7cbc21d982ce68de805fec2c42e3561c/src/libraries/System.Private.CoreLib/src/System/Resources/FastResourceComparer.cs#L42

            uint hash = 5381;
            for (int i = 0; i < key.Length; i++)
                hash = ((hash << 5) + hash) ^ key[i];
            return hash;
        }

    }
}
