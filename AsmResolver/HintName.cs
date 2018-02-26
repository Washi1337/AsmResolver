using System;
using System.Text;

namespace AsmResolver
{
    /// <summary>
    /// Represents a hint-name pair defined in the import data directory.
    /// </summary>
    public class HintName : FileSegment
    {
        internal static HintName FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            var hintName = new HintName
            {
                StartOffset = reader.Position,
                Hint = reader.ReadUInt16(),
                Name = reader.ReadAsciiString(),
            };

            if (reader.Position % 2 != 0)
                reader.Position++;

            return hintName;
        }

        private HintName()
        {
        }

        public HintName(ushort hint, string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            Hint = hint;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the hint number of the symbol.
        /// </summary>
        public ushort Hint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the symbol.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            var size = (uint)(sizeof (ushort) + Encoding.ASCII.GetByteCount(Name) + 1);
            if ((StartOffset + size) % 2 != 0)
                size++;
            return size;
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt16(Hint);
            writer.WriteBytes(Encoding.ASCII.GetBytes(Name));
            writer.WriteByte(0);
            if (writer.Position % 2 != 0)
                writer.WriteByte(0);
        }

        public override string ToString()
        {
            return string.Format("Hint: {0:X}, Name: {1}", Hint, Name);
        }
    }
}
