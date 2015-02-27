using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
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

        public uint ImportLookupTableRva
        {
            get;
            set;
        }

        public uint TimeDateStamp
        {
            get;
            set;
        }

        public uint ForwarderChain
        {
            get;
            set;
        }

        public uint NameRva
        {
            get;
            set;
        }

        public uint ImportAddressTableRva
        {
            get;
            set;
        }

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

        public string Name
        {
            get;
            set;
        }

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
