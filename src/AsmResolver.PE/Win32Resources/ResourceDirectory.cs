using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public IResourceDirectory? ParentDirectory
        {
            get;
            private set;
        }

        IResourceDirectory? IOwnedCollectionElement<IResourceDirectory>.Owner
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

        private int GetEntryIndex(uint id)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                var candidate = Entries[i];
                if (candidate.Id == id)
                    return i;
            }

            return -1;
        }

        /// <inheritdoc />
        public IResourceEntry GetEntry(uint id)
        {
            if (!TryGetEntry(id, out var entry))
                throw new KeyNotFoundException($"Directory does not contain an entry with id {id}.");
            return entry;
        }

        /// <inheritdoc />
        public IResourceDirectory GetDirectory(uint id)
        {
            if (!TryGetDirectory(id, out var directory))
                throw new KeyNotFoundException($"Directory does not contain a directory with id {id}.");
            return directory;
        }

        /// <inheritdoc />
        public IResourceDirectory GetDirectory(ResourceType type)
        {
            if (!TryGetDirectory(type, out var directory))
                throw new KeyNotFoundException($"Directory does not contain a directory of type {type}.");
            return directory;
        }

        /// <inheritdoc />
        public IResourceData GetData(uint id)
        {
            if (!TryGetData(id, out var data))
                throw new KeyNotFoundException($"Directory does not contain a data entry with id {id}.");
            return data;
        }

        /// <inheritdoc />
        public bool TryGetEntry(uint id, [NotNullWhen(true)] out IResourceEntry? entry)
        {
            int index = GetEntryIndex(id);
            if (index != -1)
            {
                entry = Entries[index];
                return true;
            }

            entry = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetDirectory(uint id, [NotNullWhen(true)] out IResourceDirectory? directory)
        {
            if (TryGetEntry(id, out var entry) && entry.IsDirectory)
            {
                directory = (IResourceDirectory) entry;
                return true;
            }

            directory = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetDirectory(ResourceType type, [NotNullWhen(true)] out IResourceDirectory? directory) =>
            TryGetDirectory((uint) type, out directory);

        /// <inheritdoc />
        public bool TryGetData(uint id, [NotNullWhen(true)] out IResourceData? data)
        {
            if (TryGetEntry(id, out var entry) && entry.IsData)
            {
                data = (IResourceData) entry;
                return true;
            }

            data = null;
            return false;
        }

        /// <inheritdoc />
        public void AddOrReplaceEntry(IResourceEntry entry)
        {
            int index = GetEntryIndex(entry.Id);
            if (index == -1)
                Entries.Add(entry);
            else
                Entries[index] = entry;
        }

        /// <inheritdoc />
        public bool RemoveEntry(uint id)
        {
            int index = GetEntryIndex(id);
            if (index == -1)
                return false;

            Entries.RemoveAt(index);
            return true;
        }

        /// <inheritdoc />
        public bool RemoveEntry(ResourceType type) => RemoveEntry((uint) type);

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
