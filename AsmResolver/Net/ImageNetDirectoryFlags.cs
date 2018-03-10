using System;

namespace AsmResolver.Net
{
    [Flags]
    public enum ImageNetDirectoryFlags : uint
    {
        /// <summary>
        /// Specifies no .NET directory flags.
        /// </summary>
        None = 0,
        /// <summary>
        /// The file contains only managed IL code.
        /// </summary>
        IlOnly = 0x00000001,
        /// <summary>
        /// The application requires a 32 bit machine.
        /// </summary>
        Bit32Required = 0x00000002,
        /// <summary>
        /// The application is a dll coded in IL code.
        /// </summary>
        IlLibrary = 0x00000004,
        /// <summary>
        /// The application is signed.
        /// </summary>
        StrongNameSigned = 0x00000008,
        /// <summary>
        /// The application has a native entry point
        /// </summary>
        NativeEntryPoint = 0x00000010,
        /// <summary>
        /// The application contains debug data.
        /// </summary>
        TrackDebugData = 0x00010000,
    }
}