namespace AsmResolver.PE.File.Headers
{
    public enum SubSystem
    {
        Unknown = 0,
        Native = 1,
        WindowsGui = 2,
        WindowsCui = 3,
        PosixCui = 7,
        WindowsCeGui = 9,
        EfiApplication = 10,
        EfiBootServiceDriver = 11,
        EfiRuntime = 12,
        EfiRom = 13,
        Xbox = 14,
    }
}