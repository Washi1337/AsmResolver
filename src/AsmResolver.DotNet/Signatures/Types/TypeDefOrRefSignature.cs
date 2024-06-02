using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents a type signature that references a type in the metadata tables of a .NET module.
    /// </summary>
    public class TypeDefOrRefSignature : TypeSignature
    {
        private ITypeDefOrRef _type;
        private bool _isValueType;

        /// <summary>
        /// Creates a new type signature referencing a type in a type metadata table.
        /// </summary>
        /// <param name="type">The type to reference.</param>
        public TypeDefOrRefSignature(ITypeDefOrRef type)
            : this(type, type.IsValueType)
        {
        }

        /// <summary>
        /// Creates a new type signature referencing a type in a type metadata table.
        /// </summary>
        /// <param name="type">The type to reference.</param>
        /// <param name="isValueType">Indicates whether the referenced type is a value type or not.</param>
        public TypeDefOrRefSignature(ITypeDefOrRef type, bool isValueType)
        {
            _type = type;
            _isValueType = isValueType;
        }

        /// <summary>
        /// Gets the metadata type that is referenced by this signature.
        /// </summary>
        public ITypeDefOrRef Type
        {
            get => _type;
            set
            {
                _type = value;
                _isValueType = value.IsValueType;
            }
        }

        /// <inheritdoc />
        public override ElementType ElementType => IsValueType ? ElementType.ValueType : ElementType.Class;

        /// <inheritdoc />
        public override string Name => Type?.Name ?? NullTypeToString;

        /// <inheritdoc />
        public override string? Namespace => Type.Namespace;

        /// <inheritdoc />
        public override IResolutionScope? Scope => Type.Scope;

        /// <inheritdoc />
        public override ModuleDefinition? Module => Type.Module;

        /// <inheritdoc />
        public override bool IsValueType => _isValueType;

        /// <inheritdoc />
        public override TypeDefinition? Resolve() => Type.Resolve();

        /// <inheritdoc />
        public override bool IsImportedInModule(ModuleDefinition module) => Type.IsImportedInModule(module);

        /// <inheritdoc />
        public override ITypeDefOrRef ToTypeDefOrRef() => Type;

        /// <inheritdoc />
        public override ITypeDefOrRef? GetUnderlyingTypeDefOrRef() => Type;

        /// <inheritdoc />
        public override TypeSignature GetUnderlyingType()
        {
            var type = Type.Resolve();

            if (type is {IsEnum: true})
                return type.GetEnumUnderlyingType() ?? this;

            return this;
        }

        /// <inheritdoc />
        public override TypeSignature GetReducedType()
        {
            var underlyingType = GetUnderlyingType();
            return !ReferenceEquals(underlyingType, this)
                ? underlyingType.GetReducedType()
                : this;
        }

        /// <inheritdoc />
        public override TypeSignature? GetDirectBaseClass()
        {
            var type = Type.Resolve();
            if (type is null)
                return null;

            // Interfaces have System.Object as direct base class.
            return type.IsInterface
                ? Module!.CorLibTypeFactory.Object
                : type.BaseType!.ToTypeSignature(false).StripModifiers();
        }

        /// <inheritdoc />
        public override IEnumerable<TypeSignature> GetDirectlyImplementedInterfaces()
        {
            var type = Type.Resolve();
            if (type is null)
                return Enumerable.Empty<TypeSignature>();

            return type.Interfaces.Select(i => i.Interface!.ToTypeSignature(false));
        }

        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) =>
            visitor.VisitTypeDefOrRef(this);

        /// <inheritdoc />
        public override TResult AcceptVisitor<TState, TResult>(ITypeSignatureVisitor<TState, TResult> visitor,
            TState state) =>
            visitor.VisitTypeDefOrRef(this, state);

        /// <inheritdoc />
        protected override void WriteContents(in BlobSerializationContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte) ElementType);
            WriteTypeDefOrRef(context, Type, "Underlying type");
        }
    }
}
