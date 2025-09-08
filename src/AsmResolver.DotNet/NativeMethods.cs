using System.Runtime.InteropServices;
using System.Text;

namespace AsmResolver.DotNet;

internal static class NativeMethods
{
    public static unsafe string? RealPath(string path)
    {
        const int PATH_MAX = 4096;

        byte[] bytes = new byte[PATH_MAX + 1];
        int length = 0;

        fixed (byte* realName = bytes)
        {
            if (realpath(path, realName) == 0)
                return null;

            for (byte* ptr = realName; *ptr != 0 && ptr != realName + PATH_MAX; ptr++)
                length++;
        }

        return Encoding.ASCII.GetString(bytes, 0, length);

        [DllImport ("libc")]
        static extern nint realpath([MarshalAs(UnmanagedType.LPStr)] string path, byte* buffer);
    }
}
