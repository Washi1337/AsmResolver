namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides members for all operation code categories.
    /// </summary>
    public enum CilOpCodeType : byte
    {
        /// <summary>
        /// Deprecated, should not be used.
        /// </summary>
        Annotation,
        
        /// <summary>
        /// Indicates the operation code is a macro instruction that expands to another instruction, but taking less space.
        /// </summary>
        Macro,
        
        /// <summary>
        /// Indicates the operation code is a reserved instruction.
        /// </summary>
        Internal,
        
        /// <summary>
        /// Indicates the operation code applies to objects.
        /// </summary>
        ObjModel,
        
        /// <summary>
        /// Indicates the operation code is a prefix to another instruction.
        /// </summary>
        Prefix,
        
        /// <summary>
        /// Indicates the operation code is a built-in primitive instruction.
        /// </summary>
        Primitive,
    }
}