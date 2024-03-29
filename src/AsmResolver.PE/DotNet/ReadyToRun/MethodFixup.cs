using System;
using System.Diagnostics;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Describes a fixup that needs to be applied to a native method body before it can be executed.
    /// </summary>
    [DebuggerDisplay("ImportIndex = {ImportIndex}, SlotIndex = {SlotIndex}")]
    public readonly struct MethodFixup : IEquatable<MethodFixup>
    {
        /// <summary>
        /// Constructs a new method fixup.
        /// </summary>
        /// <param name="importIndex">The index to the import section containing the fixup.</param>
        /// <param name="slotIndex">The index of the fixup slot within the import section to apply.</param>
        public MethodFixup(uint importIndex, uint slotIndex)
        {
            ImportIndex = importIndex;
            SlotIndex = slotIndex;
        }

        /// <summary>
        /// Gets the index to the import section containing the fixup.
        /// </summary>
        public uint ImportIndex
        {
            get;
        }

        /// <summary>
        /// Gets the index of the fixup slot within the import section to apply.
        /// </summary>
        public uint SlotIndex
        {
            get;
        }

        /// <inheritdoc />
        public bool Equals(MethodFixup other)
        {
            return ImportIndex == other.ImportIndex && SlotIndex == other.SlotIndex;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MethodFixup other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) ImportIndex * 397) ^ (int) SlotIndex;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"ImportIndex = {ImportIndex}, SlotIndex = {SlotIndex}";
    }
}
