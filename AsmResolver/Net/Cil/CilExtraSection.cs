using System;

namespace AsmResolver.Net.Cil
{
    public class CilExtraSection : FileSegment
    {
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

        public CilExtraSectionAttributes Attributes
        {
            get;
            set;
        }

        public bool IsExceptionHandler
        {
            get { return Attributes.HasFlag(CilExtraSectionAttributes.ExceptionHandler); }
            set { Attributes = Attributes.SetFlag(CilExtraSectionAttributes.ExceptionHandler, value); }
        }

        public bool HasMoreSections
        {
            get { return Attributes.HasFlag(CilExtraSectionAttributes.HasMoreSections); }
            set { Attributes = Attributes.SetFlag(CilExtraSectionAttributes.HasMoreSections, value); }
        }

        public bool IsFat
        {
            get { return Attributes.HasFlag(CilExtraSectionAttributes.FatFormat); }
            set { Attributes = Attributes.SetFlag(CilExtraSectionAttributes.FatFormat, value); }
        }

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