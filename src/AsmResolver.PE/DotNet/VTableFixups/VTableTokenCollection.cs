using System;
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
        protected override void InsertItem(int index, MetadataToken item)
        {
            if (Count >= 0xFFFF)
                throw new InvalidOperationException("Number of VTable tokens exceeds the maximum of 65535.");
            base.InsertItem(index, item);
        }

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
            for (int i = 0; i < Items.Count; i++)
            {
                var token = Items[i];
                if ((Type & VTableType.VTable32Bit) != 0)
                    writer.WriteUInt32(token.ToUInt32());
                else
                    writer.WriteUInt64(token.ToUInt32());
            }
        }

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();

        /// <summary>
        /// Constructs a reference to an element within the collection.
        /// </summary>
        /// <param name="index">The index of the element to reference.</param>
        /// <returns>The reference.</returns>
        public ISegmentReference GetReferenceToIndex(int index) => this.ToReference((int) GetOffsetToIndex(index));

        /// <summary>
        /// Gets the byte offset to an element within the collection that is relative to the start of the list.
        /// </summary>
        /// <param name="index">The index of the element to reference.</param>
        /// <returns>The offset.</returns>
        public uint GetOffsetToIndex(int index)
        {
            int entrySize = (Type & VTableType.VTable32Bit) != 0
                ? sizeof(uint)
                : sizeof(ulong);

            return (uint) (index * entrySize);
        }
    }
}
