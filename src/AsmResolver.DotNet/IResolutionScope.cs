namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be referenced by a ResolutionScope coded index.
    /// </summary>
    public interface IResolutionScope : IMetadataMember, INameProvider, IModuleProvider
    {
    }
}