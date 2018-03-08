using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class CustomAttributeArgument : BlobSignature
    {
        public static CustomAttributeArgument FromReader(MetadataImage image, TypeSignature typeSignature, IBinaryStreamReader reader)
        {
            var signature = new CustomAttributeArgument()
            {
                ArgumentType = typeSignature
            };

            if (typeSignature.ElementType != ElementType.SzArray)
            {
                signature.Elements.Add(ElementSignature.FromReader(image, typeSignature, reader));
            }
            else
            {
                var arrayType = ((SzArrayTypeSignature)typeSignature).BaseType;

                var elementCount = reader.CanRead(sizeof (uint)) ? reader.ReadUInt32() : uint.MaxValue;
                if (elementCount != uint.MaxValue)
                {
                    for (uint i = 0; i < elementCount; i++)
                    {
                        signature.Elements.Add(ElementSignature.FromReader(image, arrayType, reader));
                    }
                }
            }

            return signature;
        }

        private CustomAttributeArgument()
        {
            Elements = new List<ElementSignature>();
        }

        public CustomAttributeArgument(TypeSignature argumentType, ElementSignature value)
            : this(argumentType, new[] { value })
        {
        }

        public CustomAttributeArgument(TypeSignature argumentType, params ElementSignature[] values)
            : this()
        {
            if (argumentType == null)
                throw new ArgumentNullException("argumentType");
            if (values == null)
                throw new ArgumentNullException("values");
            ArgumentType = argumentType;
            foreach (var value in values)
                Elements.Add(value);
        }

        public TypeSignature ArgumentType
        {
            get;
            set;
        }

        public IList<ElementSignature> Elements
        {
            get;
            private set;
        }

        public override uint GetPhysicalLength()
        {
            return
                (uint)
                    (ArgumentType.ElementType != ElementType.SzArray
                        ? Elements[0].GetPhysicalLength()
                        : sizeof(uint) + Elements.Sum(x => x.GetPhysicalLength()));
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            if (ArgumentType.ElementType != ElementType.SzArray)
                Elements[0].Write(buffer, writer);
            else
            {
                writer.WriteUInt32((uint)Elements.Count);
                foreach (var element in Elements)
                    element.Write(buffer, writer);
            }
        }
    }
}
