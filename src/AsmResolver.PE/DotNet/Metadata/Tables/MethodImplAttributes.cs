using System;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides members defining all flags that dictate the implementation of a method body.
    /// </summary>
    [Flags]
    public enum MethodImplAttributes : ushort
    {
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

        /// <summary>
        /// Provides a bitmask for obtaining the flags related to the code type of the method.
        /// </summary>
        CodeTypeMask = 0x0003,

        /// <summary>
        /// Method implementation is unmanaged.
        /// </summary>
        Unmanaged = 0x0004,

        /// <summary>
        /// Method implementation is managed.
        /// </summary>
        Managed = 0x0000,

        /// <summary>
        /// Provides a bitmask for obtaining the flags specifying whether the code is managed or unmanaged.
        /// </summary>
        ManagedMask = 0x0004,

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

        /// <summary>
        /// Method should be inlined if possible.
        /// </summary>
        AggressiveInlining = 0x0100,

        /// <summary>
        /// Method may contain hot code and should be aggressively optimized.
        /// </summary>
        AggressiveOptimization = 0x0200,

        /// <summary>
        /// Specifies that the JIT compiler should look for security mitigation attributes, such as the user-defined
        /// <c>System.Runtime.CompilerServices.SecurityMitigationsAttribute</c>. If found, the JIT compiler applies
        /// any related security mitigations. Available starting with .NET Framework 4.8.
        /// </summary>
        /// <remarks>
        /// This is an undocumented flag and is currently not used:
        /// Original addition: https://github.com/dotnet/dotnet-api-docs/pull/2253
        /// Documentation removal: https://github.com/dotnet/dotnet-api-docs/pull/4652
        /// </remarks>
        SecurityMitigations,
    }
}
