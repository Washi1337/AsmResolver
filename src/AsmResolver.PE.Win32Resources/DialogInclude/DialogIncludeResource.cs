using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsmResolver.PE.Win32Resources.DialogInclude;

/// <summary>
/// Represents a dialog include resource (RT_DLGINCLUDE) stored in the Win32 resources of a PE image.
/// </summary>
public class DialogIncludeResource : IWin32Resource
{
    /// <summary>
    /// Creates a new dialog include resource with the specified identifier.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    public DialogIncludeResource(uint id)
        : this(id, 0, string.Empty)
    {
    }

    /// <summary>
    /// Creates a new dialog include resource with the specified identifier and header file name.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="lcid">The language identifier.</param>
    /// <param name="headerFileName">The header file name.</param>
    public DialogIncludeResource(uint id, uint lcid, string headerFileName)
    {
        Id = id;
        Lcid = lcid;
        HeaderFileName = headerFileName;
    }

    /// <summary>
    /// Gets the numeric identifier of the dialog include resource.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets or sets the language identifier of the dialog include resource.
    /// </summary>
    public uint Lcid { get; set; }

    /// <summary>
    /// Gets or sets the header file name referenced by this dialog include resource.
    /// </summary>
    public string HeaderFileName { get; set; }

    /// <summary>
    /// Reads all dialog include resources from the provided root resource directory of a PE image.
    /// </summary>
    /// <param name="rootDirectory">The root resource directory.</param>
    /// <returns>The dialog include resources found, or an empty collection if none were present.</returns>
    public static IEnumerable<DialogIncludeResource> FromDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.DialogInclude, out var dlgIncludeDirectory))
            yield break;

        foreach (var idDirectory in dlgIncludeDirectory.Entries.OfType<ResourceDirectory>())
        {
            foreach (var language in idDirectory.Entries.OfType<ResourceData>())
            {
                byte[] bytes = language.Contents?.WriteIntoArray() ?? [];
                string headerFileName = Encoding.ASCII.GetString(bytes).TrimEnd('\0');

                yield return new DialogIncludeResource(
                    idDirectory.Id,
                    language.Id,
                    headerFileName
                );
            }
        }
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.DialogInclude, out var dlgIncludeDirectory))
        {
            dlgIncludeDirectory = new ResourceDirectory(ResourceType.DialogInclude);
            rootDirectory.InsertOrReplaceEntry(dlgIncludeDirectory);
        }

        if (!dlgIncludeDirectory.TryGetDirectory(Id, out var idDirectory))
        {
            idDirectory = new ResourceDirectory(Id);
            dlgIncludeDirectory.InsertOrReplaceEntry(idDirectory);
        }

        byte[] bytes = Encoding.ASCII.GetBytes(HeaderFileName + '\0');
        idDirectory.InsertOrReplaceEntry(new ResourceData(Lcid, new DataSegment(bytes)));
    }
}
