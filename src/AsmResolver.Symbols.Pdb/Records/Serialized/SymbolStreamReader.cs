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
                ushort length = lookahead.ReadUInt16();

                // If we are at an S_END symbol, we reached the end of a scope.
                // Silently consume the end symbol and go up the scope stack.
                if ((CodeViewSymbolType) lookahead.ReadUInt16() == CodeViewSymbolType.End)
                {
                    reader.Offset += sizeof(ushort) + (uint) length;
                    scopeStack.Pop();
                    continue;
                }
            }

            // Read the next symbol.
            var nextSymbol = CodeViewSymbol.FromReader(context, ref reader);

            // If we're in a scope, add it to the scope, otherwise add it to the end result.
            if (scopeStack.Count > 0)
                scopeStack.Peek().Symbols.Add(nextSymbol);
            else
                result.Add(nextSymbol);

            // Are we entering a new scope with this next symbol?
            if (nextSymbol is IScopeCodeViewSymbol scopeSymbol)
                scopeStack.Push(scopeSymbol);
        }

        return result;
    }
}
