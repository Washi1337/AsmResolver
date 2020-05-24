namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that is either a method definition or a method reference, and can be referenced by a
    /// MethodDefOrRef coded index. 
    /// </summary>
    public interface IMethodDefOrRef : IMethodDescriptor, IHasCustomAttribute
    {
        /// <summary>
        /// When this member is defined in a type, gets the enclosing type.
        /// </summary>
        new ITypeDefOrRef DeclaringType
        {
            get;
        }
    }
}