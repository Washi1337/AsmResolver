using System;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents a blob type signature referencing an element type defined in a common object runtime library such as
    /// mscorlib (.NET framework) or System.Private.CorLib (.NET Core).
    /// </summary>
    public class CorLibTypeSignature : TypeSignature
    {
        internal CorLibTypeSignature(IResolutionScope corlibScope, ElementType elementType, string name)
        {
            Scope = corlibScope;
            ElementType = elementType;
            Name = name;

            if (corlibScope is ModuleDefinition module)
                Type = module.TopLevelTypes.First(t => t.IsTypeOf(Namespace, Name));
            else
                Type = new TypeReference(corlibScope.Module, corlibScope, Namespace, Name);
        }

        /// <summary>
        /// Gets the reference to the type as it is defined in the common object runtime library.
        /// </summary>
        public ITypeDefOrRef Type
        {
            get;
        }

        /// <inheritdoc />
        public override string Name
        {
            get;
        }

        /// <inheritdoc />
        public override string Namespace => "System";

        /// <inheritdoc />
        public override bool IsValueType =>
            ElementType switch
            {
                ElementType.String => false,
                ElementType.Object => false,
                ElementType.Void => true,
                ElementType.Boolean => true,
                ElementType.Char => true,
                ElementType.I1 => true,
                ElementType.U1 => true,
                ElementType.I2 => true,
                ElementType.U2 => true,
                ElementType.I4 => true,
                ElementType.U4 => true,
                ElementType.I8 => true,
                ElementType.U8 => true,
                ElementType.R4 => true,
                ElementType.R8 => true,
                ElementType.TypedByRef => true,
                ElementType.I => true,
                ElementType.U => true,
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <inheritdoc />
        public override TypeDefinition Resolve()
        {
            return Type.Resolve();
        }

        /// <inheritdoc />
        public override ITypeDefOrRef GetUnderlyingTypeDefOrRef()
        {
            return Type;
        }

        /// <inheritdoc />
        public override ElementType ElementType
        {
            get;
        }

        /// <inheritdoc />
        public override IResolutionScope Scope
        {
            get;
        }

        /// <inheritdoc />
        public override ITypeDefOrRef ToTypeDefOrRef() => Type;

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context) =>
            context.Writer.WriteByte((byte) ElementType);
        
        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) => 
            visitor.VisitCorLibType(this);
    }
}