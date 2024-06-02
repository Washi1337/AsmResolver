using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.Exceptions;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides a lazy-initialized implementation of the <see cref="RuntimeFunctionsSection"/> class that reads
    /// entries from an x86 64-bit PE file.
    /// </summary>
    public class SerializedX64RuntimeFunctionsSection : RuntimeFunctionsSection<X64RuntimeFunction>
    {
        private readonly PEReaderContext _context;
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Reads a runtime function section from the provided input stream.
        /// </summary>
        /// <param name="context">The context in which the reader is situated in.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedX64RuntimeFunctionsSection(PEReaderContext context, ref BinaryStreamReader reader)
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
        protected override IList<X64RuntimeFunction> GetEntries()
        {
            var result =  base.GetEntries();

            var reader = _reader.Fork();
            while (reader.CanRead(X64RuntimeFunction.EntrySize))
                result.Add(X64RuntimeFunction.FromReader(_context, ref reader));

            return result;
        }
    }
}
