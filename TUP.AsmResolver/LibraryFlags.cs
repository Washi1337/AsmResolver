using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Flags that describes a dynamic loaded library file.
    /// </summary>
    [Flags]
    public enum LibraryFlags
    {
        /// <summary>
        /// Specifies that the DLL can be relocated at load time.
        /// </summary>
        DynamicBase = 0x40,
        /// <summary>
        /// Specifies that the Code Integrity checks are enforced.
        /// </summary>
        ForceIntegrity = 0x80,
        /// <summary>
        /// Specifies that the image is NX compatible.
        /// </summary>
        NXCompatible = 0x100,
        /// <summary>
        /// Specifies that it's isolation aware, but do not isolate the image.
        /// </summary>
        NoIsolation = 0x200,
        /// <summary>
        /// Specifies that it does not use structured exception handling (SEH). No SE handler may be called in this image.
        /// </summary>
        NoSEH = 0x400,
        /// <summary>
        /// Specifies that the image is not bindable.
        /// </summary>
        NoBind = 0x800,
        /// <summary>
        /// Specifies that the image uses WDM model.
        /// </summary>
        WDMDriver = 0x2000,
        /// <summary>
        /// Specifies that the image is Terminal Server aware.
        /// </summary>
        TerminalServerAware = 0x8000,

    }
}
