namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Describes the type of data that is stored within a thunk symbol.
/// </summary>
public enum ThunkOrdinal : byte
{
    NoType,
    Adjustor,
    VCall,
    PCode,
    Load,
    IncrementalTrampoline,
    BranchIslandTrampoline
}
