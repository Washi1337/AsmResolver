namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that is either a reference to an external file, assembly or type, and can be referenced by
    /// an Implementation coded index.
    /// </summary>
    public interface IImplementation : IMetadataMember, IFullNameProvider, IModuleProvider
    {
    }
}