using System;
using System.CodeDom.Compiler;

namespace AsmResolver.SourceGenerators;

internal static class IndentedTextWriterExtensions
{
    public static void OpenBrace(this IndentedTextWriter self)
    {
        self.WriteLine('{');
        self.Indent++;
    }

    public static void CloseBrace(this IndentedTextWriter self)
    {
        self.Indent--;
        self.WriteLine('}');
    }

    public static void WriteLines(this IndentedTextWriter self, string s)
    {
        foreach (string line in s.Split(["\r\n", "\r", "\n"], StringSplitOptions.None))
            self.WriteLine(line);
    }
}
