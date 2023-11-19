namespace AsmResolver
{
    /// <summary>
    /// Provides an implementation of a reference factory that constructs <see cref="VirtualAddress"/> objects.
    /// </summary>
    public class VirtualAddressFactory : ISegmentReferenceFactory
    {
        /// <summary>
        /// Gets the default instance of this factory.
        /// </summary>
        public static VirtualAddressFactory Instance
        {
            get;
        } = new();

        /// <inheritdoc />
        public ISegmentReference GetReferenceToRva(uint rva) => rva != 0
            ? new VirtualAddress(rva)
            : SegmentReference.Null;
    }
}
