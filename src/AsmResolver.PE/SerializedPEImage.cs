using System;
using System.Collections.Generic;
using AsmResolver.PE.Debug;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.Exceptions;
using AsmResolver.PE.Exceptions.X64;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Relocations;
using AsmResolver.PE.Tls;
using AsmResolver.PE.Win32Resources;

namespace AsmResolver.PE
{
    /// <summary>
    /// Provides an implementation of a PE image that gets its data from a PE file.
    /// </summary>
    public class SerializedPEImage : PEImage
    {
        private readonly MachineType _originalArchitecture;

        /// <summary>
        /// Opens a PE image from a file.
        /// </summary>
        /// <param name="peFile">The file to base the image from.</param>
        /// <param name="readerParameters">The parameters to use while reading the PE image.</param>
        public SerializedPEImage(IPEFile peFile, PEReaderParameters readerParameters)
        {
            PEFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
            ReaderContext = new PEReaderContext(peFile, readerParameters);

            FilePath = peFile.FilePath;
            MachineType = PEFile.FileHeader.Machine;
            Characteristics = PEFile.FileHeader.Characteristics;
            TimeDateStamp = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(peFile.FileHeader.TimeDateStamp);
            PEKind = PEFile.OptionalHeader.Magic;
            SubSystem = PEFile.OptionalHeader.SubSystem;
            DllCharacteristics = PEFile.OptionalHeader.DllCharacteristics;
            ImageBase = PEFile.OptionalHeader.ImageBase;

            _originalArchitecture = MachineType;
        }

        /// <summary>
        /// Gets the underlying PE file.
        /// </summary>
        public IPEFile PEFile
        {
            get;
        }

        /// <summary>
        /// Gets the reading parameters used for reading the PE image.
        /// </summary>
        public PEReaderContext ReaderContext
        {
            get;
        }

        /// <inheritdoc />
        protected override IList<IImportedModule> GetImports()
        {
            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.ImportDirectory);
            return dataDirectory.IsPresentInPE
                ? new SerializedImportedModuleList(ReaderContext, dataDirectory)
                : new List<IImportedModule>();
        }

        /// <inheritdoc />
        protected override IExportDirectory? GetExports()
        {
            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.ExportDirectory);
            if (!dataDirectory.IsPresentInPE || !PEFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;

            return new SerializedExportDirectory(ReaderContext, ref reader);
        }

        /// <inheritdoc />
        protected override IResourceDirectory? GetResources()
        {
            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.ResourceDirectory);
            if (!dataDirectory.IsPresentInPE || !PEFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;

            return new SerializedResourceDirectory(ReaderContext, null, ref reader);
        }

        /// <inheritdoc />
        protected override IExceptionDirectory? GetExceptions()
        {
            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.ExceptionDirectory);
            if (!dataDirectory.IsPresentInPE || !PEFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;

            return _originalArchitecture switch
            {
                MachineType.Amd64 => new X64ExceptionDirectory(ReaderContext, reader),
                _ => ReaderContext.NotSupportedAndReturn<IExceptionDirectory>()
            };
        }

        /// <inheritdoc />
        protected override IList<BaseRelocation> GetRelocations()
        {
            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.BaseRelocationDirectory);
            return dataDirectory.IsPresentInPE
                ? new SerializedRelocationList(ReaderContext, dataDirectory)
                : new List<BaseRelocation>();
        }

        /// <inheritdoc />
        protected override IDotNetDirectory? GetDotNetDirectory()
        {
            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.ClrDirectory);
            if (!dataDirectory.IsPresentInPE || !PEFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;

            return new SerializedDotNetDirectory(ReaderContext, ref reader);
        }

        /// <inheritdoc />
        protected override IList<DebugDataEntry> GetDebugData()
        {
            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.DebugDirectory);

            var result = new List<DebugDataEntry>();
            if (dataDirectory.IsPresentInPE && PEFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
            {
                uint count = dataDirectory.Size / DebugDataEntry.DebugDataEntryHeaderSize;
                for (int i = 0; i < count; i++)
                    result.Add(new SerializedDebugDataEntry(ReaderContext, ref reader));
            }

            return result;
        }

        /// <inheritdoc />
        protected override ITlsDirectory? GetTlsDirectory()
        {
            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.TlsDirectory);
            if (!dataDirectory.IsPresentInPE || !PEFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;

            return new SerializedTlsDirectory(ReaderContext, ref reader);
        }
    }
}
