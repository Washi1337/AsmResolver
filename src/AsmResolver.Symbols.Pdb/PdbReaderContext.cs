namespace AsmResolver.Symbols.Pdb;

/// <summary>
/// Provides a context in which a PDB image reader exists in. This includes the PDB image as well as reader parameters.
/// </summary>
public class PdbReaderContext
{
    /// <summary>
    /// Creates a new PDB reader context.
    /// </summary>
    /// <param name="parentImage">The image for which the data is to be read.</param>
    /// <param name="parameters">The parameters used while reading the PDB image.</param>
    public PdbReaderContext(SerializedPdbImage parentImage, PdbReaderParameters parameters)
    {
        ParentImage = parentImage;
        Parameters = parameters;
    }

    /// <summary>
    /// Gets the image for which the data is read.
    /// </summary>
    public SerializedPdbImage ParentImage
    {
        get;
    }

    /// <summary>
    /// Gets the parameters used for reading the data.
    /// </summary>
    public PdbReaderParameters Parameters
    {
        get;
    }
}
