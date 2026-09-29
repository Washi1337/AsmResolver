namespace AsmResolver.PE.Configuration;

/// <summary>
/// Represents one entry in the control flow guard function table of a loader configuration data directory.
/// </summary>
/// <param name="EntryPoint">The entry point to the function.</param>
/// <param name="Flags">The flags associated to the function.</param>
public record struct ControlFlowGuardFunction(ISegmentReference EntryPoint, ControlFlowGuardFunctionFlags Flags);
