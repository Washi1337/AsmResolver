namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a member that can potentially be resolved to its definition.
    /// </summary>
    public interface IResolvable
    {
        /// <summary>
        /// Resolves the member to its definition if possible.
        /// </summary>
        /// <returns>The resolved member.</returns>
        IMetadataMember Resolve();
    }
}
