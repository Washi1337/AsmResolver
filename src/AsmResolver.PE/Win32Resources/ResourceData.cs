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
using AsmResolver.Lazy;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides an implementation for a single data entry in a Win32 resource directory.
    /// </summary>
    public class ResourceData : IResourceData
    {
        private readonly LazyVariable<IReadableSegment> _contents;

        /// <summary>
        /// Initializes a new resource data entry.
        /// </summary>
        protected ResourceData()
        {
            _contents = new LazyVariable<IReadableSegment>(GetContents);
        }
        
        /// <summary>
        /// Creates a new named data entry.
        /// </summary>
        /// <param name="name">The name of the entry.</param>
        /// <param name="contents">The data to store in the entry.</param>
        public ResourceData(string name, IReadableSegment contents)
            : this()
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <summary>
        /// Creates a new data entry defined by a numerical identifier..
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="contents">The data to store in the entry.</param>
        public ResourceData(uint id, IReadableSegment contents)
            : this()
        {
            Id = id;
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint Id
        {
            get;
            set;
        }

        /// <inheritdoc />
        bool IResourceEntry.IsDirectory => false;

        /// <inheritdoc />
        bool IResourceEntry.IsData => true;
        
        /// <inheritdoc />
        public IReadableSegment Contents
        {
            get => _contents.Value;
            set => _contents.Value = value;
        }
        
        /// <inheritdoc />
        public uint CodePage
        {
            get;
            set;
        }

        /// <summary>
        /// Obtains the contents of the data entry.
        /// </summary>
        /// <returns>The contents.</returns>
        /// <remarks>
        /// This method is called upon initializing the value for the <see cref="Contents"/> property.
        /// </remarks>
        protected virtual IReadableSegment GetContents()
        {
            return null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Data ({Name ?? Id.ToString()})";
        }
        
    }
}