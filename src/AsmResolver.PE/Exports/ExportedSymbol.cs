namespace AsmResolver.PE.Exports
{
    /// <summary>
    /// Represents a single symbol that is exported by a dynamically linked library.
    /// </summary>
    public class ExportedSymbol
    {
        /// <summary>
        /// Creates a new symbol that is exported by ordinal.
        /// </summary>
        /// <param name="address">The reference to the segment representing the symbol.</param>
        public ExportedSymbol(ISegmentReference address)
        {
            Name = null;
            Address = address;
        }

        /// <summary>
        /// Creates a new symbol that is exported by name.
        /// </summary>
        /// <param name="name">The name of the symbol.</param>
        /// <param name="address">The reference to the segment representing the symbol.</param>
        public ExportedSymbol(string name, ISegmentReference address)
        {
            Name = name;
            Address = address;
        }
        
        /// <summary>
        /// Gets or sets the name of the exported symbol.
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the reference to the segment representing the symbol.
        /// </summary>
        /// <remarks>
        /// For exported functions, this reference points to the first instruction that is executed.
        /// For exported fields, this reference points to the first byte of data that this field consists of.
        /// </remarks>
        public ISegmentReference Address
        {
            get;
            set;
        }
    }
}