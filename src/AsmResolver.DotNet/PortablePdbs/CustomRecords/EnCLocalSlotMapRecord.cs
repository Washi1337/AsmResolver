using System;
using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class EnCLocalSlotMapRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("755F52A8-91C5-45BE-B4B8-209571E552BD");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public Slot[]? Slots { get; set; }

    public static EnCLocalSlotMapRecord FromReader(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        int offsetBaseline = -1;
        if (reader.CanRead(1) && reader.PeekByte() == 0xff)
        {
            reader.Offset += 1;
            offsetBaseline = -(int)reader.ReadCompressedUInt32();
        }

        var slotList = new List<Slot>();
        while (reader.CanRead(1))
        {
            byte kindByte = reader.ReadByte();
            bool hasOrdinal = (kindByte & 0x80) != 0;
            byte kind = (byte) (kindByte & 0x7f);

            // syntax offsets are not supposed to be optional, but roslyn skips emitting them when it gives a kind of 0 anyway
            // https://github.com/dotnet/roslyn/blob/e768d1f09e9a1cab9f02e7828b344ea8f7ae1e5d/src/Compilers/Core/Portable/Emit/EditAndContinueMethodDebugInformation.cs#L162-L190
            int syntaxOffset = kindByte != 0 ? (int) reader.ReadCompressedUInt32() + offsetBaseline : 0;
            uint? ordinal = hasOrdinal ? reader.ReadCompressedUInt32() : null;

            slotList.Add(new Slot
            {
                Kind = kind,
                SyntaxOffset = syntaxOffset,
                Ordinal = ordinal,
            });
        }

        return new EnCLocalSlotMapRecord
        {
            Slots = slotList.ToArray(),
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }

    public struct Slot
    {
        public byte Kind { get; set; }
        public int SyntaxOffset { get; set; }
        public uint? Ordinal { get; set; }
    }
}
