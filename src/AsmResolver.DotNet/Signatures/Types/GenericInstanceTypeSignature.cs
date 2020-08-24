using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents an instantiation of a generic type.
    /// </summary>
    public class GenericInstanceTypeSignature : TypeSignature, IGenericArgumentsProvider
    {
        internal new static GenericInstanceTypeSignature FromReader(ModuleDefinition module, IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            var genericType = TypeSignature.FromReader(module, reader, protection);
            var signature = new GenericInstanceTypeSignature(genericType.ToTypeDefOrRef(), genericType.ElementType == ElementType.ValueType);

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
                return $"{GenericType?.Name ?? NullTypeToString}<{genericArgString}>";
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
        protected override void WriteContents(BlobSerializationContext context)
        {
            var writer = context.Writer;
            
            writer.WriteByte((byte) ElementType);
            writer.WriteByte((byte) (IsValueType ? ElementType.ValueType : ElementType.Class));
            WriteTypeDefOrRef(context, GenericType, "Underlying generic type");
            writer.WriteCompressedUInt32((uint) TypeArguments.Count);
            
            for (int i = 0; i < TypeArguments.Count; i++)
                TypeArguments[i].Write(context);
        }
        
        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) => 
            visitor.VisitGenericInstanceType(this);
    }
}