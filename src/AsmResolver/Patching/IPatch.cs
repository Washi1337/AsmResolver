namespace AsmResolver.Patching
{
    /// <summary>
    /// Provides a mechanism for patching an instance of <see cref="ISegment"/> after it was serialized into its
    /// binary representation.
    /// </summary>
    public interface IPatch
    {
        /// <summary>
        /// Applies the patch.
        /// </summary>
        /// <param name="context">The context in which to</param>
        void Apply(in PatchContext context);
    }
}
