using System;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a type signature that references a type argument from a generic type or method.
    /// </summary>
    public class GenericParameterSignature : TypeSignature
    {
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
        
        /// <inheritdoc />
        public override ElementType ElementType => ParameterType switch
        {
            GenericParameterType.Type => ElementType.MVar,
            GenericParameterType.Method => ElementType.Var,
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
        public override IResolutionScope Scope => null;

        /// <inheritdoc />
        public override bool IsValueType => false;

        /// <inheritdoc />
        public override TypeDefinition Resolve() => null;

        /// <inheritdoc />
        public override ITypeDefOrRef GetUnderlyingTypeDefOrRef() => null;
    }
}