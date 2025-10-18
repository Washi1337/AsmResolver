namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol that describes information about the compiler that was used to compile the file.
/// </summary>
public abstract partial class CompileSymbol : CodeViewSymbol
{
    /// <summary>
    /// Initializes an empty compile symbol.
    /// </summary>
    protected CompileSymbol()
    {
    }

    /// <summary>
    /// Gets or sets the identifier of the language that was used to compile the file.
    /// </summary>
    public SourceLanguage Language
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets attributes associated to the compile symbol.
    /// </summary>
    public CompileAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the architecture the file is targeting.
    /// </summary>
    public CpuType Machine
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the front-end major version of the file.
    /// </summary>
    public ushort FrontEndMajorVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the front-end minor version of the file.
    /// </summary>
    public ushort FrontEndMinorVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the front-end build version of the file.
    /// </summary>
    public ushort FrontEndBuildVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the back-end major version of the file.
    /// </summary>
    public ushort BackEndMajorVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the back-end minor version of the file.
    /// </summary>
    public ushort BackEndMinorVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the back-end build version of the file.
    /// </summary>
    public ushort BackEndBuildVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the version of the compiler that was used to compile the file.
    /// </summary>
    [LazyProperty]
    public partial Utf8String CompilerVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the compiler version string.
    /// </summary>
    /// <returns>The compiler version.</returns>
    /// <remarks>
    /// This method is called upon the initialization of the <see cref="CompilerVersion"/> property.
    /// </remarks>
    protected virtual Utf8String GetCompilerVersion() => Utf8String.Empty;
}
