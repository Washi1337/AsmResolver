using System;
using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.File;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides an implementation of a .NET ReadyToRun directory that was stored in a PE file.
    /// </summary>
    public class SerializedReadyToRunDirectory : ReadyToRunDirectory
    {
        private readonly PEReaderContext _context;
        private readonly uint _numberOfSections = 0;
        private readonly BinaryStreamReader _sectionReader;

        /// <summary>
        /// Reads a .NET ReadyToRun directory from an input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedReadyToRunDirectory(PEReaderContext context, ref BinaryStreamReader reader)
        {
            if (reader.ReadUInt32() != (uint) ManagedNativeHeaderSignature.Rtr)
                throw new BadImageFormatException("Input stream does not point to a valid R2R header.");

            Offset = reader.Offset;
            Rva = reader.Rva;

            _context = context;
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            Attributes = (ReadyToRunAttributes) reader.ReadUInt32();
            _numberOfSections = reader.ReadUInt32();
            _sectionReader = reader.Fork();
        }

        /// <inheritdoc />
        protected override IList<IReadyToRunSection> GetSections()
        {
            var result = new List<IReadyToRunSection>();

            var reader = _sectionReader.Fork();
            for (int i = 0; i < _numberOfSections; i++)
            {
                // Read header.
                if (!reader.CanRead(sizeof(uint) + DataDirectory.DataDirectorySize))
                    break;

                var type = (ReadyToRunSectionType) reader.ReadUInt32();
                var header = DataDirectory.FromReader(ref reader);

                // Get reader to raw contents.
                if (!_context.File.TryCreateDataDirectoryReader(header, out var contentsReader))
                {
                    _context.BadImage($"Invalid RVA and/or size for ReadyToRun section {i}.");
                    continue;
                }

                // Parse.
                result.Add(_context.Parameters.ReadyToRunSectionReader.ReadSection(_context, type, ref contentsReader));
            }

            return result;
        }
    }
}
