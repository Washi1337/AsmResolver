using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class EnCLambdaClosureMapRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("A643004C-0240-496F-A783-30D64F4979DE");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public uint MethodOrdinal { get; set; }

    public int[]? ClosureSyntaxOffsets { get; set; }

    public Lambda[]? Lambdas { get; set; }

    public static EnCLambdaClosureMapRecord FromReader(in BlobReaderContext context, ref BinaryStreamReader reader)
    {
        uint methodOrdinal = reader.ReadCompressedUInt32();
        int offsetBaseline = Math.Min(-(int)reader.ReadCompressedUInt32(), -1);
        uint closureCount = reader.ReadCompressedUInt32();

        int[] closures = new int[closureCount];
        for (int i = 0; i < closures.Length; i++)
        {
            closures[i] = (int)reader.ReadCompressedUInt32() + offsetBaseline;
        }

        var lambdaList = new List<Lambda>();
        while (reader.CanRead(1))
        {
            lambdaList.Add(new Lambda
            {
                SyntaxOffset = (int)reader.ReadCompressedUInt32() + offsetBaseline,
                ClosureOrdinal = reader.ReadCompressedUInt32(),
            });
        }

        return new EnCLambdaClosureMapRecord
        {
            MethodOrdinal = methodOrdinal,
            ClosureSyntaxOffsets = closures,
            Lambdas = lambdaList.ToArray(),
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }

    public struct Lambda
    {
        public int SyntaxOffset { get; set; }
        public uint ClosureOrdinal { get; set; }
    }
}
