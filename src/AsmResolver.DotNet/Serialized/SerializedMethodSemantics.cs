using System;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="MethodSemantics"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedMethodSemantics : MethodSemantics
    {
        private readonly IMetadata _metadata;
        private readonly SerializedModuleDefinition _parentModule;
        private readonly MethodSemanticsRow _row;

        /// <summary>
        /// Creates a method semantics object from a method semantics row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="parentModule"></param>
        /// <param name="token">The token to initialize the semantics for.</param>
        /// <param name="row">The metadata table row to base the semantics on.</param>
        public SerializedMethodSemantics(IMetadata metadata, SerializedModuleDefinition parentModule, MetadataToken token, MethodSemanticsRow row)
            : base(token)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _parentModule = parentModule;
            _row = row;
            
            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override MethodDefinition GetMethod()
        {
            var token = new MetadataToken(TableIndex.Method, _row.Method);
            return _parentModule.TryLookupMember(token, out var member)
                ? member as MethodDefinition
                : null;
        }

        /// <inheritdoc />
        protected override IHasSemantics GetAssociation()
        {
            var encoder = _metadata.GetStream<TablesStream>().GetIndexEncoder(CodedIndex.HasSemantics);
            var token = encoder.DecodeIndex(_row.Association);
            return _parentModule.TryLookupMember(token, out var member)
                ? member as IHasSemantics
                : null;
        }
    }
}