namespace AsmResolver
{
    /// <summary>
    /// Represents objects that can be referenced by a virtual address.
    /// </summary>
    public interface ISymbol
    {
        /// <summary>
        /// Gets a reference the object.
        /// </summary>
        /// <returns>The object.</returns>
        ISegmentReference? GetReference();
    }
}
