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
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides a basic implementation of a resource directory that can be initialized and added to another resource
    /// directory or used as a root resource directory of a PE image.
    /// </summary>
    public class ResourceDirectory : IResourceDirectory
    {
        private IList<IResourceEntry> _entries;

        /// <summary>
        /// Initializes a new resource directory entry.
        /// </summary>
        protected ResourceDirectory()
        {
        }
        
        /// <summary>
        /// Creates a new named resource directory. 
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        public ResourceDirectory(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        
        /// <summary>
        /// Creates a new resource directory defined by a numeric identifier. 
        /// </summary>
        /// <param name="id">The identifier.</param>
        public ResourceDirectory(uint id)
        {
            Id = id;
        }

        /// <summary>
        /// Creates a new resource directory defined by its resource type. 
        /// </summary>
        /// <param name="type">The type.</param>
        public ResourceDirectory(ResourceType type)
        {
            Type = type;
        }

        /// <inheritdoc />
        public IResourceDirectory ParentDirectory
        {
            get;
            private set;
        }

        IResourceDirectory IOwnedCollectionElement<IResourceDirectory>.Owner
        {
            get => ParentDirectory;
            set => ParentDirectory = value;
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
        bool IResourceEntry.IsDirectory => true;

        /// <inheritdoc />
        bool IResourceEntry.IsData => false;

        /// <inheritdoc />
        public ResourceType Type
        {
            get => (ResourceType) Id;
            set => Id = (uint) value;
        }

        /// <inheritdoc />
        public uint Characteristics
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ushort MajorVersion
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ushort MinorVersion
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IList<IResourceEntry> Entries
        {
            get
            {
                if (_entries is null)
                    Interlocked.CompareExchange(ref _entries, GetEntries(), null);
                return _entries;
            }
        }

        /// <summary>
        /// Obtains the list of entries in the directory.
        /// </summary>
        /// <returns>The list of entries.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Entries"/> property.
        /// </remarks> 
        protected virtual IList<IResourceEntry> GetEntries() => 
            new OwnedCollection<IResourceDirectory, IResourceEntry>(this);

        /// <inheritdoc />
        public override string ToString() => $"Directory ({Name ?? Id.ToString()})";
    }
}