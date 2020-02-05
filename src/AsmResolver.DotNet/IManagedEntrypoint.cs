namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that is either a method definition or a reference to an external file, that can be used to
    /// indicate the managed entrypoint of a .NET module.
    /// </summary>
    public interface IManagedEntrypoint : IMetadataMember
    {
    }
}