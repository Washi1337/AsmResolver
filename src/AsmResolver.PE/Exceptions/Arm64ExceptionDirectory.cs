using System;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.Exceptions;

internal class Arm64ExceptionDirectory : ExceptionDirectory<Arm64RuntimeFunction>
{
    private readonly PEReaderContext _context;
    private readonly BinaryStreamReader _reader;

    public Arm64ExceptionDirectory(PEReaderContext context, in BinaryStreamReader reader)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _reader = reader;
    }

    /// <inheritdoc />
    protected override IList<Arm64RuntimeFunction> GetFunctions()
    {
        var reader = _reader.Fork();
        var result = new List<Arm64RuntimeFunction>();

        while (reader.CanRead(X64RuntimeFunction.EntrySize))
            result.Add(Arm64RuntimeFunction.FromReader(_context, ref reader));

        return result;
    }
}
