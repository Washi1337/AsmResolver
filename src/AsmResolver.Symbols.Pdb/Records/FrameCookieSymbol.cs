namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Describes the position and type of a security cookie within a stack frame.
/// </summary>
public class FrameCookieSymbol : CodeViewSymbol
{
    /// <summary>
    /// Initializes an empty frame cookie symbol.
    /// </summary>
    protected FrameCookieSymbol()
    {
    }

    /// <summary>
    /// Creates a new frame cookie symbol.
    /// </summary>
    /// <param name="frameOffset">The offset relative to the frame pointer.</param>
    /// <param name="register">The register storing the cookie.</param>
    /// <param name="cookieType">The type of cookie that is computed.</param>
    /// <param name="attributes">The attributes associated to the cookie.</param>
    public FrameCookieSymbol(int frameOffset, ushort register, FrameCookieType cookieType, byte attributes)
    {
        FrameOffset = frameOffset;
        Register = register;
        CookieType = cookieType;
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.FrameCookie;

    /// <summary>
    /// Gets or sets the offset relative to the frame pointer where the cookie is stored.
    /// </summary>
    public int FrameOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the register storing the cookie.
    /// </summary>
    public ushort Register
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the type of cookie that is computed.
    /// </summary>
    public FrameCookieType CookieType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets attributes describing the cookie.
    /// </summary>
    /// <remarks>
    /// This flags field is not well understood.
    /// </remarks>
    public byte Attributes
    {
        get;
        set;
    }

    /// <inheritdoc />
    public override string ToString() => $"S_FRAMECOOKIE: {CookieType} +{FrameOffset:X}";
}
