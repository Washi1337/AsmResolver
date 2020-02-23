using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents an instantiation of a generic type.
    /// </summary>
    public class GenericInstanceTypeSignature : TypeSignature
    {
        internal new static GenericInstanceTypeSignature FromReader(ModuleDefinition module, IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            var elementType = (ElementType) reader.ReadByte();
            var genericType = ReadTypeDefOrRef(module, reader, protection, true);
            var signature = new GenericInstanceTypeSignature(genericType, elementType == ElementType.ValueType);

            if (!reader.TryReadCompressedUInt32(out uint count))
                return signature;

            for (int i = 0; i < count; i++)
                signature.TypeArguments.Add(TypeSignature.FromReader(module, reader, protection));

            return signature;
        }

        /// <summary>
        /// Creates a new instantiation of a generic type.
        /// </summary>
        /// <param name="genericType">The type to instantiate.</param>
        /// <param name="isValueType">Indicates the type is a value type or not.</param>
        public GenericInstanceTypeSignature(ITypeDefOrRef genericType, bool isValueType)
            : this(genericType, isValueType, Enumerable.Empty<TypeSignature>())
        {
        }

        /// <summary>
        /// Creates a new instantiation of a generic type.
        /// </summary>
        /// <param name="genericType">The type to instantiate.</param>
        /// <param name="isValueType">Indicates the type is a value type or not.</param>
        /// <param name="typeArguments">The arguments to use for instantiating the generic type.</param>
        public GenericInstanceTypeSignature(ITypeDefOrRef genericType, bool isValueType,
            params TypeSignature[] typeArguments)
            : this(genericType, isValueType, typeArguments.AsEnumerable())
        {
        }

        private GenericInstanceTypeSignature(ITypeDefOrRef genericType, bool isValueType,
            IEnumerable<TypeSignature> typeArguments)
        {
            GenericType = genericType;
            TypeArguments = new List<TypeSignature>(typeArguments);
            IsValueType = isValueType;
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.GenericInst;

        /// <summary>
        /// Gets or sets the underlying generic type definition or reference.
        /// </summary>
        public ITypeDefOrRef GenericType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of type arguments used to instantiate the generic type.
        /// </summary>
        public IList<TypeSignature> TypeArguments
        {
            get;
        }

        /// <inheritdoc />
        public override string Name
        {
            get
            {
                string genericArgString = string.Join(", ", TypeArguments);
                return $"{GenericType.Name}<{genericArgString}>";
            }
        }

        /// <inheritdoc />
        public override string Namespace => GenericType.Namespace;

        /// <inheritdoc />
        public override IResolutionScope Scope => GenericType.Scope;

        /// <inheritdoc />
        public override bool IsValueType
        {
            get;
        }

        /// <inheritdoc />
        public override TypeDefinition Resolve() => GenericType.Resolve();

        /// <inheritdoc />
        public override ITypeDefOrRef GetUnderlyingTypeDefOrRef() => GenericType;

        /// <inheritdoc />
        protected override void WriteContents(IBinaryStreamWriter writer, ITypeCodedIndexProvider provider)
        {
            writer.WriteByte((byte) ElementType);
            writer.WriteByte((byte) (IsValueType ? ElementType.ValueType : ElementType.Class));
            TypeSignature.WriteTypeDefOrRef(writer, provider, GenericType);
            writer.WriteCompressedUInt32((uint) TypeArguments.Count);
            for (int i = 0; i < TypeArguments.Count; i++)
                TypeArguments[i].Write(writer, provider);
        }
    }
}