using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class CustomAttributeSignature : BlobSignature
    {
        public static CustomAttributeSignature FromReader(CustomAttribute parent, IBinaryStreamReader reader)
        {
            if (!reader.CanRead(sizeof (ushort)) || reader.ReadUInt16() != 0x0001)
                throw new ArgumentException("Signature doesn't refer to a valid custom attribute signature.");

            var signature = new CustomAttributeSignature();

            if (parent.Constructor != null)
            {
                var methodSignature = parent.Constructor.Signature as MethodSignature;
                if (methodSignature != null)
                {
                    foreach (var parameter in methodSignature.Parameters)
                    {
                        signature.FixedArguments.Add(CustomAttributeArgument.FromReader(parent.Image, parameter.ParameterType, reader));
                    }
                }
            }

            var namedElementCount = reader.CanRead(sizeof (ushort)) ? reader.ReadUInt16() : 0;
            for (uint i = 0; i < namedElementCount; i++)
            {
                signature.NamedArguments.Add(CustomAttributeNamedArgument.FromReader(parent.Image, reader));
            }

            return signature;
        }

        public CustomAttributeSignature()
            : this(Enumerable.Empty<CustomAttributeArgument>())
        {
        }

        public CustomAttributeSignature(IEnumerable<CustomAttributeArgument> fixedArguments)
        {
            FixedArguments = new List<CustomAttributeArgument>(fixedArguments);
            NamedArguments = new List<CustomAttributeNamedArgument>();   
        }

        public IList<CustomAttributeArgument> FixedArguments
        {
            get;
            private set;
        }

        public IList<CustomAttributeNamedArgument> NamedArguments
        {
            get;
            private set;
        }

        public override uint GetPhysicalLength()
        {
            return (uint)(sizeof (ushort) +
                          FixedArguments.Sum(x => x.GetPhysicalLength()) +
                          sizeof (ushort) +
                          NamedArguments.Sum(x => x.GetPhysicalLength()));
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteUInt16(0x0001);
            foreach (var argument in FixedArguments)
                argument.Write(buffer, writer);
            writer.WriteUInt16((ushort)NamedArguments.Count);
            foreach (var argument in NamedArguments)
                argument.Write(buffer, writer);
        }
    }
}
