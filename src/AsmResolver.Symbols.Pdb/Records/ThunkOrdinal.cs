namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Describes the type of data that is stored within a thunk symbol.
/// </summary>
public enum ThunkOrdinal : byte
{
#pragma warning disable CS1591
    NoType,
    Adjustor,
    VCall,
    PCode,
    Load,
    IncrementalTrampoline,
    BranchIslandTrampoline
#pragma warning restore CS1591
}
