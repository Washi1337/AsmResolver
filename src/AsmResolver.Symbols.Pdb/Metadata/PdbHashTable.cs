using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata;

/// <summary>
/// Provides methods for serializing and deserializing dictionaries as PDB hash tables.
/// </summary>
public static class PdbHashTable
{
    // Reference implementation from PDB/include/map.h
    // Specifically, Map::load, Map::find and Map::save.

    /// <summary>
    /// Reads a single PDB hash table from the input stream and converts it into a dictionary.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    /// <param name="mapper">A function that maps the raw key-value pairs into high level constructs.</param>
    /// <typeparam name="TKey">The type of keys in the final dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the final dictionary.</typeparam>
    /// <returns>The reconstructed dictionary.</returns>
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

        return result;
    }

    /// <summary>
    /// Serializes a dictionary to a PDB hash table.
    /// </summary>
    /// <param name="dictionary">The dictionary to serialize.</param>
    /// <param name="writer">The output stream to write to.</param>
    /// <param name="hasher">A function that computes the hash code for a single key within the dictionary.</param>
    /// <param name="mapper">A function that maps every key-value pair to raw key-value uint32 pairs.</param>
    /// <typeparam name="TKey">The type of keys in the input dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the input dictionary.</typeparam>
    public static void WriteAsPdbHashTable<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        IBinaryStreamWriter writer,
        Func<TKey, uint> hasher,
        Func<TKey, TValue, (uint, uint)> mapper)
        where TKey : notnull
    {
        var hashTable = dictionary.ToPdbHashTable(hasher, mapper);

        // Write count and capacity.
        writer.WriteInt32(dictionary.Count);
        writer.WriteUInt32(hashTable.Capacity);

        // Determine which words in the present bitvector to write.
        uint wordCount = (hashTable.Capacity + sizeof(uint) - 1) / sizeof(uint);
        uint[] words = new uint[wordCount];
        hashTable.Present.CopyTo(words, 0);
        while (wordCount > 0 && words[wordCount - 1] == 0)
            wordCount--;

        // Write the present bitvector.
        writer.WriteUInt32(wordCount);
        for (int i = 0; i < wordCount; i++)
            writer.WriteUInt32(words[i]);

        // Write deleted bitvector. We just always do 0 (i.e. no deleted buckets).
        writer.WriteUInt32(0);

        // Write all buckets.
        for (int i = 0; i < hashTable.Keys.Length; i++)
        {
            if (hashTable.Present.Get(i))
            {
                writer.WriteUInt32(hashTable.Keys[i]);
                writer.WriteUInt32(hashTable.Values[i]);
            }
        }
    }

    private static HashTableInfo ToPdbHashTable<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        Func<TKey, uint> hasher,
        Func<TKey, TValue, (uint, uint)> mapper)
        where TKey : notnull
    {
        // "Simulate" adding all items to the hash table, effectively calculating the capacity of the map.
        // TODO: This can probably be calculated with a single formula instead.
        uint capacity = 1;
        for (int i = 0; i <= dictionary.Count; i++)
        {
            // Reference implementation allows only 67% of the capacity to be used.
            uint maxLoad = capacity * 2 / 3 + 1;
            if (i >= maxLoad)
                capacity = 2 * maxLoad;
        }

        // Define buckets.
        uint[] keys = new uint[capacity];
        uint[] values = new uint[capacity];
        var present = new BitArray((int) capacity, false);

        // Fill in buckets.
        foreach (var item in dictionary)
        {
            uint hash = hasher(item.Key);
            (uint key, uint value) = mapper(item.Key, item.Value);

            uint index = hash % capacity;
            while (present.Get((int) index))
                index = (index + 1) % capacity;

            keys[index] = key;
            values[index] = value;
            present.Set((int) index, true);
        }

        return new HashTableInfo(capacity, keys, values, present);
    }

    private readonly struct HashTableInfo
    {
        public readonly uint Capacity;
        public readonly uint[] Keys;
        public readonly uint[] Values;
        public readonly BitArray Present;

        public HashTableInfo(uint capacity, uint[] keys, uint[] values, BitArray present)
        {
            Capacity = capacity;
            Keys = keys;
            Values = values;
            Present = present;
        }
    }

}
