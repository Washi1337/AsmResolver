using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

internal static class SymbolStreamReader
{
    public static IList<ICodeViewSymbol> ReadSymbols(
        PdbReaderContext context,
        ref BinaryStreamReader reader)
    {
        var result = new List<ICodeViewSymbol>();

        var scopeStack = new Stack<ICodeViewSymbolProvider>();

        while (reader.CanRead(sizeof(ushort) * 2))
        {
            if (scopeStack.Count > 0)
            {
                var lookahead = reader.Fork();
                _ = lookahead.ReadUInt16(); // length.

                if ((CodeViewSymbolType) lookahead.ReadUInt16() == CodeViewSymbolType.End)
                    scopeStack.Pop();
            }

            var nextSymbol = CodeViewSymbol.FromReader(context, ref reader);

            if (scopeStack.Count > 0)
                scopeStack.Peek().Symbols.Add(nextSymbol);
            else
                result.Add(nextSymbol);

            if (nextSymbol is IScopeCodeViewSymbol scopeSymbol)
                scopeStack.Push(scopeSymbol);
        }

        return result;
    }
}
