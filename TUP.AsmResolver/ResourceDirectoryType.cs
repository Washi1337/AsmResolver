using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents the type of a <see cref="TUP.AsmResolver.ResourceDirectory"/>.
    /// </summary>
    public enum ResourceDirectoryType
    {
        /// <summary>
        /// Specifies that the directory contains cursor files.
        /// </summary>
        Cursor = 1,
        /// <summary>
        /// Specifies that the directory contains bitmap files.
        /// </summary>
        Bitmap = 2,
        /// <summary>
        /// Specifies that the directory contains icon files.
        /// </summary>
        Icon = 3,
        /// <summary>
        /// Specifies that the directory contains cursor files.
        /// </summary>
        Menu = 4,
        /// <summary>
        /// Specifies that the directory contains cursor files.
        /// </summary>
        Dialog = 5,
        /// <summary>
        /// Specifies that the directory contains string table files.
        /// </summary>
        String = 6,
        /// <summary>
        /// Specifies that the directory contains font directory files.
        /// </summary>
        FontDirectory = 7,
        /// <summary>
        /// Specifies that the directory contains font files.
        /// </summary>
        Font = 8,
        /// <summary>
        /// Specifies that the directory contains accelerator files.
        /// </summary>
        Accelerator = 9,
        /// <summary>
        /// Specifies that the directory contains RC data files.
        /// </summary>
        RCData = 10,
        /// <summary>
        /// Specifies that the directory contains message table files.
        /// </summary>
        MessageTable = 11,
        /// <summary>
        /// Specifies that the directory contains cursor group files.
        /// </summary>
        GroupCursor = 12,
        /// <summary>
        /// Specifies that the directory contains icon group files.
        /// </summary>
        GroupIcon = 14,
        /// <summary>
        /// Specifies that the directory contains version files.
        /// </summary>
        Version = 16,
        /// <summary>
        /// Specifies that the directory contains dialog include resource files.
        /// </summary>
        DialogInclude = 17,
        /// <summary>
        /// Specifies that the directory contains Plug-n-Play resource files.
        /// </summary>
        PlugPlay = 19,
        /// <summary>
        /// Specifies that the directory contains VXD resource files.
        /// </summary>
        VXD = 20,
        /// <summary>
        /// Specifies that the directory contains animated cursor files.
        /// </summary>
        AniCursor = 21,
        /// <summary>
        /// Specifies that the directory contains animated icon files.
        /// </summary>
        AniIcon = 22,
        /// <summary>
        /// Specifies that the directory contains Hyper Text Markup Language (HTML) files.
        /// </summary>
        HTML = 23,
        /// <summary>
        /// Specifies that the directory contains manifest files.
        /// </summary>
        Manifest = 24,
        /// <summary>
        /// Specifies that the directory contains other type of files.
        /// </summary>
        CustomNamed = 25
    }
}
