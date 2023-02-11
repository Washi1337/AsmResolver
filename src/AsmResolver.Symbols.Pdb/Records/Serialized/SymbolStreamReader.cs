using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

internal static class SymbolStreamReader
{
    public static IList<ICodeViewSymbol> ReadSymbols(
        PdbReaderContext context,
        ref BinaryStreamReader reader,
        bool untilEndSymbol)
    {
        var result = new List<ICodeViewSymbol>();

        while (reader.CanRead(sizeof(ushort) * 2))
        {
            if (untilEndSymbol)
            {
                var lookahead = reader.Fork();
                _ = lookahead.ReadUInt16(); // length.
                if ((CodeViewSymbolType) lookahead.ReadUInt16() == CodeViewSymbolType.End)
                    break;
            }

            result.Add(CodeViewSymbol.FromReader(context, ref reader));
        }

        return result;
    }
}
