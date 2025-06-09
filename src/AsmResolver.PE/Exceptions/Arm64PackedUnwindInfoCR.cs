namespace AsmResolver.PE.Exceptions;

/// <summary>
/// Defines all variants for frame chain setup code in an ARM64 function.
/// </summary>
public enum Arm64PackedUnwindInfoCR : byte
{
    /// <summary>
    /// Indicates an unchained function where LR is not saved on the stack.
    /// </summary>
    UnchainedFunctionNoStackSave = 0b00,

    /// <summary>
    /// Indicates an unchained function where LR is saved on the stack.
    /// </summary>
    UnchainedFunctionStackSave = 0b01,

    /// <summary>
    /// Indicates a chained function with a <c>pacibsp</c> signed return address.
    /// </summary>
    ChainedFunctionPacibsp = 0b10,

    /// <summary>
    /// Indicates a chained function that uses a store/load instruction pair in the prolog/epilog for the LR register.
    /// </summary>
    ChainedFunctionStoreLoad = 0b11
}
