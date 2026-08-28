using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsmResolver.PE.Win32Resources.Html;

/// <summary>
/// Represents a single HTML resource entry stored in the Win32 resources of a PE image.
/// </summary>
public class HtmlResource : IWin32Resource
{
    private ISegment? _data;

    /// <summary>
    /// Creates a new HTML resource with the specified identifier.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    public HtmlResource(uint id)
        : this(id, 0, (ISegment?) null)
    {
    }

    /// <summary>
    /// Creates a new HTML resource with the specified identifier and content.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="lcid">The language identifier.</param>
    /// <param name="data">The raw HTML data.</param>
    public HtmlResource(uint id, uint lcid, ISegment? data)
    {
        Id = id;
        Lcid = lcid;
        _data = data;
    }

    /// <summary>
    /// Creates a new HTML resource with the specified identifier and string content.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="lcid">The language identifier.</param>
    /// <param name="content">The HTML content as a string.</param>
    public HtmlResource(uint id, uint lcid, string content)
    {
        Id = id;
        Lcid = lcid;
        Content = content;
    }

    /// <summary>
    /// Gets the numeric identifier of the HTML resource.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets or sets the language identifier of the HTML resource.
    /// </summary>
    public uint Lcid { get; set; }

    /// <summary>
    /// Gets or sets the raw HTML data segment.
    /// </summary>
    public ISegment? Data
    {
        get => _data;
        set => _data = value;
    }

    /// <summary>
    /// Gets or sets the HTML content as a UTF-8 string.
    /// </summary>
    public string? Content
    {
        get
        {
            if (_data is null)
                return null;
            byte[] bytes = _data.WriteIntoArray();
            return Encoding.UTF8.GetString(bytes);
        }
        set
        {
            _data = value is not null
                ? new DataSegment(Encoding.UTF8.GetBytes(value))
                : null;
        }
    }

    /// <summary>
    /// Reads all HTML resources from the provided root resource directory of a PE image.
    /// </summary>
    /// <param name="rootDirectory">The root resource directory.</param>
    /// <returns>The HTML resources found, or an empty collection if none were present.</returns>
    public static IEnumerable<HtmlResource> FromDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Html, out var htmlDirectory))
            yield break;

        foreach (var idDirectory in htmlDirectory.Entries.OfType<ResourceDirectory>())
        {
            foreach (var language in idDirectory.Entries.OfType<ResourceData>())
            {
                yield return new HtmlResource(
                    idDirectory.Id,
                    language.Id,
                    language.Contents
                );
            }
        }
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Html, out var htmlDirectory))
        {
            htmlDirectory = new ResourceDirectory(ResourceType.Html);
            rootDirectory.InsertOrReplaceEntry(htmlDirectory);
        }

        if (!htmlDirectory.TryGetDirectory(Id, out var idDirectory))
        {
            idDirectory = new ResourceDirectory(Id);
            htmlDirectory.InsertOrReplaceEntry(idDirectory);
        }

        byte[] bytes = _data?.WriteIntoArray() ?? [];
        idDirectory.InsertOrReplaceEntry(new ResourceData(Lcid, new DataSegment(bytes)));
    }
}
