using System;

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Provides members describing all possible flags that can be set in the <see cref="OptionalHeader.DllCharacteristics"/>
    /// field.
    /// </summary>
    [Flags]
    public enum DllCharacteristics
    {
        /// <summary>
        /// Indicates the image can handle a high entropy 64-bit virtual address space. 
        /// </summary>
        HighEntropyVA  = 0x0020,
        
        /// <summary>
        /// Indicates the DLL can be relocated at load time. 
        /// </summary>
        DynamicBase = 0x0040,
        
        /// <summary>
        /// Indicates Code Integrity checks are enforced. 
        /// </summary>
        ForceIntegrity = 0x0080,
        
        /// <summary>
        /// Indicates the image is NX compatible. 
        /// </summary>
        NxCompat = 0x0100,
        
        /// <summary>
        /// Indicates the image is isolation aware, but do not isolate the image. 
        /// </summary>
        NoIsolation = 0x0200,
        
        /// <summary>
        /// Indicates the image does not use structured exception (SE) handling.
        /// No SE handler may be called in this image. 
        /// </summary>
        NoSeh = 0x0400,
        
        /// <summary>
        /// Indicates the image must not be bound.
        /// </summary>
        NoBind = 0x0800,
        
        /// <summary>
        /// Indicates the image must execute in an AppContainer. 
        /// </summary>
        AppContainer = 0x1000,
        
        /// <summary>
        /// Indicates the image is a WDM driver. 
        /// </summary>
        WdmDriver = 0x2000,
        
        /// <summary>
        /// Indicates the image supports Control Flow Guard. 
        /// </summary>
        ControlFLowGuard = 0x4000,
        
        /// <summary>
        /// Indicates the image is Terminal Server aware.
        /// </summary>
        TerminalServerAware = 0x8000,
    }
}