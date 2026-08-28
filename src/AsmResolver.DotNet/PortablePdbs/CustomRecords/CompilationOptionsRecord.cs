using System;
using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class CompilationOptionsRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("B5FEEC05-8CD0-4A83-96DA-466284BB4BD8");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public Dictionary<Utf8String, Utf8String>? Options { get; set; }

    public static CompilationOptionsRecord FromReader(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        var options = new Dictionary<Utf8String, Utf8String>();
        while (reader.CanRead(1))
        {
            options[reader.ReadUtf8String()] = reader.ReadUtf8String();
        }
        return new CompilationOptionsRecord
        {
            Options = options,
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }
}
