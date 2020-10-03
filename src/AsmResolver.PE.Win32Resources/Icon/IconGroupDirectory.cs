using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.PE.Win32Resources.Icon
{
    /// <summary>
    /// Represents a single win32 icon group directory.
    /// </summary>
    public class IconGroupDirectory : SegmentBase
    {
        /// <summary>
        /// Used to keep track of all icon entries associated with this icon group.
        /// </summary>
        private readonly IDictionary<ushort, (IconGroupDirectoryEntry, IconEntry)> _entries = new Dictionary<ushort, (IconGroupDirectoryEntry, IconEntry)>();

        /// <summary>
        /// Reads a single icon group directory.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="iconResourceDirectory">The icon resource directory used to extract associated icon entries from.</param>
        /// <returns>The icon group directory.</returns>
        public static IconGroupDirectory FromReader(IBinaryStreamReader reader, IResourceDirectory iconResourceDirectory)
        {
            var result = new IconGroupDirectory
            {
                Offset = reader.Offset,
                Rva = reader.Rva,
                Reserved = reader.ReadUInt16(),
                Type = reader.ReadUInt16(),
                Count = reader.ReadUInt16()
            };

            for (int i = 0; i < result.Count; i++)
            {
                var (iconGroupDirectoryEntry, iconEntry) = ReadNextEntry(reader, iconResourceDirectory);
                result.AddEntry(iconGroupDirectoryEntry, iconEntry);
            }

            return result;
        }

        private static (IconGroupDirectoryEntry, IconEntry) ReadNextEntry(IBinaryStreamReader reader, IResourceDirectory iconResourceDirectory)
        {
            var entry = IconGroupDirectoryEntry.FromReader(reader);

            // search for icon reference in icon resource directory
            var iconDirectory = iconResourceDirectory
                .Entries
                .OfType<IResourceDirectory>()
                .FirstOrDefault(d => d.Id == entry.Id);

            var iconDataEntry = iconDirectory
                ?.Entries
                .OfType<IResourceData>()
                .FirstOrDefault();

            if (iconDataEntry is null)
                throw new ArgumentException("Non-existent icon reference.");

            var entry2 = IconEntry.FromReader(iconDataEntry.CreateReader());

            return (entry, entry2);
        }

        /// <summary>
        /// Gets or sets an icon entry by its id.
        /// </summary>
        /// <param name="id">The id of the icon entry.</param>
        public (IconGroupDirectoryEntry, IconEntry) this[ushort id]
        {
            get => _entries[id];
            set
            {
                if (value.Item1 is null)
                    _entries.Remove(id);
                else
                    _entries[id] = value;
            }
        }

        /// <summary>
        /// Adds or overrides the existing entry with the same id to the group icon resource. 
        /// </summary>
        /// <param name="iconGroupDirectoryEntry">The icon group directory entry to add.</param>
        /// <param name="iconEntry">The icon entry to add.</param>
        public void AddEntry(IconGroupDirectoryEntry iconGroupDirectoryEntry, IconEntry iconEntry) =>
            _entries[iconGroupDirectoryEntry.Id] = (iconGroupDirectoryEntry, iconEntry);

        /// <summary>
        /// Removes an existing entry with a specified id from the group icon resource.
        /// </summary>
        /// <param name="id">The group icon id.</param>
        /// <returns><c>True</c> if the icon resource was successfully removed, otherwise <c>false</c>.</returns>
        public bool RemoveEntry(ushort id) => _entries.Remove(id);

        /// <summary>
        /// Gets a collection of icon entries stored in the icon group. 
        /// </summary>
        public IEnumerable<(IconGroupDirectoryEntry, IconEntry)> GetIconEntries() => _entries.Values;

        /// <summary>
        /// The size of the header in bytes.
        /// </summary>
        public const uint HeaderSize = sizeof(ushort) // Reserved
                                          + sizeof(ushort) // Type
                                          + sizeof(ushort); // Count
        
        /// <summary>
        /// Reserved field. Must be 0.
        /// </summary>
        public ushort Reserved
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the resource type.
        /// </summary>
        public ushort Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the amount of entries in this resource directory.
        /// </summary>
        public ushort Count
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            uint totalEntrySize = 0;
            foreach (var entry in _entries)
                totalEntrySize += entry.Value.Item1.GetPhysicalSize();

            return HeaderSize + totalEntrySize;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt16(Reserved);
            writer.WriteUInt16(Type);
            writer.WriteUInt16(Count);
            foreach (var entry in _entries)
                entry.Value.Item1.Write(writer);
        }
    }
}