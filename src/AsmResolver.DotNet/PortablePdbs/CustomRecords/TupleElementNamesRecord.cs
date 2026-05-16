using System;
using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class TupleElementNamesRecord : CustomDebugRecord
{
    public static Guid KnownGuid { get; } = new("ED9FDF71-8879-4747-8ED3-FE5EDE3CE710");

    public override Guid Kind => KnownGuid;

    public override bool HasBlob => true;

    public Utf8String[]? ElementNames { get; set; }

    public static TupleElementNamesRecord FromReader(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        var nameList = new List<Utf8String>();
        while (reader.CanRead(1))
        {
            nameList.Add(reader.ReadUtf8String());
        }
        return new TupleElementNamesRecord
        {
            ElementNames = nameList.ToArray(),
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }
}
