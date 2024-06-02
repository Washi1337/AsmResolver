namespace AsmResolver.DotNet.Signatures.Parsing
{
    internal readonly struct TypeNameToken
    {
        public TypeNameToken(TypeNameTerminal terminal, string text)
        {
            Terminal = terminal;
            Text = text;
        }
        
        public TypeNameTerminal Terminal
        {
            get;
        }

        public string Text
        {
            get;
        }

        public override string ToString() => $"{Text} ({Terminal})";
    }
}