using System;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.Shims;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class EnCStateMachineStateMapRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("8B78CD68-2EDE-420B-980B-E15884B8AAA3");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public State[]? States { get; set; }

    public static EnCStateMachineStateMapRecord FromReader(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        var stateCount = reader.ReadCompressedUInt32();
        State[] states;
        if (stateCount == 0)
        {
            states = ArrayShim.Empty<State>();
        }
        else
        {
            states = new State[stateCount];
            var syntaxBaseline = -(int)reader.ReadCompressedUInt32();
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = new State
                {
                    StateNumber = reader.ReadCompressedInt32(),
                    SyntaxOffset = (int)reader.ReadCompressedUInt32() + syntaxBaseline,
                };
            }
        }

        return new EnCStateMachineStateMapRecord
        {
            States = states,
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }

    public struct State
    {
        public int StateNumber { get; set; }
        public int SyntaxOffset { get; set; }
    }
}
