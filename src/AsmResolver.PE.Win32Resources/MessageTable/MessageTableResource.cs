using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.MessageTable;

/// <summary>
/// Represents a message table resource (RT_MESSAGETABLE) stored in the Win32 resources of a PE image.
/// </summary>
public class MessageTableResource : IWin32Resource
{
    /// <summary>
    /// Creates a new empty message table resource.
    /// </summary>
    public MessageTableResource()
    {
    }

    /// <summary>
    /// Gets the list of message entries in this message table resource.
    /// </summary>
    public IList<MessageTableEntry> Entries { get; } = new List<MessageTableEntry>();

    /// <summary>
    /// Reads a message table resource from the provided root resource directory of a PE image.
    /// </summary>
    /// <param name="rootDirectory">The root resource directory.</param>
    /// <returns>The message table resource, or <c>null</c> if none was found.</returns>
    public static MessageTableResource? FromDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.MessageTable, out var messageTableDirectory))
            return null;

        var idDirectory = messageTableDirectory.Entries.OfType<ResourceDirectory>().FirstOrDefault();
        if (idDirectory is null)
            return null;

        var dataEntry = idDirectory.Entries.OfType<ResourceData>().FirstOrDefault();
        if (dataEntry is null)
            return null;

        var reader = dataEntry.CreateReader();
        return FromReader(ref reader);
    }

    /// <summary>
    /// Reads a message table resource from an input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The parsed message table resource.</returns>
    public static MessageTableResource FromReader(ref BinaryStreamReader reader)
    {
        var result = new MessageTableResource();
        ulong dataStart = reader.Offset;

        uint numberOfBlocks = reader.ReadUInt32();

        // Read block headers.
        var blocks = new (uint LowId, uint HighId, uint Offset)[numberOfBlocks];
        for (int i = 0; i < numberOfBlocks; i++)
        {
            blocks[i] = (reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32());
        }

        // Read entries for each block.
        for (int i = 0; i < numberOfBlocks; i++)
        {
            reader.Offset = dataStart + blocks[i].Offset;

            for (uint id = blocks[i].LowId; id <= blocks[i].HighId; id++)
            {
                ushort length = reader.ReadUInt16();
                ushort flags = reader.ReadUInt16();

                int textLength = length - 4; // subtract header size
                if (textLength < 0)
                    textLength = 0;

                byte[] textBytes = new byte[textLength];
                reader.ReadBytes(textBytes, 0, textLength);

                string text;
                if (flags == 0x0001)
                {
                    // Unicode
                    text = Encoding.Unicode.GetString(textBytes).TrimEnd('\0');
                }
                else
                {
                    // ANSI
                    text = Encoding.GetEncoding(28591).GetString(textBytes).TrimEnd('\0');
                }

                result.Entries.Add(new MessageTableEntry(id, text, flags == 0x0001));
            }
        }

        return result;
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.MessageTable, out var messageTableDirectory))
        {
            messageTableDirectory = new ResourceDirectory(ResourceType.MessageTable);
            rootDirectory.InsertOrReplaceEntry(messageTableDirectory);
        }

        var idDirectory = new ResourceDirectory(1);

        using var stream = new MemoryStream();
        var writer = new BinaryStreamWriter(stream);
        WriteMessageTable(writer);

        idDirectory.Entries.Add(new ResourceData(0, new DataSegment(stream.ToArray())));
        messageTableDirectory.InsertOrReplaceEntry(idDirectory);
    }

    private void WriteMessageTable(BinaryStreamWriter writer)
    {
        if (Entries.Count == 0)
        {
            writer.WriteUInt32(0);
            return;
        }

        // Sort entries by ID and group into contiguous blocks.
        var sorted = Entries.OrderBy(e => e.Id).ToList();
        var blocks = new List<(uint LowId, uint HighId, List<MessageTableEntry> Entries)>();

        uint low = sorted[0].Id;
        uint high = sorted[0].Id;
        var currentBlock = new List<MessageTableEntry> { sorted[0] };

        for (int i = 1; i < sorted.Count; i++)
        {
            if (sorted[i].Id == high + 1)
            {
                high = sorted[i].Id;
                currentBlock.Add(sorted[i]);
            }
            else
            {
                blocks.Add((low, high, currentBlock));
                low = sorted[i].Id;
                high = sorted[i].Id;
                currentBlock = new List<MessageTableEntry> { sorted[i] };
            }
        }
        blocks.Add((low, high, currentBlock));

        // Write number of blocks.
        writer.WriteUInt32((uint) blocks.Count);

        // Reserve space for block headers (12 bytes each).
        long blockHeaderStart = (long) writer.Offset;
        for (int i = 0; i < blocks.Count; i++)
        {
            writer.WriteUInt32(0); // LowId placeholder
            writer.WriteUInt32(0); // HighId placeholder
            writer.WriteUInt32(0); // Offset placeholder
        }

        // Write entries for each block and record offsets.
        var offsets = new uint[blocks.Count];
        for (int i = 0; i < blocks.Count; i++)
        {
            offsets[i] = (uint) writer.Offset;

            foreach (var entry in blocks[i].Entries)
            {
                byte[] textBytes;
                if (entry.IsUnicode)
                    textBytes = Encoding.Unicode.GetBytes(entry.Text + '\0');
                else
                    textBytes = Encoding.GetEncoding(28591).GetBytes(entry.Text + '\0');

                // Total length = 4 (header) + text bytes, DWORD aligned.
                ushort totalLength = (ushort) (4 + textBytes.Length);
                ushort aligned = (ushort) ((totalLength + 3) & ~3);

                writer.WriteUInt16(aligned);
                writer.WriteUInt16(entry.IsUnicode ? (ushort) 0x0001 : (ushort) 0x0000);
                writer.WriteBytes(textBytes);

                // Pad to DWORD alignment.
                int padding = aligned - totalLength;
                for (int p = 0; p < padding; p++)
                    writer.WriteByte(0);
            }
        }

        // Go back and fill in block headers.
        long endPos = (long) writer.Offset;
        writer.Offset = (ulong) blockHeaderStart;
        for (int i = 0; i < blocks.Count; i++)
        {
            writer.WriteUInt32(blocks[i].LowId);
            writer.WriteUInt32(blocks[i].HighId);
            writer.WriteUInt32(offsets[i]);
        }
        writer.Offset = (ulong) endPos;
    }
}
