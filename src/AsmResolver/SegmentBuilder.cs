using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver
{
    /// <summary>
    /// Represents a collection of segments concatenated (and aligned) after each other.
    /// </summary>
    public class SegmentBuilder : ISegment, IEnumerable<ISegment>
    {
        private readonly List<AlignedSegment> _items = new();
        private uint _physicalSize;
        private uint _virtualSize;

        /// <summary>
        /// Gets the number of sub segments that are stored into the segment.
        /// </summary>
        public int Count => _items.Count;

        /// <inheritdoc />
        public ulong Offset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public uint Rva
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <summary>
        /// Gets a value indicating whether the collection of concatenated segments is empty.
        /// </summary>
        public bool IsEmpty => _items.Count == 0;

        /// <summary>
        /// Adds the provided segment with no alignment.
        /// </summary>
        /// <param name="segment">The segment to add.</param>
        public void Add(ISegment segment) => Add(segment, 1);

        /// <summary>
        /// Adds the provided segment to the offset that is the next multiple of the provided alignment.
        /// </summary>
        /// <param name="segment">The segment to add.</param>
        /// <param name="alignment">The alignment of the segment.</param>
        public void Add(ISegment segment, uint alignment)
        {
            _items.Add(new AlignedSegment(segment, alignment));
        }

        /// <inheritdoc />
        public void UpdateOffsets(in RelocationParameters parameters)
        {
            Offset = parameters.Offset;
            Rva = parameters.Rva;

            var current = parameters;
            foreach (var item in _items)
            {
                current.Align(item.Alignment);
                item.Segment.UpdateOffsets(current);

                uint physicalSize = item.Segment.GetPhysicalSize();
                uint virtualSize = item.Segment.GetVirtualSize();

                current.Advance(physicalSize, virtualSize);
            }

            _physicalSize = (uint) (current.Offset - parameters.Offset);
            _virtualSize = current.Rva - parameters.Rva;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize() => _physicalSize;

        /// <inheritdoc />
        public uint GetVirtualSize() => _virtualSize;

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var current = _items[i];
                writer.Align(current.Alignment);
                current.Segment.Write(writer);
            }
        }

        /// <summary>
        /// Returns an object that enumerates all segments in the segment builder.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<ISegment> GetEnumerator() => _items.Select(s => s.Segment).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [DebuggerDisplay(nameof(Segment) + ", Alignment: " + nameof(Alignment))]
        private readonly struct AlignedSegment
        {
            public AlignedSegment(ISegment segment, uint alignment)
            {
                Segment = segment;
                Alignment = alignment;
            }

            public ISegment Segment
            {
                get;
            }

            public uint Alignment
            {
                get;
            }
        }

    }
}
