namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Represents a label to a CIL instruction referenced by a fixed offset relative to the start of the CIL method body. 
    /// </summary>
    public class CilOffsetLabel : ICilLabel
    {
        /// <summary>
        /// Creates a new fixed offset CIL label.
        /// </summary>
        /// <param name="offset">The offset of the instruction to reference.</param>
        public CilOffsetLabel(int offset)
        {
            Offset = offset;
        }

        /// <inheritdoc />
        public int Offset
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString() => "IL_" + Offset.ToString("X4");

        /// <inheritdoc />
        public bool Equals(ICilLabel other) => other != null && Offset == other.Offset;

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as ICilLabel);

        /// <inheritdoc />
        public override int GetHashCode() => Offset;
    }
}