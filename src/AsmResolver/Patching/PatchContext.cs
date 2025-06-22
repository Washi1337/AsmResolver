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
        /// <param name="writerBase">The offset the writer actually started writing the segment at.</param>
        /// <param name="writer">The object responsible for writing the patches.</param>
        public PatchContext(ISegment segment, ulong imageBase, ulong writerBase, BinaryStreamWriter writer)
        {
            Segment = segment;
            ImageBase = imageBase;
            WriterBase = writerBase;
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
        /// Gets the actual offset the writer started writing the segment at.
        /// </summary>
        /// <remarks>
        /// This property is in most cases equivalent to <c>Segment.Offset</c>. However, callers may be calling
        /// <see cref="PatchedSegment.Write"/> with a <see cref="BinaryStreamWriter"/> that is not writing the entire
        /// enclosing file (e.g., when calling <see cref="Extensions.WriteIntoArray(ISegment)"/>).
        /// Implementations of <see cref="IPatch"/> should therefore always prefer <see cref="WriterBase"/> over
        /// <c>Segment.Offset</c> when adjusting the current offset of <see cref="Writer"/>.
        /// </remarks>
        public ulong WriterBase
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
