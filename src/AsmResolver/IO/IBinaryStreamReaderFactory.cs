using System;

namespace AsmResolver.IO
{
    public interface IBinaryStreamReaderFactory : IDisposable
    {
        BinaryStreamReader CreateReader(ulong address, uint rva, uint length);
    }
}
