using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Relocations;
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
        /// <param name="readParameters">The parameters to use while reading the PE image.</param>
        public SerializedPEImage(PEFile peFile, PEReadParameters readParameters)
        {
            PEFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
            ReadParameters = readParameters;

            MachineType = PEFile.FileHeader.Machine;
            Characteristics = PEFile.FileHeader.Characteristics;
            TimeDateStamp = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(peFile.FileHeader.TimeDateStamp);
            PEKind = PEFile.OptionalHeader.Magic;
            SubSystem = PEFile.OptionalHeader.SubSystem;
            DllCharacteristics = PEFile.OptionalHeader.DllCharacteristics;
            ImageBase = PEFile.OptionalHeader.ImageBase;
        }

        /// <summary>
        /// Gets the underlying PE file.
        /// </summary>
        public PEFile PEFile
        {
            get;
        }

        /// <summary>
        /// Gets the reading parameters used for reading the PE image.
        /// </summary>
        public PEReadParameters ReadParameters
        {
            get;
        }

        /// <inheritdoc />
        protected override IList<IImportedModule> GetImports()
        {
            var dataDirectory = PEFile.OptionalHeader.DataDirectories[OptionalHeader.ImportDirectoryIndex];
            return dataDirectory.IsPresentInPE
                ? (IList<IImportedModule>) new SerializedImportedModuleList(PEFile, dataDirectory)
                : new List<IImportedModule>();
        }

        /// <inheritdoc />
        protected override IExportDirectory GetExports()
        {
            var dataDirectory = PEFile.OptionalHeader.DataDirectories[OptionalHeader.ExportDirectoryIndex];
            if (!dataDirectory.IsPresentInPE || !PEFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;

            return new SerializedExportDirectory(PEFile, reader);
        }

        /// <inheritdoc />
        protected override IResourceDirectory GetResources()
        {
            var dataDirectory = PEFile.OptionalHeader.DataDirectories[OptionalHeader.ResourceDirectoryIndex];
            if (!dataDirectory.IsPresentInPE || !PEFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;

            return new SerializedResourceDirectory(PEFile, null, reader);
        }

        /// <inheritdoc />
        protected override IList<BaseRelocation> GetRelocations()
        {
            var dataDirectory = PEFile.OptionalHeader.DataDirectories[OptionalHeader.BaseRelocationDirectoryIndex];
            return dataDirectory.IsPresentInPE
                ? new SerializedRelocationList(PEFile, dataDirectory)
                : (IList<BaseRelocation>) new List<BaseRelocation>();
        }

        /// <inheritdoc />
        protected override IDotNetDirectory GetDotNetDirectory()
        {
            var dataDirectory = PEFile.OptionalHeader.DataDirectories[OptionalHeader.ClrDirectoryIndex];
            if (!dataDirectory.IsPresentInPE || !PEFile.TryCreateDataDirectoryReader(dataDirectory, out var reader))
                return null;
            
            return new SerializedDotNetDirectory(PEFile, reader, ReadParameters.MetadataStreamReader);
        }
    }
}