using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace AsmResolver.SourceGenerators;

internal static class Extensions
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

    public static EquatableArray<T> ToEquatableArray<T>(this ImmutableArray<T> self) => new(self);

    public static EquatableArray<T> ToEquatableArray<T>(this IEnumerable<T> self) => self.ToImmutableArray().ToEquatableArray();
}
