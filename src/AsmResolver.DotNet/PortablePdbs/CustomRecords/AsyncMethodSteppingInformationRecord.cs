using System;
using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class AsyncMethodSteppingInformationRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("54FD2AC5-E925-401A-9C2A-F94F171072F8");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public uint CatchHandlerOffset { get; set; }

    public OffsetPair[]? Pairs { get; set; }

    public static unsafe AsyncMethodSteppingInformationRecord FromReader(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        var catchOffset = reader.ReadUInt32();
        var pairList = new List<OffsetPair>();
        while (reader.CanRead(1))
        {
            pairList.Add(new OffsetPair
            {
                YieldOffset = reader.ReadUInt32(),
                ResumeOffset = reader.ReadUInt32(),
                ResumeMethod = context.OwningModule.LookupMember<MethodDefinition>(new MetadataToken(TableIndex.Method, reader.ReadCompressedUInt32())),
            });
        }
        return new AsyncMethodSteppingInformationRecord
        {
            CatchHandlerOffset = catchOffset,
            Pairs = pairList.ToArray(),
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }

    public struct OffsetPair
    {
        public uint YieldOffset { get; set; }
        public uint ResumeOffset { get; set; }
        public MethodDefinition ResumeMethod { get; set; }
    }
}
