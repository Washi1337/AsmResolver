namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Represents a single source file that was used to compile a module or compiland in a binary.
/// </summary>
public class PdbSourceFile
{
    /// <summary>
    /// Creates a new PDB source file.
    /// </summary>
    /// <param name="fileName">The file name of the source file.</param>
    public PdbSourceFile(Utf8String fileName)
    {
        FileName = fileName;
    }

    /// <summary>
    /// Gets or sets the file name of the source file.
    /// </summary>
    public Utf8String FileName
    {
        get;
        set;
    }

    /// <inheritdoc />
    public override string ToString() => FileName;
}
