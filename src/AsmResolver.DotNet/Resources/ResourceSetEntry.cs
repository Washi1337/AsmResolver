using System;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Represents a single element in a resource set.
    /// </summary>
    public partial class ResourceSetEntry
    {
        private readonly object _lock = new();

        /// <summary>
        /// Creates a new empty resource set entry.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="typeCode">The type of the element's value.</param>
        public ResourceSetEntry(string name, ResourceTypeCode typeCode)
        {
            Name = name;
            Type = IntrinsicResourceType.Get(typeCode);
        }

        /// <summary>
        /// Creates a new empty resource set entry.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="type">The type of the element's value.</param>
        public ResourceSetEntry(string name, ResourceType type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Creates a new resource set entry.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="typeCode">The type of the element's value.</param>
        /// <param name="data">The value of the element.</param>
        public ResourceSetEntry(string name, ResourceTypeCode typeCode, object? data)
        {
            Name = name;
            Type = IntrinsicResourceType.Get(typeCode);
            Data = data;
        }

        /// <summary>
        /// Creates a new resource set entry.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="type">The type of the element's value.</param>
        /// <param name="data">The value of the element.</param>
        public ResourceSetEntry(string name, ResourceType type, object? data)
        {
            Name = name;
            Type = type;
            Data = data;
        }

        /// <summary>
        /// Gets the name of the entry.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Gets the type code associated to the element.
        /// </summary>
        public ResourceType Type
        {
            get;
        }

        /// <summary>
        /// Gets the value of this resource entry.
        /// </summary>
        [LazyProperty]
        public partial object? Data
        {
            get;
            set;
        }

        /// <summary>
        /// Obtains the value of the resource entry.
        /// </summary>
        /// <returns>The value.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Data"/> property.
        /// </remarks>
        protected virtual object? GetData() => null;

        /// <inheritdoc />
        public override string ToString() => $"{Name} : {Type.FullName}";

        internal void Write(BinaryStreamWriter writer, int formatVersion, int typeCode, IResourceDataSerializer serializer)
        {
            writer.Write7BitEncodedInt32(typeCode);

            switch (formatVersion)
            {
                case 1:
                    WriteV1(writer, serializer);
                    break;

                case 2:
                    WriteV2(writer, serializer);
                    break;

                default:
                    throw new NotSupportedException("Invalid or unsupported format version number.");
            }
        }

        private void WriteV1(BinaryStreamWriter writer, IResourceDataSerializer serializer)
        {
            if (Data is null)
                return;

            string typeName = Type.FullName;
            int commaIndex = typeName.IndexOf(',');
            if (commaIndex >= 0)
                typeName = typeName.Remove(commaIndex);

            switch (typeName)
            {
                case "System.String":
                    writer.WriteBinaryFormatterString((string) Data);
                    break;
                case "System.Boolean":
                    writer.WriteByte((byte) ((bool) Data ? 1 : 0));
                    break;
                case "System.Char":
                    writer.WriteUInt16((char) Data);
                    break;
                case "System.Byte":
                    writer.WriteByte((byte) Data);
                    break;
                case "System.SByte":
                    writer.WriteSByte((sbyte) Data);
                    break;
                case "System.Int16":
                    writer.WriteInt16((short) Data);
                    break;
                case "System.UInt16":
                    writer.WriteUInt16((ushort) Data);
                    break;
                case "System.Int32":
                    writer.WriteInt32((int) Data);
                    break;
                case "System.UInt32":
                    writer.WriteUInt32((uint) Data);
                    break;
                case "System.Int64":
                    writer.WriteInt64((long) Data);
                    break;
                case "System.UInt64":
                    writer.WriteUInt64((ulong) Data);
                    break;
                case "System.Single":
                    writer.WriteSingle((float) Data);
                    break;
                case "System.Double":
                    writer.WriteDouble((double) Data);
                    break;
                case "System.Decimal":
                    writer.WriteDecimal((decimal) Data);
                    break;
                case "System.DateTime":
                    writer.WriteInt64(((DateTime) Data).Ticks);
                    break;
                case "System.TimeSpan":
                    writer.WriteInt64(((TimeSpan) Data).Ticks);
                    break;
                default:
                    serializer.Serialize(writer, Type, Data);
                    break;
            }
        }

        private void WriteV2(BinaryStreamWriter writer, IResourceDataSerializer serializer)
        {
            if (Data is null)
                return;

            if (Type is not IntrinsicResourceType intrinsicType)
            {
                WriteV1(writer, serializer);
                return;
            }

            switch (intrinsicType.TypeCode)
            {
                case ResourceTypeCode.Null:
                    break;
                case ResourceTypeCode.String:
                    writer.WriteBinaryFormatterString((string) Data);
                    break;
                case ResourceTypeCode.Boolean:
                    writer.WriteByte((byte) ((bool) Data ? 1 : 0));
                    break;
                case ResourceTypeCode.Char:
                    writer.WriteUInt16((char) Data);
                    break;
                case ResourceTypeCode.Byte:
                    writer.WriteByte((byte) Data);
                    break;
                case ResourceTypeCode.SByte:
                    writer.WriteSByte((sbyte) Data);
                    break;
                case ResourceTypeCode.Int16:
                    writer.WriteInt16((short) Data);
                    break;
                case ResourceTypeCode.UInt16:
                    writer.WriteUInt16((ushort) Data);
                    break;
                case ResourceTypeCode.Int32:
                    writer.WriteInt32((int) Data);
                    break;
                case ResourceTypeCode.UInt32:
                    writer.WriteUInt32((uint) Data);
                    break;
                case ResourceTypeCode.Int64:
                    writer.WriteInt64((long) Data);
                    break;
                case ResourceTypeCode.UInt64:
                    writer.WriteUInt64((ulong) Data);
                    break;
                case ResourceTypeCode.Single:
                    writer.WriteSingle((float) Data);
                    break;
                case ResourceTypeCode.Double:
                    writer.WriteDouble((double) Data);
                    break;
                case ResourceTypeCode.Decimal:
                    writer.WriteDecimal((decimal) Data);
                    break;
                case ResourceTypeCode.DateTime:
                    writer.WriteInt64(((DateTime) Data).Ticks);
                    break;
                case ResourceTypeCode.TimeSpan:
                    writer.WriteInt64(((TimeSpan) Data).Ticks);
                    break;
                case ResourceTypeCode.ByteArray:
                case ResourceTypeCode.Stream:
                    byte[] data = (byte[]) Data;
                    writer.WriteInt32(data.Length);
                    writer.WriteBytes(data);
                    break;
                default:
                    serializer.Serialize(writer, Type, Data);
                    break;
            }
        }
    }
}
