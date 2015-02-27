using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
    public class ImageResourceDirectory : FileSegment
    {
        internal static ImageResourceDirectory FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;

            var directory = new ImageResourceDirectory
            {
                StartOffset = reader.Position,
                Characteristics = reader.ReadUInt32(),
                TimeDateStamp = reader.ReadUInt32(),
                MajorVersion = reader.ReadUInt16(),
                MinorVersion = reader.ReadUInt16(),
            };


            var numberOfNamedEntries = reader.ReadUInt16();
            var numberOfIdEntries = reader.ReadUInt16();

            for (int i = 0; i < numberOfNamedEntries; i++)
                directory.Entries.Add(ImageResourceDirectoryEntry.FromReadingContext(context));

            for (int i = 0; i < numberOfIdEntries; i++)
                directory.Entries.Add(ImageResourceDirectoryEntry.FromReadingContext(context));

            return directory;
        }

        public ImageResourceDirectory()
        {
            Entries = new List<ImageResourceDirectoryEntry>();
        }

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

        public ushort NamedEntriesCount
        {
            get { return (ushort)Entries.Count(x => x.HasName); }
        }

        public ushort IdEntriesCount
        {
            get { return (ushort)Entries.Count(x => !x.HasName); }
        }

        public IList<ImageResourceDirectoryEntry> Entries
        {
            get;
            private set;
        }

        public override uint GetPhysicalLength()
        {
            return (uint)(2 * sizeof (uint) +
                          4 * sizeof (ushort) +
                          Entries.Count * 2 * sizeof (uint));
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(Characteristics);
            writer.WriteUInt32(TimeDateStamp);
            writer.WriteUInt16(MajorVersion);
            writer.WriteUInt16(MinorVersion);
            writer.WriteUInt16(NamedEntriesCount);
            writer.WriteUInt16(IdEntriesCount);

            foreach (var entry in Entries)
                entry.Write(context);
        }
    }
}
