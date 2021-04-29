using System.Collections.Generic;
using System.Collections.ObjectModel;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.VTableFixups
{
    /// <summary>
    /// Represents the VTable Fixup Directory in the Cor20 header.
    /// </summary>
    public class VTableFixupDirectory : Collection<VTableFixup>, ISegment
    {
        private readonly Dictionary<VTableFixup, uint> _tableRvas = new();

        /// <inheritdoc />
        public ulong Offset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public uint Rva
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <inheritdoc />
        public void UpdateOffsets(ulong newOffset, uint newRva)
        {
            Offset = newOffset;
            Rva = newRva;
            uint endRvaForDirectory = newRva + GetPhysicalSize();
            foreach (var vTable in Items)
            {
                _tableRvas[vTable] = endRvaForDirectory;
                endRvaForDirectory += vTable.GetEntriesSize();
            }
        }

        /// <inheritdoc />
        public uint GetPhysicalSize() =>
            (uint) Items.Count *
            (sizeof(uint) //rva
             + sizeof(ushort) //entries
             + sizeof(ushort)); //type

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            foreach (var vTable in Items)
            {
                writer.WriteUInt32(_tableRvas[vTable]);
                writer.WriteUInt16((ushort) vTable.Tokens.Count);
                writer.WriteUInt16((ushort) vTable.Type);
            }
        }

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();
    }
}
