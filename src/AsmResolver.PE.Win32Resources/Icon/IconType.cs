namespace AsmResolver.PE.Win32Resources.Icon;

/// <summary>
/// Provides members describing all possible icon resources that can be stored in a PE image.
/// </summary>
public enum IconType : ushort
{
    /// <summary>
    /// Indicates the icons stored in the group are ICO files.
    /// </summary>
    Icon = 1,

    /// <summary>
    /// Indicates the icons stored in the group are CUR files.
    /// </summary>
    Cursor = 2,
}
