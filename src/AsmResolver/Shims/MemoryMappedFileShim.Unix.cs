using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AsmResolver.Shims;

internal sealed unsafe partial class MemoryMappedFileShim
{
    private const int SEEK_END = 2;
    private const int PROT_READ = 1;
    private const int MAP_PRIVATE = 2;

    [DllImport("libc", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int open([In] string path, uint flags);

    [DllImport("libc", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static extern bool close(int fd);

    [DllImport("libc", ExactSpelling = true, SetLastError = true)]
    private static extern nint mmap(void* addr, nuint length, int prot, int flags, int fd, long offset);

    [DllImport("libc", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static extern bool munmap(void* adrr, nuint length);

    [DllImport("libc", ExactSpelling = true, SetLastError = true)]
    private static extern long lseek(int fd, long offset, int whence);

    private void AllocateFileUnix(string path)
    {
        int fd = open(path, 0);
        if (fd == -1)
            throw new Win32Exception();

        _size = lseek(fd, 0, SEEK_END);
        if (_size == -1)
            throw new Win32Exception();

        nint mapping = mmap(null, (nuint)_size, PROT_READ, MAP_PRIVATE, fd, 0);
        if (mapping == -1)
            throw new Win32Exception();
        _file = (byte*)mapping;

        // mmap explicitly documents that it is fine to close the fd after mapping
        close(fd);
    }

    private void DisposeCoreUnix(void* filePointer)
    {
        munmap(filePointer, (nuint)_size);
    }
}
