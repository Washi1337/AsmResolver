using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class LocalVariableSignature : CallingConventionSignature
    {
        public new static LocalVariableSignature FromReader(MetadataImage image, IBinaryStreamReader reader,
            bool readToEnd = false)
        {
            return FromReader(image, reader, readToEnd, new RecursionProtection());
        }

        public new static LocalVariableSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            bool readToEnd, RecursionProtection protection)
        {
            var signature = new LocalVariableSignature
            {
                Attributes = (CallingConventionAttributes) reader.ReadByte()
            };

            uint count = reader.ReadCompressedUInt32();
            for (int i = 0; i < count; i++)
                signature.Variables.Add(VariableSignature.FromReader(image, reader, protection));

            if (readToEnd)
                signature.ExtraData = reader.ReadToEnd();

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

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint) (sizeof(byte) +
                           Variables.Count.GetCompressedSize() +
                           Variables.Sum(x => x.GetPhysicalLength(buffer))) +
                   base.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
            foreach (var variable in Variables)
                variable.Prepare(buffer);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte(0x07);
            writer.WriteCompressedUInt32((uint)Variables.Count);
            foreach (var variable in Variables)
                variable.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
