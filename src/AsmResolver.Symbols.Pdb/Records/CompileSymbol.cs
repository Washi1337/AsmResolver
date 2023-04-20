namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol that describes information about the compiler that was used to compile the file.
/// </summary>
public abstract class CompileSymbol : CodeViewSymbol
{
    private readonly LazyVariable<CompileSymbol, Utf8String> _compilerVersion;

    /// <summary>
    /// Initializes an empty compile symbol.
    /// </summary>
    protected CompileSymbol()
    {
        _compilerVersion = new LazyVariable<CompileSymbol, Utf8String>(x => x.GetCompilerVersion());
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
    public Utf8String CompilerVersion
    {
        get => _compilerVersion.GetValue(this);
        set => _compilerVersion.SetValue(value);
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
