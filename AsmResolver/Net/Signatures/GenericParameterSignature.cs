using System;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a reference to a type parameter defined in either a generic method or type.
    /// </summary>
    public class GenericParameterSignature : TypeSignature
    {
        /// <summary>
        /// Reads a single generic parameter signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the parameter was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="parameterType">Determines whether the parameter signature is referencing a type parameter from the enclosing method or type.</param>
        /// <returns>The read signature.</returns>
        public static GenericParameterSignature FromReader(MetadataImage image, IBinaryStreamReader reader, GenericParameterType parameterType)
        {
            return reader.TryReadCompressedUInt32(out uint index) 
                ? new GenericParameterSignature(parameterType, (int) index)
                : null;
        }

        public GenericParameterSignature(GenericParameterType parameterType, int index)
        {
            ParameterType = parameterType;
            Index = index;
        }

        /// <inheritdoc />
        public override ElementType ElementType
        {
            get
            {
                switch (ParameterType)
                {
                    case GenericParameterType.Type:
                        return ElementType.Var;
                    case GenericParameterType.Method:
                        return ElementType.MVar;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether a type parameter of the enclosing method or type is used.
        /// </summary>
        public GenericParameterType ParameterType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the type parameter that was referenced.
        /// </summary>
        public int Index
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string Name
        {
            get
            {
                switch (ParameterType)
                {
                    case GenericParameterType.Method:
                        return "!!" + Index.ToString();
                    case GenericParameterType.Type:
                        return "!" + Index.ToString();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <inheritdoc />
        public override string Namespace => string.Empty;

        /// <inheritdoc />
        public override IResolutionScope ResolutionScope => null;

        public override TypeSignature InstantiateGenericTypes(IGenericContext context)
        {
            IGenericArgumentsProvider provider;
            switch (ParameterType)
            {
                case GenericParameterType.Type:
                    provider = context.Type;
                    break;
                case GenericParameterType.Method:
                    provider = context.Method;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Index >= 0 && Index < provider.GenericArguments.Count)
                return provider.GenericArguments[Index];
            return this;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof(byte) +
                   Index.GetCompressedSize() +
                   base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            writer.WriteCompressedUInt32((uint)Index);

            base.Write(buffer, writer);
        }
    }
}
