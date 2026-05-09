using System;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.Exceptions
{
    internal class X64ExceptionDirectory : ExceptionDirectory<X64RuntimeFunction>
    {
        private readonly PEReaderContext _context;
        private readonly BinaryStreamReaderState _readerState;

        public X64ExceptionDirectory(PEReaderContext context, in BinaryStreamReader reader)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _readerState = reader.GetState();
        }

        /// <inheritdoc />
        protected override IList<X64RuntimeFunction> GetFunctions()
        {
            var reader = _readerState.CreateReader();
            var result = new List<X64RuntimeFunction>();

            while (reader.CanRead(X64RuntimeFunction.EntrySize))
                result.Add(X64RuntimeFunction.FromReader(_context, ref reader));

            return result;
        }
    }
}
