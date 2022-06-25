using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Info;

/// <summary>
/// Represents the PDB Info Stream (also known as the PDB stream)
/// </summary>
public class InfoStream : SegmentBase
{
    /// <summary>
    /// Gets the default fixed MSF stream index for the PDB Info stream.
    /// </summary>
    public const int StreamIndex = 1;

    private const int HeaderSize =
            sizeof(InfoStreamVersion) // Version
            + sizeof(uint) // Signature
            + sizeof(uint) // Aage
            + 16 //UniqueId
            + sizeof(uint) // NameBufferSize
        ;

    private IDictionary<Utf8String, int>? _streamIndices;
    private IList<PdbFeature>? _features;

    /// <summary>
    /// Gets or sets the version of the file format of the PDB info stream.
    /// </summary>
    /// <remarks>
    /// Modern tooling only recognize the VC7.0 file format.
    /// </remarks>
    public InfoStreamVersion Version
    {
        get;
        set;
    } = InfoStreamVersion.VC70;

    /// <summary>
    /// Gets or sets the 32-bit UNIX time-stamp of the PDB file.
    /// </summary>
    public uint Signature
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of times the PDB file has been written.
    /// </summary>
    public uint Age
    {
        get;
        set;
    } = 1;

    /// <summary>
    /// Gets or sets the unique identifier assigned to the PDB file.
    /// </summary>
    public Guid UniqueId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a mapping from stream names to their respective stream index within the underlying MSF file.
    /// </summary>
    public IDictionary<Utf8String, int> StreamIndices
    {
        get
        {
            if (_streamIndices is null)
                Interlocked.CompareExchange(ref _streamIndices, GetStreamIndices(), null);
            return _streamIndices;
        }
    }

    /// <summary>
    /// Gets a list of characteristics that this PDB has.
    /// </summary>
    public IList<PdbFeature> Features
    {
        get
        {
            if (_features is null)
                Interlocked.CompareExchange(ref _features, GetFeatures(), null);
            return _features;
        }
    }

    /// <summary>
    /// Reads a single PDB info stream from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The parsed info stream.</returns>
    public static InfoStream FromReader(BinaryStreamReader reader) => new SerializedInfoStream(reader);

    /// <summary>
    /// Obtains the stream name to index mapping of the PDB file.
    /// </summary>
    /// <returns>The mapping.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="StreamIndices"/> property.
    /// </remarks>
    protected virtual IDictionary<Utf8String, int> GetStreamIndices() => new Dictionary<Utf8String, int>();

    /// <summary>
    /// Obtains the features of the PDB file.
    /// </summary>
    /// <returns>The features.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Features"/> property.
    /// </remarks>
    protected virtual IList<PdbFeature> GetFeatures() => new List<PdbFeature>
    {
        PdbFeature.VC140
    };

    /// <inheritdoc />
    public override uint GetPhysicalSize()
    {
        uint totalSize = HeaderSize;

        // Name buffer
        foreach (var entry in StreamIndices)
            totalSize += (uint) entry.Key.ByteCount + 1u;

        // Stream indices hash table.
        totalSize += StreamIndices.GetPdbHashTableSize(ComputeStringHash);

        // Last NI
        totalSize += sizeof(uint);

        // Feature codes.
        totalSize += (uint) Features.Count * sizeof(PdbFeature);

        return totalSize;
    }

    /// <inheritdoc />
    public override void Write(IBinaryStreamWriter writer)
    {
        // Write basic info stream header.
        writer.WriteUInt32((uint) Version);
        writer.WriteUInt32(Signature);
        writer.WriteUInt32(Age);
        writer.WriteBytes(UniqueId.ToByteArray());

        // Construct name buffer, keeping track of the offsets of every name.
        using var nameBuffer = new MemoryStream();
        var nameWriter = new BinaryStreamWriter(nameBuffer);

        var stringOffsets = new Dictionary<Utf8String, uint>();
        foreach (var entry in StreamIndices)
        {
            uint offset = (uint) nameWriter.Offset;
            nameWriter.WriteBytes(entry.Key.GetBytesUnsafe());
            nameWriter.WriteByte(0);
            stringOffsets.Add(entry.Key, offset);
        }

        writer.WriteUInt32((uint) nameBuffer.Length);
        writer.WriteBytes(nameBuffer.ToArray());

        // Write the hash table.
        StreamIndices.WriteAsPdbHashTable(writer,
            ComputeStringHash,
            (key, value) => (stringOffsets[key], (uint) value));

        // last NI, safe to put always zero.
        writer.WriteUInt32(0);

        // Write feature codes.
        var features = Features;
        for (int i = 0; i < features.Count; i++)
            writer.WriteUInt32((uint) features[i]);
    }

    private static uint ComputeStringHash(Utf8String str)
    {
        // Note: The hash of a single entry is **deliberately** truncated to a 16 bit number. This is because
        // the reference implementation of the name table returns a number of type HASH, which is a typedef
        // for "unsigned short". If we don't do this, this will result in wrong buckets being filled in the
        // hash table, and thus the serialization would fail. See NMTNI::hash() in Microsoft/microsoft-pdb.

        return (ushort) PdbHash.ComputeV1(str);
    }
}
