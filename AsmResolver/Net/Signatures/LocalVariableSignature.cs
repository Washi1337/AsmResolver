using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class LocalVariableSignature : CallingConventionSignature
    {
        public new static LocalVariableSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            var signature = new LocalVariableSignature
            {
                Attributes = (CallingConventionAttributes)reader.ReadByte()
            };
            
            var count = reader.ReadCompressedUInt32();

            for (int i = 0; i < count; i++)
                signature.Variables.Add(VariableSignature.FromReader(image, reader));
            return signature;
        }

        public LocalVariableSignature()
        {
            Variables = new List<VariableSignature>();
        }

        public LocalVariableSignature(IEnumerable<TypeSignature> variableTypes)
            : this(variableTypes.Select(x => new VariableSignature(x)))
        {
        }

        public LocalVariableSignature(IEnumerable<VariableSignature> variables)
        {
            Variables = new List<VariableSignature>(variables);
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

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte(0x07);
            writer.WriteCompressedUInt32((uint)Variables.Count);
            foreach (var variable in Variables)
                variable.Write(buffer, writer);
        }
    }
}
