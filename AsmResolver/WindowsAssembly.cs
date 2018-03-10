using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.Emit;
using AsmResolver.Net;

namespace AsmResolver
{
    /// <summary>
    /// Represents a windows assembly image.
    /// </summary>
    public class WindowsAssembly : IOffsetConverter
    {
        /// <summary>
        /// Reads a windows assembly image from a file.
        /// </summary>
        /// <param name="file">The path of the file to read.</param>
        /// <returns>The assembly representing the executable file.</returns>
        public static WindowsAssembly FromFile(string file)
        {
            return FromBytes(File.ReadAllBytes(file), new ReadingParameters());
        }

        /// <summary>
        /// Reads a windows assembly image from the given byte array.
        /// </summary>
        /// <param name="bytes">The bytes to read the assembly from.</param>
        /// <returns>The assembly representing the executable.</returns>
        public static WindowsAssembly FromBytes(byte[] bytes)
        {
            return FromBytes(bytes, new ReadingParameters());
        }

        /// <summary>
        /// Reads a windows assembly image from the given byte array, using the specified reading parameters.
        /// </summary>
        /// <param name="bytes">The bytes to read the assembly from.</param>
        /// <param name="parameters">The extra parameters the reading procedure should use to read the assembly.</param>
        /// <returns>The assembly representing the executable.</returns>
        public static WindowsAssembly FromBytes(byte[] bytes, ReadingParameters parameters)
        {
            return FromReader(new MemoryStreamReader(bytes), parameters);
        }

        /// <summary>
        /// Reads a windows assembly image from a binary stream.
        /// </summary>
        /// <param name="stream">The stream reader to use for reading the assembly.</param>
        /// <returns>The assembly representing the executable.</returns>
        public static WindowsAssembly FromReader(IBinaryStreamReader stream)
        {
            return FromReader(stream, new ReadingParameters());
        }

        /// <summary>
        /// Reads a windows assembly image from a binary stream, using the specified reading parameters.
        /// </summary>
        /// <param name="stream">The stream reader to use for reading the assembly.</param>
        /// <param name="parameters">The extra parameters the reading procedure should use to read the assembly.</param>
        /// <returns>The assembly representing the executable.</returns>
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
            var dataDirectories = application.NtHeaders.OptionalHeader.DataDirectories;

            if (directoryIndex >= 0 && directoryIndex < dataDirectories.Count)
            {
                var dataDirectory = dataDirectories[directoryIndex];

                if (dataDirectory.VirtualAddress != 0)
                    return context.CreateSubContext(application.RvaToFileOffset(dataDirectory.VirtualAddress));
            }

            return null;
        }

        private ImageExportDirectory _exportDirectory;
        private ImageResourceDirectory _resourceDirectory;
        private ImageNetDirectory _netDirectory;
        private ImageImportDirectory _importDirectory;
        private ImageRelocationDirectory _relocDirectory;
        private ImageDosHeader _dosHeader;
        private ImageNtHeaders _ntHeaders;
        private ImageDebugDirectory _debugDirectory;

        /// <summary>
        /// Creates a new empty windows assembly image.
        /// </summary>
        public WindowsAssembly()
        {
            SectionHeaders = new ImageSectionHeaderCollection(this);
        }

        /// <summary>
        /// Gets the reading context that was used to read the assembly, or null if the assembly was created.
        /// </summary>
        public ReadingContext ReadingContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the DOS header of the assembly.
        /// </summary>
        public ImageDosHeader DosHeader
        {
            get { return _dosHeader ?? (_dosHeader = new ImageDosHeader()); }
        }

        /// <summary>
        /// Gets the NT headers of the assembly.
        /// </summary>
        public ImageNtHeaders NtHeaders
        {
            get { return _ntHeaders ?? (_ntHeaders = new ImageNtHeaders()); }
        }

        /// <summary>
        /// Gets the section headers defined in the assembly.
        /// </summary>
        public IList<ImageSectionHeader> SectionHeaders
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the import directory of the assembly.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the export directory of the assembly, if available.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the relocation directory of the assembly, if available.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the root of the native resources directory of the assembly.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the managed .NET directory header (COR20 header) of the assembly.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the debugging information directory of the assembly.
        /// </summary>
        public ImageDebugDirectory DebugDirectory
        {
            get
            {
                if (_debugDirectory != null)
                    return _debugDirectory;

                if (ReadingContext != null)
                {
                    var context = CreateDataDirectoryContext(ReadingContext, ImageDataDirectory.DebugDirectoryIndex);
                    if (context != null)
                        return _debugDirectory = ImageDebugDirectory.FromReadingContext(context);
                }

                return null;
            }
            set { _debugDirectory = value; }
        }

        public IEnumerable<ImageSection> GetSections()
        {
            return SectionHeaders.Select(x => x.Section);
        }

        public ImageSection GetSectionByName(string name)
        {
            var header = SectionHeaders.FirstOrDefault(x => x.Name == name);
            return header != null ? header.Section : null;
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

        /// <summary>
        /// Determines the image section the given relative virtual address (RVA) is located at.
        /// </summary>
        /// <param name="rva">The relative virtual address to check.</param>
        /// <returns>The section the <paramref name="rva"/> is located.</returns>
        public ImageSectionHeader GetSectionHeaderByRva(long rva)
        {
            return
                SectionHeaders.FirstOrDefault(
                    sectionHeader =>
                        rva >= sectionHeader.VirtualAddress &&
                        rva < sectionHeader.VirtualAddress + sectionHeader.VirtualSize);
        }

        /// <summary>
        /// Determines the image section the given absolute file offset is located at.
        /// </summary>
        /// <param name="fileOffset">The absolute file offset to check.</param>
        /// <returns>The section the <paramref name="fileOffset"/> is located.</returns>
        public ImageSectionHeader GetSectionHeaderByFileOffset(long fileOffset)
        {
            return
                SectionHeaders.FirstOrDefault(
                    sectionHeader =>
                        fileOffset >= sectionHeader.PointerToRawData &&
                        fileOffset < sectionHeader.PointerToRawData + sectionHeader.SizeOfRawData);
        }

        // TODO
        /// <summary>
        /// Rebuilds and writes the assembly to a specific file path.
        /// </summary>
        /// <param name="file">The file path to write the image to.</param>
        public void Write(string file, WindowsAssemblyBuilder builder)
        {
            using (var stream = File.Create(file))
            {
                Write(new BinaryStreamWriter(stream), builder);
            }
        }

        /// <summary>
        /// Rebuilds and writes the assembly to a specific binary stream.
        /// </summary>
        /// <param name="writer">The writer to write the image to.</param>
        public void Write(IBinaryStreamWriter writer, WindowsAssemblyBuilder builder)
        {
            builder.Build();
            builder.Write(new WritingContext(this, writer));
        }
    }
}
