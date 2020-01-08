namespace AsmResolver.DotNet.Blob
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
        public override TypeDefinition Resolve()
        {
            return BaseType.Resolve();
        }

        /// <inheritdoc />
        public override TypeSignature GetLeafType()
        {
            return BaseType.GetLeafType();
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return (uint) (sizeof(byte) + BaseType.GetPhysicalSize() + ExtraData.Length);
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte) ElementType);
            BaseType.Write(writer);
            base.Write(writer);
        }
    }
}