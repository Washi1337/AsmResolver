namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Provides parameters for configuring the reading process of a PDB image.
/// </summary>
public class PdbReaderParameters
{
    /// <summary>
    /// Creates new PDB reader parameters.
    /// </summary>
    public PdbReaderParameters()
        : this(ThrowErrorListener.Instance)
    {
    }

    /// <summary>
    /// Creates new PDB reader parameters with the provided error listener object.
    /// </summary>
    /// <param name="errorListener">The object used for receiving parser errors.</param>
    public PdbReaderParameters(IErrorListener errorListener)
    {
        ErrorListener = errorListener;
    }

    /// <summary>
    /// Gets or sets the object responsible for receiving and processing parser errors.
    /// </summary>
    public IErrorListener ErrorListener
    {
        get;
        set;
    }
}
