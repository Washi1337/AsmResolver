using System.Collections.Generic;

namespace AsmResolver.PE.Win32Resources.Menu;

/// <summary>
/// Represents a single item in a menu resource.
/// </summary>
public class MenuItem
{
    /// <summary>
    /// Gets or sets the type of the menu item.
    /// </summary>
    public MenuItemType Type { get; set; }

    /// <summary>
    /// Gets or sets the display text of the menu item.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the command identifier of the menu item.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Gets or sets the option flags for a standard menu item (mtOption).
    /// </summary>
    public ushort Flags { get; set; }

    /// <summary>
    /// Gets or sets the type flags for an extended menu item (dwType).
    /// </summary>
    public uint TypeFlags { get; set; }

    /// <summary>
    /// Gets or sets the state flags for an extended menu item (dwState).
    /// </summary>
    public uint State { get; set; }

    /// <summary>
    /// Gets or sets the help identifier for an extended popup menu item.
    /// </summary>
    public uint HelpId { get; set; }

    /// <summary>
    /// Gets the collection of child menu items for popup items.
    /// </summary>
    public IList<MenuItem> SubItems { get; } = new List<MenuItem>();
}
