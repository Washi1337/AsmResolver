using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
    public class ImageNtHeaders : FileSegment
    {
        private ImageOptionalHeader _optionalHeader;
        private ImageFileHeader _fileHeader;

        internal static ImageNtHeaders FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;

            return new ImageNtHeaders
            {
                StartOffset = reader.Position,
                Signature = reader.ReadUInt32(),
                FileHeader = ImageFileHeader.FromReadingContext(context),
                OptionalHeader = ImageOptionalHeader.FromReadingContext(context),
            };
        }

        public uint Signature
        {
            get;
            set;
        }

        public ImageFileHeader FileHeader
        {
            get { return _fileHeader ?? (_fileHeader = new ImageFileHeader()); }
            private set { _fileHeader = value; }
        }

        public ImageOptionalHeader OptionalHeader
        {
            get { return _optionalHeader ?? (_optionalHeader = new ImageOptionalHeader()); }
            private set { _optionalHeader = value; }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (uint) + FileHeader.GetPhysicalLength() + OptionalHeader.GetPhysicalLength();
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteUInt32(Signature);
            FileHeader.Write(context);
            OptionalHeader.Write(context);
        }
    }
}
