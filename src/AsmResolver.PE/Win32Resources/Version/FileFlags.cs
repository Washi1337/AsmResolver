using System;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Provides members describing the attributes of the file in a version resource.
    /// </summary>
    [Flags]
    public enum FileFlags
    {
        /// <summary>
        /// Indicates  the file contains debugging information or is compiled with debugging features enabled. 
        /// </summary>
        Debug = 0x01,
        
        /// <summary>
        /// Indicates the file is a development version and not a commercially released product. 
        /// </summary>
        PreRelease = 0x02,
        
        /// <summary>
        /// Indicates the file has been modified and is not identical to the original shipping file of the
        /// same version number. 
        /// </summary>
        Patched = 0x04,
        
        /// <summary>
        /// Indicates the file was not built using standard release procedures. If this flag is set, the
        /// StringFileInfo structure should contain a PrivateBuild entry. 
        /// </summary>
        PrivateBuild = 0x08,
        
        /// <summary>
        /// Indicates the file's version structure was created dynamically; therefore, some of the members
        /// in this structure may be empty or incorrect. This flag should never be set in a file's
        /// VS_VERSIONINFO data. 
        /// </summary>
        InfoInferred = 0x10,
        
        /// <summary>
        /// Indicates the file was built by the original company using standard release procedures but is a
        /// variation of the normal file of the same version number. If this flag is set, the StringFileInfo
        /// structure should contain a SpecialBuild entry. 
        /// </summary>
        SpecialBuild = 0x20,
    }
}