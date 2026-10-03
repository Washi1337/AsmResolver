using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.StringResource;

/// <summary>
/// Provides a high level view on string table resources (RT_STRING) stored in a PE image.
/// </summary>
public class StringTableResource : IWin32Resource
{
    /// <summary>
    /// Gets the dictionary mapping string IDs to their values.
    /// </summary>
    public IDictionary<uint, string> Strings { get; } = new SortedDictionary<uint, string>();

    /// <summary>
    /// Reads string table resources from the provided root resource directory.
    /// </summary>
    /// <param name="rootDirectory">The root resource directory.</param>
    /// <returns>The resource, or <c>null</c> if no string table resource was present.</returns>
    public static StringTableResource? FromDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.String, out var stringDirectory))
            return null;

        var result = new StringTableResource();

        foreach (var block in stringDirectory.Entries.OfType<ResourceDirectory>())
        {
            uint blockId = block.Id;

            foreach (var language in block.Entries.OfType<ResourceData>())
            {
                var reader = language.CreateReader();
                ReadBlock(result, blockId, ref reader);
            }
        }

        return result;
    }

    private static void ReadBlock(StringTableResource resource, uint blockId, ref BinaryStreamReader reader)
    {
        uint baseId = (blockId - 1) * 16;

        for (int i = 0; i < 16 && reader.CanRead(sizeof(ushort)); i++)
        {
            ushort length = reader.ReadUInt16();
            if (length == 0)
                continue;

            uint byteCount = (uint)(length * 2);
            if (!reader.CanRead(byteCount))
                break;

            byte[] data = new byte[byteCount];
            reader.ReadBytes(data, 0, (int)byteCount);
            string value = Encoding.Unicode.GetString(data, 0, data.Length);
            resource.Strings[baseId + (uint)i] = value;
        }
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        var stringDirectory = new ResourceDirectory(ResourceType.String);

        // Group strings by block ID.
        var blocks = new SortedDictionary<uint, string[]>();
        foreach (var kvp in Strings)
        {
            uint blockId = (kvp.Key / 16) + 1;
            if (!blocks.TryGetValue(blockId, out var entries))
            {
                entries = new string[16];
                blocks[blockId] = entries;
            }

            entries[kvp.Key % 16] = kvp.Value;
        }

        foreach (var kvp in blocks)
        {
            using var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);

            for (int i = 0; i < 16; i++)
            {
                string? s = kvp.Value[i];
                if (string.IsNullOrEmpty(s))
                {
                    writer.WriteUInt16(0);
                }
                else
                {
                    writer.WriteUInt16((ushort)s.Length);
                    writer.WriteBytes(Encoding.Unicode.GetBytes(s));
                }
            }

            var blockDir = new ResourceDirectory(kvp.Key);
            blockDir.Entries.Add(new ResourceData(0, new DataSegment(stream.ToArray())));
            stringDirectory.Entries.Add(blockDir);
        }

        rootDirectory.InsertOrReplaceEntry(stringDirectory);
    }
}
