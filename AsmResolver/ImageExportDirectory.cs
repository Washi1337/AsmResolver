using System.Collections.Generic;

namespace AsmResolver
{
    /// <summary>
    /// Represents an export directory in a windows assembly image.
    /// </summary>
    public class ImageExportDirectory : FileSegment
    {
        internal static ImageExportDirectory FromReadingContext(ReadingContext context)
        {
            var application = context.Assembly;
            var reader = context.Reader;

            var directory = new ImageExportDirectory
            {
                _readingContext = context,

                StartOffset = reader.Position,

                Characteristics = reader.ReadUInt32(),
                TimeDateStamp = reader.ReadUInt32(),
                MajorVersion = reader.ReadUInt16(),
                MinorVersion = reader.ReadUInt16(),
                NameRva = reader.ReadUInt32(),
                OrdinalBase = reader.ReadUInt32(),
                NumberOfFunctions = reader.ReadUInt32(),
                NumberOfNames = reader.ReadUInt32(),
                AddressOfFunctions = reader.ReadUInt32(),
                AddressOfNames = reader.ReadUInt32(),
                AddressOfNameOrdinals = reader.ReadUInt32(),
            };

            var nameReader = reader.CreateSubReader(application.RvaToFileOffset(directory.NameRva));
            directory.Name = nameReader.ReadAsciiString();

            return directory;
        }

        private ReadingContext _readingContext;
        private List<ImageSymbolExport> _exports;
        
        /// <summary>
        /// Gets or sets the characteristics of the export directory. Currently not used.
        /// </summary>
        public uint Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the raw time stamp value of the export directory.
        /// </summary>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version of the export directory.
        /// </summary>
        public ushort MajorVersion
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the minor version of the export directory.
        /// </summary>
        public ushort MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the relative virtual address to the name of the export directory.
        /// </summary>
        public uint NameRva
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ordinal base of the export directory.
        /// </summary>
        public uint OrdinalBase
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of functions defined in the export directory.
        /// </summary>
        public uint NumberOfFunctions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of named functions defined in the export directory.
        /// </summary>
        public uint NumberOfNames
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the relative virtual address of the functions table.
        /// </summary>
        public uint AddressOfFunctions
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the relative virtual address of the function names table.
        /// </summary>
        public uint AddressOfNames
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the relative virtual address of the name ordinals table.
        /// </summary>
        public uint AddressOfNameOrdinals
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the name of the export direcotry.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the symbol exports the windows assembly image defines.
        /// </summary>
        public IList<ImageSymbolExport> Exports
        {
            get
            {
                if (_exports != null)
                    return _exports;

                _exports = new List<ImageSymbolExport>();

                if (_readingContext == null)
                    return _exports;

                var application = _readingContext.Assembly;
                var reader = _readingContext.Reader;


                var addresses = ReadAddresses(reader, application);
                var ordinalRvaTable = ReadOrdinalRvaTable(reader, application);

                for (ushort i = 0; i < addresses.Length; i++)
                {
                    var export = new ImageSymbolExport()
                    {
                        Rva = addresses[i],
                        NameOrdinal = i
                    };

                    uint rva;
                    if (ordinalRvaTable.TryGetValue(i, out rva))
                    {
                        export.NameRva = rva;
                        var nameReader = reader.CreateSubReader(application.RvaToFileOffset(export.NameRva.Value));
                        export.Name = nameReader.ReadAsciiString();
                    }

                    // TODO: set IsForwarder and ForwarderName properties.
                    _exports.Add(export);
                }

                return _exports;
            }
        }

        private Dictionary<ushort, uint> ReadOrdinalRvaTable(IBinaryStreamReader reader, WindowsAssembly application)
        {
            var dictionary = new Dictionary<ushort, uint>();
            if (NumberOfNames > 0)
            {
                var nameOrdinalReader = reader.CreateSubReader(application.RvaToFileOffset(AddressOfNameOrdinals));
                var nameRvaReader = reader.CreateSubReader(application.RvaToFileOffset(AddressOfNames));

                for (int i = 0; i < NumberOfNames; i++)
                    dictionary.Add(nameOrdinalReader.ReadUInt16(), nameRvaReader.ReadUInt32());
            }
            return dictionary;
        }

        private uint[] ReadAddresses(IBinaryStreamReader reader, WindowsAssembly application)
        {
            var addressReader = reader.CreateSubReader(application.RvaToFileOffset(AddressOfFunctions));
            var addresses = new uint[NumberOfFunctions];

            for (int i = 0; i < NumberOfFunctions; i++)
            {
                addresses[i] = addressReader.ReadUInt32();
            }
            return addresses;
        }

        public override uint GetPhysicalLength()
        {
            return 2 * sizeof (uint) +
                   2 * sizeof (ushort) +
                   7 * sizeof (uint);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(Characteristics);
            writer.WriteUInt32(TimeDateStamp);
            writer.WriteUInt16(MajorVersion);
            writer.WriteUInt16(MinorVersion);
            writer.WriteUInt32(NameRva);
            writer.WriteUInt32(OrdinalBase);
            writer.WriteUInt32(NumberOfFunctions);
            writer.WriteUInt32(NumberOfNames);
            writer.WriteUInt32(AddressOfFunctions);
            writer.WriteUInt32(AddressOfNames);
            writer.WriteUInt32(AddressOfNameOrdinals);
        }
    }
}
