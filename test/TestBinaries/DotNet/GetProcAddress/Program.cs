using System.Runtime.InteropServices;

namespace GetProcAddress;

internal static class Program
{
    [DllImport("kernel32.dll")]
    private static extern uint GetLastError();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nuint LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nuint GetProcAddress(nuint hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

    public static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: GetProcAddress <dll> <function>");
            return 0;
        }

        nuint module = LoadLibraryW(args[0]);
        if (module == 0)
        {
            Console.Error.WriteLine($"Could not load DLL {args[0]}. Error: {GetLastError()}");
            return 1;
        }

        nuint func = GetProcAddress(module, args[1]);
        if (func == 0)
        {
            Console.Error.WriteLine($"Could not get address of function {args[1]} in {args[0]}. Error: {GetLastError()}");
            return 1;
        }

        ulong va = func;
        uint rva = (uint) (func - module);
        Console.WriteLine($"{Path.GetFileNameWithoutExtension(args[0])}+0x{rva:X} (0x{va:X8})");
        return 0;
    }
}
