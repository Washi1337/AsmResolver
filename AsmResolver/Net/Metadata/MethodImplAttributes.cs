using System;

namespace AsmResolver.Net.Metadata
{
    [Flags]
    public enum MethodImplAttributes : ushort
    {
        // code impl mask
        CodeTypeMask = 0x0003,
        /// <summary>
        /// Method implementation is IL.
        /// </summary>
        IL = 0x0000,
        /// <summary>
        /// Method implementation is native.
        /// </summary>
        Native = 0x0001,
        /// <summary>
        /// Method implementation is OPTIL.
        /// </summary>
        OPTIL = 0x0002,
        /// <summary>
        /// Method implementation is provided by the runtime.
        /// </summary>
        Runtime = 0x0003,
        // managed mask
        ManagedMask = 0x0004, // Flags specifying whether the code is managed or unmanaged.
        /// <summary>
        /// Method implementation is unmanaged.
        /// </summary>
        Unmanaged = 0x0004,
        /// <summary>
        /// Method implementation is managed.
        /// </summary>
        Managed = 0x0000,
        // implementation info and interop
        /// <summary>
        /// Indicates the method is defined; used primarily in merge scenarios.
        /// </summary>
        ForwardRef = 0x0010,
        /// <summary>
        /// Method will not be optimized when generating native code.
        /// </summary>
        NoOptimization = 0x0040,
        /// <summary>
        /// Indicates the method signature is not to be mangled to do HRESULT conversion.
        /// </summary>
        PreserveSig = 0x0080,
        /// <summary>
        /// Reserved for internal use.
        /// </summary>
        InternalCall = 0x1000,
        /// <summary>
        /// Method is single threaded through the body.
        /// </summary>
        Synchronized = 0x0020,
        /// <summary>
        /// Method may not be inlined.
        /// </summary>
        NoInlining = 0x0008,
    }
}