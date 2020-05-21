// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

using System;

namespace AsmResolver
{
    /// <summary>
    /// Provides a base implementation for a reference to a segment in a binary file.
    /// </summary>
    public readonly struct SegmentReference : ISegmentReference
    {
        /// <summary>
        /// Represents the null reference. 
        /// </summary>
        public static SegmentReference Null
        {
            get;
        } = new SegmentReference(null);
        
        public SegmentReference(ISegment segment)
        {
            Segment = segment;
        }
        
        /// <inheritdoc />
        public uint FileOffset => Segment?.FileOffset ?? 0;

        /// <inheritdoc />
        public uint Rva => Segment?.Rva ?? 0;
        
        /// <inheritdoc />
        public bool CanUpdateOffsets => Segment.CanUpdateOffsets;
        
        /// <inheritdoc />
        public bool IsBounded => true;
        
        /// <inheritdoc />
        public bool CanRead => Segment is IReadableSegment;

        /// <summary>
        /// Gets the referenced segment.
        /// </summary>
        public ISegment Segment
        {
            get;
        }
        
        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva) => Segment.UpdateOffsets(newFileOffset, newRva);

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader()
        {
            return CanRead
                ? ((IReadableSegment) Segment).CreateReader()
                : throw new InvalidOperationException("Cannot read the segment using a binary reader.");
        }

        ISegment ISegmentReference.GetSegment() => Segment;
    }
}