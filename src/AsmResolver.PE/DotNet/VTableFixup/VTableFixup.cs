using System;
using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.File;

namespace AsmResolver.PE.DotNet.VTableFixup
{
    /// <summary>
    /// Represents a VTable declared by the VTable Fixup Directory
    /// </summary>
    public class VTableFixup : SegmentBase
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
        public List<MetadataToken> Tokens
        {
            get;
        } = new();

        /// <summary>
        /// Reads a vtable from the provided input stream.
        /// </summary>
        /// <param name="file">The original PE file that is currently being parsed.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>A VTable Fixup.</returns>
        public static VTableFixup FromReader(IPEFile file, ref BinaryStreamReader reader)
        {
            var tableReader = file.CreateReaderAtRva(reader.ReadUInt32());
            ushort entries = tableReader.ReadUInt16();
            var vtable = new VTableFixup
            {
                Rva = tableReader.StartRva,
                Offset = tableReader.StartOffset,
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

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
