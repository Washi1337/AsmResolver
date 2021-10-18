using System;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Represents an existing entry in a <see cref="SerializedResourceSet"/>.
    /// </summary>
    public class SerializedResourceSetEntry : ResourceSetEntry
    {
        private readonly SerializedResourceSet _parentSet;
        private readonly BinaryStreamReader _contentsReader;

        /// <summary>
        /// Initializes a new <see cref="SerializedResourceSetEntry"/>.
        /// </summary>
        /// <param name="parentSet">The parent set that this entry is defined in.</param>
        /// <param name="name">The name of the entry.</param>
        /// <param name="type">The type of the value.</param>
        /// <param name="contentsReader">The input stream to use for reading the raw data of the value.</param>
        public SerializedResourceSetEntry(SerializedResourceSet parentSet, string name, ResourceType type, in BinaryStreamReader contentsReader)
            : base(name, type)
        {
            _parentSet = parentSet;
            _contentsReader = contentsReader;
        }

        /// <inheritdoc />
        protected override object? GetData() => _parentSet.OriginalFormatVersion switch
        {
            1 => GetDataV1(),
            2 => GetDataV2(),
            _ => throw new NotSupportedException("Invalid or unsupported resource set data version.")
        };

        private object? GetDataV1()
        {
            // Reference:
            // https://github.com/dotnet/runtime/blob/9d771a26f058a9fa4a49850d4778bbab7aa79a22/src/libraries/System.Private.CoreLib/src/System/Resources/ResourceReader.cs#L535

            if (Type is IntrinsicResourceType {TypeCode: ResourceTypeCode.Null})
                return null;

            string typeName = Type.FullName;
            int commaIndex = typeName.IndexOf(',');
            if (commaIndex >= 0)
                typeName = typeName.Remove(commaIndex);

            var reader = _contentsReader;
            return typeName switch
            {
                "System.String" => reader.ReadBinaryFormatterString(),
                "System.Boolean" => reader.ReadByte() != 0,
                "System.Char" => (char) reader.ReadUInt16(),
                "System.Byte" => reader.ReadByte(),
                "System.SByte" => reader.ReadSByte(),
                "System.Int16" => reader.ReadInt16(),
                "System.UInt16" => reader.ReadUInt16(),
                "System.Int32" => reader.ReadInt32(),
                "System.UInt32" => reader.ReadUInt32(),
                "System.Int64" => reader.ReadInt64(),
                "System.UInt64" => reader.ReadUInt64(),
                "System.Single" => reader.ReadSingle(),
                "System.Double" => reader.ReadDouble(),
                "System.Decimal" => reader.ReadDecimal(),
                "System.DateTime" => new DateTime(reader.ReadInt64()),
                "System.TimeSpan" => new TimeSpan(reader.ReadInt64()),
                _ => _parentSet.DataSerializer.Deserialize(ref reader, Type),
            };
        }

        private object? GetDataV2()
        {
            // Reference:
            // https://github.com/dotnet/runtime/blob/9d771a26f058a9fa4a49850d4778bbab7aa79a22/src/libraries/System.Private.CoreLib/src/System/Resources/ResourceReader.cs#L610

            var reader = _contentsReader;

            if (Type is not IntrinsicResourceType intrinsicType)
                return _parentSet.DataSerializer.Deserialize(ref reader, Type);

            return intrinsicType.TypeCode switch
            {
                ResourceTypeCode.Null => null,
                ResourceTypeCode.String => reader.ReadBinaryFormatterString(),
                ResourceTypeCode.Boolean => reader.ReadByte() != 0,
                ResourceTypeCode.Char => (char) reader.ReadUInt16(),
                ResourceTypeCode.Byte => reader.ReadByte(),
                ResourceTypeCode.SByte => reader.ReadSByte(),
                ResourceTypeCode.Int16 => reader.ReadInt16(),
                ResourceTypeCode.UInt16 => reader.ReadUInt16(),
                ResourceTypeCode.Int32 => reader.ReadInt32(),
                ResourceTypeCode.UInt32 => reader.ReadUInt32(),
                ResourceTypeCode.Int64 => reader.ReadInt64(),
                ResourceTypeCode.UInt64 => reader.ReadUInt64(),
                ResourceTypeCode.Single => reader.ReadSingle(),
                ResourceTypeCode.Double => reader.ReadDouble(),
                ResourceTypeCode.Decimal => reader.ReadDecimal(),
                ResourceTypeCode.DateTime => DateTime.FromBinary(reader.ReadInt64()),
                ResourceTypeCode.TimeSpan => new TimeSpan(reader.ReadInt64()),
                ResourceTypeCode.ByteArray => ReadByteArray(),
                ResourceTypeCode.Stream => ReadByteArray(),
                _ => _parentSet.DataSerializer.Deserialize(ref reader, Type),
            };

            byte[] ReadByteArray()
            {
                int length = reader.ReadInt32();
                if (length < 0)
                    throw new FormatException("Resource data length is negative.");
                byte[] data = new byte[length];
                reader.ReadBytes(data, 0, length);
                return data;
            }
        }
    }
}
