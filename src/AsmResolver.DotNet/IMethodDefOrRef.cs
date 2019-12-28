namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that is either a method definition or a method reference, and can be referenced by a
    /// MethodDefOrRef coded index. 
    /// </summary>
    public interface IMethodDefOrRef : IMemberDescriptor, IHasCustomAttribute
    {
    }
}