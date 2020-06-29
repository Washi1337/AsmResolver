using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.PE.Win32Resources.Icon
{
    /// <summary>
    /// Represents a view on win32 icon group resource directories and includes access to their icon entries. 
    /// </summary>
    public class IconResource : IWin32Resource
    {
        /// <summary>
        /// Used to keep track of icon groups.
        /// </summary>
        private readonly IDictionary<uint, IconGroupDirectory> _entries = new Dictionary<uint, IconGroupDirectory>();

        /// <summary>
        /// Obtains the icon group resources from the provided root win32 resources directory.
        /// </summary>
        /// <param name="rootDirectory">The root resources directory to extract the icon group from.</param>
        /// <returns>The icon group resources, or <c>null</c> if none was found.</returns>
        /// <exception cref="ArgumentException">Occurs when the resource data is not readable.</exception>
        public static IconResource FromDirectory(IResourceDirectory rootDirectory)
        {
            var groupIconDirectory = (IResourceDirectory) rootDirectory.Entries[ResourceDirectoryHelper.IndexOfResourceDirectoryType(rootDirectory, ResourceType.GroupIcon)];

            var iconDirectory = (IResourceDirectory)rootDirectory.Entries[ResourceDirectoryHelper.IndexOfResourceDirectoryType(rootDirectory, ResourceType.Icon)];

            if (groupIconDirectory is null || iconDirectory is null)
                return null;

            var result = new IconResource();

            foreach (var iconGroupResource in groupIconDirectory.Entries.OfType<IResourceDirectory>())
            {
                var dataEntry = iconGroupResource
                    ?.Entries
                    .OfType<IResourceData>()
                    .FirstOrDefault();

                if (dataEntry is null)
                    return null;

                if (!dataEntry.CanRead)
                    throw new ArgumentException("Icon group data is not readable.");
                
                result.AddEntry(iconGroupResource.Id, IconGroupDirectory.FromReader(dataEntry.CreateReader(), iconDirectory));
            }

            return result;
        }

        /// <summary>
        /// Gets or sets an icon group by its id.
        /// </summary>
        /// <param name="id">The id of the icon group.</param>
        public IconGroupDirectory this[uint id]
        {
            get => _entries[id];
            set
            {
                if (value is null)
                    _entries.Remove(id);
                else
                    _entries[id] = value;
            }
        }

        /// <summary>
        /// Adds or overrides the existing entry with the same id to the icon group resource. 
        /// </summary>
        /// <param name="id">The id to use for the entry.</param>
        /// <param name="entry">The entry to add.</param>
        public void AddEntry(uint id, IconGroupDirectory entry) => _entries[id] = entry;

        /// <summary>
        /// Gets a collection of entries stored in the icon group directory. 
        /// </summary>
        /// <returns>The collection of icon group entries.</returns>
        public IEnumerable<IconGroupDirectory> GetIconGroups() => _entries.Values;

        /// <inheritdoc />
        public void WriteToDirectory(IResourceDirectory rootDirectory)
        {
            // Find and remove old group icon directory.
            int groupIconIndex = ResourceDirectoryHelper.IndexOfResourceDirectoryType(rootDirectory, ResourceType.GroupIcon);

            if (groupIconIndex == -1)
                groupIconIndex = rootDirectory.Entries.Count;
            else
                rootDirectory.Entries.RemoveAt(groupIconIndex);

            // Find and remove old icon directory.
            int iconIndex = ResourceDirectoryHelper.IndexOfResourceDirectoryType(rootDirectory, ResourceType.Icon);

            if (iconIndex == -1)
                iconIndex = rootDirectory.Entries.Count;
            else
                rootDirectory.Entries.RemoveAt(iconIndex);

            if (groupIconIndex == iconIndex)
                iconIndex += 1;

            // Construct new directory.
            var newGroupIconDirectory = new ResourceDirectory(ResourceType.GroupIcon);
            foreach (var entry in _entries)
            {
                newGroupIconDirectory.Entries.Add(new ResourceDirectory(entry.Key)
                    {Entries = {new ResourceData(0u, entry.Value)}});
            }

            // Construct new directory.
            var newIconDirectory = new ResourceDirectory(ResourceType.Icon);
            foreach (var entry in _entries)
            {
                foreach (var icon in entry.Value.GetIconEntries())
                {
                    newIconDirectory.Entries.Add(new ResourceDirectory(icon.Item1.Id)
                        {Entries = {new ResourceData(0u, icon.Item2)}});
                }
            }

            // Insert.
            rootDirectory.Entries.Insert(groupIconIndex, newGroupIconDirectory);
            rootDirectory.Entries.Insert(iconIndex, newIconDirectory);
        }
    }
}