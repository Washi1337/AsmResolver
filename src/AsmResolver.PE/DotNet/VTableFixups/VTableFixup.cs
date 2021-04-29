using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.VTableFixups
{
    /// <summary>
    /// Represents a VTable declared by the VTable Fixup Directory
    /// </summary>
    public class VTableFixup
    {
        /// <summary>
        /// Gets or sets the type of the entries
        /// </summary>
        public VTableType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a list of the MetadataTokens in the VTable
        /// </summary>
        public IList<MetadataToken> Tokens
        {
            get;
        } = new List<MetadataToken>();

        /// <summary>
        /// Reads a single vtable from the provided input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns></returns>
        public static VTableFixup FromReader(PEReaderContext context, ref BinaryStreamReader reader)
        {
            if (!context.File.TryCreateReaderAtRva(reader.ReadUInt32(), out var tableReader))
            {
                context.BadImage("VTable fixups directory contains an invalid RVA for the entries of a vtable.");
                return null;
            }

            ushort entries = reader.ReadUInt16();
            var vtable = new VTableFixup
            {
                Type = (VTableType) reader.ReadUInt16()
            };

            for (int i = 0; i < entries; i++)
            {
                vtable.Tokens.Add(vtable.Type.HasFlag(VTableType.VTable32Bit)
                    ? new MetadataToken(tableReader.ReadUInt32())
                    : new MetadataToken((uint) tableReader.ReadInt64()));
            }

            return vtable;
        }

        internal uint GetEntriesSize() =>
            (uint) Tokens.Count *
            (uint) (Type.HasFlag(VTableType.VTable32Bit)
                ? 4
                : 8);
    }
}
