using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class SourceLinkRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("CC110556-A091-4D38-9FEC-25AB9A351A6A");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public Utf8String? SourceLinkJson { get; set; }

    public static SourceLinkRecord FromReader(in BlobReaderContext context, ref BinaryStreamReader reader)
    {
        return new SourceLinkRecord
        {
            SourceLinkJson = new Utf8String(reader.ReadToEnd()),
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }
}
