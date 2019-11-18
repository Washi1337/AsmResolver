namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a (reference to a) member defined in a .NET assembly.
    /// </summary>
    public interface IMemberDescriptor : IFullNameProvider, IModuleProvider
    {
        /// <summary>
        /// When this member is defined in a type, gets the enclosing type.
        /// </summary>
        ITypeDescriptor DeclaringType
        {
            get;
        }
    }
}