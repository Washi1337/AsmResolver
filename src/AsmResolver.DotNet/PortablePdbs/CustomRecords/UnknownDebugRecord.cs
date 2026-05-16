using System;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class UnknownDebugRecord : CustomDebugRecord
{
    public UnknownDebugRecord(Guid kind, byte[]? data)
    {
        Kind = kind;
        Data = data;
    }

    public override Guid Kind
    {
        get;
    }

    public override bool HasBlob => Data is not null;

    public byte[]? Data { get; set; }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        context.Writer.WriteBytes(Data!);
    }
}
