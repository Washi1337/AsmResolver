namespace AsmResolver.DotNet.Signatures
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
        /// /// <param name="diagnosticSource">When available, the object that is reported in diagnostics when index encoding of the type fails.</param>
        /// <returns>The coded index.</returns>
        uint GetTypeDefOrRefIndex(ITypeDefOrRef type, object? diagnosticSource = null);
    }
}
