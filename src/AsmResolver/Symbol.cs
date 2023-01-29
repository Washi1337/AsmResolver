namespace AsmResolver
{
    /// <summary>
    /// Represents a simple symbol that maps a name to a memory address.
    /// </summary>
    public class Symbol : ISymbol
    {
        /// <summary>
        /// Creates a new symbol.
        /// </summary>
        /// <param name="address">The address of the symbol.</param>
        public Symbol(ISegmentReference address)
        {
            Address = address;
        }

        /// <summary>
        /// Gets the address of the symbol.
        /// </summary>
        public ISegmentReference Address
        {
            get;
        }

        /// <inheritdoc />
        public ISegmentReference GetReference() => Address;

        /// <inheritdoc />
        public override string? ToString() => Address.ToString();
    }
}
