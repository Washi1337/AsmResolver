using System;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a sentinel type signature to be used in a method signature, indicating the start of any vararg
    /// argument types.
    /// </summary>
    /// <remarks>
    /// This type signature should not be used directly.
    /// </remarks>
    public sealed class SentinelTypeSignature : TypeSignature
    {
        /// <summary>
        /// The singleton instance of the sentinel type signature.
        /// </summary>
        public static SentinelTypeSignature Instance { get; } = new();

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.Sentinel;

        /// <inheritdoc />
        public override string Name => "<<SENTINEL>>";

        /// <inheritdoc />
        public override string? Namespace => null;

        /// <inheritdoc />
        public override IResolutionScope? Scope => null;

        /// <inheritdoc />
        public override bool IsValueType => false;

        private SentinelTypeSignature()
        {
        }

        /// <inheritdoc />
        public override TypeDefinition? Resolve(ModuleDefinition context) => throw new InvalidOperationException();

        /// <inheritdoc />
        public override ITypeDefOrRef? GetUnderlyingTypeDefOrRef() => null;

        /// <inheritdoc />
        public override bool IsImportedInModule(ModuleDefinition module) => true;

        /// <inheritdoc />
        protected override void WriteContents(in BlobSerializationContext context) =>
            context.Writer.WriteByte((byte) ElementType);

        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) =>
            visitor.VisitSentinelType(this);

        /// <inheritdoc />
        public override TResult AcceptVisitor<TState, TResult>(ITypeSignatureVisitor<TState, TResult> visitor,
            TState state) =>
            visitor.VisitSentinelType(this, state);
    }
}
