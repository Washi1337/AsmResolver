using System;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Represents a serialized resource set that was read from an existing file.
    /// </summary>
    public class SerializedResourceSet : ResourceSet
    {
        private readonly int _originalCount;
        private readonly BinaryStreamReader _dataSectionReader;
        private readonly BinaryStreamReader _entryReader;

        /// <summary>
        /// Reads a resource set from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="dataSerializer">The serializer to use for deserializing instances of user-defined types.</param>
        /// <exception cref="FormatException">Occurs when an invalid format was recognized.</exception>
        /// <exception cref="NotSupportedException">Occurs when an unsupported resource reader or resource set type is specified in the file.</exception>
        public SerializedResourceSet(BinaryStreamReader reader, IResourceDataSerializer dataSerializer)
        {
            DataSerializer = dataSerializer;
            uint startOffset = reader.RelativeOffset;

            ManagerHeader = ResourceManagerHeader.FromReader(ref reader);
            if (!ManagerHeader.ResourceReaderName.StartsWith("System.Resources.ResourceReader")
                && !ManagerHeader.ResourceReaderName.StartsWith("System.Resources.Extensions.DeserializingResourceReader"))
            {
                throw new NotSupportedException($"Unsupported resource reader type {ManagerHeader.ResourceReaderName}.");
            }

            // Move to resource set header.
            reader.RelativeOffset = startOffset + ManagerHeader.GetPhysicalSize();

            FormatVersion = OriginalFormatVersion = reader.ReadInt32();
            if (OriginalFormatVersion is not 1 and not 2)
                throw new NotSupportedException($"Invalid or unsupported resource set version {OriginalFormatVersion}.");

            _originalCount = reader.ReadInt32();
            if (_originalCount < 0)
                throw new FormatException("Negative entry count.");

            // Read user type table.
            int userTypeCount = reader.ReadInt32();
            if (userTypeCount < 0)
                throw new FormatException("Negative user type count.");

            OriginalUserTypes = new UserDefinedResourceType[userTypeCount];
            for (int i = 0; i < userTypeCount; i++)
                OriginalUserTypes[i] = new UserDefinedResourceType(reader.ReadBinaryFormatterString());
            reader.AlignRelative(8);

            // Skip string hashtable.
            reader.RelativeOffset += (uint) (_originalCount * sizeof(uint) * 2);

            // Slice data section.
            _dataSectionReader = reader.ForkRelative(reader.ReadUInt32());

            // Set up reader for entries table.
            _entryReader = reader.Fork();
        }

        /// <inheritdoc />
        public override int Count => IsInitialized ? Items.Count : _originalCount;

        internal int OriginalFormatVersion
        {
            get;
        }

        internal UserDefinedResourceType[] OriginalUserTypes
        {
            get;
        }

        internal IResourceDataSerializer DataSerializer
        {
            get;
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            var headers = new ResourceSetEntryHeader[_originalCount];

            // Read all headers of every entry.
            var entryReader = _entryReader;
            for (int i = 0; i < _originalCount; i++)
                headers[i] = ResourceSetEntryHeader.FromReader(ref entryReader);

            // Sort by offset.
            Array.Sort(headers, static (a, b) => a.Offset.CompareTo(b.Offset));

            // Create entries with sliced readers.
            for (int i = 0; i < _originalCount; i++)
            {
                // Determine size of data.
                uint nextOffset = i < _originalCount - 1
                    ? headers[i + 1].Offset
                    : _dataSectionReader.Length;
                uint size = nextOffset - headers[i].Offset;

                // Slice data reader.
                var contentsReader = _dataSectionReader.ForkRelative(headers[i].Offset, size);

                var type = GetResourceType(contentsReader.Read7BitEncodedInt32());
                Items.Add(new SerializedResourceSetEntry(this, headers[i].Name, type, contentsReader));
            }
        }

        private ResourceType GetResourceType(int typeCode) => OriginalFormatVersion switch
        {
            1 => GetResourceTypeV1(typeCode),
            2 => GetResourceTypeV2(typeCode),
            _ => throw new NotSupportedException("Invalid or unsupported resource set data version.")
        };

        private ResourceType GetResourceTypeV1(int typeCode)
        {
            if (typeCode == -1)
                return IntrinsicResourceType.Get(ResourceTypeCode.Null);
            if (typeCode < 0 || typeCode >= OriginalUserTypes.Length)
                throw new FormatException($"Invalid resource type code {typeCode}.");
            return OriginalUserTypes[typeCode];
        }

        private ResourceType GetResourceTypeV2(int typeCode)
        {
            if (typeCode < (int) ResourceTypeCode.StartOfUserTypes)
                return IntrinsicResourceType.Get((ResourceTypeCode) typeCode);
            int index = typeCode - (int) ResourceTypeCode.StartOfUserTypes;
            if (index >= OriginalUserTypes.Length)
                throw new FormatException($"Invalid resource type code {typeCode}.");
            return OriginalUserTypes[index];
        }
    }
}
