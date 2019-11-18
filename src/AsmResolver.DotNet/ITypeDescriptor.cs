namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a type in a managed assembly. 
    /// </summary>
    public interface ITypeDescriptor : IFullNameProvider
    {
        /// <summary>
        /// Gets the namespace the type resides in.
        /// </summary>
        string Namespace
        {
            get;
        }

        /// <summary>
        /// Gets the resolution scope that defines the type.
        /// </summary>
        IResolutionScope Scope
        {
            get;
        }

        /// <summary>
        /// When this type is nested, gets the enclosing type.
        /// </summary>
        ITypeDescriptor DeclaringType
        {
            get;
        }
        
    }
}