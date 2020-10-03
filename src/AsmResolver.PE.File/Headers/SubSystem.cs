namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Provides members describing all supported subsystems that a portable executable can run in.
    /// </summary>
    public enum SubSystem
    {
        /// <summary>
        /// Indicates an unknown subsystem.
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// Indicates device drivers and native Windows processes. 
        /// </summary>
        Native = 1,
        
        /// <summary>
        /// Indicates the Windows graphical user interface subsystem.
        /// </summary>
        WindowsGui = 2,
        
        /// <summary>
        /// Indicates the Windows character subsystem.
        /// </summary>
        WindowsCui = 3,
        
        /// <summary>
        /// Indicates the POSIX character subsystem.
        /// </summary>
        PosixCui = 7,
        
        /// <summary>
        /// Indicates a native Win9x driver. 
        /// </summary>
        NativeWindows = 8,
        
        /// <summary>
        /// Indicates a Windows CE graphical user interface application.
        /// </summary>
        WindowsCeGui = 9,
        
        /// <summary>
        /// Indicates an extensible firmware interface application.
        /// </summary>
        EfiApplication = 10,
        
        /// <summary>
        /// Indicates an extensible firmware interface driver with boot services.
        /// </summary>
        EfiBootServiceDriver = 11,
        
        /// <summary>
        /// Indicates an extensible firmware interface driver with run-time services.
        /// </summary>
        EfiRuntime = 12,
        
        /// <summary>
        /// Indicates an extensible firmware interface ROM image.
        /// </summary>
        EfiRom = 13,
        
        /// <summary>
        /// Indicates an XBOX application.
        /// </summary>
        Xbox = 14,
        
        /// <summary>
        /// Indicates a Windows boot application.
        /// </summary>
        WindowsBootApplication = 16
    }
}