using System;
using System.Collections.ObjectModel;
using AsmResolver.IO;

namespace AsmResolver.Collections
{
    /// <summary>
    /// Represents a table of references that can be written as one block of contiguous bytes.
    /// </summary>
    public class ReferenceTable : Collection<ISegmentReference>, ISegment
    {
        private ulong _imageBase;
        private bool _is32Bit;

        /// <summary>
        /// Creates a new reference table.
        /// </summary>
        /// <param name="attributes">The attributes describing the shape of the table.</param>
        public ReferenceTable(ReferenceTableAttributes attributes)
        {
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the attributes describing the shape of the table.
        /// </summary>
        public ReferenceTableAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets the type of reference that is stored in the table.
        /// </summary>
        public ReferenceTableAttributes ReferenceType => Attributes & ReferenceTableAttributes.ReferenceTypeMask;

        /// <summary>
        /// Gets a value indicating whether the table contains offsets.
        /// </summary>
        public bool IsOffsetTable => ReferenceType == ReferenceTableAttributes.Offset;

        /// <summary>
        /// Gets a value indicating whether the table contains RVAs.
        /// </summary>
        public bool IsRvaTable => ReferenceType == ReferenceTableAttributes.Rva;

        /// <summary>
        /// Gets a value indicating whether the table contains VAs.
        /// </summary>
        public bool IsVaTable => ReferenceType == ReferenceTableAttributes.Va;

        /// <summary>
        /// Gets the size in bytes that each reference occupies in the table.
        /// </summary>
        public ReferenceTableAttributes ReferenceSize => Attributes & ReferenceTableAttributes.SizeMask;

        /// <summary>
        /// Gets a value indicating whether the size of a single reference changes depending on whether
        /// this table is put in a 32 or 64 bit application.
        /// </summary>
        public bool IsAdaptive => ReferenceSize == ReferenceTableAttributes.Adaptive;

        /// <summary>
        /// Gets a value indicating whether the size of a single reference in the table is 32 bits.
        /// </summary>
        public bool Is32BitTable => ReferenceSize == ReferenceTableAttributes.Force32Bit
                                    || IsAdaptive && _is32Bit;

        /// <summary>
        /// Gets a value indicating whether the size of a single reference in the table is 64 bits.
        /// </summary>
        public bool Is64BitTable => ReferenceSize == ReferenceTableAttributes.Force64Bit
                                    || IsAdaptive && !_is32Bit;

        /// <summary>
        /// Gets a value indicating whether the table ends with a zero entry.
        /// </summary>
        public bool IsZeroTerminated => (Attributes & ReferenceTableAttributes.ZeroTerminated) != 0;

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
        public void UpdateOffsets(in RelocationParameters parameters)
        {
            _imageBase = parameters.ImageBase;
            _is32Bit = parameters.Is32Bit;
            Offset = parameters.Offset;
            Rva = parameters.Rva;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            int actualCount = Count + (IsZeroTerminated ? 1 : 0);
            return (uint) (actualCount * (_is32Bit ? sizeof(uint) : sizeof(ulong)));
        }

        /// <inheritdoc />
        public uint GetVirtualSize() => GetPhysicalSize();

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];

                ulong value = ReferenceType switch
                {
                    ReferenceTableAttributes.Offset => item.Offset,
                    ReferenceTableAttributes.Rva => item.Rva,
                    ReferenceTableAttributes.Va => item.Rva + _imageBase,
                    _ => throw new ArgumentOutOfRangeException()
                };

                writer.WriteNativeInt(value, Is32BitTable);
            }

            if (IsZeroTerminated)
                writer.WriteNativeInt(0, Is32BitTable);
        }
    }
}
