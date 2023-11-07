using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides a lazy-initialized implementation of the <see cref="ImportSectionsSection"/> that is read from a file.
    /// </summary>
    public sealed class SerializedImportSectionsSection : ImportSectionsSection
    {
        private readonly PEReaderContext _context;
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Reads a single import sections section from the provided input stream.
        /// </summary>
        /// <param name="context">The context in which the reader is situated in.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedImportSectionsSection(PEReaderContext context, ref BinaryStreamReader reader)
        {
            Offset = reader.Offset;
            Rva = reader.Rva;

            _context = context;
            _reader = reader;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override BinaryStreamReader CreateReader() => _reader.Fork();

        /// <inheritdoc />
        protected override IList<ImportSection> GetSections()
        {
            var result = new List<ImportSection>();

            var reader = _reader.Fork();
            while (reader.CanRead(ImportSection.ImportSectionSize))
                result.Add(new SerializedImportSection(_context, ref reader));

            return result;
        }
    }
}
