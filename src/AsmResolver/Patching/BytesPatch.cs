using System.Diagnostics;
using AsmResolver.IO;

namespace AsmResolver.Patching
{
    /// <summary>
    /// Patches an instance of <see cref="ISegment"/> with a sequence of bytes.
    /// </summary>
    [DebuggerDisplay("Patch {RelativeOffset} with {NewData.Length} bytes")]
    public sealed class BytesPatch : IPatch
    {
        /// <summary>
        /// Creates a new bytes patch.
        /// </summary>
        /// <param name="relativeOffset">The offset to start writing at.</param>
        /// <param name="newData">The new data.</param>
        public BytesPatch(uint relativeOffset, byte[] newData)
        {
            RelativeOffset = relativeOffset;
            NewData = newData;
        }

        /// <summary>
        /// Gets the offset relative to the start of the segment to start writing at.
        /// </summary>
        public uint RelativeOffset
        {
            get;
        }

        /// <summary>
        /// Gets the data to write.
        /// </summary>
        public byte[] NewData
        {
            get;
        }

        /// <inheritdoc />
        public void Apply(in PatchContext context)
        {
            context.Writer.Offset = context.Segment.Offset + RelativeOffset;
            context.Writer.WriteBytes(NewData);
        }
    }
}
