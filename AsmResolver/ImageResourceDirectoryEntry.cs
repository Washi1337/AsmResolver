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
