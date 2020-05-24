using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents a type modifier indicating a boxing of a value type.
    /// </summary>
    public class BoxedTypeSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Creates a new boxed type signature..
        /// </summary>
        /// <param name="baseType">The type to box..</param>
        public BoxedTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.Boxed;

        /// <inheritdoc />
        public override string Name => BaseType.Name;

        /// <inheritdoc />
        public override bool IsValueType => false;

        /// <inheritdoc />
        public override TypeSignature InstantiateGenericTypes(GenericContext context) => 
            new BoxedTypeSignature(BaseType.InstantiateGenericTypes(context));
    }
}