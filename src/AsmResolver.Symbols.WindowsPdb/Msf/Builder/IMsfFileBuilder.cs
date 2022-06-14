namespace AsmResolver.Symbols.WindowsPdb.Msf.Builder;

/// <summary>
/// Provides members for constructing new MSF files.
/// </summary>
public interface IMsfFileBuilder
{
    /// <summary>
    /// Reconstructs a new writable MSF file from an instance of <see cref="MsfFile"/>.
    /// </summary>
    /// <param name="file">The file to reconstruct.</param>
    /// <returns>The reconstructed buffer.</returns>
    MsfFileBuffer CreateFile(MsfFile file);
}
