namespace AsmResolver.DotNet.Signatures
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
        public override ModuleDefinition? ContextModule => BaseType.ContextModule;

        /// <inheritdoc />
        public override string? Namespace => BaseType.Namespace;

        /// <inheritdoc />
        public override IResolutionScope? Scope => BaseType.Scope;

        /// <inheritdoc />
        public override ITypeDefOrRef? GetUnderlyingTypeDefOrRef() => BaseType.GetUnderlyingTypeDefOrRef();

        /// <inheritdoc />
        public override bool IsImportedInModule(ModuleDefinition module) => BaseType.IsImportedInModule(module);

        /// <inheritdoc />
        protected override void WriteContents(in BlobSerializationContext context)
        {
            context.Writer.WriteByte((byte) ElementType);
            WriteBaseType(context);
        }

        /// <summary>
        /// Writes <see cref="BaseType"/> to the output stream.
        /// </summary>
        /// <param name="context">The output stream.</param>
        protected void WriteBaseType(BlobSerializationContext context) => BaseType.Write(context);
    }
}
