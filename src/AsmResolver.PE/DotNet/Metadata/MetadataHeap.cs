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

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides a base implementation of a metadata heap. 
    /// </summary>
    public abstract class MetadataHeap : SegmentBase, IMetadataStream
    {
        /// <summary>
        /// Initializes the metadata heap with a name.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        protected MetadataHeap(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        
        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        public virtual bool CanRead => false;

        /// <summary>
        /// Gets a value indicating whether any index into this metadata heap will need 2 or 4 bytes to be encoded.
        /// </summary>
        public IndexSize IndexSize => GetPhysicalSize() > ushort.MaxValue ? IndexSize.Long : IndexSize.Short;
        
        /// <inheritdoc />
        public virtual IBinaryStreamReader CreateReader() => throw new NotSupportedException();
        
    }
}