using System;

namespace AsmResolver
{
    /// <summary>
    /// Represents a module in the import data directory of a windows assembly image.
    /// </summary>
    public class ImageModuleImport : FileSegment
    {
        internal static ImageModuleImport FromReadingContext(ReadingContext context)
        {
            var application = context.Assembly;
            var reader = context.Reader;
            var moduleImport = new ImageModuleImport
            {
                StartOffset = reader.Position,
                ImportLookupTableRva = reader.ReadUInt32(),
                TimeDateStamp = reader.ReadUInt32(),
                ForwarderChain = reader.ReadUInt32(),
                NameRva = reader.ReadUInt32(),
                ImportAddressTableRva = reader.ReadUInt32(),
            };

            if (moduleImport.IsEmpty)
                return moduleImport;

            var nameReader = reader.CreateSubReader(application.RvaToFileOffset(moduleImport.NameRva));
            moduleImport.Name = nameReader.ReadAsciiString();

            moduleImport._readingContext =
                context.CreateSubContext(application.RvaToFileOffset(moduleImport.ImportLookupTableRva));

            return moduleImport;
        }

        private ImageSymbolImportCollection _imports;
        private ReadingContext _readingContext;

        public ImageModuleImport()
        {
        }

        public ImageModuleImport(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the relative virtual address (RVA) of the import lookup table.
        /// </summary>
        public uint ImportLookupTableRva
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the raw time stamp
        /// </summary>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the first forwarder reference.
        /// </summary>
        public uint ForwarderChain
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the relative virtual address of the name.
        /// </summary>
        public uint NameRva
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the relative virtual address of the import address table.
        /// </summary>
        public uint ImportAddressTableRva
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the module import is empty and is therefore used as a stop delimeter in the import directory.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return ImportAddressTableRva == 0 && 
                    TimeDateStamp == 0 &&
                    ForwarderChain == 0 && 
                    NameRva == 0 &&
                    ImportAddressTableRva == 0;
            }
        }

        /// <summary>
        /// Gets the symbols defined in the module that are being imported by the image.
        /// </summary>
        public ImageSymbolImportCollection SymbolImports
        {
            get
            {
                if (_imports != null)
                    return _imports;

                _imports = new ImageSymbolImportCollection(this);

                if (_readingContext == null)
                    return _imports;

                while (true)
                {
                    var import = ImageSymbolImport.FromReadingContext(_readingContext);
                    if (import.Lookup == 0)
                        break;
                    _imports.Add(import);
                }

                return _imports;
            }
        }

        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Determines the import address to use for a specific function.
        /// </summary>
        /// <param name="import">The imported symbol to get the address from.</param>
        /// <param name="is32Bit">Specifies whether the address should be a 32-bit or 64-bit address.</param>
        /// <returns></returns>
        public ulong GetSymbolImportAddress(ImageSymbolImport import, bool is32Bit)
        {
            int index = SymbolImports.IndexOf(import);
            if (index == -1)
                throw new ArgumentException("Symbol is not present in the module import.", "import");
            return (ulong)(ImportAddressTableRva + index * (is32Bit ? sizeof (uint) : sizeof (ulong)));
        }

        public override uint GetPhysicalLength()
        {
            return 5 * sizeof (uint);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(ImportLookupTableRva);
            writer.WriteUInt32(TimeDateStamp);
            writer.WriteUInt32(ForwarderChain);
            writer.WriteUInt32(NameRva);
            writer.WriteUInt32(ImportAddressTableRva);
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, SymbolImports Count: {1}", Name, SymbolImports.Count);
        }
    }
}
