namespace AsmResolver.PE.DotNet.Metadata.Tables;

/// <summary>
/// Provides extension methods to the <see cref="TableIndex"/> enumeration.
/// </summary>
public static class TableIndexExtensions
{
    /// <summary>
    /// Determines whether the provided index is a valid table index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns><c>true</c> if valid, <c>false</c> otherwise.</returns>
    public static bool IsValidTableIndex(this TableIndex index)
    {
        return index is >= TableIndex.Module and <= TableIndex.GenericParamConstraint
            or >= TableIndex.Document and <= TableIndex.CustomDebugInformation
            or TableIndex.String;
    }
}
