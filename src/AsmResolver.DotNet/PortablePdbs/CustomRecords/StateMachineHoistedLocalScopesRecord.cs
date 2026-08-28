using System;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class StateMachineHoistedLocalScopesRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("6DA9A61E-F8C7-4874-BE62-68BC5630DF71");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public Scope[]? Scopes { get; set; }

    public static unsafe StateMachineHoistedLocalScopesRecord FromReader(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        var scopes = new Scope[reader.RemainingLength / sizeof(Scope)];
        for (int i = 0; i < scopes.Length; i++)
        {
            scopes[i] = new Scope
            {
                StartOffset = reader.ReadUInt32(),
                Length = reader.ReadUInt32(),
            };
        }
        return new StateMachineHoistedLocalScopesRecord
        {
            Scopes = scopes,
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }

    public struct Scope
    {
        public uint StartOffset { get; set; }
        public uint Length { get; set; }
    }
}
