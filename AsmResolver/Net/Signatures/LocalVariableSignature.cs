using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class LocalVariableSignature : BlobSignature
    {
        public static LocalVariableSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            if (!reader.CanRead(sizeof(byte)) || reader.ReadByte() != 0x7)
                throw new ArgumentException("Signature doesn't refer to a valid local variable signature.");

            var count = reader.ReadCompressedUInt32();

            var signature = new LocalVariableSignature();
            for (int i = 0; i < count; i++)
                signature.Variables.Add(VariableSignature.FromReader(header, reader));
            return signature;
        }

        public LocalVariableSignature()
        {
            Variables = new List<VariableSignature>();
        }

        public IList<VariableSignature> Variables
        {
            get;
            private set;
        }

        public override uint GetPhysicalLength()
        {
            return (uint)(sizeof (byte) +
                          Variables.Count.GetCompressedSize() +
                          Variables.Sum(x => x.GetPhysicalLength()));
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte(0x07);
            writer.WriteCompressedUInt32((uint)Variables.Count);
            foreach (var variable in Variables)
                variable.Write(context);
        }
    }
}
