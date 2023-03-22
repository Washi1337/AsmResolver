using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents an instantiation of a generic type.
    /// </summary>
    public class GenericInstanceTypeSignature : TypeSignature, IGenericArgumentsProvider
    {
        private readonly List<TypeSignature> _typeArguments;
        private ITypeDefOrRef _genericType;
        private bool _isValueType;

        internal new static GenericInstanceTypeSignature FromReader(ref BlobReaderContext context, ref BinaryStreamReader reader)
        {
            var genericType = TypeSignature.FromReader(ref context, ref reader);
            var signature = new GenericInstanceTypeSignature(genericType.ToTypeDefOrRef(), genericType.ElementType == ElementType.ValueType);

            if (!reader.TryReadCompressedUInt32(out uint count))
            {
                context.ReaderContext.BadImage("Invalid number of type arguments in generic type signature.");
                return signature;
            }

            signature._typeArguments.Capacity = (int) count;
            for (int i = 0; i < count; i++)
                signature._typeArguments.Add(TypeSignature.FromReader(ref context, ref reader));

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
            _genericType = genericType;
            _typeArguments = new List<TypeSignature>(typeArguments);
            _isValueType = isValueType;
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.GenericInst;

        /// <summary>
        /// Gets or sets the underlying generic type definition or reference.
        /// </summary>
        public ITypeDefOrRef GenericType
        {
            get => _genericType;
            set
            {
                _genericType = value;
                _isValueType = _genericType.IsValueType;
            }
        }

        /// <summary>
        /// Gets a collection of type arguments used to instantiate the generic type.
        /// </summary>
        public IList<TypeSignature> TypeArguments => _typeArguments;

        /// <inheritdoc />
        public override string? Name
        {
            get
            {
                string genericArgString = string.Join(", ", TypeArguments);
                return $"{GenericType.Name ?? NullTypeToString}<{genericArgString}>";
            }
        }

        /// <inheritdoc />
        public override string? Namespace => GenericType.Namespace;

        /// <inheritdoc />
        public override IResolutionScope? Scope => GenericType.Scope;

        /// <inheritdoc />
        public override ModuleDefinition? Module => GenericType.Module;

        /// <inheritdoc />
        public override bool IsValueType => _isValueType;

        /// <inheritdoc />
        public override TypeDefinition? Resolve() => GenericType.Resolve();

        /// <inheritdoc />
        public override ITypeDefOrRef? GetUnderlyingTypeDefOrRef() => GenericType;

        /// <inheritdoc />
        public override bool IsImportedInModule(ModuleDefinition module)
        {
            if (!GenericType.IsImportedInModule(module))
                return false;

            for (int i = 0; i < TypeArguments.Count; i++)
            {
                if (!TypeArguments[i].IsImportedInModule(module))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override TypeSignature? GetDirectBaseClass()
        {
            var genericType = GenericType.Resolve();
            if (genericType is null)
                return null;

            // Interfaces have System.Object as direct base class.
            if (genericType.IsInterface)
                return Module!.CorLibTypeFactory.Object;

            if (genericType.BaseType is not { } baseType)
                return null;

            // If the base type is not generic, treat it as a normal TypeDefOrRef.
            if (baseType is TypeDefinition or TypeReference)
                return baseType.ToTypeSignature(IsValueType);

            // At this point we expect a type specification. Substitute any generic type arguments present in it.
            return baseType is TypeSpecification { Signature: { } signatureBaseType }
                ? signatureBaseType.StripModifiers().InstantiateGenericTypes(GenericContext.FromType(this))
                : null;
        }

        /// <inheritdoc />
        public override IEnumerable<TypeSignature> GetDirectlyImplementedInterfaces()
        {
            var type = GenericType.Resolve();
            if (type is null)
                return Enumerable.Empty<TypeSignature>();

            var context = GenericContext.FromType(this);
            return type.Interfaces.Select(i => i.Interface!.ToTypeSignature(false).InstantiateGenericTypes(context));
        }

        /// <inheritdoc />
        protected override bool IsDirectlyCompatibleWith(TypeSignature other, SignatureComparer comparer)
        {
            if (base.IsDirectlyCompatibleWith(other, comparer))
                return true;

            // Other type must be a generic instance with the same generic base type and type argument count.
            if (other is not GenericInstanceTypeSignature otherGenericInstance
                || otherGenericInstance.TypeArguments.Count != TypeArguments.Count
                || !comparer.Equals(GenericType, otherGenericInstance.GenericType))
            {
                return false;
            }

            // If resolution fails, assume no parameter variance.
            var genericType = GenericType.Resolve();

            // Check that every type argument is compatible with each other.
            for (int i = 0; i < TypeArguments.Count; i++)
            {
                var variance = genericType?.GenericParameters[i].Attributes & GenericParameterAttributes.VarianceMask;

                bool argumentIsCompatible = variance switch
                {
                    GenericParameterAttributes.NonVariant =>
                        comparer.Equals(TypeArguments[i].StripModifiers(), otherGenericInstance.TypeArguments[i].StripModifiers()),
                    GenericParameterAttributes.Covariant =>
                        TypeArguments[i].IsCompatibleWith(otherGenericInstance.TypeArguments[i], comparer),
                    GenericParameterAttributes.Contravariant =>
                        otherGenericInstance.TypeArguments[i].IsCompatibleWith(TypeArguments[i], comparer),
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (!argumentIsCompatible)
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override void WriteContents(in BlobSerializationContext context)
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

        /// <inheritdoc />
        public override TResult AcceptVisitor<TState, TResult>(ITypeSignatureVisitor<TState, TResult> visitor,
            TState state) =>
            visitor.VisitGenericInstanceType(this, state);

    }
}
