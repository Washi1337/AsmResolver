namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Provides members describing the function of the file.
    /// </summary>
    public enum FileSubType
    {
        /// <summary>
        /// Indicates the file type is unknown to the system. 
        /// </summary>
        Unknown = 0x00000000,
        
        /// <summary>
        /// Indicates the file contains a printer driver. 
        /// </summary>
        DriverPrinter = 0x00000001,
        
        /// <summary>
        /// Indicates the file contains a keyboard driver. 
        /// </summary>
        DriverKeyboard = 0x00000002,
        
        /// <summary>
        /// Indicates the file contains a language driver. 
        /// </summary>
        DriverLanguage = 0x00000003,
        
        /// <summary>
        /// Indicates the file contains a display driver.  
        /// </summary>
        DriverDisplay = 0x0000004,
        
        /// <summary>
        /// Indicates the file contains a mouse driver. 
        /// </summary>
        DriverMouse = 0x00000005,
        
        /// <summary>
        /// Indicates the file contains a network driver. 
        /// </summary>
        DriverNetwork = 0x00000006,
        
        /// <summary>
        /// Indicates the file contains a system driver. 
        /// </summary>
        DriverSystem = 0x00000007,
        
        /// <summary>
        /// Indicates the file contains an installable driver. 
        /// </summary>
        DriverInstallable = 0x00000008,
        
        /// <summary>
        /// Indicates the file contains a sound driver. 
        /// </summary>
        DriverSound = 0x00000009,
        
        /// <summary>
        /// Indicates the file contains a communications driver. 
        /// </summary>
        DriverCommunications = 0x0000000A,
        
        /// <summary>
        /// Indicates the file contains a versioned printer driver. 
        /// </summary>
        DriverVersionedPrinter = 0x0000000C,
        
        /// <summary>
        /// Indicates the file contains a raster font. 
        /// </summary>
        FontRaster = 0x00000001,
        
        /// <summary>
        /// Indicates the file contains a vector font. 
        /// </summary>
        FontVector = 0x00000002,
        
        /// <summary>
        /// Indicates the file contains a TrueType font. 
        /// </summary>
        FontTrueType = 0x00000003,
    }
}