using System;
using System.Collections.Generic;

namespace AsmResolver.PE.Exceptions.X64
{
    internal class X64ExceptionDirectory : ExceptionDirectory<X64RuntimeFunction>
    {
        private readonly PEReaderContext _context;
        private readonly IBinaryStreamReader _reader;

        public X64ExceptionDirectory(PEReaderContext context, IBinaryStreamReader reader)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        /// <inheritdoc />
        protected override IList<X64RuntimeFunction> GetEntries()
        {
            var reader = _reader.Fork();
            var result = new List<X64RuntimeFunction>();

            while (reader.CanRead(X64RuntimeFunction.EntrySize))
                result.Add(X64RuntimeFunction.FromReader(_context, reader));

            return result;
        }
    }
}
