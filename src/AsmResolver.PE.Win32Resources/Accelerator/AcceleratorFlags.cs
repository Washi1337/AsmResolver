using System;

namespace AsmResolver.PE.Win32Resources.Accelerator;

/// <summary>
/// Provides flags for accelerator table entries.
/// </summary>
[Flags]
public enum AcceleratorFlags : ushort
{
    /// <summary>
    /// No flags set. The key is an ASCII character.
    /// </summary>
    None = 0,

    /// <summary>
    /// The key is a virtual-key code.
    /// </summary>
    VirtKey = 0x01,

    /// <summary>
    /// No top-level menu item is highlighted when the accelerator is used.
    /// </summary>
    NoInvert = 0x02,

    /// <summary>
    /// The SHIFT key must be held down when the accelerator key is pressed.
    /// </summary>
    Shift = 0x04,

    /// <summary>
    /// The CTRL key must be held down when the accelerator key is pressed.
    /// </summary>
    Control = 0x08,

    /// <summary>
    /// The ALT key must be held down when the accelerator key is pressed.
    /// </summary>
    Alt = 0x10,
}
