using System;

namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Represents a single extra section that appears after a fat CIL method body.
    /// </summary>
    public class CilExtraSection : FileSegment
    {
        /// <summary>
        /// Reads an extra section from the given input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The read extra section.</returns>
        /// <exception cref="NotSupportedException">Occurs when the header does not indicate a valid or supported
        /// section format.</exception>
        public static CilExtraSection FromReader(IBinaryStreamReader reader)
        {
            var section = new CilExtraSection
            {
                Attributes = (CilExtraSectionAttributes) reader.ReadByte()
            };

            if (!section.IsExceptionHandler)
                throw new NotSupportedException("Invalid or unsupported extra section.");

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
            section.Data = reader.ReadBytes(dataSize);
            
            return section;
        }

        /// <summary>
        /// Gets or sets the attributes of the extra section.
        /// </summary>
        public CilExtraSectionAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the extra section contains exception handler information.
        /// </summary>
        public bool IsExceptionHandler
        {
            get { return Attributes.HasFlag(CilExtraSectionAttributes.ExceptionHandler); }
            set { Attributes = Attributes.SetFlag(CilExtraSectionAttributes.ExceptionHandler, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this section is succeeded by another extra section or not.
        /// </summary>
        public bool HasMoreSections
        {
            get { return Attributes.HasFlag(CilExtraSectionAttributes.HasMoreSections); }
            set { Attributes = Attributes.SetFlag(CilExtraSectionAttributes.HasMoreSections, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating the section uses the fat format for its contents.
        /// </summary>
        public bool IsFat
        {
            get { return Attributes.HasFlag(CilExtraSectionAttributes.FatFormat); }
            set { Attributes = Attributes.SetFlag(CilExtraSectionAttributes.FatFormat, value); }
        }

        /// <summary>
        /// Gets or sets the raw data in the section.
        /// </summary>
        public byte[] Data
        {
            get;
            set;
        }
        
        public override uint GetPhysicalLength()
        {
            return (uint) Data.Length + 4;
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
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