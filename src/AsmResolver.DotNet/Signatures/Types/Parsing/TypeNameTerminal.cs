namespace AsmResolver.DotNet.Signatures.Types.Parsing
{
    internal enum TypeNameTerminal
    {
        Eof,
        Identifier,
        Number,
        Star,
        Plus,
        Equals,
        Dot,
        Comma,
        Tick,
        Ampersand, 
        OpenBracket,
        CloseBracket,
        Ellipsis,
    }
}