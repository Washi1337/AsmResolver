using System;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents a type signature that references a type argument from a generic type or method.
    /// </summary>
    public class GenericParameterSignature : TypeSignature
    {
        private readonly IResolutionScope _scope;

        /// <summary>
        /// Creates a new reference to a generic parameter. 
        /// </summary>
        /// <param name="parameterType">Indicates the parameter signature is declared by a type or a method.</param>
        /// <param name="index">The index of the referenced parameter.</param>
        public GenericParameterSignature(GenericParameterType parameterType, int index)
        {
            ParameterType = parameterType;
            Index = index;
        }

        /// <summary>
        /// Creates a new reference to a generic parameter. 
        /// </summary>
        /// <param name="module">The module in which this generic parameter signature resides.</param>
        /// <param name="parameterType">Indicates the parameter signature is declared by a type or a method.</param>
        /// <param name="index">The index of the referenced parameter.</param>
        public GenericParameterSignature(ModuleDefinition module, GenericParameterType parameterType, int index)
        {
            _scope = module;
            ParameterType = parameterType;
            Index = index;
        }
        
        /// <inheritdoc />
        public override ElementType ElementType => ParameterType switch
        {
            GenericParameterType.Type => ElementType.Var,
            GenericParameterType.Method => ElementType.MVar,
            _ => throw new ArgumentOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets a value indicating whether this parameter signature is declared by a type or a method.
        /// generic parameter.
        /// </summary>
        public GenericParameterType ParameterType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the referenced generic parameter.
        /// </summary>
        public int Index
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string Name => ParameterType switch
        {
            GenericParameterType.Type => $"!{Index}",
            GenericParameterType.Method => $"!!{Index}",
            _ => throw new ArgumentOutOfRangeException()
        };

        /// <inheritdoc />
        public override string Namespace => null;

        /// <inheritdoc />
        public override IResolutionScope Scope => _scope;

        /// <inheritdoc />
        public override bool IsValueType => false;

        /// <inheritdoc />
        public override TypeDefinition Resolve() => null;

        /// <inheritdoc />
        public override ITypeDefOrRef GetUnderlyingTypeDefOrRef() => null;

        /// <inheritdoc />
        protected override void WriteContents(IBinaryStreamWriter writer, ITypeCodedIndexProvider provider)
        {
            writer.WriteByte((byte) ElementType);
            writer.WriteCompressedUInt32((uint) Index);
        }

        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) => 
            visitor.VisitGenericParameter(this);
    }
}