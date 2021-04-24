using System;
using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.File;

namespace AsmResolver.PE.DotNet.VTableFixup
{
    /// <summary>
    /// Represents the VTable Fixup Directory in the Cor20 header.
    /// </summary>
    public class VTableFixupDirectory : SegmentBase
    {
        /// <summary>
        /// Gets the list of VTable fixups declared
        /// </summary>
        public List<VTableFixup> VTables
        {
            get;
        } = new();

        /// <summary>
        /// Reads the vtable fixup directive from the provided input stream.
        /// </summary>
        /// <param name="file">The original PE file that is currently being parsed.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The VTable Fixup Directory.</returns>
        public static VTableFixupDirectory FromReader(IPEFile file, ref BinaryStreamReader reader)
        {
            var directory = new VTableFixupDirectory
            {
                Rva = reader.Rva,
                Offset = reader.Offset,
            };
            for (int i = 0; i < reader.Length / 8; i++)
            {
                directory.VTables.Add(VTableFixup.FromReader(file, ref reader));
            }
            return directory;
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
