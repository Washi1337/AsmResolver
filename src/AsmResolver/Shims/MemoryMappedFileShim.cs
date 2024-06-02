using System;
using System.Threading;

namespace AsmResolver.Shims;

internal sealed unsafe partial class MemoryMappedFileShim : IDisposable
{
    private byte* _file;
    private long _size;

    public MemoryMappedFileShim(string path)
    {
        if (RuntimeInformationShim.IsRunningOnWindows)
            AllocateFileWindows(path);
        else
            AllocateFileUnix(path);
    }

    public long Size => _size;
    public byte* BasePointer => _file;

    public byte ReadByte(long address)
    {
        if (_file == null)
            throw new ObjectDisposedException("disposed");
        if ((ulong)address >= (ulong)_size)
            throw new ArgumentOutOfRangeException(nameof(address));
        return _file[address];
    }

    private void DisposeCore()
    {
        void* filePointer;
        fixed (byte** p = &_file)
        {
            filePointer = (void*)Interlocked.Exchange(ref *(IntPtr*)p, default);
        }
        if (filePointer == null)
            return;
        if (RuntimeInformationShim.IsRunningOnWindows)
            DisposeCoreWindows(filePointer);
        else
            DisposeCoreUnix(filePointer);
    }

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    ~MemoryMappedFileShim()
    {
        DisposeCore();
    }
}
