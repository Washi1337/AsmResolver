using System;

namespace AsmResolver.PE.Code
{
    /// <summary>
    /// Provides information about a code or data segment referenced within a code segment for which
    /// the final RVA is yet to be determined.
    /// </summary>
    public readonly struct AddressFixup
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AddressFixup"/> structure.
        /// </summary>
        /// <param name="offset">The offset relative to the start of the code segment pointing to the reference.</param>
        /// <param name="type">The type of fixup to apply at the offset.</param>
        /// <param name="referencedObject">The reference to write the RVA for.</param>
        public AddressFixup(uint offset, AddressFixupType type, ISymbol referencedObject)
        {
            Offset = offset;
            Symbol = referencedObject ?? throw new ArgumentNullException(nameof(referencedObject));
            Type = type;
        }
        
        /// <summary>
        /// Gets the offset relative to the start of the code segment pointing to the reference.
        /// </summary>
        public uint Offset
        {
            get;
        }

        /// <summary>
        /// Gets the type of fixup to apply at the offset.
        /// </summary>
        public AddressFixupType Type
        {
            get;
        }

        /// <summary>
        /// Gets the object that is referenced at the offset.
        /// </summary>
        public ISymbol Symbol
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString() => $"+{Offset:X8} <{Symbol}> ({Type})";
    }
}