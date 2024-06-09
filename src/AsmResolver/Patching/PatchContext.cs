using AsmResolver.IO;

namespace AsmResolver.Patching
{
    /// <summary>
    /// Provides members describing the context in which a patch may be situated in.
    /// </summary>
    public readonly struct PatchContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PatchContext"/> structure.
        /// </summary>
        /// <param name="segment">The segment to be patched.</param>
        /// <param name="imageBase">The image base to assume while patching.</param>
        /// <param name="writer">The object responsible for writing the patches.</param>
        public PatchContext(ISegment segment, ulong imageBase, BinaryStreamWriter writer)
        {
            Segment = segment;
            ImageBase = imageBase;
            Writer = writer;
        }

        /// <summary>
        /// Gets the segment to be patched.
        /// </summary>
        public ISegment Segment
        {
            get;
        }

        /// <summary>
        /// Gets the image base that is assumed while patching.
        /// </summary>
        public ulong ImageBase
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for writing the patches.
        /// </summary>
        public BinaryStreamWriter Writer
        {
            get;
        }
    }
}
