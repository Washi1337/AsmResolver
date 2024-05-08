namespace AsmResolver.Patching
{
    /// <summary>
    /// Provides a mechanism for patching an instance of <see cref="ISegment"/> after it was serialized into its
    /// binary representation.
    /// </summary>
    public interface IPatch
    {
        /// <summary>
        /// Assigns a new file and virtual offset to the segment and all its sub-components.
        /// </summary>
        /// <param name="parameters">The parameters containing the new offset information for the segment.</param>
        void UpdateOffsets(in RelocationParameters parameters);

        /// <summary>
        /// Applies the patch.
        /// </summary>
        /// <param name="context">The context in which to</param>
        void Apply(in PatchContext context);
    }
}
