using System;
using System.IO;
using System.IO.Compression;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class EmbeddedSourceRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("0E8A571B-6926-466E-B4AD-8AB04611F5FE");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public bool WriteCompressed
    {
        get;
        set;
    }

    public byte[]? Source { get; set; }

    public static EmbeddedSourceRecord FromReader(in BlobReaderContext context, ref BinaryStreamReader reader)
    {
        var uncompressedSize = reader.ReadInt32();
        var data = reader.ReadToEnd();

        if (uncompressedSize != 0)
        {
            using var deflateStream = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress);
            data = new byte[uncompressedSize];
            using var output = new MemoryStream(data);
            deflateStream.CopyTo(output);
        }

        return new EmbeddedSourceRecord
        {
            Source = data,
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }
}
