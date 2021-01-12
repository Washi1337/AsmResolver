using System;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Resources;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Provides an implementation of a .NET directory that was stored in a PE file.
    /// </summary>
    public class SerializedDotNetDirectory : DotNetDirectory
    {
        private readonly PEReadContext _context;
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
        /// <param name="context">The reader context.</param>
        /// <param name="reader">The input stream.</param>
        /// <exception cref="ArgumentNullException">Occurs when any of the arguments are <c>null</c>.</exception>
        public SerializedDotNetDirectory(PEReadContext context, IBinaryStreamReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Offset = reader.Offset;

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
            if (!_metadataDirectory.IsPresentInPE)
                return null;

            if (!_context.File.TryCreateDataDirectoryReader(_metadataDirectory, out var directoryReader))
            {
                _context.BadImage(".NET data directory contains an invalid metadata directory RVA and/or size.");
                return null;
            }
            
            return new SerializedMetadata(_context, directoryReader);

        }

        /// <inheritdoc />
        protected override DotNetResourcesDirectory GetResources()
        {
            if (!_resourcesDirectory.IsPresentInPE)
                return null;

            if (!_context.File.TryCreateDataDirectoryReader(_resourcesDirectory, out var directoryReader))
            {
                _context.BadImage(".NET data directory contains an invalid resources directory RVA and/or size.");
                return null;
            }

            return new SerializedDotNetResourcesDirectory(directoryReader);

        }

        /// <inheritdoc />
        protected override IReadableSegment GetStrongName()
        {
            if (!_strongNameDirectory.IsPresentInPE)
                return null;

            if (!_context.File.TryCreateDataDirectoryReader(_strongNameDirectory, out var directoryReader))
            {
                _context.BadImage(".NET data directory contains an invalid strong name directory RVA and/or size.");
                return null;
            }
            
            // TODO: interpretation instead of raw contents.
            return DataSegment.FromReader(directoryReader);

        }

        /// <inheritdoc />
        protected override IReadableSegment GetCodeManagerTable()
        {
            if (!_codeManagerDirectory.IsPresentInPE)
                return null;

            if (!_context.File.TryCreateDataDirectoryReader(_codeManagerDirectory, out var directoryReader))
            {
                _context.BadImage(".NET data directory contains an invalid code manager directory RVA and/or size.");
                return null;
            }
            
            // TODO: interpretation instead of raw contents.
            return DataSegment.FromReader(directoryReader);
        }

        /// <inheritdoc />
        protected override IReadableSegment GetVTableFixups()
        {
            if (!_vtableFixupsDirectory.IsPresentInPE)
                return null;

            if (!_context.File.TryCreateDataDirectoryReader(_vtableFixupsDirectory, out var directoryReader))
            {
                _context.BadImage(".NET data directory contains an invalid VTable fixups directory RVA and/or size.");
                return null;
            }

            // TODO: interpretation instead of raw contents.
            return DataSegment.FromReader(directoryReader);
        }

        /// <inheritdoc />
        protected override IReadableSegment GetExportAddressTable()
        {
            if (!_exportsDirectory.IsPresentInPE)
                return null;

            if (!_context.File.TryCreateDataDirectoryReader(_exportsDirectory, out var directoryReader))
            {
                _context.BadImage(".NET data directory contains an invalid export address directory RVA and/or size.");
                return null;
            }
            
            // TODO: interpretation instead of raw contents.
            return DataSegment.FromReader(directoryReader);
        }

        /// <inheritdoc />
        protected override IReadableSegment GetManagedNativeHeader()
        {
            if (!_nativeHeaderDirectory.IsPresentInPE)
                return null;

            if (!_context.File.TryCreateDataDirectoryReader(_nativeHeaderDirectory, out var directoryReader))
            {
                _context.BadImage(".NET data directory contains an invalid native header directory RVA and/or size.");
                return null;
            }
            
            // TODO: interpretation instead of raw contents.
            return DataSegment.FromReader(directoryReader);

        }

    }
}