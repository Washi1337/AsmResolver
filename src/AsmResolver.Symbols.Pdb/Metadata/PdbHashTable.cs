using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata;

internal static class PdbHashTable
{
    public static Dictionary<TKey, TValue> FromReader<TKey, TValue>(
        ref BinaryStreamReader reader,
        Func<uint, uint, (TKey, TValue)> mapper)
        where TKey : notnull
    {
        uint size = reader.ReadUInt32();
        uint capacity = reader.ReadUInt32();

        uint presentWordCount = reader.ReadUInt32();
        reader.RelativeOffset += presentWordCount * sizeof(uint);

        uint deletedWordCount = reader.ReadUInt32();
        reader.RelativeOffset += deletedWordCount * sizeof(uint);

        var result = new Dictionary<TKey, TValue>();
        for (int i = 0; i < size; i++)
        {
            (uint rawKey, uint rawValue) = (reader.ReadUInt32(), reader.ReadUInt32());
            var (key, value) = mapper(rawKey, rawValue);
            result.Add(key, value);
        }

        uint lastNi = reader.ReadUInt32();

        return result;
    }
}
