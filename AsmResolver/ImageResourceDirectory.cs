using System.Collections.Generic;
using System.Linq;

namespace AsmResolver
{
    /// <summary>
    /// Represents a directory in the native resources data directory of a windows assembly image.
    /// </summary>
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

        /// <summary>
        /// Reserved.
        /// </summary>
        public uint Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the raw time stamp the resource data was created by the resource compiler.
        /// </summary>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version of the resource.
        /// </summary>
        public ushort MajorVersion
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the minor version of the resource.
        /// </summary>
        public ushort MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the number of named entries defined in the directory.
        /// </summary>
        public ushort NamedEntriesCount
        {
            get { return (ushort)Entries.Count(x => x.HasName); }
        }

        /// <summary>
        /// Gets the number of non-named entries defined in the directory.
        /// </summary>
        public ushort IdEntriesCount
        {
            get { return (ushort)Entries.Count(x => !x.HasName); }
        }

        /// <summary>
        /// Gets the entries defined in the directory.
        /// </summary>
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
