using System;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a sentinel type signature to be used in a method signature, indicating the start of any vararg
    /// argument types. 
    /// </summary>
    /// <remarks>
    /// This type signature should not be used directly.
    /// </remarks>
    public class SentinelTypeSignature : TypeSignature
    {
        internal SentinelTypeSignature()
        {
        }
        
        /// <inheritdoc />
        public override ElementType ElementType => ElementType.Sentinel;

        /// <inheritdoc />
        public override string Name => "<<SENTINEL>>";

        /// <inheritdoc />
        public override string Namespace => null;

        /// <inheritdoc />
        public override IResolutionScope Scope => null;

        /// <inheritdoc />
        public override bool IsValueType => false;

        /// <inheritdoc />
        public override TypeDefinition Resolve() => throw new InvalidOperationException();

        /// <inheritdoc />
        public override ITypeDefOrRef GetUnderlyingTypeDefOrRef() => null;

        /// <inheritdoc />
        protected override void WriteContents(IBinaryStreamWriter writer, ITypeCodedIndexProvider provider) => 
            writer.WriteByte((byte) ElementType);

        /// <inheritdoc />
        public override TypeSignature InstantiateGenericTypes(GenericContext context) => this;
    }
}