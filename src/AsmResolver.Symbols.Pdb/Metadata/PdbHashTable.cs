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
        uint count = reader.ReadUInt32();
        reader.ReadUInt32(); // Capacity

        uint presentWordCount = reader.ReadUInt32();
        reader.RelativeOffset += presentWordCount * sizeof(uint);

        uint deletedWordCount = reader.ReadUInt32();
        reader.RelativeOffset += deletedWordCount * sizeof(uint);

        var result = new Dictionary<TKey, TValue>((int) count);
        for (int i = 0; i < count; i++)
        {
            (uint rawKey, uint rawValue) = (reader.ReadUInt32(), reader.ReadUInt32());
            var (key, value) = mapper(rawKey, rawValue);
            result.Add(key, value);
        }

        return result;
    }

    /// <summary>
    /// Computes the number of bytes required to store the provided dictionary as a PDB hash table.
    /// </summary>
    /// <param name="dictionary">The dictionary to serialize.</param>
    /// <param name="hasher">A function that computes the hash code for a single key within the dictionary.</param>
    /// <typeparam name="TKey">The type of keys in the input dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the input dictionary.</typeparam>
    /// <returns>The number of bytes required.</returns>
    public static uint GetPdbHashTableSize<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        Func<TKey, uint> hasher)
        where TKey : notnull
    {
        var info = dictionary.ToPdbHashTable(hasher, null);

        return sizeof(uint) // Count
               + sizeof(uint) // Capacity
               + sizeof(uint) // Present bitvector word count
               + info.PresentWordCount * sizeof(uint) // Present bitvector words
               + sizeof(uint) // Deleted bitvector word count (== 0)
               + (sizeof(uint) + sizeof(uint)) * (uint) dictionary.Count
            ;
    }

    /// <summary>
    /// Serializes a dictionary to a PDB hash table to an output stream.
    /// </summary>
    /// <param name="dictionary">The dictionary to serialize.</param>
    /// <param name="writer">The output stream to write to.</param>
    /// <param name="hasher">A function that computes the hash code for a single key within the dictionary.</param>
    /// <param name="mapper">A function that maps every key-value pair to raw key-value uint32 pairs.</param>
    /// <typeparam name="TKey">The type of keys in the input dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the input dictionary.</typeparam>
    public static void WriteAsPdbHashTable<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        BinaryStreamWriter writer,
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
        for (int i = 0; i < hashTable.Keys!.Length; i++)
        {
            if (hashTable.Present.Get(i))
            {
                writer.WriteUInt32(hashTable.Keys![i]);
                writer.WriteUInt32(hashTable.Values![i]);
            }
        }
    }

    private static HashTableInfo ToPdbHashTable<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        Func<TKey, uint> hasher,
        Func<TKey, TValue, (uint, uint)>? mapper)
        where TKey : notnull
    {
        uint capacity = ComputeRequiredCapacity(dictionary.Count);

        // Avoid allocating buckets if we actually don't need to (e.g. if we're simply measuring the total size).
        uint[]? keys;
        uint[]? values;

        if (mapper is null)
        {
            keys = null;
            values = null;
        }
        else
        {
            keys = new uint[capacity];
            values = new uint[capacity];
        }

        var present = new BitArray((int) capacity, false);

        // Fill in buckets.
        foreach (var item in dictionary)
        {
            // Find empty bucket to place key-value pair in.
            uint hash = hasher(item.Key);
            uint index = hash % capacity;
            while (present.Get((int) index))
                index = (index + 1) % capacity;

            // Mark bucket as used.
            present.Set((int) index, true);

            // Store key-value pair.
            if (mapper is not null)
            {
                (uint key, uint value) = mapper(item.Key, item.Value);
                keys![index] = key;
                values![index] = value;
            }
        }

        // Determine final word count in present bit vector.
        uint wordCount = (capacity + sizeof(uint) - 1) / sizeof(uint);
        uint[] words = new uint[wordCount];
        present.CopyTo(words, 0);
        while (wordCount > 0 && words[wordCount - 1] == 0)
            wordCount--;

        return new HashTableInfo(capacity, keys, values, present, wordCount);
    }

    private static uint ComputeRequiredCapacity(int totalItemCount)
    {
        // "Simulate" adding all items to the hash table, effectively calculating the capacity of the map.
        // TODO: This can probably be calculated with a single formula instead.

        uint capacity = 1;
        for (int i = 0; i <= totalItemCount; i++)
        {
            // Reference implementation allows only 67% of the capacity to be used.
            uint maxLoad = capacity * 2 / 3 + 1;
            if (i >= maxLoad)
                capacity = 2 * maxLoad;
        }

        return capacity;
    }

    private readonly struct HashTableInfo
    {
        public readonly uint Capacity;
        public readonly uint[]? Keys;
        public readonly uint[]? Values;
        public readonly BitArray Present;
        public readonly uint PresentWordCount;

        public HashTableInfo(uint capacity, uint[]? keys, uint[]? values, BitArray present, uint presentWordCount)
        {
            Capacity = capacity;
            Keys = keys;
            Values = values;
            Present = present;
            PresentWordCount = presentWordCount;
        }
    }

}
