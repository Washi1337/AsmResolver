using System.Collections.ObjectModel;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.VTableFixups
{
    /// <summary>
    /// Represents the segment of the metadata tokens of a VTable.
    /// </summary>
    public class VTableTokenCollection : Collection<MetadataToken>, ISegment
    {
        /// <summary>
        /// Gets or sets the type of the entries
        /// </summary>
        public VTableType Type
        {
            get;
            set;
        }

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
        }

        /// <inheritdoc />
        public uint GetPhysicalSize() =>
            (uint) Count *
            (uint) (Type.HasFlag(VTableType.VTable32Bit)
                ? 4
                : 8);

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            foreach (var token in Items)
            {
                if ((Type & VTableType.VTable32Bit) != 0)
                    writer.WriteUInt32(token.ToUInt32());
                else
                    writer.WriteUInt64(token.ToUInt32());
            }
        }

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();
    }
}
