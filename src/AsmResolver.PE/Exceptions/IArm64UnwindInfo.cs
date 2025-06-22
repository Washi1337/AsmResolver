namespace AsmResolver.PE.Exceptions;

/// <summary>
/// Provides members describing unwind information for an ARM64 runtime function.
/// </summary>
public interface IArm64UnwindInfo : IUnwindInfo
{
    /// <summary>
    /// Gets the raw contents of the data field for the RUNTIME_FUNCTION entry.
    /// </summary>
    uint FieldValue
    {
        get;
    }

    /// <summary>
    /// Gets the number of instructions that this unwind info is protecting.
    /// </summary>
    uint FunctionLength
    {
        get;
    }
}
