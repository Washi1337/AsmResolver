namespace AsmResolver.DotNet.Code.Native
{
    /// <summary>
    /// Represents a symbol within a native method body.
    /// </summary>
    public class NativeLocalSymbol : ISymbol
    {
        /// <summary>
        /// Creates a new native local symbol.
        /// </summary>
        /// <param name="body">The body that defines this symbol.</param>
        /// <param name="offset">The offset relative to the start of the method body.</param>
        public NativeLocalSymbol(NativeMethodBody body, uint offset)
        {
            Body = body;
            Offset = offset;
        }

        /// <summary>
        /// Gets the body that this symbol is defined in.
        /// </summary>
        public NativeMethodBody Body
        {
            get;
        }

        /// <summary>
        /// Gets the offset of the symbol, relative to the start of the method body.
        /// </summary>
        public uint Offset
        {
            get;
        }

        /// <inheritdoc />
        public ISegmentReference? GetReference() => Body.Address is not null
            ? new RelativeReference(Body.Address, (int) Offset)
            : null;

        /// <inheritdoc />
        public override string ToString() => $"{Body.Owner.SafeToString()}+{Offset:X}";
    }
}
