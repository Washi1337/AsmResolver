using AsmResolver.Patching;

namespace AsmResolver.PE.Code
{
    /// <summary>
    /// Provides extensions to <see cref="PatchedSegment"/> that adds patch overloads to quickly construct instances of
    /// <see cref="AddressFixupPatch"/>.
    /// </summary>
    public static class AddressFixupExtensions
    {
        /// <summary>
        /// Adds an address fixup to the list of patches to apply.
        /// </summary>
        /// <param name="segment">The segment to add the patch to.</param>
        /// <param name="relativeOffset">The offset to start writing the address at, relative to the start of the segment.</param>
        /// <param name="type">The type of address to write.</param>
        /// <param name="referencedObject">The reference to write the RVA for.</param>
        /// <returns>The patched segment.</returns>
        public static PatchedSegment Patch(this PatchedSegment segment, uint relativeOffset, AddressFixupType type,
            ISymbol referencedObject)
        {
            return segment.Patch(new AddressFixup(relativeOffset, type, referencedObject));
        }

        /// <summary>
        /// Adds an address fixup to the list of patches to apply.
        /// </summary>
        /// <param name="segment">The segment to add the patch to.</param>
        /// <param name="fixup">The fixup to apply.</param>
        /// <returns>The patched segment.</returns>
        public static PatchedSegment Patch(this PatchedSegment segment, in AddressFixup fixup)
        {
            segment.Patches.Add(new AddressFixupPatch(fixup));
            return segment;
        }
    }
}
