namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides members defining all possible flags that can be assigned to a security declaration attribute.
    /// </summary>
    public enum SecurityAction : ushort
    {
        /// <summary>
        /// Indicates all callers higher in the call stack are required to have been granted the permission specified
        /// by the current permission object.
        /// </summary>
        Demand = 2,

        /// <summary>
        /// Indicates the calling code can access the resource identified by the current permission object,
        /// even if callers higher in the stack have not been granted permission to access the resource.
        /// </summary>
        Assert = 3,

        /// <summary>
        /// Indicates the ability to access the resource specified by the current permission object is denied to callers,
        /// even if they have been granted permission to access it.
        /// </summary>
        /// <remarks>
        /// This value is deprecated.
        /// </remarks>
        Deny = 4,

        /// <summary>
        /// Indicatges only the resources specified by this permission object can be accessed, even if the code has
        /// been granted permission to access other resources.
        /// </summary>
        PermitOnly = 5,

        /// <summary>
        /// Indicates the immediate caller is required to have been granted the specified permission.
        /// </summary>
        LinkDemand = 6,

        /// <summary>
        /// Indicates the derived class inheriting the class or overriding a method is required to have been granted the
        /// specified permission.
        /// </summary>
        InheritanceDemand = 7,

        /// <summary>
        /// Indicates the request for the minimum permissions required for code to run. This action can only be used
        /// within the scope of the assembly.
        /// </summary>
        RequestMinimum = 8,

        /// <summary>
        /// Indicates the request for additional permissions that are optional (not required to run). This request
        /// implicitly refuses all other permissions not specifically requested. This action can only be used within
        /// the scope of the assembly.
        /// </summary>
        RequestOptional = 9,

        /// <summary>
        /// The request that permissions that might be misused will not be granted to the calling code.
        /// This action can only be used within the scope of the assembly.
        /// </summary>
        RequestRefuse = 10,
    }
}
