#if !NET35
using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace AsmResolver.Shims;

internal sealed unsafe class MemoryMappedFileShim : IDisposable
{
    private readonly MemoryMappedFile _file;
    private readonly MemoryMappedViewAccessor _accessor;

    public MemoryMappedFileShim(string path)
    {
        _file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        _accessor = _file.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
    }

    public long Size => _accessor.Capacity;
    public byte* BasePointer => (byte*)_accessor.SafeMemoryMappedViewHandle.DangerousGetHandle();

    public void Dispose()
    {
        _accessor.Dispose();
        _file.Dispose();
    }
}
#endif
