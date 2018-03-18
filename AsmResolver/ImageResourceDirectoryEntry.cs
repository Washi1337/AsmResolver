using System.Text;

namespace AsmResolver
{
    /// <summary>
    /// Represents a resource directory entry in the resource data directory of a windows assembly image.
    /// </summary>
    public class ImageResourceDirectoryEntry : FileSegment
    {
        internal static ImageResourceDirectoryEntry FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            var resourceDirectory =
                context.Assembly.NtHeaders.OptionalHeader.DataDirectories[ImageDataDirectory.ResourceDirectoryIndex];

            var entry = new ImageResourceDirectoryEntry
            {
                StartOffset = reader.Position,
                NameId = reader.ReadUInt32(),
                OffsetToData = reader.ReadUInt32(),
            };
            
            entry.HasData = (entry.OffsetToData >> 31) == 0;

            uint actualDataOffset = entry.OffsetToData & ~(1 << 31);

            entry.HasName = (entry.NameId >> 31) == 1;
            if (entry.HasName)
            {
                entry._nameReadingContext =
                    context.CreateSubContext(context.Assembly.RvaToFileOffset(resourceDirectory.VirtualAddress) +
                                             (entry.NameId & ~(1 << 31)));
            }

            entry._dataReadingContext =
                context.CreateSubContext(context.Assembly.RvaToFileOffset(resourceDirectory.VirtualAddress) +
                                         actualDataOffset);
            return entry;
        }

        private ReadingContext _dataReadingContext;
        private ReadingContext _nameReadingContext;
        private ImageResourceDirectory _subDirectory;
        private ImageResourceDataEntry _dataEntry;
        private string _name;

        public ImageResourceDirectoryEntry()
        {
        }

        public ImageResourceDirectoryEntry(uint nameId)
        {
            NameId = nameId;
        }

        /// <summary>
        /// Gets or sets the ID or the relative virtual address of the name of the resource directory entry.
        /// </summary>
        public uint NameId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating a custom name is used for the resource directory entry.
        /// </summary>
        public bool HasName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets (if available) or sets the name of the resource directory entry.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null && _nameReadingContext != null)
                {
                    ushort length = _nameReadingContext.Reader.ReadUInt16();
                    _name = Encoding.Unicode.GetString(_nameReadingContext.Reader.ReadBytes(length * 2));
                }
                return _name;
            }
            set
            {
                _name = value;
                _nameReadingContext = null;
            }
        }

        /// <summary>
        /// Gets or sets the type of data the resource directory contains.
        /// </summary>
        public ImageResourceDirectoryType ResourceType
        {
            get
            {
                if (HasName)
                    return ImageResourceDirectoryType.Custom;
                return (ImageResourceDirectoryType) NameId;
            }
            set
            {
                if (value == ImageResourceDirectoryType.Custom)
                    HasName = true;
                else
                    NameId = (uint) value;
            }
        }

        /// <summary>
        /// Gets or sets the offset relative to the start of the start of resource data directory to the data.
        /// </summary>
        public uint OffsetToData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the directory entry is supplied with data or not. If <c>True</c>, the <see cref="DataEntry"/> property is non-null. If <c>False</c>, the <see cref="SubDirectory"/> is non-null.
        /// </summary>
        public bool HasData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the sub directory defined by the resource directory entry, or null if the entry specifies a data entry.
        /// </summary>
        public ImageResourceDirectory SubDirectory
        {
            get
            {
                if (!HasData && _subDirectory == null && _dataReadingContext != null)
                {
                    _subDirectory = ImageResourceDirectory.FromReadingContext(_dataReadingContext);
                    _dataReadingContext = null;
                }
                return _subDirectory;
            }
            set
            {
                if ((_subDirectory = value) != null)
                {
                    DataEntry = null;
                    HasData = false;
                }
                _dataReadingContext = null;
            }
        }

        /// <summary>
        /// Gets or sets the data entry defined by the resource directory entry, or null if the entry specifies a sub directory.
        /// </summary>
        public ImageResourceDataEntry DataEntry
        {
            get
            {
                if (HasData && _dataEntry == null && _dataReadingContext != null)
                {
                    _dataEntry = ImageResourceDataEntry.FromReadingContext(_dataReadingContext);
                    _dataReadingContext = null;
                }
                return _dataEntry;
            }
            set
            {
                if ((_dataEntry = value) != null)
                {
                    SubDirectory = null;
                    HasData = true;
                }
            }
        }

        public override uint GetPhysicalLength()
        {
            return 2 * sizeof (uint);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(NameId);
            writer.WriteUInt32(OffsetToData);
        }
    }
}
