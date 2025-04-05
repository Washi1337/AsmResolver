#if NET35
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace AsmResolver.Shims;

internal sealed unsafe partial class MemoryMappedFileShim
{
    private const uint GENERIC_READ = 0x80000000;
    private const uint FILE_SHARE_READ = 0x2;
    private const uint OPEN_EXISTING = 3;
    private const uint PAGE_READONLY = 2;
    // not on msdn, taken from https://github.com/terrafx/terrafx.interop.windows/blob/e681ccb7239bf9f5629083cd1e396b2876c24aae/sources/Interop/Windows/Windows/um/memoryapi/FILE.cs#L14
    private const uint FILE_MAP_READ = 4;

    private void* _fileHandle;
    private void* _mappingHandle;

    [DllImport("kernel32", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint CreateFileW(char* name, uint access, uint shareMode, void* securityAttributes, uint create, uint attributes);

    [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileSizeEx(void* handle, out long size);

    [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(void* handle);

    [DllImport("kernel32", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern nint CreateFileMappingA(void* handle, void* mapAttributes, uint protection, uint maxSizeHigh,
        uint maxSizeLow, sbyte* name);

    [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
    private static extern void* MapViewOfFile(void* handle, uint access, uint offsetHigh, uint offsetLow, nuint size);

    [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnmapViewOfFile(void* p);

    private void AllocateFileWindows(string path)
    {
        nint handle;
        fixed (char* pathPointer = path)
        {
            handle = CreateFileW(pathPointer, GENERIC_READ, FILE_SHARE_READ, null, OPEN_EXISTING, 0);
        }
        if (handle == -1)
            throw new Win32Exception();

        _fileHandle = (void*)handle;
        if (!GetFileSizeEx(_fileHandle, out _size))
            throw new Win32Exception();

        handle = CreateFileMappingA(_fileHandle, null, PAGE_READONLY, 0, 0, null);
        if (handle == -1)
            throw new Win32Exception();
        _mappingHandle = (void*)handle;

        _file = (byte*)MapViewOfFile(_mappingHandle, FILE_MAP_READ, 0, 0, 0);
        if (_file == null)
            throw new Win32Exception();
    }

    private void DisposeCoreWindows(void* filePointer)
    {
        if (!UnmapViewOfFile(filePointer))
            return;
        if (!CloseHandle(_mappingHandle))
            return;
        CloseHandle(_fileHandle);
    }
}
#endif
