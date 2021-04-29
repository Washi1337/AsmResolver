using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.File;

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
        public List<MetadataToken> Tokens
        {
            get;
        } = new();

        /// <summary>
        /// Reads a single vtable from the provided input stream.
        /// </summary>
        /// <param name="file">The original PE file that is currently being parsed.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns></returns>
        public static VTableFixup FromReader(IPEFile file, ref BinaryStreamReader reader)
        {
            var tableReader = file.CreateReaderAtRva(reader.ReadUInt32());
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
    }
}
