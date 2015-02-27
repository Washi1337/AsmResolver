using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;
using AsmResolver;

namespace AsmResolver
{
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

            var usesName = (entry.NameId >> 31) == 1; // TODO: get/set resource name
            entry.HasData = (entry.OffsetToData >> 31) == 0;

            var actualDataOffset = entry.OffsetToData & ~(1 << 31);

            entry._dataReadingContext =
                context.CreateSubContext(context.Assembly.RvaToFileOffset(resourceDirectory.VirtualAddress) +
                                         actualDataOffset);
            return entry;
        }

        private ReadingContext _dataReadingContext;
        private ImageResourceDirectory _subDirectory;
        private ImageResourceDataEntry _dataEntry;

        private ImageResourceDirectoryEntry()
        {
        }

        public ImageResourceDirectoryEntry(uint nameId)
        {
            NameId = nameId;
        }

        public uint NameId
        {
            get;
            set;
        }

        public bool HasName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public uint OffsetToData
        {
            get;
            set;
        }

        public bool HasData
        {
            get;
            set;
        }

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
