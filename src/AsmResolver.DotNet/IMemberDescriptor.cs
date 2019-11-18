namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a (reference to a) member defined in a .NET assembly.
    /// </summary>
    public interface IMemberDescriptor : IFullNameProvider
    {
        /// <summary>
        /// Gets the module that defines the member definition or reference.
        /// </summary>
        /// <remarks>
        /// For member references, this does not obtain the module definition that the member is defined in. 
        /// Rather, it obtains the module definition that references this reference.
        /// </remarks>
        ModuleDefinition Module
        {
            get;
        }

        /// <summary>
        /// When this member is defined in a type, gets the enclosing type.
        /// </summary>
        ITypeDescriptor DeclaringType
        {
            get;
        }
    }
}