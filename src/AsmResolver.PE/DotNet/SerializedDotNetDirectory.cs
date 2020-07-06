using System;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Resources;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Provides an implementation of a .NET directory that was stored in a PE file.
    /// </summary>
    public class SerializedDotNetDirectory : DotNetDirectory
    {
        private readonly PEFile _peFile;
        private readonly IMetadataStreamReader _metadataStreamReader;
        private readonly DataDirectory _metadataDirectory;
        private readonly DataDirectory _resourcesDirectory;
        private readonly DataDirectory _strongNameDirectory;
        private readonly DataDirectory _codeManagerDirectory;
        private readonly DataDirectory _vtableFixupsDirectory;
        private readonly DataDirectory _exportsDirectory;
        private readonly DataDirectory _nativeHeaderDirectory;

        /// <summary>
        /// Reads a .NET directory from an input stream.
        /// </summary>
        /// <param name="peFile">The PE file containing the .NET directory.</param>
        /// <param name="reader">The input stream.</param>
        /// <param name="metadataStreamReader"></param>
        /// <exception cref="ArgumentNullException">Occurs when any of the arguments are <c>null</c>.</exception>
        public SerializedDotNetDirectory(PEFile peFile, IBinaryStreamReader reader,
            IMetadataStreamReader metadataStreamReader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
            _metadataStreamReader = metadataStreamReader;

            uint cb = reader.ReadUInt32();
            MajorRuntimeVersion = reader.ReadUInt16();
            MinorRuntimeVersion = reader.ReadUInt16();
            _metadataDirectory = DataDirectory.FromReader(reader);
            Flags = (DotNetDirectoryFlags) reader.ReadUInt32();
            Entrypoint = reader.ReadUInt32();
            _resourcesDirectory = DataDirectory.FromReader(reader);
            _strongNameDirectory = DataDirectory.FromReader(reader);
            _codeManagerDirectory = DataDirectory.FromReader(reader);
            _vtableFixupsDirectory = DataDirectory.FromReader(reader);
            _exportsDirectory = DataDirectory.FromReader(reader);
            _nativeHeaderDirectory = DataDirectory.FromReader(reader);
        }

        /// <inheritdoc />
        protected override IMetadata GetMetadata()
        {
            if (_metadataDirectory.IsPresentInPE
                && _peFile.TryCreateDataDirectoryReader(_metadataDirectory, out var directoryReader))
            {
                return new SerializedMetadata(directoryReader, _metadataStreamReader);
            }

            return null;
        }

        /// <inheritdoc />
        protected override DotNetResourcesDirectory GetResources()
        {
            if (_resourcesDirectory.IsPresentInPE
                && _peFile.TryCreateDataDirectoryReader(_resourcesDirectory, out var directoryReader))
            {
                return new SerializedDotNetResourcesDirectory(directoryReader);
            }

            return null;

        }

        /// <inheritdoc />
        protected override IReadableSegment GetStrongName()
        {
            if (_strongNameDirectory.IsPresentInPE
                && _peFile.TryCreateDataDirectoryReader(_strongNameDirectory, out var directoryReader))
            {
                // TODO: interpretation instead of raw contents.
                return DataSegment.FromReader(directoryReader);
            }

            return null;
        }

        /// <inheritdoc />
        protected override IReadableSegment GetCodeManagerTable()
        {
            if (_codeManagerDirectory.IsPresentInPE
            && _peFile.TryCreateDataDirectoryReader(_codeManagerDirectory, out var directoryReader))
            {
                // TODO: interpretation instead of raw contents.
                return DataSegment.FromReader(directoryReader);
            }

            return null;
        }

        /// <inheritdoc />
        protected override IReadableSegment GetVTableFixups()
        {
            if (_vtableFixupsDirectory.IsPresentInPE
                && _peFile.TryCreateDataDirectoryReader(_vtableFixupsDirectory, out var directoryReader))
            {
                // TODO: interpretation instead of raw contents.
                return DataSegment.FromReader(directoryReader);
            }

            return null;
        }

        /// <inheritdoc />
        protected override IReadableSegment GetExportAddressTable()
        {
            if (_exportsDirectory.IsPresentInPE
                && _peFile.TryCreateDataDirectoryReader(_exportsDirectory, out var directoryReader))
            {
                // TODO: interpretation instead of raw contents.
                return DataSegment.FromReader(directoryReader);
            }

            return null;
        }

        /// <inheritdoc />
        protected override IReadableSegment GetManagedNativeHeader()
        {
            if (_nativeHeaderDirectory.IsPresentInPE
                && _peFile.TryCreateDataDirectoryReader(_nativeHeaderDirectory, out var directoryReader))
            {
                // TODO: interpretation instead of raw contents.
                return DataSegment.FromReader(directoryReader);
            }

            return null;
        }

    }
}