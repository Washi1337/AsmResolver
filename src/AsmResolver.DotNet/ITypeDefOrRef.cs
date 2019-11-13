namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type definition or reference that can be referenced by a TypeDefOrRef coded index. 
    /// </summary>
    public interface ITypeDefOrRef : IMetadataMember, INameProvider
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
    }
}