using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents an argument value in a custom attribute construction.
    /// </summary>
    public class CustomAttributeArgument
    {
        /// <summary>
        /// Reads a single argument from the provided input stream.
        /// </summary>
        /// <param name="parentModule">The module the signature resides in.</param>
        /// <param name="argumentType">The type of the argument to read.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The argument.</returns>
        public static CustomAttributeArgument FromReader(ModuleDefinition parentModule, TypeSignature argumentType,
            IBinaryStreamReader reader)
        {
            return argumentType.ElementType != ElementType.SzArray
                ? ReadSimpleArgument(parentModule, argumentType, reader)
                : ReadSzArrayArgument(parentModule, argumentType, reader);
        }
        
        private static CustomAttributeArgument ReadSimpleArgument(ModuleDefinition parentModule,
            TypeSignature argumentType, IBinaryStreamReader reader)
        {
            var result = new CustomAttributeArgument(argumentType);
            result.Elements.Add(CustomAttributeArgumentElement.FromReader(parentModule, argumentType, reader));
            return result;
        }

        private static CustomAttributeArgument ReadSzArrayArgument(ModuleDefinition parentModule,
            TypeSignature argumentType, IBinaryStreamReader reader)
        {
            var result = new CustomAttributeArgument(argumentType);
            
            var arrayElementType = ((SzArrayTypeSignature) argumentType).BaseType;
            uint elementCount = reader.CanRead(sizeof(uint)) ? reader.ReadUInt32() : uint.MaxValue;
            result.IsNullArray = elementCount == uint.MaxValue;
            
            if (!result.IsNullArray)
            {
                for (uint i = 0; i < elementCount; i++)
                {
                    var element = CustomAttributeArgumentElement.FromReader(parentModule, arrayElementType, reader);
                    result.Elements.Add(element);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a new custom attribute argument. 
        /// </summary>
        /// <param name="argumentType">The type of the argument.</param>
        public CustomAttributeArgument(TypeSignature argumentType)
        {
            ArgumentType = argumentType ?? throw new ArgumentNullException(nameof(argumentType));
        }

        /// <summary>
        /// Gets or sets the type of the argument value.
        /// </summary>
        public TypeSignature ArgumentType
        {
            get;
            set;
        }

        /// <summary>
        /// When <see cref="ArgumentType"/> is not a <see cref="SzArrayTypeSignature"/>, gets the first element of the
        /// </summary>
        public CustomAttributeArgumentElement Element => Elements.Count > 0 ? Elements[0] : default;

        /// <summary>
        /// Gets a collection of all elements that the argument is built with.
        /// </summary>
        public IList<CustomAttributeArgumentElement> Elements
        {
            get;
        } = new List<CustomAttributeArgumentElement>();

        /// <summary>
        /// Gets or sets a value indicating whether the argument represents the null array value.
        /// </summary>
        public bool IsNullArray
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return !IsNullArray
                ? ArgumentType.ElementType == ElementType.SzArray
                    ? $"{{{string.Join(", ", Elements)}}}"
                    : Element.ToString()
                : "null";
        }

        /// <summary>
        /// Writes the fixed argument to the provided output stream.
        /// </summary>
        public void Write(BlobSerializationContext context)
        {
            if (ArgumentType is SzArrayTypeSignature szArrayType)
                WriteArray(szArrayType, context);
            else
                WriteSimple(context);
        }

        private void WriteSimple(BlobSerializationContext context)
        {
            Element.Write(context, ArgumentType);
        }

        private void WriteArray(SzArrayTypeSignature szArrayType, BlobSerializationContext context)
        {
            var writer = context.Writer;
            
            if (IsNullArray)
            {
                writer.WriteUInt32(uint.MaxValue);
                return;
            }
            
            var elementType = szArrayType.BaseType;
            
            writer.WriteUInt32((uint) Elements.Count);
            for (int i = 0; i < Elements.Count; i++)
                Elements[i].Write(context, elementType);
        }
    }
}