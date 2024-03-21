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

    public byte ReadByte(long address)
    {
        if (_file == null)
            throw new ObjectDisposedException("disposed");
        if ((ulong)address >= (ulong)_size)
            throw new ArgumentOutOfRangeException(nameof(address));
        return _file[address];
    }

    public ReadOnlySpan<byte> GetSpan(long offset, int length)
    {
        if (_file == null)
            throw new ObjectDisposedException("disposed");
        long newSize = _size - offset;
        if (offset < 0 || newSize < 0 || length > newSize)
            throw new ArgumentOutOfRangeException();
        return new(_file + offset, length);
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
