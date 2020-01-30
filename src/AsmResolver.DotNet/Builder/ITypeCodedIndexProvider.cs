namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides members for obtaining coded indices into a metadata tables stream.
    /// </summary>
    public interface ITypeCodedIndexProvider
    {
        /// <summary>
        /// Obtains a coded index to the provided type reference.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The coded index.</returns>
        uint GetTypeDefOrRefIndex(ITypeDefOrRef type);
    }
}