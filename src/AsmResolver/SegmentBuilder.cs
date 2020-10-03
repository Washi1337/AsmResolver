using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver
{
    /// <summary>
    /// Represents a collection of segments concatenated (and aligned) after each other.
    /// </summary>
    public class SegmentBuilder : ISegment, IEnumerable<ISegment>
    {
        private readonly List<AlignedSegment> _items = new List<AlignedSegment>();
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

        public void Add(ISegment segment) => Add(segment, 1);

        public void Add(ISegment segment, uint alignment)
        {
            _items.Add(new AlignedSegment(segment, alignment));
        }
        
        /// <inheritdoc />
        public void UpdateOffsets(ulong newOffset, uint newRva)
        {
            Offset = newOffset;
            Rva = newRva;
            _physicalSize = 0;
            _virtualSize = 0;
            
            foreach (var item in _items)
            {
                uint physicalPadding = (uint) (newOffset.Align(item.Alignment) - newOffset);
                uint virtualPadding = newRva.Align(item.Alignment) - newRva;
                
                newOffset += physicalPadding;
                newRva += virtualPadding;
                
                item.Segment.UpdateOffsets(newOffset, newRva);

                uint physicalSize = item.Segment.GetPhysicalSize();
                uint virtualSize = item.Segment.GetVirtualSize();
                
                newOffset += physicalSize;
                newRva += virtualSize;
                _physicalSize += physicalPadding + physicalSize;
                _virtualSize += virtualPadding + virtualSize;
            }
        }

        /// <inheritdoc />
        public uint GetPhysicalSize() => _physicalSize;

        /// <inheritdoc />
        public uint GetVirtualSize() => _virtualSize;

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            ulong start = writer.Offset;
            for (int i = 0; i < _items.Count; i++)
            {
                var current = _items[i];
                writer.Offset = current.Segment.Offset - Offset + start;
                current.Segment.Write(writer);
            }
        }

        public IEnumerator<ISegment> GetEnumerator() => _items.Select(s => s.Segment).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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