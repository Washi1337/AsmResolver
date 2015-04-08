using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using Microsoft.Win32;

namespace AsmResolver
{
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

        public uint NameRva
        {
            get;
            set;
        }

        public uint OrdinalBase
        {
            get;
            set;
        }

        public uint NumberOfFunctions
        {
            get;
            set;
        }

        public uint NumberOfNames
        {
            get;
            set;
        }

        public uint AddressOfFunctions
        {
            get;
            set;
        }

        public uint AddressOfNames
        {
            get;
            set;
        }

        public uint AddressOfNameOrdinals
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

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

                var addressReader = reader.CreateSubReader(application.RvaToFileOffset(AddressOfFunctions));

                var nameOrdinalReader = AddressOfNameOrdinals != 0
                    ? reader.CreateSubReader(application.RvaToFileOffset(AddressOfNameOrdinals))
                    : null;

                var nameRvaReader = AddressOfNames != 0
                    ? reader.CreateSubReader(application.RvaToFileOffset(AddressOfNames))
                    : null;
            
                for (int i = 0; i < NumberOfFunctions; i++)
                {
                    var export = new ImageSymbolExport()
                    {
                        Rva = addressReader.ReadUInt32(),
                    };

                    if (i >= NumberOfFunctions - NumberOfNames)
                    {
                        if (nameOrdinalReader != null)
                            export.NameOrdinal = nameOrdinalReader.ReadUInt16();

                        if (nameRvaReader != null)
                        {
                            export.NameRva = nameRvaReader.ReadUInt32();
                            // TODO: set IsForwarder and ForwarderName properties.
                            var nameReader = reader.CreateSubReader(application.RvaToFileOffset(export.NameRva.Value));
                            export.Name = nameReader.ReadAsciiString();
                        }
                    }

                    _exports.Add(export);
                }

                return _exports;
            }
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
