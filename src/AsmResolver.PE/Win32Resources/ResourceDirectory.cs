using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.Collections;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Represents a single directory containing Win32 resources of a PE image.
    /// </summary>
    public class ResourceDirectory : IResourceEntry
    {
        private IList<IResourceEntry>? _entries;

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
        public ResourceDirectory? ParentDirectory
        {
            get;
            private set;
        }

        ResourceDirectory? IOwnedCollectionElement<ResourceDirectory>.Owner
        {
            get => ParentDirectory;
            set => ParentDirectory = value;
        }

        /// <inheritdoc />
        public string? Name
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

        /// <summary>
        /// Gets the type of resources stored in the directory.
        /// </summary>
        public ResourceType Type
        {
            get => (ResourceType) Id;
            set => Id = (uint) value;
        }

        /// <summary>
        /// Gets or sets the flags of the directory.
        /// </summary>
        /// <remarks>
        /// This field is reserved and is usually set to zero.
        /// </remarks>
        public uint Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time that the resource data was created by the compiler.
        /// </summary>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version number of the directory.
        /// </summary>
        public ushort MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor version number of the directory.
        /// </summary>
        public ushort MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of entries that are stored in the directory.
        /// </summary>
        public IList<IResourceEntry> Entries
        {
            get
            {
                if (_entries is null)
                    Interlocked.CompareExchange(ref _entries, GetEntries(), null);
                return _entries;
            }
        }

        private bool TryGetEntryIndex(uint id, out int index)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                var candidate = Entries[i];
                if (candidate.Id == id)
                {
                    index = i;
                    return true;
                }

                if (candidate.Id > id)
                {
                    index = i;
                    return false;
                }
            }

            index = Entries.Count;
            return false;
        }

        /// <summary>
        /// Looks up an entry in the directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the entry to lookup.</param>
        /// <returns>The entry.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Occurs when no entry with the provided identifier was found.
        /// </exception>
        public IResourceEntry GetEntry(uint id)
        {
            if (!TryGetEntry(id, out var entry))
                throw new KeyNotFoundException($"Directory does not contain an entry with id {id}.");
            return entry;
        }

        /// <summary>
        /// Looks up an directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the directory to lookup.</param>
        /// <returns>The directory.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Occurs when no directory with the provided identifier was found.
        /// </exception>
        public ResourceDirectory GetDirectory(uint id)
        {
            if (!TryGetDirectory(id, out var directory))
                throw new KeyNotFoundException($"Directory does not contain a directory with id {id}.");
            return directory;
        }

        /// <summary>
        /// Looks up an directory by its resource type.
        /// </summary>
        /// <param name="type">The type of resources to lookup.</param>
        /// <returns>The directory.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Occurs when no directory with the provided identifier was found.
        /// </exception>
        public ResourceDirectory GetDirectory(ResourceType type)
        {
            if (!TryGetDirectory(type, out var directory))
                throw new KeyNotFoundException($"Directory does not contain a directory of type {type}.");
            return directory;
        }

        /// <summary>
        /// Looks up a data entry in the directory by its unique identifier.
        /// </summary>
        /// <param name="id">The id of the data entry to lookup.</param>
        /// <returns>The data entry.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Occurs when no data entry with the provided identifier was found.
        /// </exception>
        public ResourceData GetData(uint id)
        {
            if (!TryGetData(id, out var data))
                throw new KeyNotFoundException($"Directory does not contain a data entry with id {id}.");
            return data;
        }

        /// <summary>
        /// Attempts to looks up an entry in the directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the entry to lookup.</param>
        /// <param name="entry">The found entry, or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the entry was found, <c>false</c> otherwise.</returns>
        public bool TryGetEntry(uint id, [NotNullWhen(true)] out IResourceEntry? entry)
        {
            if (!TryGetEntryIndex(id, out int index))
            {
                entry = null;
                return false;
            }

            entry = Entries[index];
            return true;
        }

        /// <summary>
        /// Attempts to looks up a directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the directory to lookup.</param>
        /// <param name="directory">The found directory, or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the directory was found, <c>false</c> otherwise.</returns>
        public bool TryGetDirectory(uint id, [NotNullWhen(true)] out ResourceDirectory? directory)
        {
            if (TryGetEntry(id, out var entry) && entry.IsDirectory)
            {
                directory = (ResourceDirectory) entry;
                return true;
            }

            directory = null;
            return false;
        }

        /// <summary>
        /// Attempts to looks up a directory by its resource type.
        /// </summary>
        /// <param name="type">The type of resources to lookup.</param>
        /// <param name="directory">The found directory, or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the directory was found, <c>false</c> otherwise.</returns>
        public bool TryGetDirectory(ResourceType type, [NotNullWhen(true)] out ResourceDirectory? directory) =>
            TryGetDirectory((uint) type, out directory);

        /// <summary>
        /// Attempts to looks up a data entry in the directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the data entry to lookup.</param>
        /// <param name="data">The found data entry, or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the data entry was found, <c>false</c> otherwise.</returns>
        public bool TryGetData(uint id, [NotNullWhen(true)] out ResourceData? data)
        {
            if (TryGetEntry(id, out var entry) && entry.IsData)
            {
                data = (ResourceData) entry;
                return true;
            }

            data = null;
            return false;
        }

        /// <summary>
        /// Replaces an existing entry with the same ID with the provided entry, or inserts the new entry into the directory.
        /// </summary>
        /// <param name="entry">The entry to store in the directory.</param>
        public void InsertOrReplaceEntry(IResourceEntry entry)
        {
            if (TryGetEntryIndex(entry.Id, out int index))
                Entries[index] = entry;
            else
                Entries.Insert(index, entry);
        }

        /// <summary>
        /// Removes an entry in the directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the entry to remove.</param>
        /// <returns><c>true</c> if the data entry was found and removed, <c>false</c> otherwise.</returns>
        public bool RemoveEntry(uint id)
        {
            if (!TryGetEntryIndex(id, out int index))
                return false;

            Entries.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes a directory in the directory by its resource type.
        /// </summary>
        /// <param name="type">The type of resources to remove.</param>
        /// <returns><c>true</c> if the directory was found and removed, <c>false</c> otherwise.</returns>
        public bool RemoveEntry(ResourceType type) => RemoveEntry((uint) type);

        /// <summary>
        /// Obtains the list of entries in the directory.
        /// </summary>
        /// <returns>The list of entries.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Entries"/> property.
        /// </remarks>
        protected virtual IList<IResourceEntry> GetEntries() =>
            new OwnedCollection<ResourceDirectory, IResourceEntry>(this);

        /// <inheritdoc />
        public override string ToString() => $"Directory ({Name ?? Id.ToString()})";
    }
}
