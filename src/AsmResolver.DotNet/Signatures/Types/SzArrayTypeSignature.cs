using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents a type signature describing a single dimension array with 0 as a lower bound.
    /// </summary>
    public class SzArrayTypeSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Reads a single dimension array type signature with 0 as a lower bound from an input stream.
        /// </summary>
        /// <param name="parentModule">The module containing the signature.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The signature.</returns>
        public new static SzArrayTypeSignature FromReader(ModuleDefinition parentModule, IBinaryStreamReader reader)
        {
            return FromReader(parentModule, reader, RecursionProtection.CreateNew());
        }
        
        /// <summary>
        /// Reads a single dimension array type signature with 0 as a lower bound from an input stream.
        /// </summary>
        /// <param name="parentModule">The module containing the signature.</param>
        /// <param name="reader">The input stream.</param>
        /// <param name="protection">The object instance responsible for detecting infinite recursion.</param>
        /// <returns>The signature.</returns>
        public new static SzArrayTypeSignature FromReader(ModuleDefinition parentModule, IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            return new SzArrayTypeSignature(TypeSignature.FromReader(parentModule, reader, protection));
        }
        
        /// <summary>
        /// Creates a new single-dimension array signature with 0 as a lower bound.
        /// </summary>
        /// <param name="baseType">The type of the elements to store in the array.</param>
        public SzArrayTypeSignature(TypeSignature baseType) 
            : base(baseType)
        {
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.SzArray;
        
        /// <inheritdoc />
        public override string Name => $"{BaseType?.Name ?? NullTypeToString}[]";

        /// <inheritdoc />
        public override bool IsValueType => false;
        
        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) => 
            visitor.VisitSzArrayType(this);
    }
}