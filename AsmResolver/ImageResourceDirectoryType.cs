namespace AsmResolver
{
    /// <summary>
    /// Provides a set of standard native resource types that can be present in a resource directory of an assembly image.
    /// </summary>
    public enum ImageResourceDirectoryType
    {
        Custom = 0,
        Cursor = 1,
        Bitmap = 2,
        Icon = 3,
        Menu = 4,
        Dialog = 5,
        StringTable = 6,
        FontDirectory = 7,
        Font = 8,
        Accelerator = 9,
        Unformatted = 10,
        MessageTable = 11,
        GroupCursor = 12,
        GroupIcon = 14,
        VersionInfo = 16,
        Configuration = 24,
    }
}