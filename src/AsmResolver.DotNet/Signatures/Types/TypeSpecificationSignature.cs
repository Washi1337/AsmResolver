namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Provides a base for type signatures that are based on another type signature.
    /// </summary>
    public abstract class TypeSpecificationSignature : TypeSignature
    {
        /// <summary>
        /// Initializes a new type specification.
        /// </summary>
        /// <param name="baseType">The type to base the specification on.</param>
        protected TypeSpecificationSignature(TypeSignature baseType)
        {
            BaseType = baseType;
        }
        
        /// <summary>
        /// Gets the type this specification is based on. 
        /// </summary>
        public TypeSignature BaseType
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string Namespace => BaseType.Namespace;

        /// <inheritdoc />
        public override IResolutionScope Scope => BaseType.Scope;

        /// <inheritdoc />
        public override TypeDefinition Resolve() => 
            BaseType.Resolve();

        /// <inheritdoc />
        public override ITypeDefOrRef GetUnderlyingTypeDefOrRef() => 
            BaseType.GetUnderlyingTypeDefOrRef();

        /// <inheritdoc />
        protected override void WriteContents(IBinaryStreamWriter writer, ITypeCodedIndexProvider provider)
        {
            writer.WriteByte((byte) ElementType);
            BaseType.Write(writer, provider);
        }
    }
}