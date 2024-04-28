using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Represents a single directory containing Win32 resources of a PE image.
    /// </summary>
    public interface IResourceDirectory : IResourceEntry
    {
        /// <summary>
        /// Gets the type of resources stored in the directory.
        /// </summary>
        ResourceType Type
        {
            get;
        }

        /// <summary>
        /// Gets or sets the flags of the directory.
        /// </summary>
        /// <remarks>
        /// This field is reserved and is usually set to zero.
        /// </remarks>
        uint Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time that the resource data was created by the compiler.
        /// </summary>
        uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version number of the directory.
        /// </summary>
        ushort MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor version number of the directory.
        /// </summary>
        ushort MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of entries that are stored in the directory.
        /// </summary>
        IList<IResourceEntry> Entries
        {
            get;
        }

        /// <summary>
        /// Looks up an entry in the directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the entry to lookup.</param>
        /// <returns>The entry.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Occurs when no entry with the provided identifier was found.
        /// </exception>
        IResourceEntry GetEntry(uint id);

        /// <summary>
        /// Looks up an directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the directory to lookup.</param>
        /// <returns>The directory.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Occurs when no directory with the provided identifier was found.
        /// </exception>
        IResourceDirectory GetDirectory(uint id);

        /// <summary>
        /// Looks up an directory by its resource type.
        /// </summary>
        /// <param name="type">The type of resources to lookup.</param>
        /// <returns>The directory.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Occurs when no directory with the provided identifier was found.
        /// </exception>
        IResourceDirectory GetDirectory(ResourceType type);

        /// <summary>
        /// Looks up a data entry in the directory by its unique identifier.
        /// </summary>
        /// <param name="id">The id of the data entry to lookup.</param>
        /// <returns>The data entry.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Occurs when no data entry with the provided identifier was found.
        /// </exception>
        IResourceData GetData(uint id);

        /// <summary>
        /// Attempts to looks up an entry in the directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the entry to lookup.</param>
        /// <param name="entry">The found entry, or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the entry was found, <c>false</c> otherwise.</returns>
        bool TryGetEntry(uint id, [NotNullWhen(true)] out IResourceEntry? entry);

        /// <summary>
        /// Attempts to looks up a directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the directory to lookup.</param>
        /// <param name="directory">The found directory, or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the directory was found, <c>false</c> otherwise.</returns>
        bool TryGetDirectory(uint id, [NotNullWhen(true)] out IResourceDirectory? directory);

        /// <summary>
        /// Attempts to looks up a directory by its resource type.
        /// </summary>
        /// <param name="type">The type of resources to lookup.</param>
        /// <param name="directory">The found directory, or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the directory was found, <c>false</c> otherwise.</returns>
        bool TryGetDirectory(ResourceType type, [NotNullWhen(true)] out IResourceDirectory? directory);

        /// <summary>
        /// Attempts to looks up a data entry in the directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the data entry to lookup.</param>
        /// <param name="data">The found data entry, or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the data entry was found, <c>false</c> otherwise.</returns>
        bool TryGetData(uint id, [NotNullWhen(true)] out IResourceData? data);

        /// <summary>
        /// Replaces an existing entry with the same ID with the provided entry, or inserts the new entry into the directory.
        /// </summary>
        /// <param name="entry">The entry to store in the directory.</param>
        void InsertOrReplaceEntry(IResourceEntry entry);

        /// <summary>
        /// Removes an entry in the directory by its unique identifier.
        /// </summary>
        /// <param name="id">The identifier of the entry to remove.</param>
        /// <returns><c>true</c> if the data entry was found and removed, <c>false</c> otherwise.</returns>
        bool RemoveEntry(uint id);

        /// <summary>
        /// Removes a directory in the directory by its resource type.
        /// </summary>
        /// <param name="type">The type of resources to remove.</param>
        /// <returns><c>true</c> if the directory was found and removed, <c>false</c> otherwise.</returns>
        bool RemoveEntry(ResourceType type);
    }
}
