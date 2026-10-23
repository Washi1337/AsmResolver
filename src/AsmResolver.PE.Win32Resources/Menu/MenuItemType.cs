namespace AsmResolver.PE.Win32Resources.Menu;

/// <summary>
/// Provides members describing the type of a menu item.
/// </summary>
public enum MenuItemType
{
    /// <summary>
    /// Indicates the menu item is a normal command item.
    /// </summary>
    Normal,

    /// <summary>
    /// Indicates the menu item is a popup (submenu) that contains child items.
    /// </summary>
    Popup,

    /// <summary>
    /// Indicates the menu item is a separator.
    /// </summary>
    Separator
}
