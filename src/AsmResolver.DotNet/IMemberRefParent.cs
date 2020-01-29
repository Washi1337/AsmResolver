namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be referenced by a MemberRefParent coded index.
    /// </summary>
    public interface IMemberRefParent : IMetadataMember, INameProvider, IModuleProvider
    {
    }
}