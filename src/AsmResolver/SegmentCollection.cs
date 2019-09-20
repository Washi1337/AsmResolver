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
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver
{
    /// <summary>
    /// Represents a collection of segments concatenated (and aligned) after each other.
    /// </summary>
    public class SegmentCollection : ISegment, ICollection<ISegment>
    {
        private readonly IList<ISegment> _items = new List<ISegment>();
        private uint _physicalSize;
        private uint _virtualSize;
        
        public SegmentCollection()
            : this(1)
        {
        }

        public SegmentCollection(uint alignment)
        {
            if (alignment <= 0)
                throw new ArgumentOutOfRangeException(nameof(alignment));
            Alignment = alignment;
        }

        /// <inheritdoc />
        public int Count => _items.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public uint FileOffset
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

        public uint Alignment
        {
            get;
        }
        
        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            FileOffset = newFileOffset;
            Rva = newRva;
            _physicalSize = 0;
            _virtualSize = 0;
            
            foreach (var item in _items)
            {
                uint physicalPadding = newFileOffset.Align(Alignment) - newFileOffset;
                uint virtualPadding = newRva.Align(Alignment) - newRva;

                newFileOffset += physicalPadding;
                newRva += virtualPadding;
                
                item.UpdateOffsets(newFileOffset, newRva);

                uint physicalSize = item.GetPhysicalSize();
                uint virtualSize = item.GetVirtualSize();
                
                newFileOffset += physicalSize;
                newRva += virtualSize;
                _physicalSize += physicalPadding + physicalSize;
                _virtualSize += virtualPadding + virtualSize;
            }
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return _physicalSize;
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return _virtualSize;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            uint start = writer.FileOffset;
            for (int i = 0; i < _items.Count; i++)
            {
                var current = _items[i];
                writer.FileOffset = current.FileOffset - FileOffset + start;
                current.Write(writer);
            }
        }

        public void Add(ISegment item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(ISegment item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(ISegment[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(ISegment item)
        {
            return _items.Remove(item);
        }

        public IEnumerator<ISegment> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
    }
}