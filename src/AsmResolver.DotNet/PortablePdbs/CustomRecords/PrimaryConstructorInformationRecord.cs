using System;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class PrimaryConstructorInformationRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("9D40ACE1-C703-4D0E-BF41-7243060A8FB5");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => false;

    protected override void WriteContents(in BlobSerializationContext context) => throw new NotImplementedException();
}
