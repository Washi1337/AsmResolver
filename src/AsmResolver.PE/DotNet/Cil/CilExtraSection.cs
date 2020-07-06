using System;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Represents a single section that is appended to the end of a CIL method body, containing additional metadata
    /// such as exception handlers.
    /// </summary>
    /// <remarks>
    /// This class does not do any verification on the actual contents of the section.
    /// </remarks>
    public class CilExtraSection : SegmentBase
    {
        /// <summary>
        /// Reads a single extra section from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The extra section that was read.</returns>
        public static CilExtraSection FromReader(IBinaryStreamReader reader)
        {
            var section = new CilExtraSection
            {
                Attributes = (CilExtraSectionAttributes) reader.ReadByte()
            };

            int dataSize;
            if (section.IsFat)
            {
                dataSize = reader.ReadByte() |
                           (reader.ReadByte() << 0x08) |
                           (reader.ReadByte() << 0x10);
            }
            else
            {
                dataSize = reader.ReadByte();
                reader.ReadUInt16();
            }

            section.Data = new byte[dataSize];
            reader.ReadBytes(section.Data, 0, dataSize);
            
            return section;
        }

        private byte[] _data;

        private CilExtraSection()
        {
        }
        
        /// <summary>
        /// Creates a new extra section that can be appended to a method body.
        /// </summary>
        /// <param name="attributes">The attributes associated to this section.</param>
        /// <param name="data">The raw contents of the section.</param>
        public CilExtraSection(CilExtraSectionAttributes attributes, byte[] data)
        {
            Attributes = attributes;
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
        
        /// <summary>
        /// Gets or sets the attributes associated to this section.
        /// </summary>
        /// <remarks>
        /// This property does not update automatically if more sections are added to- or removed from the enclosing
        /// method body. 
        /// </remarks>
        public CilExtraSectionAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the section contains an exception handler table.
        /// </summary>
        public bool IsEHTable
        {
            get => (Attributes & CilExtraSectionAttributes.EHTable) == CilExtraSectionAttributes.EHTable;
            set => Attributes = (Attributes & ~CilExtraSectionAttributes.EHTable)
                                | (value ? CilExtraSectionAttributes.EHTable : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the sectio contains an OptIL table.
        /// </summary>
        public bool IsOptILTable
        {
            get => (Attributes & CilExtraSectionAttributes.OptILTable) == CilExtraSectionAttributes.OptILTable;
            set => Attributes = (Attributes & ~CilExtraSectionAttributes.OptILTable)
                                | (value ? CilExtraSectionAttributes.OptILTable : 0);
        }
        
        /// <summary>
        /// Gets or sets a value indicating the data stored in the section is using the fat format.
        /// </summary>
        public bool IsFat
        {
            get => (Attributes & CilExtraSectionAttributes.FatFormat) == CilExtraSectionAttributes.FatFormat;
            set => Attributes = (Attributes & ~CilExtraSectionAttributes.FatFormat)
                                | (value ? CilExtraSectionAttributes.FatFormat : 0);
        }
        
        /// <summary>
        /// Gets or sets a value indicating there is at least one more section following this section.
        /// </summary>
        /// <remarks>
        /// This property does not update automatically if more sections are added to- or removed from the enclosing
        /// method body. 
        /// </remarks>
        public bool HasMoreSections
        {
            get => (Attributes & CilExtraSectionAttributes.MoreSections) == CilExtraSectionAttributes.MoreSections;
            set => Attributes = (Attributes & ~CilExtraSectionAttributes.MoreSections)
                                | (value ? CilExtraSectionAttributes.MoreSections : 0);
        }

        /// <summary>
        /// Gets or sets the actual contents of the section.
        /// </summary>
        public byte[] Data
        {
            get => _data;
            set => _data = value ?? throw new ArgumentNullException(nameof(value));
        }
 
        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint) Data.Length + 4;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte) Attributes);
        
            if (IsFat)
            {
                writer.WriteByte((byte)(Data.Length & 0xFF));
                writer.WriteByte((byte)((Data.Length & 0xFF00) >> 0x08));
                writer.WriteByte((byte)((Data.Length & 0xFF0000) >> 0x10));
            }
            else
            {
                writer.WriteByte((byte) Data.Length);
                writer.WriteUInt16(0);
            }
            
            writer.WriteBytes(Data);
        }
        
    }
}