using System;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides members describing all possible flags that can be associated to a single ReadyToRun header.
    /// </summary>
    [Flags]
    public enum ReadyToRunCoreHeaderAttributes
    {
        /// <summary>
        /// Indicates the original IL image was platform neutral. The platform neutrality is part of assembly name.
        /// This flag can be used to reconstruct the full original assembly name.
        /// </summary>
        PlatformNeutralSource = 0x00000001,

        /// <summary>
        /// Indicates the image represents a composite R2R file resulting from a combined compilation of a larger number
        /// of input MSIL assemblies.
        /// </summary>
        Composite = 0x00000002,

        /// <summary>
        ///
        /// </summary>
        Partial = 0x00000004,

        /// <summary>
        /// Indicates PInvoke stubs compiled into image are non-shareable (no secret parameter).
        /// </summary>
        NonSharedPInvokeStubs = 0x00000008,

        /// <summary>
        /// Indicates the input MSIL is embedded in the R2R image.
        /// </summary>
        EmbeddedMsil = 0x00000010,

        /// <summary>
        /// Indicates this is a component assembly of a composite R2R image.
        /// </summary>
        Component = 0x00000020,

        /// <summary>
        /// Indicates this R2R module has multiple modules within its version bubble (For versions before version 6.3,
        /// all modules are assumed to possibly have this characteristic).
        /// </summary>
        MultiModuleVersionBubble = 0x00000040,

        /// <summary>
        /// Indicates this R2R module has code in it that would not be naturally encoded into this module.
        /// </summary>
        UnrelatedR2RCode = 0x00000080,
    }
}
