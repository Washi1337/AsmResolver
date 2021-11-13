using System.Text;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Resources
{
    internal readonly struct ResourceSetEntryHeader : IWritable
    {
        public ResourceSetEntryHeader(string name, uint offset)
        {
            Name = name;
            Offset = offset;
        }

        public string Name
        {
            get;
        }

        public uint Offset
        {
            get;
        }

        public static ResourceSetEntryHeader FromReader(ref BinaryStreamReader reader) => new(
            reader.ReadBinaryFormatterString(Encoding.Unicode),
            reader.ReadUInt32());

        /// <inheritdoc />
        public uint GetPhysicalSize() => Name.GetBinaryFormatterSize(Encoding.Unicode) + sizeof(uint);

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            writer.WriteBinaryFormatterString(Name, Encoding.Unicode);
            writer.WriteUInt32(Offset);
        }

#if DEBUG
        /// <inheritdoc />
        public override string ToString() => $"{nameof(Name)}: {Name}, {nameof(Offset)}: {Offset:X8}";
#endif
    }
}
