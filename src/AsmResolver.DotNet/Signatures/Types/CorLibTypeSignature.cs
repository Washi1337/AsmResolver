using System;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables;

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
            {
                Type = module.TopLevelTypes.First(t => t.IsTypeOf("System", name));
            }
            else if (corlibScope.Module is not null)
            {
                Type = new TypeReference(corlibScope.Module, corlibScope, "System", name);
            }
            else
            {
                throw new ArgumentException("Provided CorLib was not valid.");
            }
        }

        /// <summary>
        /// Gets the reference to the type as it is defined in the common object runtime library.
        /// </summary>
        public ITypeDefOrRef Type
        {
            get;
        }

        /// <inheritdoc />
        public override string? Name
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
        public override TypeDefinition? Resolve()
        {
            return Type.Resolve();
        }

        /// <inheritdoc />
        public override bool IsImportedInModule(ModuleDefinition module) => Module == module;

        /// <inheritdoc />
        public override ITypeDefOrRef? GetUnderlyingTypeDefOrRef()
        {
            return Type;
        }

        /// <inheritdoc />
        public override ElementType ElementType
        {
            get;
        }

        /// <inheritdoc />
        public override IResolutionScope? Scope
        {
            get;
        }

        /// <inheritdoc />
        public override ITypeDefOrRef ToTypeDefOrRef() => Type;

        /// <inheritdoc />
        public override TypeSignature GetReducedType()
        {
            var factory = Module!.CorLibTypeFactory;
            return ElementType switch
            {
                ElementType.I1 or ElementType.U1 => factory.SByte,
                ElementType.I2 or ElementType.U2 => factory.Int16,
                ElementType.I4 or ElementType.U4 => factory.Int32,
                ElementType.I8 or ElementType.U8 => factory.Int64,
                ElementType.I or ElementType.U => factory.IntPtr,
                _ => base.GetReducedType()
            };
        }

        /// <inheritdoc />
        public override TypeSignature GetVerificationType()
        {
            var factory = Module!.CorLibTypeFactory;
            return GetReducedType().ElementType switch
            {
                ElementType.I1 or ElementType.Boolean => factory.SByte,
                ElementType.I2 or ElementType.Char => factory.Int16,
                ElementType.I4 => factory.Int32,
                ElementType.I8 => factory.Int64,
                ElementType.I => factory.IntPtr,
                _ => base.GetVerificationType()
            };
        }

        /// <inheritdoc />
        public override TypeSignature GetIntermediateType()
        {
            var factory = Module!.CorLibTypeFactory;
            var verificationType = GetVerificationType();
            return verificationType.ElementType switch
            {
                ElementType.I1 or ElementType.I2 or ElementType.I4 => factory.Int32,
                ElementType.R4 or ElementType.R8 => factory.Double, // Technically, this is F.
                _ => verificationType
            };
        }

        /// <inheritdoc />
        protected override void WriteContents(in BlobSerializationContext context) =>
            context.Writer.WriteByte((byte) ElementType);

        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) =>
            visitor.VisitCorLibType(this);

        /// <inheritdoc />
        public override TResult AcceptVisitor<TState, TResult>(ITypeSignatureVisitor<TState, TResult> visitor,
            TState state) =>
            visitor.VisitCorLibType(this, state);
    }
}
