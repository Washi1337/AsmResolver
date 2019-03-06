using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class PropertySignature : CallingConventionSignature, IHasTypeSignature
    {
        public new static PropertySignature FromReader(MetadataImage image, IBinaryStreamReader reader,
            bool readToEnd = false)
        {
            return FromReader(image, reader, readToEnd, new RecursionProtection());
        }        
        
        public new static PropertySignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader, 
            bool readToEnd,
            RecursionProtection protection)
        {
            var signature = new PropertySignature
            {
                Attributes = (CallingConventionAttributes)reader.ReadByte(),
            };

            if (!reader.TryReadCompressedUInt32(out uint paramCount))
                return null;

            signature.PropertyType = TypeSignature.FromReader(image, reader, false, protection);

            for (int i = 0; i < paramCount; i++)
                signature.Parameters.Add(ParameterSignature.FromReader(image, reader, protection));

            if (readToEnd)
                signature.ExtraData = reader.ReadToEnd();
            
            return signature;
        }

        public PropertySignature()
        {
            Parameters = new List<ParameterSignature>();
        }

        public PropertySignature(TypeSignature propertyType)
            : this(propertyType, Enumerable.Empty<ParameterSignature>())
        {
        }

        public PropertySignature(TypeSignature propertyType, IEnumerable<ParameterSignature> parameters)
        {
            PropertyType = propertyType;
            Parameters = new List<ParameterSignature>(parameters);
        }

        public TypeSignature PropertyType
        {
            get;
            set;
        }

        TypeSignature IHasTypeSignature.TypeSignature => PropertyType;

        public IList<ParameterSignature> Parameters
        {
            get;
        }

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint)(sizeof (byte) +
                          Parameters.Count.GetCompressedSize() +
                          PropertyType.GetPhysicalLength(buffer) +
                          Parameters.Sum(x => x.GetPhysicalLength(buffer)))
                + base.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
            PropertyType.Prepare(buffer);
            foreach (var parameter in Parameters)
                parameter.Prepare(buffer);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)Attributes);
            writer.WriteCompressedUInt32((uint)Parameters.Count);
            PropertyType.Write(buffer, writer);
            foreach (var parameter in Parameters)
                parameter.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
