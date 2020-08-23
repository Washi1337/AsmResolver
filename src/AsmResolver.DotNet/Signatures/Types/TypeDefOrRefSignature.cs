using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents a type signature that references a type in the metadata tables of a .NET module.
    /// </summary>
    public class TypeDefOrRefSignature : TypeSignature
    {
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
            Type = type;
            IsValueType = isValueType;
            
        }

        /// <summary>
        /// Gets the metadata type that is referenced by this signature. 
        /// </summary>
        public ITypeDefOrRef Type
        {
            get;
        }

        /// <inheritdoc />
        public override ElementType ElementType => IsValueType ? ElementType.ValueType : ElementType.Class;

        /// <inheritdoc />
        public override string Name => Type?.Name ?? NullTypeToString;

        /// <inheritdoc />
        public override string Namespace => Type?.Namespace;

        /// <inheritdoc />
        public override IResolutionScope Scope => Type?.Scope;

        /// <inheritdoc />
        public override bool IsValueType
        {
            get;
        }

        /// <inheritdoc />
        public override TypeDefinition Resolve()
        {
            return Type.Resolve();
        }

        /// <inheritdoc />
        public override ITypeDefOrRef ToTypeDefOrRef() => Type;

        /// <inheritdoc />
        public override ITypeDefOrRef GetUnderlyingTypeDefOrRef() => Type;
        
        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) => 
            visitor.VisitTypeDefOrRef(this);
        
        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte) ElementType);
            WriteTypeDefOrRef(context, Type, "Underlying type");
        }
    }
}