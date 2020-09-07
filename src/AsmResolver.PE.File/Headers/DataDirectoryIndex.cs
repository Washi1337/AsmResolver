namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Provides members describing the semantics of each data directory entry in an optional header.
    /// </summary>
    public enum DataDirectoryIndex
    {
        /// <summary>
        /// Indicates the data directory entry containing exported symbols.
        /// </summary>
        ExportDirectory = 0,
        
        /// <summary>
        /// Indicates the directory entry containing a register of imported symbols.
        /// </summary>
        ImportDirectory = 1,
        
        /// <summary>
        /// Indicates the directory entry referencing the root win32 resource directory.
        /// </summary>
        ResourceDirectory = 2,
        
        /// <summary>
        /// Indicates the directory entry containing a register of exception handlers.
        /// </summary>
        ExceptionDirectory = 3,
        
        /// <summary>
        /// Indicates the directory entry containing authentication signatures.
        /// </summary>
        CertificateDirectory = 4,
        
        /// <summary>
        /// Indicates the directory entry containing base relocations that need to be applied to the image after
        /// loading.
        /// </summary>
        BaseRelocationDirectory = 5,
        
        /// <summary>
        /// Indicates the directory entry containing debug information.
        /// </summary>
        DebugDirectory = 6,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        ArchitectureDirectory = 7,
        
        /// <summary>
        /// The RVA of the value to be stored in the global pointer register.
        /// The size member of this structure must be set to zero. 
        /// </summary>
        GlobalPtrDirectory = 8,
        
        /// <summary>
        /// Indicates the directory entry containing the thread local storage (TLS) table.
        /// </summary>
        TlsDirectory = 9,
        
        /// <summary>
        /// Indicates the directory entry containing the load configuration table.
        /// </summary>
        LoadConfigDirectory = 10,
        
        /// <summary>
        /// Indicates the directory entry containing the bound import directory.
        /// </summary>
        BoundImportDirectory = 11,
        
        /// <summary>
        /// Indicates the directory entry containing the import address table.
        /// </summary>
        IatDirectory = 12,
        
        /// <summary>
        /// Indicates the directory entry containing the delay import descriptor table.
        /// </summary>
        DelayImportDescrDirectory = 13,
        
        /// <summary>
        /// Indicates the directory entry containing the CLR runtime headers.
        /// </summary>
        ClrDirectory = 14,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        ReservedDirectory = 15,
    }
}