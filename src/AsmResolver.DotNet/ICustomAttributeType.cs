namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be referenced by a CustomAttributeType coded index,
    /// </summary>
    public interface ICustomAttributeType : IMethodDescriptor, IHasCustomAttribute
    {
        /// <summary>
        /// When this member is defined in a type, gets the enclosing type.
        /// </summary>
        new ITypeDefOrRef? DeclaringType
        {
            get;
        }
    }
}
