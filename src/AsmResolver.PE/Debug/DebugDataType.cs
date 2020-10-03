namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Provides members defining all possible debug type data that is stored in a debug directory entry.
    /// </summary>
    public enum DebugDataType : uint
    {
        /// <summary>
        /// Indicates an unknown value that is ignored by all tools. 
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// Indicates the COFF debug information (line numbers, symbol table, and string table).
        /// This type of debug information is also pointed to by fields in the file headers. 
        /// </summary>
        Coff = 1,
        
        /// <summary>
        /// Indicates the Visual C++ debug information type.
        /// </summary>
        CodeView = 2,
        
        /// <summary>
        /// Indicates frame pointer omission (FPO) information. This information tells the debugger how to
        /// interpret nonstandard stack frames, which use the EBP register for a purpose other than as a frame pointer. 
        /// </summary>
        Fpo = 0,
        
        /// <summary>
        /// Indicates the location of a DBG file. 
        /// </summary>
        Misc = 4,
        
        /// <summary>
        /// Indicates a copy of .pdata section. 
        /// </summary>
        Exception = 5,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        FixUp = 6,
        
        /// <summary>
        /// Indicates a mapping from an RVA in image to an RVA in source image. 
        /// </summary>
        OMapToSrc = 7,
        
        /// <summary>
        /// Indicates a mapping from an RVA in source image to an RVA in image. 
        /// </summary>
        OMapFromSrc = 8,
        
        /// <summary>
        /// Reserved for Borland.
        /// </summary>
        Borland = 9,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        Reserved10 = 10,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        ClsId = 11,
        
        /// <summary>
        /// Indicates additional Visual C++ debug information.
        /// </summary>
        VcFeature = 12,
        
        /// <summary>
        /// Indicates Profile Guided Optimization data.
        /// </summary>
        Pogo = 13,
        
        /// <summary>
        /// Indicates Look into Incremental Link Time Code Generation (ILTCG) data. 
        /// </summary>
        Iltcg = 14,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        Mpx = 15,
        
        /// <summary>
        /// Indicates PE determinism or reproducibility.
        /// </summary>
        Repro = 16,
        
        /// <summary>
        /// Indicates extended DLL characteristics bits.
        /// </summary>
        ExDllCharacteristics = 20
    }
}