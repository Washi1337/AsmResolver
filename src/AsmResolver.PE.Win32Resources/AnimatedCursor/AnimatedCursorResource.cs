using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.AnimatedCursor;

/// <summary>
/// Represents a single RT_ANICURSOR or RT_ANIICON resource containing RIFF/ACON animated cursor or icon data.
/// </summary>
public class AnimatedCursorResource : IWin32Resource
{
    private const uint RiffSignature = 0x46464952; // "RIFF"
    private const uint AconSignature = 0x4E4F4341; // "ACON"
    private const uint AnihSignature = 0x68696E61; // "anih"
    private const uint AnihDataSize = 36;
    private const uint AfIconFlag = 0x01;

    /// <summary>
    /// Creates a new animated cursor/icon resource.
    /// </summary>
    /// <param name="id">The numeric identifier of the resource.</param>
    /// <param name="lcid">The language identifier.</param>
    /// <param name="resourceType">The resource type (AniCursor or AniIcon).</param>
    /// <param name="rawData">The raw RIFF/ACON data.</param>
    public AnimatedCursorResource(uint id, uint lcid, ResourceType resourceType, ISegment rawData)
    {
        Id = id;
        Lcid = lcid;
        ResourceType = resourceType;
        RawData = rawData;
    }

    /// <summary>
    /// Gets the numeric identifier of the resource.
    /// </summary>
    public uint Id
    {
        get;
    }

    /// <summary>
    /// Gets or sets the language identifier of the resource.
    /// </summary>
    public uint Lcid
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the resource type (AniCursor or AniIcon).
    /// </summary>
    public ResourceType ResourceType
    {
        get;
    }

    /// <summary>
    /// Gets or sets the raw RIFF/ACON data.
    /// </summary>
    public ISegment RawData
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the number of frames in the animation, or 0 if the header could not be parsed.
    /// </summary>
    public uint FrameCount => GetAnihField(1);

    /// <summary>
    /// Gets the number of steps in the animation, or 0 if the header could not be parsed.
    /// </summary>
    public uint StepCount => GetAnihField(2);

    /// <summary>
    /// Gets the default display rate (in jiffies), or 0 if the header could not be parsed.
    /// </summary>
    public uint DisplayRate => GetAnihField(7);

    /// <summary>
    /// Gets a value indicating whether the frames contain icon data (AF_ICON flag set).
    /// </summary>
    public bool HasIconFrames => (GetAnihField(8) & AfIconFlag) != 0;

    private uint GetAnihField(int fieldIndex)
    {
        if (RawData is not IReadableSegment readable || readable.GetPhysicalSize() < 12)
            return 0;

        var reader = readable.CreateReader();

        // Verify RIFF header.
        if (reader.ReadUInt32() != RiffSignature)
            return 0;

        uint riffSize = reader.ReadUInt32();
        if (reader.ReadUInt32() != AconSignature)
            return 0;

        // Scan for anih chunk.
        ulong riffEnd = reader.Offset + riffSize - 4;
        while (reader.Offset + 8 <= riffEnd && reader.CanRead(8))
        {
            uint chunkId = reader.ReadUInt32();
            uint chunkSize = reader.ReadUInt32();

            if (chunkId == AnihSignature)
            {
                if (chunkSize >= AnihDataSize && reader.CanRead(AnihDataSize))
                {
                    // ANIHEADER: cbSize, nFrames, nSteps, cx, cy, bpp, planes, jifRate, flags
                    uint[] fields = new uint[9];
                    for (int i = 0; i < 9; i++)
                        fields[i] = reader.ReadUInt32();

                    return fieldIndex < fields.Length ? fields[fieldIndex] : 0;
                }

                return 0;
            }

            // Skip chunk data (with RIFF padding to even boundary).
            ulong skip = chunkSize;
            if (skip % 2 != 0)
                skip++;
            reader.Offset += skip;
        }

        return 0;
    }

    /// <summary>
    /// Reads all animated cursor resources (RT_ANICURSOR) from the provided root resource directory.
    /// </summary>
    /// <param name="rootDirectory">The root resource directory.</param>
    /// <returns>A collection of animated cursor resources.</returns>
    public static IEnumerable<AnimatedCursorResource> FromDirectory(ResourceDirectory rootDirectory)
        => FromDirectory(rootDirectory, ResourceType.AniCursor);

    /// <summary>
    /// Reads all animated cursor or icon resources of the specified type from the root resource directory.
    /// </summary>
    /// <param name="rootDirectory">The root resource directory.</param>
    /// <param name="type">The resource type (AniCursor or AniIcon).</param>
    /// <returns>A collection of animated cursor/icon resources.</returns>
    public static IEnumerable<AnimatedCursorResource> FromDirectory(ResourceDirectory rootDirectory, ResourceType type)
    {
        if (!rootDirectory.TryGetDirectory(type, out var typeDirectory))
            yield break;

        foreach (var idDir in typeDirectory.Entries.OfType<ResourceDirectory>())
        {
            foreach (var language in idDir.Entries.OfType<ResourceData>())
            {
                if (language.Contents is not null)
                    yield return new AnimatedCursorResource(idDir.Id, language.Id, type, language.Contents);
            }
        }
    }

    /// <inheritdoc />
    public void InsertIntoDirectory(ResourceDirectory rootDirectory)
    {
        if (!rootDirectory.TryGetDirectory(ResourceType, out var typeDirectory))
        {
            typeDirectory = new ResourceDirectory(ResourceType);
            rootDirectory.InsertOrReplaceEntry(typeDirectory);
        }

        var idDir = new ResourceDirectory(Id);
        idDir.Entries.Add(new ResourceData(Lcid, RawData));
        typeDirectory.InsertOrReplaceEntry(idDir);
    }
}
