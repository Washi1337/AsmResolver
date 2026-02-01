using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class DefaultNamespaceRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("58b2eab6-209f-4e4e-a22c-b2d0f910c782");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public Utf8String? Namespace { get; set; }

    public static DefaultNamespaceRecord FromReader(in BlobReaderContext context, ref BinaryStreamReader reader)
    {
        return new DefaultNamespaceRecord
        {
            Namespace = new Utf8String(reader.ReadToEnd()),
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }
}
