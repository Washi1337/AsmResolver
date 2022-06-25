using System;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Info;

/// <summary>
/// Implements an PDB info stream that pulls its data from an input stream.
/// </summary>
public class SerializedInfoStream : InfoStream
{
    private readonly BinaryStreamReader _reader;
    private ulong _featureOffset;

    /// <summary>
    /// Parses a PDB info stream from an input stream reader.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    public SerializedInfoStream(BinaryStreamReader reader)
    {
        Version = (InfoStreamVersion) reader.ReadUInt32();
        Signature = reader.ReadUInt32();
        Age = reader.ReadUInt32();

        byte[] guidBytes = new byte[16];
        reader.ReadBytes(guidBytes, 0, guidBytes.Length);

        UniqueId = new Guid(guidBytes);

        _reader = reader;
    }

    /// <inheritdoc />
    protected override IDictionary<Utf8String, int> GetStreamIndices()
    {
        var reader = _reader.Fork();
        uint length = reader.ReadUInt32();

        var stringsReader = reader.ForkRelative(reader.RelativeOffset, length);
        var hashTableReader = reader.ForkRelative(reader.RelativeOffset + length);

        var result = PdbHashTable.FromReader(ref hashTableReader, (key, value) =>
        {
            var stringReader = stringsReader.ForkRelative(key);
            var keyString = stringReader.ReadUtf8String();
            return (keyString, (int) value);
        });

        hashTableReader.ReadUInt32(); // lastNi (unused).

        _featureOffset = hashTableReader.Offset;
        return result;
    }

    /// <inheritdoc />
    protected override IList<PdbFeature> GetFeatures()
    {
        // We need to read the stream name->index mapping to be able to read the features list of the PDB.
        _ = StreamIndices;

        var result = new List<PdbFeature>();

        var reader = _reader.ForkAbsolute(_featureOffset);
        while (reader.CanRead(sizeof(uint)))
            result.Add((PdbFeature) reader.ReadUInt32());

        return result;
    }
}
