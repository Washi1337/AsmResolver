namespace AsmResolver.PE.Win32Resources.Accelerator;

/// <summary>
/// Represents a single entry in an accelerator table resource.
/// </summary>
public class AcceleratorEntry
{
    /// <summary>
    /// Gets or sets the accelerator flags.
    /// </summary>
    public AcceleratorFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets the virtual-key code or ASCII character.
    /// </summary>
    public ushort Key { get; set; }

    /// <summary>
    /// Gets or sets the command identifier.
    /// </summary>
    public uint Command { get; set; }
}
