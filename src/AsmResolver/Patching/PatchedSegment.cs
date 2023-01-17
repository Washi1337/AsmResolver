using System;
using System.Collections.Generic;
using System.Diagnostics;
using AsmResolver.IO;

namespace AsmResolver.Patching
{
    /// <summary>
    /// Provides a wrapper around an instance of a <see cref="ISegment"/> that patches its binary representation
    /// while it is being serialized to an output stream.
    /// </summary>
    [DebuggerDisplay("Patched {Contents} (Count = {Patches.Count})")]
    public class PatchedSegment : IReadableSegment
    {
        private ulong _imageBase;

        /// <summary>
        /// Wraps a segment into a new instance of the <see cref="PatchedSegment"/> class.
        /// </summary>
        /// <param name="contents">The segment to patch.</param>
        public PatchedSegment(ISegment contents)
        {
            Contents = contents;
        }

        /// <summary>
        /// Gets the original segment that is being patched.
        /// </summary>
        public ISegment Contents
        {
            get;
        }

        /// <summary>
        /// Gets a list of patches to apply to the segment.
        /// </summary>
        public IList<IPatch> Patches
        {
            get;
        } = new List<IPatch>();

        /// <inheritdoc />
        public ulong Offset => Contents.Offset;

        /// <inheritdoc />
        public uint Rva => Contents.Rva;

        /// <inheritdoc />
        public bool CanUpdateOffsets => Contents.CanUpdateOffsets;

        /// <inheritdoc />
        public uint GetPhysicalSize() => Contents.GetPhysicalSize();

        /// <inheritdoc />
        public uint GetVirtualSize() => Contents.GetVirtualSize();

        /// <inheritdoc />
        public void UpdateOffsets(in RelocationParameters parameters)
        {
            Contents.UpdateOffsets(in parameters);
            _imageBase = parameters.ImageBase;
        }

        /// <inheritdoc />
        public BinaryStreamReader CreateReader(ulong fileOffset, uint size) => Contents is IReadableSegment segment
            ? segment.CreateReader(fileOffset, size)
            : throw new InvalidOperationException("Segment is not readable.");

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            Contents.Write(writer);
            ApplyPatches(writer);
        }

        private void ApplyPatches(IBinaryStreamWriter writer)
        {
            ulong offset = writer.Offset;

            for (int i = 0; i < Patches.Count; i++)
                Patches[i].Apply(new PatchContext(Contents, _imageBase, writer));

            writer.Offset = offset;
        }
    }
}
