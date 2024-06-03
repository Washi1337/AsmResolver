namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides members indicating whether a reference to a generic parameter is declared in a type or a method.
    /// </summary>
    public enum GenericParameterType
    {
        /// <summary>
        /// Indicates the generic parameter type is defined in an enclosing type. 
        /// </summary>
        Type = 1,
        
        /// <summary>
        /// Indicates the generic parameter type is defined in an enclosing method.
        /// </summary>
        Method = 2,
    }
}