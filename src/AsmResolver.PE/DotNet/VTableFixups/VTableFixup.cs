using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.VTableFixups
{
    /// <summary>
    /// Represents a VTable declared by the VTable fixups directory.
    /// </summary>
    public class VTableFixup : SegmentBase
    {
        /// <summary>
        /// Creates a new VTable fixup.
        /// </summary>
        /// <param name="type"></param>
        public VTableFixup(VTableType type) => Tokens.Type = type;

        /// <summary>
        /// Gets a list of the tokens added to this vtable.
        /// </summary>
        public VTableTokenCollection Tokens
        {
            get;
        } = new ();

        /// <summary>
        /// Reads a single vtable from the provided input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns></returns>
        public static VTableFixup? FromReader(PEReaderContext context, ref BinaryStreamReader reader)
        {
            ulong offset = reader.Offset;
            uint rva = reader.Rva;

            if (!context.File.TryCreateReaderAtRva(reader.ReadUInt32(), out var tableReader))
            {
                context.BadImage("VTable fixups directory contains an invalid RVA for the entries of a vtable.");
                return null;
            }

            ushort entries = reader.ReadUInt16();
            var vtable = new VTableFixup((VTableType) reader.ReadUInt16());
            vtable.UpdateOffsets(context.GetRelocation(offset, rva));
            vtable.Tokens.UpdateOffsets(context.GetRelocation(tableReader.Offset, tableReader.Rva));

            for (int i = 0; i < entries; i++)
            {
                vtable.Tokens.Add((vtable.Tokens.Type & VTableType.VTable32Bit) != 0
                    ? new MetadataToken(tableReader.ReadUInt32())
                    : new MetadataToken((uint) tableReader.ReadInt64()));
            }

            return vtable;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() =>
            sizeof(uint)         // RVA
             + sizeof(ushort)    // Entries Count
             + sizeof(ushort);   // Type

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32(Tokens.Rva);
            writer.WriteUInt16((ushort) Tokens.Count);
            writer.WriteUInt16((ushort) Tokens.Type);
        }
    }
}
