using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
    public class ImageDebugDirectory : FileSegment
    {
        public static ImageDebugDirectory FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            var directory =  new ImageDebugDirectory()
            {
                StartOffset = reader.StartPosition,
                Characteristics = reader.ReadUInt32(),
                TimeDateStamp = reader.ReadUInt32(),
                MajorVersion = reader.ReadUInt16(),
                MinorVersion = reader.ReadUInt16(),
                Type = (DebugInformationFormat)reader.ReadUInt32(),
                SizeOfData = reader.ReadUInt32(),
                AddressOfRawData = reader.ReadUInt32(),
                PointerToRawData = reader.ReadUInt32(),
            };
            directory._dataReadingContext = context.CreateSubContext(directory.PointerToRawData, (int)directory.SizeOfData);
            return directory;
        }

        private ReadingContext _dataReadingContext;
        private DataSegment _data;

        public uint Characteristics
        {
            get;
            set;
        }

        public uint TimeDateStamp
        {
            get;
            set;
        }

        public ushort MajorVersion
        {
            get;
            set;
        }

        public ushort MinorVersion
        {
            get;
            set;
        }

        public DebugInformationFormat Type
        {
            get;
            set;
        }

        public uint SizeOfData
        {
            get;
            set;
        }

        public uint AddressOfRawData
        {
            get;
            set;
        }

        public uint PointerToRawData
        {
            get;
            set;
        }

        public DataSegment Data
        {
            get
            {
                if (_data != null || _dataReadingContext == null)
                    return _data;

                _data = new DataSegment(_dataReadingContext.Reader.ReadBytes((int)_dataReadingContext.Reader.Length));
                _dataReadingContext = null;
                return _data;
            }
            set
            {
                _data = value;
                _dataReadingContext = null;
            }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (uint) +
                   sizeof (uint) +
                   sizeof (ushort) +
                   sizeof (ushort) +
                   sizeof (uint) +
                   sizeof (uint) +
                   sizeof (uint) +
                   sizeof (uint);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(Characteristics);
            writer.WriteUInt32(TimeDateStamp);
            writer.WriteUInt16(MajorVersion);
            writer.WriteUInt16(MinorVersion);
            writer.WriteUInt32((uint)Type);
            writer.WriteUInt32(SizeOfData);
            writer.WriteUInt32(AddressOfRawData);
            writer.WriteUInt32(PointerToRawData);
        }
    }

    public enum DebugInformationFormat
    {
        Unknown,
        Coff,
        CodeView,
        FramePointerOmission,
        Miscellaneous,
        Exception,
        Fixup,
        Borland,
    }
}
