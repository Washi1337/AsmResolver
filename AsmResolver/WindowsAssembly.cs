using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AsmResolver.Builder;
using AsmResolver.Net;
using AsmResolver.Net.Builder;

namespace AsmResolver
{
    public class WindowsAssembly : IOffsetConverter
    {
        public static WindowsAssembly FromBytes(byte[] bytes, ReadingParameters parameters)
        {
            return FromReader(new MemoryStreamReader(bytes), parameters);
        }

        public static WindowsAssembly FromReader(IBinaryStreamReader stream, ReadingParameters parameters)
        {
            return FromReadingContext(new ReadingContext()
            {
                Reader = stream,
                Parameters = parameters,
            });
        }

        internal static WindowsAssembly FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;

            var application = new WindowsAssembly();
            context.Assembly = application;
            application.ReadingContext = context;

            // Read absolute essential parts of PE:
            // - DOS header
            // - NT headers
            // - Section headers

            application._dosHeader = ImageDosHeader.FromReadingContext(context);

            reader.Position = application.DosHeader.Lfanew;
            application._ntHeaders = ImageNtHeaders.FromReadingContext(context);

            reader.Position =
                application.NtHeaders.OptionalHeader.StartOffset +
                application.NtHeaders.FileHeader.SizeOfOptionalHeader;
            for (int i = 0; i < application.NtHeaders.FileHeader.NumberOfSections; i++)
                application.SectionHeaders.Add(ImageSectionHeader.FromReadingContext(context));

            return application;
        }
        
        private static ReadingContext CreateDataDirectoryContext(ReadingContext context, int directoryIndex)
        {
            var application = context.Assembly;
            var dataDirectory = application.NtHeaders.OptionalHeader.DataDirectories[directoryIndex];

            return dataDirectory.VirtualAddress == 0
                ? null
                : context.CreateSubContext(application.RvaToFileOffset(dataDirectory.VirtualAddress));
        }

        private ImageExportDirectory _exportDirectory;
        private ImageResourceDirectory _resourceDirectory;
        private ImageNetDirectory _netDirectory;
        private ImageImportDirectory _importDirectory;
        private ImageRelocationDirectory _relocDirectory;
        private ImageDosHeader _dosHeader;
        private ImageNtHeaders _ntHeaders;

        public WindowsAssembly()
        {
            SectionHeaders = new List<ImageSectionHeader>();
        }

        public ReadingContext ReadingContext
        {
            get;
            private set;
        }

        public ImageDosHeader DosHeader
        {
            get { return _dosHeader ?? (_dosHeader = new ImageDosHeader()); }
        }

        public ImageNtHeaders NtHeaders
        {
            get { return _ntHeaders ?? (_ntHeaders = new ImageNtHeaders()); }
        }

        public IList<ImageSectionHeader> SectionHeaders
        {
            get;
            private set;
        }

        public ImageImportDirectory ImportDirectory
        {
            get
            {
                if (_importDirectory != null)
                    return _importDirectory;

                if (ReadingContext != null)
                {
                    var context = CreateDataDirectoryContext(ReadingContext, ImageDataDirectory.ImportDirectoryIndex);
                    if (context != null)
                        return _importDirectory = ImageImportDirectory.FromReadingContext(context);
                }

                return _importDirectory = new ImageImportDirectory();
            }
        }

        public ImageExportDirectory ExportDirectory
        {
            get
            {
                if (_exportDirectory != null)
                    return _exportDirectory;

                if (ReadingContext != null)
                {
                    var context = CreateDataDirectoryContext(ReadingContext, ImageDataDirectory.ExportDirectoryIndex);
                    if (context != null)
                        return _exportDirectory = ImageExportDirectory.FromReadingContext(context);
                }

                return null;
            }
            set { _exportDirectory = value; }
        }

        public ImageRelocationDirectory RelocationDirectory
        {
            get
            {
                if (_relocDirectory != null)
                    return _relocDirectory;

                if (ReadingContext != null)
                {
                    var context = CreateDataDirectoryContext(ReadingContext, ImageDataDirectory.BaseRelocationDirectoryIndex);
                    if (context != null)
                        return _relocDirectory = ImageRelocationDirectory.FromReadingContext(context);
                }

                return null;
            }
            set { _relocDirectory = value; }
        }

        public ImageResourceDirectory RootResourceDirectory
        {
            get
            {
                if (_resourceDirectory != null)
                    return _resourceDirectory;

                if (ReadingContext != null)
                {
                    var context = CreateDataDirectoryContext(ReadingContext, ImageDataDirectory.ResourceDirectoryIndex);
                    if (context != null)
                        return _resourceDirectory = ImageResourceDirectory.FromReadingContext(context);
                }

                return null;
            }
            set { _resourceDirectory = value; }
        }

        public ImageNetDirectory NetDirectory
        {
            get
            {
                if (_netDirectory != null)
                    return _netDirectory;

                if (ReadingContext != null)
                {
                    var context = CreateDataDirectoryContext(ReadingContext, ImageDataDirectory.ClrDirectoryIndex);
                    if (context != null)
                    {
                        _netDirectory = ImageNetDirectory.FromReadingContext(context);
                        _netDirectory.Assembly = this;
                        return _netDirectory;
                    }
                }

                return null;
            }
            set
            {
                if (_netDirectory != null)
                    _netDirectory.Assembly = null;
                _netDirectory = value;
                if (value != null)
                    _netDirectory.Assembly = this;
            }
        }

        public long RvaToFileOffset(long rva)
        {
            var section = GetSectionHeaderByRva(rva);
            if (section == null)
                throw new ArgumentOutOfRangeException("rva");
            return section.RvaToFileOffset(rva);
        }

        public long FileOffsetToRva(long fileOffset)
        {
            var section = GetSectionHeaderByFileOffset(fileOffset);
            if (section == null)
                throw new ArgumentOutOfRangeException("fileOffset");
            return section.FileOffsetToRva(fileOffset);
        }

        public ImageSectionHeader GetSectionHeaderByRva(long rva)
        {
            return
                SectionHeaders.FirstOrDefault(
                    sectionHeader =>
                        rva >= sectionHeader.VirtualAddress &&
                        rva < sectionHeader.VirtualAddress + sectionHeader.VirtualSize);
        }

        public ImageSectionHeader GetSectionHeaderByFileOffset(long fileOffset)
        {
            return
                SectionHeaders.FirstOrDefault(
                    sectionHeader =>
                        fileOffset >= sectionHeader.PointerToRawData &&
                        fileOffset < sectionHeader.PointerToRawData + sectionHeader.SizeOfRawData);
        }

        public void Write(BuildingParameters parameters)
        {
            var builder = new NetAssemblyBuilder(this, parameters);
            var context = new NetBuildingContext(builder);
            builder.Build(context);
            builder.UpdateOffsets(context);
            builder.UpdateReferences(context);
            builder.Write(new WritingContext(this, parameters.Writer, context));
        }
    }
}
