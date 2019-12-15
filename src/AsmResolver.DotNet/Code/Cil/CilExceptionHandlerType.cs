namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Provides members that define all possible types of protected regions in a CIL method body. 
    /// </summary>
    public enum CilExceptionHandlerType : uint
    {
        /// <summary>
        /// Indicates the protected region defines a standard exception handler block that catches any exception from
        /// a specific exception type. 
        /// </summary>
        Exception,
        
        /// <summary>
        /// Indicates the protected region defines an exception handler that catches any exception that passes a filter.
        /// </summary>
        Filter,
        
        /// <summary>
        /// Indicates the protected region defines a block of code that is always finalized by a finally clause.
        /// </summary>
        Finally,
        
        /// <summary>
        /// Indicates the protected region defines a block of code that is finalized by a fault clause if there
        /// occurred an exception.
        /// </summary>
        Fault
    }
}