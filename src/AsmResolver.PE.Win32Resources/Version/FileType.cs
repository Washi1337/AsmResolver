namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Provides members describing the general type of file. 
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// Indicates the file type is unknown to the system.
        /// </summary>
        Unknown = 0x00000000,
        
        /// <summary>
        /// Indicates the file contains an application. 
        /// </summary>
        App = 0x00000001,
        
        /// <summary>
        /// Indicates the file contains a DLL. 
        /// </summary>
        Dll = 0x00000002,
        
        /// <summary>
        /// Indicates the file contains a device driver.  
        /// </summary>
        Driver = 0x00000003,
        
        /// <summary>
        /// Indicates the file contains a font. 
        /// </summary>
        Font = 0x00000004,
        
        /// <summary>
        /// Indicates the file contains a virtual device. 
        /// </summary>
        VirtualDevice = 0x00000005,
        
        /// <summary>
        /// Indicates the file contains a static-link library. 
        /// </summary>
        StaticLibrary = 0x00000007,
    }
}