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

using AsmResolver.Lazy;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// When overriden from this class, represents a single data entry in a resource directory.
    /// </summary>
    public abstract class ResourceDataBase : IResourceDirectoryEntry
    {
        private readonly LazyVariable<IReadableSegment> _contents;

        protected ResourceDataBase()
        {
            _contents = new LazyVariable<IReadableSegment>(GetContents);
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
        bool IResourceDirectoryEntry.IsDirectory => false;

        /// <inheritdoc />
        bool IResourceDirectoryEntry.IsData => true;
        
        /// <summary>
        /// Gets or sets the raw contents of the data entry.
        /// </summary>
        public IReadableSegment Contents
        {
            get => _contents.Value;
            set => _contents.Value = value;
        }
        
        /// <summary>
        /// Gets or sets the code page that is used to decode code point values within the resource data. 
        /// </summary>
        /// <remarks>
        /// Typically, the code page would be the Unicode code page.
        /// </remarks>
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
        protected abstract IReadableSegment GetContents();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Data ({Name ?? Id.ToString()})";
        }
        
    }
}