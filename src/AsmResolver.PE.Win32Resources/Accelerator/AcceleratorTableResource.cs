using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Accelerator;

/// <summary>
/// Provides a high level view on an accelerator table resource (RT_ACCELERATOR) stored in a PE image.
/// </summary>
public class AcceleratorTableResource : IWin32Resource
{
    private const ushort LastEntryFlag = 0x80;

    /// <summary>
    /// Creates a new accelerator table resource.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    public AcceleratorTableResource(uint id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets the resource identifier.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets the list of accelerator entries.
    /// </summary>
    public IList<AcceleratorEntry> Entries { get; } = new List<AcceleratorEntry>();

    /// <summary>
    /// Reads all accelerator table resources from the provided root resource directory.
    /// </summary>
    /// <param name="rootDirectory">The root resource directory.</param>
    /// <returns>The accelerator table resources.</returns>
    public static IEnumerable<AcceleratorTableResource> FromDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Accelerator, out var accelDirectory))
            yield break;

        foreach (var entry in accelDirectory.Entries.OfType<ResourceDirectory>())
        {
            foreach (var language in entry.Entries.OfType<ResourceData>())
            {
                var reader = language.CreateReader();
                var table = new AcceleratorTableResource(entry.Id);
                ReadTable(table, ref reader);
                yield return table;
            }
        }
    }

    private static void ReadTable(AcceleratorTableResource table, ref BinaryStreamReader reader)
    {
        while (reader.CanRead(8))
        {
            ushort fVirt = reader.ReadUInt16();
            ushort key = reader.ReadUInt16();
            uint cmd = reader.ReadUInt32();

            bool isLast = (fVirt & LastEntryFlag) != 0;
            var flags = (AcceleratorFlags)(ushort)(fVirt & ~LastEntryFlag);

            table.Entries.Add(new AcceleratorEntry
            {
                Flags = flags,
                Key = key,
                Command = cmd,
            });

            if (isLast)
                break;
        }
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType.Accelerator, out var accelDirectory))
        {
            accelDirectory = new ResourceDirectory(ResourceType.Accelerator);
            rootDirectory.InsertOrReplaceEntry(accelDirectory);
        }

        // Remove existing entry with same ID if present.
        for (int i = accelDirectory.Entries.Count - 1; i >= 0; i--)
        {
            if (accelDirectory.Entries[i].Id == Id)
            {
                accelDirectory.Entries.RemoveAt(i);
                break;
            }
        }

        using var stream = new MemoryStream();
        var writer = new BinaryStreamWriter(stream);

        for (int i = 0; i < Entries.Count; i++)
        {
            var entry = Entries[i];
            ushort fVirt = (ushort)entry.Flags;
            if (i == Entries.Count - 1)
                fVirt |= LastEntryFlag;

            writer.WriteUInt16(fVirt);
            writer.WriteUInt16(entry.Key);
            writer.WriteUInt32(entry.Command);
        }

        var tableDir = new ResourceDirectory(Id);
        tableDir.Entries.Add(new ResourceData(0, new DataSegment(stream.ToArray())));
        accelDirectory.Entries.Add(tableDir);
    }
}
