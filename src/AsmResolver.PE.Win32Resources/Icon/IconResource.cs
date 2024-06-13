using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Icon;

/// <summary>
/// Provides a high level view on icon and cursor resources stored in a PE image.
/// </summary>
public class IconResource : IWin32Resource
{
    /// <summary>
    /// Creates a new icon resource of the provided icon type.
    /// </summary>
    /// <param name="type">The icon type.</param>
    public IconResource(IconType type)
    {
        Type = type;
    }

    /// <summary>
    /// Gets the icon type the resource is storing.
    /// </summary>
    public IconType Type
    {
        get;
    }

    /// <summary>
    /// Gets the icon groups stored in the resource.
    /// </summary>
    public IList<IconGroup> Groups
    {
        get;
    } = new List<IconGroup>();

    /// <summary>
    /// Reads icons from the provided root resource directory of a PE image.
    /// </summary>
    /// <param name="rootDirectory">The root resource directory.</param>
    /// <param name="type">The icon type.</param>
    /// <returns>The resource, or <c>null</c> if no resource of the provided type was present.</returns>
    public static IconResource? FromDirectory(ResourceDirectory rootDirectory, IconType type)
    {
        var (groupType, entryType) = GetResourceDirectoryTypes(type);

        if (!rootDirectory.TryGetDirectory(groupType, out var groupDirectory))
            return null;

        if (!rootDirectory.TryGetDirectory(entryType, out var iconsDirectory))
            return null;

        var result = new IconResource(type);

        foreach (var group in groupDirectory.Entries.OfType<ResourceDirectory>())
        {
            foreach (var language in group.Entries.OfType<ResourceData>())
            {
                result.Groups.Add(new SerializedIconGroup(
                    group.Id,
                    language.Id,
                    iconsDirectory,
                    language.CreateReader()
                ));
            }
        }

        return result;
    }

    private static (ResourceType Group, ResourceType Entry) GetResourceDirectoryTypes(IconType type)
    {
        return type switch
        {
            IconType.Icon => (ResourceType.GroupIcon, ResourceType.Icon),
            IconType.Cursor => (ResourceType.GroupCursor, ResourceType.Cursor),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }

    /// <summary>
    /// Gets an icon group by its numeric identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>The group.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when the group was not present.</exception>
    public IconGroup GetGroup(uint id)
    {
        return TryGetGroup(id, out var group)
            ? group
            : throw new ArgumentOutOfRangeException($"Icon group {id} does not exist.");
    }

    /// <summary>
    /// Gets an icon group by its string identifier.
    /// </summary>
    /// <param name="name">The identifier.</param>
    /// <returns>The group.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when the group was not present.</exception>
    public IconGroup GetGroup(string name)
    {
        return TryGetGroup(name, out var group)
            ? group
            : throw new ArgumentOutOfRangeException($"Icon group '{name}' does not exist.");
    }

    /// <summary>
    /// Gets an icon group by its numeric identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="lcid">The language identifier.</param>
    /// <returns>The group.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when the group was not present.</exception>
    public IconGroup GetGroup(uint id, uint lcid)
    {
        return TryGetGroup(id, lcid, out var group)
            ? group
            : throw new ArgumentOutOfRangeException($"Icon group {id} (language: {lcid}) does not exist.");
    }

    /// <summary>
    /// Gets an icon group by its string identifier.
    /// </summary>
    /// <param name="name">The identifier.</param>
    /// <param name="lcid">The language identifier.</param>
    /// <returns>The group.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when the group was not present.</exception>
    public IconGroup GetGroup(string name, uint lcid)
    {
        return TryGetGroup(name, lcid, out var group)
            ? group
            : throw new ArgumentOutOfRangeException($"Icon group {name} (language: {lcid}) does not exist.");
    }

    /// <summary>
    /// Attempts to get an icon group by its numeric identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="group">The group, or <c>null</c> if none was found.</param>
    /// <returns><c>true</c> if the group was found, <c>false</c> otherwise.</returns>
    public bool TryGetGroup(uint id, [NotNullWhen(true)] out IconGroup? group)
    {
        foreach (var g in Groups)
        {
            if (g.Id == id)
            {
                group = g;
                return true;
            }
        }

        group = null;
        return false;
    }

    /// <summary>
    /// Attempts to get an icon group by its numeric identifier and language specifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="lcid">The language identifier.</param>
    /// <param name="group">The group, or <c>null</c> if none was found.</param>
    /// <returns><c>true</c> if the group was found, <c>false</c> otherwise.</returns>
    public bool TryGetGroup(uint id, uint lcid, [NotNullWhen(true)] out IconGroup? group)
    {
        foreach (var g in Groups)
        {
            if (g.Id == id && g.Lcid == lcid)
            {
                group = g;
                return true;
            }
        }

        group = null;
        return false;
    }

    /// <summary>
    /// Attempts to get an icon group by its string identifier.
    /// </summary>
    /// <param name="name">The identifier.</param>
    /// <param name="group">The group, or <c>null</c> if none was found.</param>
    /// <returns><c>true</c> if the group was found, <c>false</c> otherwise.</returns>
    public bool TryGetGroup(string name, [NotNullWhen(true)] out IconGroup? group)
    {
        foreach (var g in Groups)
        {
            if (g.Name == name)
            {
                group = g;
                return true;
            }
        }

        group = null;
        return false;
    }

    /// <summary>
    /// Attempts to get an icon group by its string identifier and language specifier.
    /// </summary>
    /// <param name="name">The identifier.</param>
    /// <param name="lcid">The language identifier.</param>
    /// <param name="group">The group, or <c>null</c> if none was found.</param>
    /// <returns><c>true</c> if the group was found, <c>false</c> otherwise.</returns>
    public bool TryGetGroup(string name, uint lcid, [NotNullWhen(true)] out IconGroup? group)
    {
        foreach (var g in Groups)
        {
            if (g.Name == name && g.Lcid == lcid)
            {
                group = g;
                return true;
            }
        }

        group = null;
        return false;
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        var (groupType, entryType) = GetResourceDirectoryTypes(Type);

        // We're always replacing the existing directories with new ones.
        var groupDirectory = new ResourceDirectory(groupType);
        var entryDirectory = new ResourceDirectory(entryType);

        var groupDirectories = new Dictionary<object, ResourceDirectory>();
        foreach (var group in Groups)
        {
            // Create the group dir if it doesn't exist.
            object groupId = (object?) group.Name ?? group.Id;
            if (!groupDirectories.TryGetValue(groupId, out var directory))
            {
                directory = group.Name is not null
                    ? new ResourceDirectory(group.Name)
                    : new ResourceDirectory(group.Id);

                groupDirectories.Add(groupId, directory);
                groupDirectory.Entries.Add(directory);
            }

            // Serialize the group.
            using var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);
            group.Write(entryDirectory, writer);

            // Add the group.
            directory.Entries.Add(new ResourceData(group.Lcid, new DataSegment(stream.ToArray())));
        }

        // Insert into the root win32 dir of the PE image.
        rootDirectory.InsertOrReplaceEntry(groupDirectory);
        rootDirectory.InsertOrReplaceEntry(entryDirectory);
    }
}
