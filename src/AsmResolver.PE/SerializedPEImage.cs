using System;
using System.Collections.Generic;
using AsmResolver.PE.Certificates;
using AsmResolver.PE.Debug;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.Exceptions;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
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
        /// <summary>
        /// Opens a PE image from a file.
        /// </summary>
        /// <param name="peFile">The file to base the image from.</param>
        /// <param name="readerParameters">The parameters to use while reading the PE image.</param>
        public SerializedPEImage(PEFile peFile, PEReaderParameters readerParameters)
        {
            PEFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
            ReaderContext = new PEReaderContext(peFile, readerParameters);

            FilePath = peFile.FilePath;
            MachineType = peFile.FileHeader.Machine;
            Characteristics = peFile.FileHeader.Characteristics;
            TimeDateStamp = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(peFile.FileHeader.TimeDateStamp);
            PEKind = peFile.OptionalHeader.Magic;
            SubSystem = peFile.OptionalHeader.SubSystem;
            DllCharacteristics = peFile.OptionalHeader.DllCharacteristics;
            ImageBase = peFile.OptionalHeader.ImageBase;
        }

        /// <inheritdoc />
        public override PEFile PEFile
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
        protected override IList<ImportedModule> GetImports()
        {
            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.ImportDirectory);
            return dataDirectory.IsPresentInPE
                ? new SerializedImportedModuleList(ReaderContext, dataDirectory)
                : new List<ImportedModule>();
        }

        /// <inheritdoc />
        protected override ExportDirectory? GetExports()
        {
            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.ExportDirectory);
            if (!dataDirectory.IsPresentInPE || !PEFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;

            return new SerializedExportDirectory(ReaderContext, ref reader);
        }

        /// <inheritdoc />
        protected override ResourceDirectory? GetResources()
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

            return ReaderContext.Platform.TargetMachine switch
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
        protected override DotNetDirectory? GetDotNetDirectory()
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
        protected override TlsDirectory? GetTlsDirectory()
        {
            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.TlsDirectory);
            if (!dataDirectory.IsPresentInPE || !PEFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;

            return new SerializedTlsDirectory(ReaderContext, ref reader);
        }

        /// <inheritdoc />
        protected override CertificateCollection GetCertificates()
        {
            var result = new CertificateCollection();

            var dataDirectory = PEFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.CertificateDirectory);
            if (!dataDirectory.IsPresentInPE)
                return result;

            // Certificate directory interprets the VirtualAddress of the data directory as a file offset as opposed to
            // an RVA. Hence, we cannot use the normal TryCreateDataDirectoryReader method.
            if (!PEFile.TryCreateReaderAtFileOffset(dataDirectory.VirtualAddress, dataDirectory.Size, out var reader))
                return result;

            result.UpdateOffsets(new RelocationParameters(dataDirectory.VirtualAddress, dataDirectory.VirtualAddress));

            var certificateReader = ReaderContext.Parameters.CertificateReader;

            while (reader.CanRead(AttributeCertificate.HeaderSize))
            {
                // Read header.
                uint length = reader.ReadUInt32();
                var revision = (CertificateRevision) reader.ReadUInt16();
                var type = (CertificateType) reader.ReadUInt16();

                // Bound contents reader to just the contents as indicated by the header.
                var contentsReader = reader.ForkRelative(
                    AttributeCertificate.HeaderSize,
                    length - AttributeCertificate.HeaderSize);

                // Read it.
                result.Add(certificateReader.ReadCertificate(ReaderContext, revision, type, contentsReader));

                // Advance to next certificate in the table.
                reader.RelativeOffset += length - AttributeCertificate.HeaderSize;
                reader.Align(8);
            }

            return result;
        }
    }
}
