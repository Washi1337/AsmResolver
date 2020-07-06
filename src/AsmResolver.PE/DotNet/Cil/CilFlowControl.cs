namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides members defining all flow control categories of a CIL operation code.  
    /// </summary>
    public enum CilFlowControl
    {
        /// <summary>
        /// Indicates the operation is an unconditional branching operation.
        /// </summary>
        Branch,
        
        /// <summary>
        /// Indicates the operation is a debugger break operation.
        /// </summary>
        Break,
        
        /// <summary>
        /// Indicates the operation calls a method, and returns afterwards to the next instruction.
        /// </summary>
        Call,
        
        /// <summary>
        /// Indicates the operation is a conditional branching operation.
        /// </summary>
        ConditionalBranch,
        
        /// <summary>
        /// Indicates the operation provides information about a subsequent instruction.
        /// </summary>
        Meta,
        
        /// <summary>
        /// Indicates the operation has no special flow control properties and will execute the next instruction
        /// in the instruction stream.
        /// </summary>
        Next,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        Phi,
        
        /// <summary>
        /// Indicates the operation exits the current method, and potentially returns a value.
        /// </summary>
        Return,
        
        /// <summary>
        /// Indicates the operation throws an exception.
        /// </summary>
        Throw,
    }
}