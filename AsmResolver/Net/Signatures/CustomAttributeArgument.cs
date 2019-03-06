using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a single fixed argument of a custom attribute associated to a member.
    /// </summary>
    public class CustomAttributeArgument : BlobSignature
    {
        /// <summary>
        /// Reads a single custom attribute argument at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the argument was defined in.</param>
        /// <param name="typeSignature">The type of the argument to read.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read argument.</returns>
        public static CustomAttributeArgument FromReader(
            MetadataImage image,
            TypeSignature typeSignature, 
            IBinaryStreamReader reader)
        {
            var signature = new CustomAttributeArgument
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
                        signature.Elements.Add(ElementSignature.FromReader(image, arrayType, reader));
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
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            ArgumentType = argumentType ?? throw new ArgumentNullException(nameof(argumentType));
            foreach (var value in values)
                Elements.Add(value);
        }

        /// <summary>
        /// Gets or sets the type of the argument.
        /// </summary>
        public TypeSignature ArgumentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of elements that make up the argument's value. For simple arguments, this list contains
        /// just one element. For array arguments, this list contains all the elements stored in the array.
        /// </summary>
        public IList<ElementSignature> Elements
        {
            get;
        }

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint)
                (ArgumentType.ElementType != ElementType.SzArray
                    ? Elements[0].GetPhysicalLength(buffer)
                    : sizeof(uint) + Elements.Sum(x => x.GetPhysicalLength(buffer)));
        }

        public override void Prepare(MetadataBuffer buffer)
        {
            ArgumentType.Prepare(buffer);
            foreach (var element in Elements)
                element.Prepare(buffer);
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
