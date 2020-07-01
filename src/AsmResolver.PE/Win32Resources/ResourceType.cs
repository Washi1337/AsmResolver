namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides members for well-known win32 resource types.
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// Indicates the resource contains a hardware-dependent cursor table.
        /// </summary>
        Cursor = 1,
        
        /// <summary>
        /// Indicates the resource contains a bitmap.
        /// </summary>
        Bitmap = 2,
        
        /// <summary>
        /// Indicates the resource contains a hardware-dependent icon.
        /// </summary>
        Icon = 3,
        
        /// <summary>
        /// Indicates the resource contains a menu.
        /// </summary>
        Menu = 4,
        
        /// <summary>
        /// Indicates the resource contains a dialog box.
        /// </summary>
        Dialog = 5,
        
        /// <summary>
        /// Indicates the resource contains a string table.
        /// </summary>
        String = 6,
        
        /// <summary>
        /// Indicates the resource contains a directory of fonts.
        /// </summary>
        FontDirectory = 7,
        
        /// <summary>
        /// Indicates the resource contains a font.
        /// </summary>
        Font = 8,
        
        /// <summary>
        /// Indicates the resource contains an accelerator resource.
        /// </summary>
        Accelerator = 9,
        
        /// <summary>
        /// Indicates the resource contains an application-defined resource.
        /// </summary>
        RcData = 10,
        
        /// <summary>
        /// Indicates the resource contains a message-table entry.
        /// </summary>
        MessageTable = 11,
        
        /// <summary>
        /// Indicates the resource contains a hardware-independent cursor.
        /// </summary>
        GroupCursor = Cursor + 11,
        
        /// <summary>
        /// Indicates the resource contains a hardware-independent icon.
        /// </summary>
        GroupIcon = Icon + 11,
        
        /// <summary>
        /// Indicates the resource contains version information.
        /// </summary>
        Version = 16,
        
        /// <summary>
        /// Indicates the resource contains associations between a string with an .rc file.
        /// Typically, the string is the name of the header file that provides symbolic names.
        /// The resource compiler parses the string but otherwise ignores the value.
        /// </summary>
        DialogInclude = 17,
        
        /// <summary>
        /// Indicates the resource contains Plug and Play.
        /// </summary>
        PlugPlay = 18,
        
        /// <summary>
        /// Indicates the resource contains virtual device driver information.
        /// </summary>
        Vxd = 20,
        
        /// <summary>
        /// Indicates the resource contains animated cursors.
        /// </summary>
        AniCursor = 21,
        
        /// <summary>
        /// Indicates the resource contains animated icons.
        /// </summary>
        AniIcon = 22,
        
        /// <summary>
        /// Indicates the resource contains an HTML file.
        /// </summary>
        Html = 23,
        
        /// <summary>
        /// Indicates the resource contains an assembly manifest.
        /// </summary>
        Manifest = 24,
    }
}